<#
  Compile, teste et publie Planning Converter sous Windows (équivalent local du workflow GitHub).
  Prérequis : SDK .NET 10 (voir global.json). Usage : .\build.ps1
  Produit : artifacts\windows-x64\PlanningConverter.exe et un journal build.log.
#>
$ErrorActionPreference = 'Stop'
Set-Location $PSScriptRoot

function Step($name, $script) {
    Write-Host "== $name" -ForegroundColor Cyan
    & $script
    if ($LASTEXITCODE -ne 0) { throw "Échec : $name (code $LASTEXITCODE)" }
}

Start-Transcript -Path build.log -Force | Out-Null
Step 'Version du SDK'  { dotnet --info }
Step 'Restore'         { dotnet restore PlanningConverter.sln }
Step 'Build Release'   { dotnet build PlanningConverter.sln -c Release --no-restore -v minimal }
Step 'Tests moteur'    { dotnet test tests/PlanningConverter.Parsing.Tests/PlanningConverter.Parsing.Tests.csproj -c Release --no-build }
Step 'Tests exports'   { dotnet test tests/PlanningConverter.Export.Tests/PlanningConverter.Export.Tests.csproj -c Release --no-build }
Step 'Publication'     { dotnet publish src/PlanningConverter.Ui/PlanningConverter.Ui.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o artifacts/windows-x64 }
Stop-Transcript | Out-Null
Write-Host "Terminé : artifacts\windows-x64\PlanningConverter.exe" -ForegroundColor Green
