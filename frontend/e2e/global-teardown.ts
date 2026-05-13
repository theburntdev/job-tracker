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

  // On Windows, dotnet run spawns a child process for the actual app.
  // taskkill /T kills the whole process tree to avoid orphan processes.
  if (os.platform() === 'win32') {
    try { execSync(`taskkill /PID ${pid} /F /T`) } catch { /* already gone */ }
  } else {
    try { process.kill(-pid, 'SIGKILL') } catch { /* already gone */ }
  }

  fs.unlinkSync(PID_FILE)
}
