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

Tauri registers `JobTracker.Api.exe` as an external binary. On app start, Tauri spawns it with `ASPNETCORE_URLS=http://localhost:5063`. On app exit, Tauri kills it. React talks to it via plain HTTP — no IPC, no changes to the React code needed.

The binary must be built and placed before running `cargo tauri dev` or `cargo tauri build`:

```powershell
# From frontend/
npm run build:sidecar
```

This runs `scripts/build-sidecar.ps1`, which publishes the .NET project in Release config and renames the output with the Rust target triple (required by Tauri's sidecar naming convention).

---

## Database

SQLite file lives at the app's data directory (resolved at runtime by the API). To inspect it with DB Browser for SQLite:

> **Close DB Browser before starting the API.** SQLite's file lock will prevent the API from acquiring a write lock during migration startup.

---

## Regenerate API types

When backend DTOs change, regenerate the TypeScript types (requires API running):

```powershell
# From frontend/ (API must be running on :5063)
npm run gen:api
```

Commits the updated `src/lib/api.types.gen.ts` so CI typechecks pass without needing the backend running.
