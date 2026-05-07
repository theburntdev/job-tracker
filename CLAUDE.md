# Job Application Tracker — Project Root

## What this is
A desktop job-application tracker. Users log job postings, track application status, store contacts, and record interview notes. Built as a learning project for Claude Code.

## Stack
- **Backend**: C# / .NET 10 — REST API (Tauri sidecar — bundled with app, not a separate deployment). Lives in `backend/`.
- **Frontend**: React (TypeScript) + Vite. Lives in `frontend/`.
- **Desktop shell**: Tauri — wraps the React/Vite frontend. Lives in `frontend/src-tauri/`. .NET MAUI ruled out (would replace React).
- **Database**: SQLite (local-first; swap to Postgres via connection string when multi-user needed).

## Repo layout
```
job-tracker/
  backend/          # C# solution — see backend/CLAUDE.md
  frontend/         # React app + Tauri shell — see frontend/CLAUDE.md
    src/            # React source
    src-tauri/      # Tauri config, Rust bridge (tauri.conf.json, Cargo.toml)
  CLAUDE.md         # ← you are here (root context)
```

## How CLAUDE.md files layer in this project
Claude Code reads CLAUDE.md files from the repo root down into subdirectories.
- Root CLAUDE.md (this file): project-wide decisions and vocabulary.
- `backend/CLAUDE.md`: C#/.NET-specific conventions and commands.
- `frontend/CLAUDE.md`: React-specific conventions and commands.
When you're working in a subdirectory, Claude has all three in context automatically.

## Cross-cutting decisions
- All dates stored and transmitted as UTC ISO-8601.
- No secrets in source — use `.env` files (gitignored) or user-secrets.
- Prefer explicit types over `var`/`any` unless the type is immediately obvious from the right-hand side.
- Write no comments unless the WHY is non-obvious to a future reader.
- SQLite WAL mode required — API sidecar and future MCP server share the same database file.

## Vocabulary (use these terms consistently in prompts)
- **Job-Application**: a submitted job application the user is tracking.
- **Stage**: current status of an Application (e.g. Applied, Screened, Interviewing, Offer, Rejected).
- **Contact**: a person associated with a Job or Application.
- **Note**: a free-text record attached to an Application or Contact.

## Future milestones
- **MCP server** (`JobTracker.Mcp`): a .NET MCP server exposing job/application data to Claude Desktop via stdio. Reuses `Infrastructure` layer against the same SQLite file.
