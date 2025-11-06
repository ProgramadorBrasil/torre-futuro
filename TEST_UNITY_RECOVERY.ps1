#Requires -Version 5.0
<#
.SYNOPSIS
Test Unity 6.0 recovery and asset database restoration

.DESCRIPTION
Validates project integrity before opening Unity editor

.EXAMPLE
.\TEST_UNITY_RECOVERY.ps1

.NOTES
Author: Recovery Script
Date: 2025-11-05
Version: 1.0
#>

param(
    [string]$ProjectPath = "D:\games\torre futuro",
    [string]$UnityVersion = "6.0.0.47f1",
    [switch]$OpenUnity = $false,
    [switch]$Force = $false
)

# Colors for output
$Colors = @{
    Success = 'Green'
    Error   = 'Red'
    Warning = 'Yellow'
    Info    = 'Cyan'
}

function Write-ColorOutput($message, $color = 'White') {
    Write-Host $message -ForegroundColor $color
}

function Test-ProjectStructure {
    Write-ColorOutput "=== PROJECT STRUCTURE VALIDATION ===" -Color $Colors.Info

    $criticalDirs = @(
        'Assets',
        'ProjectSettings',
        'Packages'
    )

    $allOk = $true
    foreach ($dir in $criticalDirs) {
        $dirPath = Join-Path $ProjectPath $dir
        if (Test-Path $dirPath -PathType Container) {
            Write-ColorOutput "  [OK] $dir/" -Color $Colors.Success
        } else {
            Write-ColorOutput "  [FAIL] $dir/ - MISSING" -Color $Colors.Error
            $allOk = $false
        }
    }

    return $allOk
}

function Test-ProjectSettingsFiles {
    Write-ColorOutput "=== PROJECTSETTINGS FILES ===" -Color $Colors.Info

    $expectedFiles = @(
        'AudioManager.asset',
        'ClusterInputManager.asset',
        'DynamicsManager.asset',
        'EditorBuildSettings.asset',
        'EditorSettings.asset',
        'GraphicsSettings.asset',
        'InputManager.asset',
        'MemorySettings.asset',
        'MultiplayerManager.asset',
        'NavMeshAreas.asset',
        'PackageManagerSettings.asset',
        'Physics2DSettings.asset',
        'PresetManager.asset',
        'ProjectSettings.asset',
        'QualitySettings.asset',
        'TagManager.asset',
        'TimeManager.asset',
        'UnityConnectSettings.asset',
        'VersionControlSettings.asset',
        'VFXManager.asset'
    )

    $settingsPath = Join-Path $ProjectPath 'ProjectSettings'
    $foundCount = 0
    $missingFiles = @()

    foreach ($file in $expectedFiles) {
        $filePath = Join-Path $settingsPath $file
        if (Test-Path $filePath -PathType Leaf) {
            $foundCount++
            Write-ColorOutput "  [OK] $file" -Color $Colors.Success
        } else {
            Write-ColorOutput "  [MISSING] $file" -Color $Colors.Error
            $missingFiles += $file
        }
    }

    Write-ColorOutput "Found: $foundCount/$($expectedFiles.Count) files" -Color $(if ($foundCount -eq $expectedFiles.Count) { $Colors.Success } else { $Colors.Error })

    return $missingFiles.Count -eq 0
}

function Test-YAMLValidity {
    Write-ColorOutput "=== YAML SYNTAX VALIDATION ===" -Color $Colors.Info

    $settingsPath = Join-Path $ProjectPath 'ProjectSettings'
    $assetFiles = Get-ChildItem -Path $settingsPath -Filter "*.asset"

    $allValid = $true
    foreach ($file in $assetFiles) {
        $firstLine = (Get-Content $file.FullName -First 1)
        if ($firstLine -match '^%YAML\s+\d+\.\d+') {
            Write-ColorOutput "  [OK] $($file.Name)" -Color $Colors.Success
        } else {
            Write-ColorOutput "  [INVALID] $($file.Name) - No YAML header" -Color $Colors.Error
            $allValid = $false
        }
    }

    return $allValid
}

function Test-TagManagerComplete {
    Write-ColorOutput "=== TAGMANAGER COMPLETENESS ===" -Color $Colors.Info

    $tagManagerPath = Join-Path $ProjectPath 'ProjectSettings\TagManager.asset'
    $content = Get-Content $tagManagerPath -Raw
    $lineCount = (Get-Content $tagManagerPath).Count

    Write-ColorOutput "  Lines: $lineCount (expected: 57)" -Color $Colors.Info
    Write-ColorOutput "  Size: $((Get-Item $tagManagerPath).Length) bytes" -Color $Colors.Info

    # Check for required sections
    $requiredSections = @(
        'TagManager:',
        'tags:',
        'layers:',
        'm_SortingLayers:'
    )

    $allPresent = $true
    foreach ($section in $requiredSections) {
        if ($content -match [regex]::Escape($section)) {
            Write-ColorOutput "  [OK] Section: $section" -Color $Colors.Success
        } else {
            Write-ColorOutput "  [MISSING] Section: $section" -Color $Colors.Error
            $allPresent = $false
        }
    }

    # Check ending (should end with proper YAML)
    if ($content -match 'locked:\s*\d+\s*$') {
        Write-ColorOutput "  [OK] File ends with proper YAML" -Color $Colors.Success
    } else {
        Write-ColorOutput "  [WARNING] File ending suspicious" -Color $Colors.Warning
    }

    return $allPresent
}

function Test-TempFolder {
    Write-ColorOutput "=== TEMP FOLDER STATUS ===" -Color $Colors.Info

    $tempPath = Join-Path $ProjectPath 'Temp'

    if (Test-Path $tempPath -PathType Container) {
        $items = @(Get-ChildItem $tempPath -Force)
        Write-ColorOutput "  Items: $($items.Count)" -Color $Colors.Info

        if ($items.Count -eq 0) {
            Write-ColorOutput "  [OK] Temp folder is empty (good state)" -Color $Colors.Success
            return $true
        } else {
            Write-ColorOutput "  [WARNING] Temp folder has $($items.Count) items" -Color $Colors.Warning
            foreach ($item in $items) {
                Write-ColorOutput "    - $($item.Name)" -Color $Colors.Warning
            }
            return $false
        }
    } else {
        Write-ColorOutput "  [OK] Temp folder does not exist (will be created)" -Color $Colors.Success
        return $true
    }
}

function Test-LibraryFolder {
    Write-ColorOutput "=== LIBRARY FOLDER STATUS ===" -Color $Colors.Info

    $libPath = Join-Path $ProjectPath 'Library'

    if (Test-Path $libPath -PathType Container) {
        $items = @(Get-ChildItem $libPath -Force)
        Write-ColorOutput "  [INFO] Library folder exists with $($items.Count) items" -Color $Colors.Warning
        Write-ColorOutput "  [RECOMMEND] Delete Library/ for clean regeneration" -Color $Colors.Warning
        Write-ColorOutput "  Run: Remove-Item -Recurse -Force '$libPath'" -Color $Colors.Info
        return $false
    } else {
        Write-ColorOutput "  [OK] Library folder does not exist (clean state)" -Color $Colors.Success
        return $true
    }
}

function Get-AssetStatistics {
    Write-ColorOutput "=== ASSET STATISTICS ===" -Color $Colors.Info

    $assetsPath = Join-Path $ProjectPath 'Assets'
    $files = @(Get-ChildItem -Path $assetsPath -Recurse -File)

    $fileCount = $files.Count
    $totalSize = ($files | Measure-Object -Property Length -Sum).Sum
    $totalSizeMB = [math]::Round($totalSize / 1MB, 2)

    Write-ColorOutput "  Files: $fileCount" -Color $Colors.Info
    Write-ColorOutput "  Size: $totalSizeMB MB" -Color $Colors.Info

    # File type breakdown
    $typeBreakdown = $files | Group-Object -Property {[System.IO.Path]::GetExtension($_)} |
                     Sort-Object -Property Count -Descending |
                     Select-Object -First 10

    Write-ColorOutput "  Top file types:" -Color $Colors.Info
    foreach ($type in $typeBreakdown) {
        Write-ColorOutput "    $($type.Name): $($type.Count) files" -Color $Colors.Info
    }

    return $fileCount -gt 0
}

function Show-RecoveryStatus {
    param([hashtable]$Results)

    Write-ColorOutput "=== RECOVERY STATUS ===" -Color $Colors.Info

    $allPassed = $true
    foreach ($test in $Results.Keys) {
        $status = $Results[$test]
        $color = if ($status) { $Colors.Success } else { $Colors.Error }
        $symbol = if ($status) { "[PASS]" } else { "[FAIL]" }
        Write-ColorOutput "  $symbol $test" -Color $color
        $allPassed = $allPassed -and $status
    }

    return $allPassed
}

function Invoke-UnityTest {
    Write-ColorOutput "=== OPENING UNITY ===" -Color $Colors.Warning

    $unityExePath = "C:\Program Files\Unity\Hub\Editor\$UnityVersion\Editor\Unity.exe"

    if (-not (Test-Path $unityExePath)) {
        Write-ColorOutput "[ERROR] Unity not found at: $unityExePath" -Color $Colors.Error
        Write-ColorOutput "Please install Unity $UnityVersion or update the path" -Color $Colors.Error
        return $false
    }

    Write-ColorOutput "[INFO] Starting Unity..." -Color $Colors.Info
    Write-ColorOutput "[INFO] Project: $ProjectPath" -Color $Colors.Info
    Write-ColorOutput "[INFO] Version: $UnityVersion" -Color $Colors.Info
    Write-ColorOutput "" -Color $Colors.Info
    Write-ColorOutput "IMPORTANT: This will take 5-10 minutes for first-time Asset Import" -Color $Colors.Warning
    Write-ColorOutput "Monitor the Console for: 'Asset Import Completed'" -Color $Colors.Warning
    Write-ColorOutput "" -Color $Colors.Info

    & $unityExePath -projectPath $ProjectPath -logFile "-"

    return $true
}

# MAIN EXECUTION
Write-ColorOutput "==========================================================" -Color $Colors.Info
Write-ColorOutput "UNITY 6.0 PROJECT RECOVERY - VALIDATION SUITE" -Color $Colors.Info
Write-ColorOutput "Torre Futuro Space RPG - Asset Database Recovery" -Color $Colors.Info
Write-ColorOutput "==========================================================" -Color $Colors.Info
Write-ColorOutput ""
Write-ColorOutput "Project: $ProjectPath" -Color $Colors.Info
Write-ColorOutput "Unity Version: $UnityVersion" -Color $Colors.Info
Write-ColorOutput ""

# Verify project path exists
if (-not (Test-Path $ProjectPath -PathType Container)) {
    Write-ColorOutput "[FATAL] Project path does not exist: $ProjectPath" -Color $Colors.Error
    exit 1
}

# Run all tests
$testResults = @{}
$testResults['Project Structure'] = Test-ProjectStructure
$testResults['ProjectSettings Files'] = Test-ProjectSettingsFiles
$testResults['YAML Validity'] = Test-YAMLValidity
$testResults['TagManager Complete'] = Test-TagManagerComplete
$testResults['Temp Folder Clean'] = Test-TempFolder
$testResults['Library Folder Status'] = Test-LibraryFolder
$testResults['Assets Exist'] = Get-AssetStatistics

# Show final status
$allPassed = Show-RecoveryStatus $testResults

# Recommendation
Write-ColorOutput "=== RECOMMENDATIONS ===" -Color $Colors.Info
if ($allPassed) {
    Write-ColorOutput "[OK] Project is ready to open in Unity!" -Color $Colors.Success
    if ($OpenUnity) {
        Invoke-UnityTest
    } else {
        Write-ColorOutput "Run: $($MyInvocation.ScriptName) -OpenUnity" -Color $Colors.Info
    }
} else {
    Write-ColorOutput "[FAIL] Project has issues that must be fixed first" -Color $Colors.Error
    Write-ColorOutput "See UNITY_RECOVERY_GUIDE.md for detailed instructions" -Color $Colors.Info
}

Write-ColorOutput ""
