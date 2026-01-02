<#
.SYNOPSIS
  Deploy build artifacts to a Windows IPC and run health checks.

.PARAMETER SourcePath
  Path to build artifacts (default: ../src/bin/Release)

.PARAMETER TargetPath
  Target path on remote IPC (e.g., C$\Apps\PCBInspector)

.PARAMETER RemoteComputer
  Remote computer name or IP (if empty, deploys locally)

.PARAMETER InstallerDir
  Directory that contains installers for runtime and drivers (optional)

.PARAMETER UsePSSession
  Use PowerShell Remoting / PSSession to execute remote tasks
#>
param(
    [string]$SourcePath = (Resolve-Path "$PSScriptRoot\..\..\src\bin\Release" -ErrorAction SilentlyContinue),
    [Parameter(Mandatory=$true)][string]$TargetPath,
    [string]$RemoteComputer = "",
    [string]$InstallerDir = "",
    [switch]$UsePSSession
)

Set-StrictMode -Version Latest
if (-not (Test-Path $SourcePath)) { Write-Error "Source path not found: $SourcePath"; exit 2 }

function Check-NetFramework48 {
    $regPath = 'HKLM:\SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full'
    $release = (Get-ItemProperty -Path $regPath -Name Release -ErrorAction SilentlyContinue).Release
    if (-not $release) { return $false }
    # .NET Framework 4.8 Release key >= 528040
    return ($release -ge 528040)
}

if (-not (Check-NetFramework48)) {
    Write-Warning ".NET Framework 4.8 not detected on local machine. Installer may be required on target." 
}

# Prepare copy action
if ($RemoteComputer) {
    if ($UsePSSession) {
        $sess = New-PSSession -ComputerName $RemoteComputer -ErrorAction Stop
        Write-Host "Copying files to remote session $RemoteComputer:$TargetPath"
        Copy-Item -Path (Join-Path $SourcePath "*") -Destination $TargetPath -ToSession $sess -Recurse -Force
        Invoke-Command -Session $sess -ScriptBlock { param($t) Write-Host "Target dir: $t" } -ArgumentList $TargetPath
    } else {
        $unc = "\\$RemoteComputer\$($TargetPath.TrimStart('\'))"
        Write-Host "Copying files to UNC path $unc"
        New-Item -ItemType Directory -Path $unc -Force | Out-Null
        Copy-Item -Path (Join-Path $SourcePath "*") -Destination $unc -Recurse -Force
    }
} else {
    Write-Host "Copying files locally to $TargetPath"
    New-Item -ItemType Directory -Path $TargetPath -Force | Out-Null
    Copy-Item -Path (Join-Path $SourcePath "*") -Destination $TargetPath -Recurse -Force
}

# Install runtimes / drivers if installer dir provided
if ($InstallerDir -and (Test-Path $InstallerDir)) {
    Write-Host "Installer directory provided: $InstallerDir";
    $installers = Get-ChildItem -Path $InstallerDir -Filter *.exe -File -ErrorAction SilentlyContinue
    foreach ($inst in $installers) {
        Write-Host "Installer found: $($inst.Name) - ensure manual review before automated install"
        # Optional: use Start-Process -Wait -FilePath $inst.FullName -ArgumentList '/S' -Verb RunAs
    }
}

# Health check (invoke smoke test script on target if available)
$smokeRemotePath = Join-Path $TargetPath "scripts\smoke-test.ps1"
if ($RemoteComputer -and $UsePSSession) {
    Write-Host "Running remote smoke test on $RemoteComputer"
    Invoke-Command -Session $sess -ScriptBlock { param($p) if (Test-Path $p) { & $p -Mode production } else { Write-Warning "Smoke test not found: $p" } } -ArgumentList $smokeRemotePath
    Remove-PSSession -Session $sess
} else {
    if (Test-Path $smokeRemotePath) {
        Write-Host "Running local smoke test: $smokeRemotePath"
        & $smokeRemotePath -Mode production
    } else {
        Write-Host "No smoke-test script found at $smokeRemotePath; skip health check"
    }
}

Write-Host "Deployment finished. Verify logs and health checks." 
exit 0
