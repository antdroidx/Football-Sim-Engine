param(
    [ValidateSet('Fast','Balance','Career')][string]$Tier = 'Fast',
    [ValidateSet('Engine','College','Pro','All')][string]$Owner = 'All',
    [switch]$List
)
$ErrorActionPreference = 'Stop'
$projects = @{
    Engine = 'tests/Football.Engine.Tests/Football.Engine.Tests.csproj'
    College = 'tests/DynastyManager.Core.Tests/DynastyManager.Core.Tests.csproj'
    Pro = 'tests/ProFootball.Tests/ProFootball.Tests.csproj'
}
Push-Location (Split-Path $PSScriptRoot -Parent)
try {
    $owners = if ($Owner -eq 'All') { @('Engine','College','Pro') } else { @($Owner) }
    $executed = 0
    foreach ($selected in $owners) {
        $arguments = @('test', $projects[$selected], '-c', 'Release', '--filter', "Category=$Tier")
        if ($List) { $arguments += '--list-tests' }
        else {
            $resultFolder = Join-Path '.artifacts/test-tiers' ([Guid]::NewGuid().ToString('N'))
            $arguments += @('--results-directory', $resultFolder, '--logger', 'trx;LogFileName=validation.trx')
        }
        & dotnet @arguments
        if ($LASTEXITCODE -ne 0) { throw "$selected/$Tier failed ($LASTEXITCODE)" }
        if (!$List) {
            $resultFile = Join-Path $resultFolder 'validation.trx'
            if (!(Test-Path -LiteralPath $resultFile)) { throw "$selected/$Tier produced no test results" }
            [xml]$results = Get-Content -LiteralPath $resultFile -Raw
            $counters = $results.SelectSingleNode("//*[local-name()='Counters']")
            $count = if ($null -eq $counters) { 0 } else { [int]$counters.total }
            if ($count -eq 0 -and $Owner -ne 'All') { throw "$selected/$Tier selected zero tests" }
            $executed += $count
        }
    }
    if (!$List -and $executed -eq 0) { throw "$Owner/$Tier selected zero tests" }
}
finally { Pop-Location }
# Device acceptance is deliberately separate: install an exact hashed APK and
# use build/*-smoke.py on a disposable emulator. No synthetic device test count.
