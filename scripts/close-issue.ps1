$number    = $args[0]
$forceFlag = $args -contains '--force' -or $args -contains '-force'

if (-not $number) {
    Write-Error "Usage: npm run close-issue -- <number> [--force]"
    exit 1
}

# Locate worktree
$match = Get-ChildItem .worktrees -Directory -ErrorAction SilentlyContinue |
    Where-Object { $_.Name -match "^$number-" } |
    Select-Object -First 1

if (-not $match) {
    Write-Error "Error: No worktree found for issue #$number in .worktrees/"
    exit 1
}

$name     = $match.Name
$worktree = ".worktrees\$name"
$branch   = "feat/$name"

# Confirm
Write-Host "About to remove:"
Write-Host "  Worktree: $worktree"
Write-Host "  Branch:   $branch"
Write-Host ""
$confirm = Read-Host "Proceed? (y/n)"
if ($confirm -ne 'y') { Write-Host "Aborted."; exit 0 }

# Kill stale processes
$worktreeFull = (Resolve-Path $worktree).Path
$stalePids = Get-CimInstance Win32_Process |
    Where-Object { $_.CommandLine -like "*$worktreeFull*" -or $_.CommandLine -like "*$name*" } |
    Select-Object -ExpandProperty ProcessId
if ($stalePids) {
    Stop-Process -Id $stalePids -Force -ErrorAction SilentlyContinue
    Write-Host "Stopped $($stalePids.Count) stale process(es)."
}

# Remove worktree
git worktree remove $worktree --force
if ($LASTEXITCODE -ne 0) {
    Remove-Item -Recurse -Force $worktree -ErrorAction SilentlyContinue
    git worktree prune
    if (Test-Path $worktree) {
        Write-Error "Failed to remove $worktree - check for locked files."
        exit 1
    }
}

# Delete branch
$localBranch = git branch --list $branch
if (-not $localBranch) {
    Write-Host "Branch $branch already removed."
} else {
    if ($forceFlag) {
        git branch -D $branch
    } else {
        git branch -d $branch
        if ($LASTEXITCODE -ne 0) {
            Write-Error "Branch not fully merged. Re-run with --force to delete anyway."
            exit 1
        }
    }
}

Write-Host ""
Write-Host "Closed issue #${number}:"
Write-Host "  Worktree $worktree removed"
Write-Host "  Branch $branch deleted"
Write-Host "Done."
