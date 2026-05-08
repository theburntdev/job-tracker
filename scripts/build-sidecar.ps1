$repoRoot = (Resolve-Path "$PSScriptRoot\..").Path
$project  = "$repoRoot\backend\src\JobTracker.Api\JobTracker.Api.csproj"
$outDir   = "$repoRoot\frontend\src-tauri\binaries"
$triple   = (rustc -vV | Select-String "host:").ToString().Split(":")[1].Trim()
$dest     = "$outDir\JobTracker.Api-$triple.exe"

Write-Host "Publishing .NET API..."
dotnet publish $project --configuration Release --output "$outDir\_raw"
if ($LASTEXITCODE -ne 0) { exit 1 }

New-Item -ItemType Directory -Force -Path $outDir | Out-Null
Copy-Item "$outDir\_raw\JobTracker.Api.exe" $dest -Force
Remove-Item "$outDir\_raw" -Recurse -Force

Write-Host "Sidecar ready: $dest"
