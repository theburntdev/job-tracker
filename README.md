# Job Application Tracker

A desktop app for tracking job applications — log postings, track application stages, store contacts, and record interview notes.

## Stack

| Layer | Tech |
|---|---|
| Desktop shell | Tauri (wraps the React frontend in a native window) |
| Frontend | React 19 + TypeScript + Vite |
| Backend | C# / .NET 10 REST API — runs as a Tauri sidecar (auto-started/stopped) |
| Database | SQLite (local file, WAL mode) |

---

## Prerequisites

Install these before cloning:

| Tool | Purpose | Install |
|---|---|---|
| [Node.js 20+](https://nodejs.org/) | Frontend build | `winget install OpenJS.NodeJS.LTS` |
| [.NET 10 SDK](https://dotnet.microsoft.com/download) | Backend build | `winget install Microsoft.DotNet.SDK.10` |
| [Rust + Cargo](https://rustup.rs/) | Tauri shell | `winget install Rustlang.Rustup` |
| Tauri CLI | Build/dev commands | `cargo install tauri-cli --version "^2"` |

Optional: [DB Browser for SQLite](https://sqlitebrowser.org/) — inspect the database file directly.

---

## First-time setup

```powershell
# 1. Install frontend dependencies
cd frontend
npm install

# 2. Build the .NET API sidecar (places binary in src-tauri/binaries/)
npm run build:sidecar

# 3. Launch the desktop app (Vite dev server + Tauri window + .NET API)
cargo tauri dev
```

The app window opens. The .NET API starts automatically in the background on `http://localhost:5063` and is killed when you close the window.

---

## Daily dev workflow

```powershell
# From frontend/ — start everything:
cargo tauri dev

# Frontend only (no native window, browser at http://localhost:5173):
npm run dev

# Re-build the sidecar after backend changes:
npm run build:sidecar
```

### Backend only (no Tauri)

> **Port conflict:** The sidecar and Visual Studio both bind to `:5063`. Only one can run at a time. Close the Tauri desktop app before launching the API in Visual Studio — app exit kills the sidecar automatically.

```powershell
# From repo root — hot reload:
dotnet watch --project backend/src/JobTracker.Api

# Run all tests:
dotnet test backend/JobTracker.sln
```

### Frontend only

```powershell
# From frontend/
npm run typecheck    # type check
npm test             # Vitest watch mode
npm run test:run     # single pass (CI)
```

---

## Project structure

```
job-tracker/
  backend/               # C# solution
    src/
      JobTracker.Api/          # ASP.NET Core host, minimal API endpoints
      JobTracker.Application/  # CQRS commands/queries/handlers
      JobTracker.Domain/       # Entities, value objects — no framework refs
      JobTracker.Infrastructure/ # EF Core, SQLite, migrations
    tests/
  frontend/              # React + Tauri
    src/                 # React source (components, routes, stores, features)
    src-tauri/           # Tauri config and Rust bridge
      binaries/          # Built .NET sidecar binary (gitignored — run build:sidecar)
      tauri.conf.json    # Window config, sidecar registration
  scripts/
    build-sidecar.ps1    # Publishes .NET API → places in src-tauri/binaries/
  CLAUDE.md              # Project-wide conventions (for Claude Code)
```

---

## How the sidecar works

Tauri registers `JobTracker.Api.exe` as an external binary. On app start, Tauri spawns it and kills it on exit. React talks to it via plain HTTP — no IPC, no changes to the React code needed.

Tauri passes these environment variables to the sidecar at launch:

| Variable | Dev build (`cargo tauri dev`) | Release build (`cargo tauri build`) |
|---|---|---|
| `ASPNETCORE_URLS` | `http://localhost:5063` | `http://localhost:5063` |
| `ASPNETCORE_ENVIRONMENT` | `Development` | `Production` |
| `DB_PATH` | `%APPDATA%\com.burntdev.jobtracker\jobtracker.db` | `%APPDATA%\com.burntdev.jobtracker\jobtracker.db` |

`ASPNETCORE_ENVIRONMENT=Production` in release builds hides the OpenAPI endpoint and applies the Tauri-specific CORS policy (`https://tauri.localhost`). You never need to set these manually — Tauri injects them automatically.

The binary must be built and placed before running `cargo tauri dev` or `cargo tauri build`:

## Troubleshooting desktop install / prod build

NSIS installer hooks auto-kill stale processes before install/uninstall:
- `JobTracker.Api.exe`
- `job-tracker.exe`

This avoids installer failures when a previous app/sidecar process is still running.

```powershell
# View sidecar runtime logs (if present)
Get-Content "$env:APPDATA\com.burntdev.jobtracker\sidecar.log" -ErrorAction SilentlyContinue

# From frontend/
npm run build:sidecar

# Manual fallback if needed:
Stop-Process -Name "JobTracker.Api" -ErrorAction SilentlyContinue
Stop-Process -Name "job-tracker" -ErrorAction SilentlyContinue
```

This runs `scripts/build-sidecar.ps1`, which publishes the .NET project in Release config and renames the output with the Rust target triple (required by Tauri's sidecar naming convention).

---

## Database

SQLite file location depends on how you're running the app:

| Scenario | Database file |
|---|---|
| VS / `dotnet watch` (UC1) | `backend/src/JobTracker.Api/jobtracker.db` (relative to working dir) |
| Tauri desktop app (UC3) | `%APPDATA%\com.burntdev.jobtracker\jobtracker.db` |

> **These are separate files.** Data added via the VS API will not appear in the desktop app and vice versa. This is intentional — the desktop app keeps user data isolated in the OS app-data folder.

`DB_PATH` is set automatically by Tauri for the sidecar. When running from VS, no configuration is needed — the API falls back to the relative path.

### WAL mode and concurrent access

The database runs in WAL (Write-Ahead Log) mode. Instead of locking the main DB file on every write, SQLite appends changes to a separate `.wal` file. Readers always read a consistent snapshot of the main file — they are never blocked by an in-progress write. Writers append to the WAL without touching what readers are reading. SQLite periodically checkpoints the WAL back into the main file.

**Practical effect:** the desktop app (via the .NET sidecar) and the MCP server can write to the same SQLite file simultaneously without deadlocking. If two writes collide at the exact same millisecond, SQLite queues the second one and retries for up to 5 seconds (`busy_timeout`) before failing — which under normal single-user load will never happen.

Both the API and MCP connections must set these pragmas on every connection:

```sql
PRAGMA journal_mode=WAL;
PRAGMA busy_timeout=5000;
```

### DB Browser for SQLite

> **Close DB Browser before starting the API or MCP server.** DB Browser holds an exclusive file lock during active use that bypasses WAL — it will prevent the API from acquiring a write lock during migration startup and can block MCP writes.

---

## Regenerate API types

When backend DTOs change, regenerate the TypeScript types (requires API running):

```powershell
# From frontend/ (API must be running on :5063)
npm run gen:api
```

Commits the updated `src/lib/api.types.gen.ts` so CI typechecks pass without needing the backend running.
