$number = $args[0]
$prFlag = $args -contains '--pr' -or $args -contains '-pr'

if (-not $number) {
    Write-Error "Usage: npm run start-issue -- <number> [--pr]"
    exit 1
}

# Fetch issue title
$gh = if ($env:GH_BIN) { $env:GH_BIN }
      elseif (Get-Command gh -ErrorAction SilentlyContinue) { "gh" }
      elseif (Test-Path "C:\Program Files\GitHub CLI\gh.exe") { "C:\Program Files\GitHub CLI\gh.exe" }
      else { Write-Error "gh CLI not found. Set GH_BIN in ~/.claude/settings.json or add gh to PATH."; exit 1 }
$title = & $gh issue view $number --json title --jq '.title' 2>&1
if ($LASTEXITCODE -ne 0) {
    Write-Error "Error: Issue #$number not found. Check the number and try again."
    exit 1
}

# Slugify
$slug = $title.ToLower() -replace '[^a-z0-9\s-]', '' -replace '\s+', '-' -replace '-+', '-'
$slug = $slug.TrimEnd('-')
if ($slug.Length -gt 50) { $slug = $slug.Substring(0, 50).TrimEnd('-') }
$name   = "$number-$slug"
$branch = "feat/$name"
$worktree = ".worktrees\$name"

# Ensure .worktrees/ is gitignored
git check-ignore -q .worktrees 2>$null
if ($LASTEXITCODE -ne 0) {
    Add-Content .gitignore "`n.worktrees/"
    git add .gitignore
    git commit -m "chore: gitignore .worktrees/"
}

# Create worktree + branch (skip if exists)
if (Test-Path $worktree) {
    Write-Host "Worktree $worktree already exists — reopening VSCode."
} else {
    git worktree add $worktree -b $branch
    if ($LASTEXITCODE -ne 0) { exit 1 }
}

# Draft PR
$prUrl = ""
if ($prFlag) {
    git -C $worktree push -u origin $branch
    $prUrl = & $gh pr create --draft --title $title --body "Closes #$number" --head $branch
}

# Report
Write-Host ""
Write-Host "Issue #${number}: `"$title`""
Write-Host "Branch:   $branch"
Write-Host "Worktree: $worktree"
if ($prUrl) { Write-Host "PR:       $prUrl (draft)" }
Write-Host ""
$worktreeFull = (Resolve-Path $worktree).Path
Write-Host "Open VSCode:"
Write-Host "  code --new-window `"$worktreeFull`""
Write-Host ""
Write-Host "Then: open terminal -> run ``claude`` -> /tdd to start."
