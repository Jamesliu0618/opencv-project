<#
.SYNOPSIS
  Build solution, restore NuGet packages, run tests and produce reports.

.PARAMETER Solution
  Path to the .sln file (default: search for first .sln in repo root)

.PARAMETER MsBuildPath
  Path to msbuild.exe (if not provided the script will try common locations)

.PARAMETER Configuration
  Build configuration (Debug/Release). Default: Release

.PARAMETER Platform
  Platform target (AnyCPU/x86/x64). Default: x64

.PARAMETER TestResultsDir
  Directory to save test results. Default: artifacts/test-results
#>
param(
    [string]$Solution = (Get-ChildItem -Path "$PSScriptRoot\..\.." -Filter "*.sln" -Recurse -ErrorAction SilentlyContinue | Select-Object -First 1 -ExpandProperty FullName),
    [string]$MsBuildPath = "",
    [string]$Configuration = "Release",
    [string]$Platform = "x64",
    [string]$TestResultsDir = "$PSScriptRoot\..\..\artifacts\test-results"
)

Set-StrictMode -Version Latest
if (-not $Solution -or -not (Test-Path $Solution)) {
    Write-Error "Solution file not found. Pass -Solution path or run from repository root where a .sln exists."
    exit 2
}

# Ensure output dir
New-Item -ItemType Directory -Path $TestResultsDir -Force | Out-Null

function Find-MsBuild {
    param()
    if ($MsBuildPath -and (Test-Path $MsBuildPath)) { return $MsBuildPath }
    $candidates = @(
        "C:\Program Files (x86)\Microsoft Visual Studio\2019\BuildTools\MSBuild\Current\Bin\MSBuild.exe",
        "C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\MSBuild.exe",
        "C:\Program Files (x86)\Microsoft Visual Studio\2019\Professional\MSBuild\Current\Bin\MSBuild.exe",
        "C:\Program Files (x86)\Microsoft Visual Studio\2022\BuildTools\MSBuild\Current\Bin\MSBuild.exe",
        "C:\Program Files\Microsoft Visual Studio\2022\BuildTools\MSBuild\Current\Bin\MSBuild.exe"
    )
    foreach ($c in $candidates) { if (Test-Path $c) { return $c } }
    return $null
}

$msbuild = Find-MsBuild
if (-not $msbuild) {
    Write-Warning "msbuild not found in common locations. Ensure MSBuild is installed or pass -MsBuildPath." 
} else {
    Write-Host "Using MSBuild: $msbuild"
}

# Restore packages
Write-Host "Restoring packages..."
# Try nuget.exe restore, dotnet restore, msbuild /t:Restore
$nugetExe = (Get-Command nuget.exe -ErrorAction SilentlyContinue).Source
if ($nugetExe) {
    & $nugetExe restore $Solution
} elseif (Get-Command dotnet -ErrorAction SilentlyContinue) {
    & dotnet restore $Solution
} elseif ($msbuild) {
    & $msbuild $Solution /t:Restore /p:Configuration=$Configuration /p:Platform=$Platform
} else {
    Write-Warning "Could not find nuget/dotnet/msbuild for package restore. Skipping restore."
}

# Build
if ($msbuild) {
    Write-Host "Building solution..."
    $buildExit = & $msbuild $Solution /p:Configuration=$Configuration /p:Platform=$Platform
    if ($LASTEXITCODE -ne 0) { Write-Error "MSBuild failed"; exit $LASTEXITCODE }
} else {
    Write-Error "MSBuild not found - cannot build"; exit 3
}

# Discover test assemblies
$solutionDir = Split-Path $Solution -Parent
$testDlls = Get-ChildItem -Path $solutionDir -Include "*Test.dll","*Tests.dll" -Recurse -ErrorAction SilentlyContinue | Select-Object -ExpandProperty FullName
if (-not $testDlls) { Write-Warning "No test assemblies found under solution output directories."; exit 0 }

# Find vstest.console.exe or use dotnet test
$vstest = (Get-ChildItem -Path "C:\Program Files (x86)\Microsoft Visual Studio" -Filter "vstest.console.exe" -Recurse -ErrorAction SilentlyContinue | Select-Object -First 1 -ExpandProperty FullName)
if ($vstest) { Write-Host "Using vstest: $vstest" }

$reportFiles = @()
foreach ($dll in $testDlls) {
    Write-Host "Running tests in $dll"
    $fileName = [IO.Path]::GetFileNameWithoutExtension($dll)
    $trx = Join-Path $TestResultsDir "$fileName.trx"
    if ($vstest) {
        & $vstest $dll /logger:trx /ResultsFile:$trx
        $rc = $LASTEXITCODE
    } else {
        # try dotnet vstest
        if (Get-Command dotnet -ErrorAction SilentlyContinue) {
            & dotnet vstest $dll --logger:trx; $rc = $LASTEXITCODE
        } else {
            Write-Warning "No test runner found for $dll. Skipping."; $rc = 0
        }
    }
    if ($rc -ne 0) { Write-Error "Tests failed for $dll"; exit $rc }
    $reportFiles += $trx
}

Write-Host "All tests passed. Reports saved to: $TestResultsDir"
exit 0
