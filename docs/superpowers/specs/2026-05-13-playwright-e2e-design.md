# Playwright E2E Test Suite Design

**Date:** 2026-05-13
**Status:** Approved

## Goal

Add end-to-end tests that give confidence in user flows — create, read, update, delete, and paginate job applications. Vitest/RTL continues to own unit and component tests. Playwright owns full-stack user flow tests.

## Scope

- Target: Vite dev server (`localhost:5173`) — not the Tauri desktop shell
- API: Real .NET backend (no mocks)
- Environment: Local development initially; design does not block future CI addition
- Test data isolation: dedicated `test.db`, wiped before each suite run

## Architecture

Playwright sits alongside the existing Vitest setup in `frontend/`. One command starts everything.

```
frontend/
  e2e/
    global-setup.ts              # wipes test.db, spawns .NET API, waits for /health
    global-teardown.ts           # kills .NET API child process
    job-applications.spec.ts     # first test file
  playwright.config.ts           # alongside vite.config.ts
  .env.test                      # VITE_API_URL=http://localhost:5001 (gitignored)
```

Backend additions:

```
backend/src/JobTracker.Api/
  Properties/launchSettings.json   # add Testing launch profile
  appsettings.Testing.json         # environment marker (no secrets)
  Program.cs                       # add GET /health endpoint
```

## Configuration

### `playwright.config.ts`

```typescript
import { defineConfig, devices } from '@playwright/test'

export default defineConfig({
  testDir: './e2e',
  globalSetup: './e2e/global-setup.ts',
  globalTeardown: './e2e/global-teardown.ts',
  use: {
    baseURL: 'http://localhost:5173',
  },
  projects: [{ name: 'chromium', use: { ...devices['Desktop Chrome'] } }],
  webServer: {
    command: 'npm run dev -- --port 5173',
    url: 'http://localhost:5173',
    reuseExistingServer: false,
  },
})
```

> **Why not `webServer` for the .NET API?** Playwright starts `webServer` entries *before* `globalSetup` runs. If the API were in `webServer`, EF migrations would create `test.db` before `globalSetup` could wipe it — and SQLite on Windows holds a file lock. Managing the API in `globalSetup`/`globalTeardown` instead gives us the correct order: wipe → start → migrate → tests → stop.

### Backend — Testing launch profile (`launchSettings.json`)

```json
"Testing": {
  "commandName": "Project",
  "applicationUrl": "http://localhost:5001",
  "environmentVariables": {
    "ASPNETCORE_ENVIRONMENT": "Testing",
    "ConnectionStrings__Default": "Data Source=test.db"
  }
}
```

> **Port note:** The dev profile uses `5063`. Testing uses `5001` intentionally — the two can run simultaneously without a port conflict. `.env.test` sets `VITE_API_URL=http://localhost:5001` to match.

### `appsettings.Testing.json`

Empty file — presence signals the `Testing` environment to ASP.NET Core. Connection string comes from the launch profile env var.

### `global-setup.ts`

Runs before `webServer` (Vite) starts and before any tests. Responsibilities:

1. Delete `test.db` if it exists (no lock — API not started yet)
2. Spawn `dotnet run --project ../backend/src/JobTracker.Api --launch-profile Testing` as a child process
3. Poll `http://localhost:5001/health` until `200 OK` (EF migrations run on API startup, creating a fresh DB)
4. Store the child process handle in a temp file or global so `globalTeardown` can kill it

### `global-teardown.ts`

Kills the .NET API child process started in `global-setup.ts`.

### `.env.test`

```
VITE_API_URL=http://localhost:5001
```

Gitignored. Playwright loads it via `envFile` option in config.

### Health endpoint (`Program.cs`)

```csharp
app.MapGet("/health", () => Results.Ok());
```

Required for `webServer` readiness check. Returns `200 OK`, no body.

## Test Structure

### Selector strategy

Priority order (mirrors RTL conventions already in use):
1. `getByRole` — semantic and accessible (preferred)
2. `getByLabel` — for form inputs
3. `getByText` — visible text
4. `getByTestId` — last resort; add `data-testid` to organisms where role/label are ambiguous

### Test isolation

Global setup wipes `test.db`. Each test seeds its own minimum data via the `request` fixture:

```typescript
test.beforeEach(async ({ request }) => {
  // Use full URL — baseURL points to Vite (5173), API is on 5001
  await request.post('http://localhost:5001/api/job-applications', {
    data: { title: 'Dev', company: 'Acme', stage: 'Applied' }
  })
})
```

No shared mutable state between tests.

### First suite — `job-applications.spec.ts`

| Test | Flow |
|------|------|
| displays empty state when no applications exist | fresh DB, load page, assert empty state |
| creates a new application and shows it in the table | open modal, fill form, submit, assert row appears |
| edits an application's stage via the dropdown | seed one app, change stage dropdown, assert updated |
| opens detail modal and updates description | seed one app, click row, edit description, save |
| deletes an application | seed one app, open detail modal, delete, assert gone |
| paginates when more than one page of results | seed >pageSize apps, navigate to page 2 |

## Running Tests

```powershell
# From frontend/
npx playwright test

# UI mode (headed, interactive)
npx playwright test --ui

# Single file
npx playwright test e2e/job-applications.spec.ts
```

## Playwright MCP (Future)

After the test suite is working, install the `@playwright/mcp` server in Claude Code MCP settings. This gives Claude browser control to inspect the running app and help write new tests interactively. It has no effect on the test suite itself — install it separately after the suite is confirmed working.

## What Is Not In Scope

- Tauri shell / desktop window testing
- CI pipeline setup (deferred)
- Authentication flows (deferred — auth not yet implemented)
- Multiple browsers beyond Chromium (add later if needed)
