# Playwright E2E Test Suite Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add a Playwright E2E test suite covering the full job-application CRUD user flows against a real .NET backend and live Vite dev server.

**Architecture:** `@playwright/test` lives in `frontend/` alongside the existing Vitest setup. Playwright manages the Vite dev server via `webServer`. A `globalSetup` script wipes `test.db`, spawns the .NET API with a `Testing` launch profile, and waits for its `/health` endpoint before any tests run. A `globalTeardown` script kills the API process. Per-test isolation uses a `DELETE /api/test/reset` endpoint (Testing-env only) called in `beforeEach`.

**Tech stack:** `@playwright/test`, `@playwright/browser-chromium`, .NET ASP.NET Core Testing environment, EF Core `ExecuteDeleteAsync` for DB reset, Vite `--mode e2e`.

> **Spec note:** The spec named the env file `.env.test`. This plan uses `.env.e2e` with `--mode e2e` to avoid conflicting with Vitest, which reads `.env.test` by default.

---

## File Map

| Status | File | Change |
|--------|------|--------|
| Create | `frontend/playwright.config.ts` | Playwright config — webServer (Vite), globalSetup/Teardown |
| Create | `frontend/.env.e2e` | `VITE_API_URL=http://localhost:5001` (gitignored) |
| Create | `frontend/e2e/global-setup.ts` | Wipes test.db, spawns .NET API, polls /health |
| Create | `frontend/e2e/global-teardown.ts` | Kills .NET API process via PID file |
| Create | `frontend/e2e/job-applications.spec.ts` | 6 E2E tests for CRUD flows |
| Modify | `frontend/package.json` | Add `@playwright/test` devDep + `test:e2e` scripts |
| Modify | `.gitignore` | Add `.env.e2e`, `frontend/e2e/.api-pid`, `playwright-report/`, `test-results/` |
| Modify | `backend/src/JobTracker.Api/Properties/launchSettings.json` | Add `Testing` launch profile (port 5001, `test.db`) |
| Create | `backend/src/JobTracker.Api/appsettings.Testing.json` | Environment marker |
| Modify | `backend/src/JobTracker.Api/Program.cs` | Add `/health`, fix CORS for Testing env, add `/api/test/reset` |
| Modify | `frontend/src/components/organisms/JobApplicationDetailModal.tsx` | Wire `htmlFor`/`id` on description label+textarea |

---

## Task 1: Backend — Testing environment

**Files:**
- Modify: `backend/src/JobTracker.Api/Properties/launchSettings.json`
- Create: `backend/src/JobTracker.Api/appsettings.Testing.json`
- Modify: `backend/src/JobTracker.Api/Program.cs`

- [ ] **Step 1: Add the Testing launch profile**

Open `backend/src/JobTracker.Api/Properties/launchSettings.json`. Add the `Testing` profile inside `"profiles"`:

```json
{
  "$schema": "https://json.schemastore.org/launchsettings.json",
  "profiles": {
    "http": {
      "commandName": "Project",
      "dotnetRunMessages": true,
      "launchBrowser": false,
      "applicationUrl": "http://localhost:5063",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      }
    },
    "https": {
      "commandName": "Project",
      "dotnetRunMessages": true,
      "launchBrowser": false,
      "applicationUrl": "https://localhost:7125;http://localhost:5063",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      }
    },
    "Testing": {
      "commandName": "Project",
      "dotnetRunMessages": true,
      "launchBrowser": false,
      "applicationUrl": "http://localhost:5001",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Testing",
        "ConnectionStrings__Default": "Data Source=test.db"
      }
    }
  }
}
```

> Port 5001 is intentionally different from dev port 5063 so both can run simultaneously.

- [ ] **Step 2: Create the Testing environment marker file**

Create `backend/src/JobTracker.Api/appsettings.Testing.json` with this content:

```json
{}
```

ASP.NET Core's env-loading merges this with `appsettings.json`. The file's presence signals the `Testing` environment to the framework; the connection string comes from the launch profile env var.

- [ ] **Step 3: Update `Program.cs` — add /health, fix CORS, add /api/test/reset**

Replace the relevant sections of `backend/src/JobTracker.Api/Program.cs`. The full updated file:

```csharp
using System.Text.Json.Serialization;
using FluentValidation;
using JobTracker.Api;
using JobTracker.Api.Endpoints;
using JobTracker.Application.Common;
using JobTracker.Application.JobApplications.GetJobApplications;
using JobTracker.Infrastructure;
using JobTracker.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((ctx, services, cfg) =>
        cfg.ReadFrom.Configuration(ctx.Configuration)
           .ReadFrom.Services(services));

    builder.Services.AddOpenApi();
    builder.Services.AddValidatorsFromAssembly(typeof(GetJobApplicationsQueryHandler).Assembly);

    builder.Services.AddMediatR(cfg =>
    {
        cfg.RegisterServicesFromAssemblies(
            typeof(Program).Assembly,
            typeof(GetJobApplicationsQueryHandler).Assembly);
        cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
    });

    builder.Services.AddExceptionHandler<ValidationExceptionHandler>();
    builder.Services.AddProblemDetails();

    builder.Services.ConfigureHttpJsonOptions(options =>
        options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));

    var dbPath = Environment.GetEnvironmentVariable("DB_PATH");
    if (!string.IsNullOrEmpty(dbPath))
        builder.Configuration["ConnectionStrings:Default"] = $"Data Source={dbPath}";

    builder.Services.AddInfrastructure(builder.Configuration);

    builder.Services.AddCors(options =>
    {
        options.AddPolicy("Dev", policy =>
            policy.WithOrigins("http://localhost:5173")
                  .AllowAnyHeader()
                  .AllowAnyMethod());

        options.AddPolicy("Tauri", policy =>
            policy.WithOrigins("https://tauri.localhost", "http://tauri.localhost", "tauri://localhost")
                  .AllowAnyHeader()
                  .AllowAnyMethod());
    });

    var app = builder.Build();

    await using (var scope = app.Services.CreateAsyncScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.MigrateAsync();
    }

    app.UseExceptionHandler();

    // Testing env also needs the Dev CORS policy (Vite on 5173)
    app.UseCors(
        app.Environment.IsDevelopment() || app.Environment.IsEnvironment("Testing")
            ? "Dev"
            : "Tauri");

    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
    }

    app.MapGet("/health", () => Results.Ok());

    app.MapJobApplicationEndpoints();

    // Test-only endpoint — wipes all job applications for per-test isolation
    if (app.Environment.IsEnvironment("Testing"))
    {
        app.MapDelete("/api/test/reset", async (AppDbContext db) =>
        {
            await db.JobApplications.ExecuteDeleteAsync();
            return Results.NoContent();
        });
    }

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
```

> Two changes from the original:
> - `UseCors` now also applies the `"Dev"` policy for `Testing` env — without this, the browser gets CORS errors because the Testing profile is not `IsDevelopment()`.
> - `/api/test/reset` is a `DELETE` endpoint registered only in Testing. It calls EF Core's `ExecuteDeleteAsync` to truncate `JobApplications` in one SQL statement. Each Playwright `beforeEach` will call this to guarantee a clean DB before the test.

- [ ] **Step 4: Check the AppDbContext namespace**

Run:
```powershell
Select-String -Path "backend\src\JobTracker.Infrastructure\AppDbContext.cs" -Pattern "^namespace"
```

Confirm the namespace matches the `using` added in the previous step. If it differs, update the `using JobTracker.Infrastructure.Persistence;` line in `Program.cs` to match.

- [ ] **Step 5: Build and verify the Testing profile starts cleanly**

```powershell
dotnet build backend/JobTracker.sln
dotnet run --project backend/src/JobTracker.Api --launch-profile Testing
```

Expected output: API starts on `http://localhost:5001`. Navigate to `http://localhost:5001/health` in a browser — should return `200 OK`. Stop it with Ctrl+C.

- [ ] **Step 6: Commit**

```powershell
git add backend/src/JobTracker.Api/Properties/launchSettings.json
git add backend/src/JobTracker.Api/appsettings.Testing.json
git add backend/src/JobTracker.Api/Program.cs
git commit -m "feat(api): add Testing env with health endpoint and test reset"
```

---

## Task 2: Frontend — Install Playwright and configure

**Files:**
- Modify: `frontend/package.json`
- Create: `frontend/playwright.config.ts`
- Create: `frontend/.env.e2e`
- Modify: `.gitignore` (repo root)

- [ ] **Step 1: Install Playwright**

```powershell
cd frontend
npm install --save-dev @playwright/test
npx playwright install chromium
cd ..
```

Expected: `@playwright/test` appears in `devDependencies`. Chromium downloads.

- [ ] **Step 2: Add scripts to `package.json`**

In `frontend/package.json`, add two entries to `"scripts"`:

```json
"test:e2e": "playwright test",
"test:e2e:ui": "playwright test --ui"
```

Full scripts block after edit:

```json
"scripts": {
  "dev": "vite",
  "build": "tsc -b && vite build",
  "preview": "vite preview",
  "typecheck": "tsc --noEmit",
  "test": "vitest",
  "test:run": "vitest run",
  "test:e2e": "playwright test",
  "test:e2e:ui": "playwright test --ui",
  "gen:api": "openapi-typescript http://localhost:5063/openapi/v1.json -o ./src/lib/api.types.gen.ts",
  "build:sidecar": "powershell -ExecutionPolicy Bypass -File ../scripts/build-sidecar.ps1"
}
```

- [ ] **Step 3: Create `frontend/playwright.config.ts`**

```typescript
import { defineConfig, devices } from '@playwright/test'

export default defineConfig({
  testDir: './e2e',
  globalSetup: './e2e/global-setup.ts',
  globalTeardown: './e2e/global-teardown.ts',
  use: {
    baseURL: 'http://localhost:5173',
    trace: 'on-first-retry',
  },
  projects: [
    { name: 'chromium', use: { ...devices['Desktop Chrome'] } },
  ],
  webServer: {
    command: 'npm run dev -- --port 5173 --mode e2e',
    url: 'http://localhost:5173',
    reuseExistingServer: false,
  },
})
```

> `--mode e2e` tells Vite to load `.env.e2e` (not `.env.test`, which Vitest uses). This sets `VITE_API_URL=http://localhost:5001` for the browser.

- [ ] **Step 4: Create `frontend/.env.e2e`**

```
VITE_API_URL=http://localhost:5001
```

- [ ] **Step 5: Update root `.gitignore`**

Add these lines to `.gitignore` in the repo root:

```
# Playwright
frontend/.env.e2e
frontend/e2e/.api-pid
frontend/playwright-report/
frontend/test-results/
```

- [ ] **Step 6: Commit**

```powershell
git add frontend/package.json frontend/playwright.config.ts
git add .gitignore
git commit -m "feat(e2e): install Playwright and add config"
```

> Do NOT commit `frontend/.env.e2e` — it's gitignored.

---

## Task 3: E2E infrastructure — global setup and teardown

**Files:**
- Create: `frontend/e2e/global-setup.ts`
- Create: `frontend/e2e/global-teardown.ts`

- [ ] **Step 1: Create the `e2e/` directory and `global-setup.ts`**

Create `frontend/e2e/global-setup.ts`:

```typescript
import { spawn } from 'child_process'
import fs from 'fs'
import path from 'path'
import { fileURLToPath } from 'url'

const __dirname = path.dirname(fileURLToPath(import.meta.url))
const REPO_ROOT = path.resolve(__dirname, '../..')
const TEST_DB = path.resolve(REPO_ROOT, 'test.db')
const PID_FILE = path.resolve(__dirname, '.api-pid')
const API_HEALTH = 'http://localhost:5001/health'
const TIMEOUT_MS = 30_000

export default async function globalSetup() {
  // 1. Wipe test DB — API is not running yet so no file lock
  if (fs.existsSync(TEST_DB)) fs.unlinkSync(TEST_DB)

  // 2. Spawn .NET API with Testing profile from repo root
  const api = spawn(
    'dotnet',
    ['run', '--project', 'backend/src/JobTracker.Api', '--launch-profile', 'Testing'],
    { cwd: REPO_ROOT, stdio: 'pipe' }
  )
  fs.writeFileSync(PID_FILE, String(api.pid))

  // 3. Poll /health until the API is ready (EF migrations run on startup)
  const deadline = Date.now() + TIMEOUT_MS
  while (Date.now() < deadline) {
    try {
      const res = await fetch(API_HEALTH)
      if (res.ok) return
    } catch {
      // not ready yet
    }
    await new Promise(r => setTimeout(r, 500))
  }

  throw new Error(`Backend API did not start within ${TIMEOUT_MS / 1000}s`)
}
```

- [ ] **Step 2: Create `global-teardown.ts`**

Create `frontend/e2e/global-teardown.ts`:

```typescript
import { execSync } from 'child_process'
import fs from 'fs'
import os from 'os'
import path from 'path'
import { fileURLToPath } from 'url'

const __dirname = path.dirname(fileURLToPath(import.meta.url))
const PID_FILE = path.resolve(__dirname, '.api-pid')

export default function globalTeardown() {
  if (!fs.existsSync(PID_FILE)) return

  const pid = parseInt(fs.readFileSync(PID_FILE, 'utf8'), 10)

  // On Windows, `dotnet run` spawns a child process for the actual app.
  // taskkill /T kills the whole process tree to avoid orphan processes.
  if (os.platform() === 'win32') {
    try { execSync(`taskkill /PID ${pid} /F /T`) } catch { /* already gone */ }
  } else {
    try { process.kill(-pid, 'SIGKILL') } catch { /* already gone */ }
  }

  fs.unlinkSync(PID_FILE)
}
```

- [ ] **Step 3: Smoke-test the infrastructure**

Create a temporary smoke test `frontend/e2e/smoke.spec.ts`:

```typescript
import { test, expect } from '@playwright/test'

test('API is reachable', async ({ request }) => {
  const res = await request.get('http://localhost:5001/health')
  expect(res.ok()).toBeTruthy()
})

test('Vite app loads', async ({ page }) => {
  await page.goto('/')
  await expect(page).toHaveTitle(/job/i)
})
```

Run from `frontend/`:
```powershell
npx playwright test e2e/smoke.spec.ts
```

Expected: both tests pass. If the API test fails, check that `dotnet run --launch-profile Testing` starts and `/health` returns 200. If the Vite test fails, check the page title selector.

- [ ] **Step 4: Delete the smoke test**

```powershell
Remove-Item frontend/e2e/smoke.spec.ts
```

- [ ] **Step 5: Commit**

```powershell
git add frontend/e2e/global-setup.ts frontend/e2e/global-teardown.ts
git commit -m "feat(e2e): add global setup and teardown for API lifecycle"
```

---

## Task 4: Fix detail modal description label association

The `JobApplicationDetailModal` description textarea has no `id`, and its `<label>` has no `htmlFor`. This means `getByLabel(/description/i)` won't find the textarea. Fix it before writing tests so the tests can use accessible queries.

**Files:**
- Modify: `frontend/src/components/organisms/JobApplicationDetailModal.tsx:213-215`

- [ ] **Step 1: Add `id` and `htmlFor` to the description label/textarea in the detail modal**

In `frontend/src/components/organisms/JobApplicationDetailModal.tsx`, find the description section (around line 213):

```tsx
            <div className="flex items-center justify-between">
              <label className={labelClass}>Description</label>
```

Change to:

```tsx
            <div className="flex items-center justify-between">
              <label className={labelClass} htmlFor="detail-description">Description</label>
```

Then find the textarea (around line 258):

```tsx
            <textarea
              className="min-h-0 flex-1 w-full resize-none rounded border border-border bg-surface-elevated px-3 py-2 text-sm text-text-primary placeholder:text-text-muted focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-brand-primary"
              value={description}
              onChange={(e) => setDescription(e.target.value)}
              placeholder="No description"
            />
```

Change to:

```tsx
            <textarea
              id="detail-description"
              className="min-h-0 flex-1 w-full resize-none rounded border border-border bg-surface-elevated px-3 py-2 text-sm text-text-primary placeholder:text-text-muted focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-brand-primary"
              value={description}
              onChange={(e) => setDescription(e.target.value)}
              placeholder="No description"
            />
```

- [ ] **Step 2: Run existing component tests to verify nothing broke**

```powershell
cd frontend
npm run test:run -- --reporter=verbose src/components/organisms/JobApplicationDetailModal.test.tsx
```

Expected: all tests pass.

- [ ] **Step 3: Commit**

```powershell
git add frontend/src/components/organisms/JobApplicationDetailModal.tsx
git commit -m "fix(detail-modal): associate description label with textarea for accessibility"
```

---

## Task 5: Write E2E tests

**Files:**
- Create: `frontend/e2e/job-applications.spec.ts`

- [ ] **Step 1: Create `frontend/e2e/job-applications.spec.ts`**

```typescript
import { test, expect } from '@playwright/test'

const API = 'http://localhost:5001'

async function seedApp(
  request: Parameters<Parameters<typeof test>[1]>[0]['request'],
  overrides: Record<string, unknown> = {}
) {
  const res = await request.post(`${API}/api/job-applications`, {
    data: {
      title: 'Software Engineer',
      company: 'Acme Corp',
      stage: 'Applied',
      ...overrides,
    },
  })
  return res.json() as Promise<{ id: string; company: string; title: string }>
}

test.beforeEach(async ({ request }) => {
  await request.delete(`${API}/api/test/reset`)
})

test('displays empty state when no applications exist', async ({ page }) => {
  await page.goto('/')
  await expect(
    page.getByText('No applications yet. Add one to get started.')
  ).toBeVisible()
})

test('creates a new application and shows it in the table', async ({ page }) => {
  await page.goto('/')
  await page.getByRole('button', { name: 'Add Application' }).click()

  const modal = page.getByRole('dialog', { name: 'Add Application' })
  await modal.getByLabel(/company/i).fill('Globex')
  await modal.getByLabel(/role/i).fill('Backend Engineer')
  await modal.getByRole('button', { name: 'Add' }).click()

  await expect(page.getByText('Globex')).toBeVisible()
  await expect(page.getByText('Backend Engineer')).toBeVisible()
})

test('edits an application stage via the table dropdown', async ({ page, request }) => {
  await seedApp(request)
  await page.goto('/')

  // The StageDropdown trigger shows the current stage label
  await page.getByRole('button', { name: 'Applied' }).first().click()
  await page.getByRole('button', { name: 'Screening' }).click()

  // Wait for the optimistic/refetch cycle to settle
  await expect(page.getByRole('button', { name: 'Screening' })).toBeVisible()
})

test('opens detail modal and saves updated description', async ({ page, request }) => {
  await seedApp(request, { company: 'Initech' })
  await page.goto('/')

  await page.getByRole('button', { name: /Open details for Initech/i }).click()

  const modal = page.getByRole('dialog')
  await modal.getByLabel(/description/i).fill('Interesting role')
  await modal.getByRole('button', { name: 'Save' }).click()

  // Modal closes on successful save
  await expect(modal).not.toBeVisible()
})

test('deletes an application', async ({ page, request }) => {
  await seedApp(request, { company: 'Umbrella' })
  await page.goto('/')

  await page.getByRole('button', { name: /Open details for Umbrella/i }).click()

  const modal = page.getByRole('dialog')
  await modal.getByRole('button', { name: 'Delete' }).click()
  await modal.getByRole('button', { name: 'Confirm' }).click()

  await expect(
    page.getByText('No applications yet. Add one to get started.')
  ).toBeVisible()
})

test('paginates when more than one page of results exists', async ({ page, request }) => {
  // Page size is 20 — seed 21 to trigger page 2
  await Promise.all(
    Array.from({ length: 21 }, (_, i) =>
      seedApp(request, { company: `Company ${i + 1}`, title: `Engineer ${i + 1}` })
    )
  )

  await page.goto('/')
  await expect(page.getByText('Page 1 of 2')).toBeVisible()

  await page.getByRole('button', { name: 'Next' }).click()
  await expect(page.getByText('Page 2 of 2')).toBeVisible()
})
```

- [ ] **Step 2: Run the tests**

```powershell
cd frontend
npx playwright test
```

Expected: 6 tests pass. If any fail, run in headed mode to debug:

```powershell
npx playwright test --headed
```

Or open the interactive UI:

```powershell
npx playwright test --ui
```

The HTML report is at `playwright-report/index.html` if any test fails.

- [ ] **Step 3: Commit**

```powershell
git add frontend/e2e/job-applications.spec.ts
git commit -m "test(e2e): add Playwright tests for job application CRUD flows"
```

---

## Self-Review Notes

- **Spec gap addressed in plan:** The spec described `beforeEach` seeding but did not address per-test cleanup. This plan adds `DELETE /api/test/reset` to the backend and calls it in every `beforeEach`, ensuring tests don't see data from prior tests.
- **Spec deviation:** Env file is `.env.e2e` (not `.env.test` as specced) to avoid polluting Vitest's test mode env. `playwright.config.ts` passes `--mode e2e` to Vite accordingly.
- **CORS gap (not in spec):** Testing env falls through to the Tauri CORS policy in the original `Program.cs`. Fixed in Task 1.
- **Accessibility gap (not in spec):** `JobApplicationDetailModal` description textarea has no `id`/`htmlFor`. Fixed in Task 4.
