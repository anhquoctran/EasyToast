# Packs FuzzyToast (nupkg + snupkg) into artifacts/nuget.
$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $PSScriptRoot
$outDir = Join-Path $root "artifacts\nuget"
New-Item -ItemType Directory -Force -Path $outDir | Out-Null

$proj = Join-Path $root "src\FuzzyToast\FuzzyToast.csproj"

Write-Host "Packing FuzzyToast..." -ForegroundColor Cyan

dotnet pack $proj -c Release -o $outDir --include-symbols "/p:SymbolPackageFormat=snupkg"
if ($LASTEXITCODE -ne 0) {
  Write-Error "dotnet pack failed (exit $LASTEXITCODE)."
  exit $LASTEXITCODE
}

Write-Host "Packages:" -ForegroundColor Green
Get-ChildItem $outDir -Filter "FuzzyToast.*" | ForEach-Object { Write-Host "  $($_.Name)" }
