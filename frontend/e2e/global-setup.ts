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
  // 1. Wipe test DB — API not running yet, no file lock
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
