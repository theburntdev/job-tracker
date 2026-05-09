$ErrorActionPreference = "Stop"

$repoRoot   = (Resolve-Path "$PSScriptRoot\..").Path
$project    = "$repoRoot\backend\src\JobTracker.Api\JobTracker.Api.csproj"
$publishDir = "$repoRoot\backend\publish\sidecar"
$binaries   = "$repoRoot\frontend\src-tauri\binaries"
$triple     = (rustc -vV | Select-String "host:").ToString().Split(":")[1].Trim()
$dest       = "$binaries\JobTracker.Api-$triple.exe"

# Map Rust triple to .NET runtime identifier
$rid = switch -Wildcard ($triple) {
    "x86_64-pc-windows-*"    { "win-x64"    }
    "aarch64-pc-windows-*"   { "win-arm64"  }
    "x86_64-apple-darwin"    { "osx-x64"    }
    "aarch64-apple-darwin"   { "osx-arm64"  }
    "x86_64-unknown-linux-*" { "linux-x64"  }
    "aarch64-unknown-linux-*"{ "linux-arm64"}
    default { throw "Unsupported Rust triple: $triple" }
}

Write-Host "Publishing .NET API (self-contained single-file, $rid)..."
dotnet publish $project `
    --configuration Release `
    --runtime $rid `
    --self-contained true `
    -p:PublishSingleFile=true `
    -p:DebugType=embedded `
    -p:IncludeNativeLibrariesForSelfExtract=true `
    --output $publishDir

if ($LASTEXITCODE -ne 0) { exit 1 }

New-Item -ItemType Directory -Force -Path $binaries | Out-Null

# binaries/ is the source of truth referenced by tauri.conf.json externalBin
Copy-Item "$publishDir\JobTracker.Api.exe"             $dest                                        -Force
Copy-Item "$publishDir\appsettings.json"               "$binaries\appsettings.json"                 -Force
Copy-Item "$publishDir\appsettings.Development.json"   "$binaries\appsettings.Development.json"     -Force

Write-Host "Sidecar binary: $dest"

# Sync to target/debug/ so cargo tauri dev picks it up immediately without waiting
# for tauri-build to detect the change on next incremental build
$debugDir = "$repoRoot\frontend\src-tauri\target\debug"
if (Test-Path $debugDir) {
    Copy-Item "$publishDir\JobTracker.Api.exe"             "$debugDir\JobTracker.Api.exe"               -Force
    Copy-Item "$publishDir\appsettings.json"               "$debugDir\appsettings.json"                 -Force
    Copy-Item "$publishDir\appsettings.Development.json"   "$debugDir\appsettings.Development.json"     -Force
    Write-Host "Dev cache synced: $debugDir"
}

Write-Host "Done."
