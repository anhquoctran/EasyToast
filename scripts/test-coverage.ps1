# Requires: .NET SDK 8+, Windows
# Runs FuzzyToast.Tests with coverlet and fails if line coverage < 95%.

$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $PSScriptRoot
$outDir = Join-Path $root "TestResults"
New-Item -ItemType Directory -Force -Path $outDir | Out-Null

$proj = Join-Path $root "tests\FuzzyToast.Tests\FuzzyToast.Tests.csproj"
$coverageOut = Join-Path $outDir "coverage"
$excludeByFile = "**/Properties/**/*.cs%2c**/*Designer.cs%2c**/Internal/Compatibility/**/*.cs%2c**/Internal/Ui/**/*.cs%2c**/Internal/WinFormsScreenProvider.cs%2c**/Internal/DpiScaling.cs%2c**/Internal/IUiMarshaler.cs"

Write-Host "Running tests with coverage (threshold 95% line)..." -ForegroundColor Cyan

dotnet test $proj -c Release `
  "/p:CollectCoverage=true" `
  "/p:CoverletOutputFormat=cobertura" `
  "/p:CoverletOutput=$coverageOut" `
  "/p:ExcludeByFile=$excludeByFile" `
  "/p:Include=[FuzzyToast]*" `
  "/p:Threshold=95" `
  "/p:ThresholdType=line" `
  "/p:ThresholdStat=total" `
  "/p:DeterministicSourcePaths=false" `
  "/p:UseSharedCompilation=false"

if ($LASTEXITCODE -ne 0) {
  Write-Error "Tests or coverage threshold failed (exit $LASTEXITCODE)."
  exit $LASTEXITCODE
}

$cobertura = "$coverageOut.cobertura.xml"
if (Test-Path $cobertura) {
  [xml]$x = Get-Content $cobertura
  $lineRate = [double]$x.coverage.'line-rate'
  $pct = [math]::Round($lineRate * 100, 2)
  Write-Host "Line coverage: $pct% (required >= 95%)" -ForegroundColor Green
  Write-Host "Report: $cobertura"
}

exit 0
