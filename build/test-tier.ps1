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
    foreach ($selected in $owners) {
        $arguments = @('test', $projects[$selected], '-c', 'Release', '--filter', "Category=$Tier")
        if ($List) { $arguments += '--list-tests' }
        & dotnet @arguments
        if ($LASTEXITCODE -ne 0) { throw "$selected/$Tier failed ($LASTEXITCODE)" }
    }
}
finally { Pop-Location }
# Device acceptance is deliberately separate: install an exact hashed APK and
# use build/*-smoke.py on a disposable emulator. No synthetic device test count.
