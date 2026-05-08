# Job Application Tracker

A desktop app for tracking job applications — log postings, track application stages, store contacts, and record interview notes.

## Stack

- **Backend**: C# / .NET 10 REST API (Tauri sidecar)
- **Frontend**: React (TypeScript) + Vite
- **Desktop shell**: Tauri
- **Database**: SQLite (local-first)

## Inspecting the database

[DB Browser for SQLite](https://sqlitebrowser.org/) lets you open the `.db` file directly and browse tables, run queries, and inspect data without any extra setup.

> **Note:** Close DB Browser before starting the API. SQLite file-locking will block the app from acquiring a write lock on startup.
