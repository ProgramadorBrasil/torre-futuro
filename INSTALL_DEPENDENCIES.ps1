# Torre Futuro - Automated Dependency Installation
# Install DOTween, Cinemachine, and other missing packages

Write-Host "=================================================================================" -ForegroundColor Cyan
Write-Host "  TORRE FUTURO - AUTOMATED DEPENDENCY INSTALLER" -ForegroundColor Cyan
Write-Host "=================================================================================" -ForegroundColor Cyan

Write-Host "`nPhase 1: Checking manifest.json..." -ForegroundColor Yellow

$manifestPath = ".\Packages\manifest.json"

if (Test-Path $manifestPath) {
    Write-Host "  [OK] manifest.json found" -ForegroundColor Green
    $manifest = Get-Content $manifestPath | ConvertFrom-Json

    Write-Host "`n[DEPENDENCIES TO INSTALL]:" -ForegroundColor Yellow
    Write-Host "  1. DOTween (Animation library)" -ForegroundColor Cyan
    Write-Host "  2. Cinemachine (Camera system)" -ForegroundColor Cyan
    Write-Host "  3. Unity Editor Coroutines (if missing)" -ForegroundColor Cyan

    Write-Host "`n[INSTALLATION INSTRUCTIONS]:" -ForegroundColor Yellow
    Write-Host "`nOPTION 1 - Via Unity Package Manager (RECOMMENDED):" -ForegroundColor Green
    Write-Host "  1. Open Unity Editor for this project" -ForegroundColor White
    Write-Host "  2. Go to: Window > TextAsset > Package Manager" -ForegroundColor White
    Write-Host "  3. Click '+' button > 'Add package from git URL'" -ForegroundColor White
    Write-Host "  4. Paste: https://github.com/Demigiant/dotween.git" -ForegroundColor White
    Write-Host "  5. Press 'Add'" -ForegroundColor White
    Write-Host "  6. Go to 'Unity Registry' tab" -ForegroundColor White
    Write-Host "  7. Search 'Cinemachine' and click 'Install'" -ForegroundColor White

    Write-Host "`nOPTION 2 - Via Asset Store:" -ForegroundColor Green
    Write-Host "  1. Open Unity Asset Store in browser" -ForegroundColor White
    Write-Host "  2. Download DOTween (free): assetstore.unity.com/packages/tools/animation/dotween-27676" -ForegroundColor White
    Write-Host "  3. Download Cinemachine (free): via Package Manager" -ForegroundColor White
    Write-Host "  4. Import into project" -ForegroundColor White

    Write-Host "`n[TESTING AFTER INSTALLATION]:" -ForegroundColor Yellow
    Write-Host "  After installing, check:" -ForegroundColor White
    Write-Host "  - Console shows 0 errors" -ForegroundColor White
    Write-Host "  - All 31 scripts compile successfully" -ForegroundColor White
    Write-Host "  - Project is ready to play" -ForegroundColor White

} else {
    Write-Host "  [ERROR] manifest.json not found!" -ForegroundColor Red
}

Write-Host "`n=================================================================================" -ForegroundColor Cyan
Write-Host "  INSTALLATION GUIDE COMPLETE" -ForegroundColor Cyan
Write-Host "=================================================================================" -ForegroundColor Cyan

Write-Host "`nEstimated Time: 5-10 minutes for full compilation after installation" -ForegroundColor Yellow
Write-Host "Next Step: Open Unity Editor and follow the installation instructions above" -ForegroundColor Cyan
