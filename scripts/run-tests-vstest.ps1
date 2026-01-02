param(
    [string]$assembly = "$(Join-Path (Get-Location) 'PCBInspection.Tests\bin\Debug\net48\PCBInspection.Tests.dll')"
)

Write-Host "Running tests for assembly: $assembly"

$vstest = Get-Command vstest.console.exe -ErrorAction SilentlyContinue
if ($vstest) {
    Write-Host "Using vstest.console.exe at $($vstest.Path)"
    & $vstest.Path $assembly | Write-Host
    exit $LASTEXITCODE
}

# fallback: try nunit3-console.exe in PATH
$nunitCmd = Get-Command nunit3-console.exe -ErrorAction SilentlyContinue
if ($nunitCmd) {
    Write-Host "Using nunit3-console.exe at $($nunitCmd.Path)"
    & $nunitCmd.Path $assembly | Write-Host
    exit $LASTEXITCODE
}

# fallback: try nunit3-console.exe in tools folder
$nuPath = Join-Path (Get-Location) "tools\nunit3-console.exe"
if (Test-Path $nuPath) {
    Write-Host "Using nunit3-console.exe at $nuPath"
    & $nuPath $assembly | Write-Host
    exit $LASTEXITCODE
}

Write-Warning "No vstest.console.exe or nunit3-console.exe found. On CI (windows-latest) vstest should be available. Locally run tests via Visual Studio Test Explorer or install NUnit console runner."
exit 2