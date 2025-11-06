# ================================================================
# UNITY PROJECT DEEP ANALYSIS TOOL - Torre Futuro
# ================================================================
# Este script analisa profundamente um projeto Unity para identificar
# problemas de performance e fornece soluções automatizadas

param(
    [string]$ProjectPath = "D:\games\torre futuro",
    [switch]$AutoFix = $false,
    [switch]$Verbose = $true
)

# Configurações e thresholds
$Config = @{
    MaxTextureSize = 2048
    MaxFileSize = 50MB
    MaxModelSize = 20MB
    CriticalFileSize = 100MB
    TextureFormats = @('.png', '.jpg', '.jpeg', '.tga', '.psd', '.tiff', '.bmp', '.exr', '.hdr')
    ModelFormats = @('.fbx', '.obj', '.3ds', '.dae', '.blend', '.max', '.mb', '.ma')
    AudioFormats = @('.wav', '.mp3', '.ogg', '.aiff', '.flac', '.m4a')
    CompressedFormats = @('.zip', '.rar', '.7z', '.tar', '.gz')
}

# ================================================================
# FUNÇÕES DE ANÁLISE
# ================================================================

function Write-ColorOutput {
    param(
        [string]$Message,
        [string]$Color = "White",
        [switch]$NoNewline
    )

    $colors = @{
        "Critical" = "Red"
        "High" = "DarkYellow"
        "Medium" = "Yellow"
        "Low" = "Green"
        "Info" = "Cyan"
        "Success" = "Green"
    }

    $actualColor = if ($colors.ContainsKey($Color)) { $colors[$Color] } else { $Color }

    if ($NoNewline) {
        Write-Host $Message -ForegroundColor $actualColor -NoNewline
    } else {
        Write-Host $Message -ForegroundColor $actualColor
    }
}

function Get-FileHash {
    param([string]$FilePath)

    $hash = Get-FileHash -Path $FilePath -Algorithm MD5 -ErrorAction SilentlyContinue
    return if ($hash) { $hash.Hash } else { $null }
}

function Analyze-ProjectStructure {
    Write-ColorOutput "`n=== ANALYZING PROJECT STRUCTURE ===" -Color "Info"

    $projectInfo = @{
        TotalSize = 0
        FileCount = 0
        AssetsByType = @{}
        LargeFiles = @()
        ProblematicFiles = @()
    }

    # Analisar todos os arquivos
    $allFiles = Get-ChildItem -Path $ProjectPath -Recurse -File -ErrorAction SilentlyContinue

    foreach ($file in $allFiles) {
        $projectInfo.FileCount++
        $projectInfo.TotalSize += $file.Length

        $ext = $file.Extension.ToLower()

        if (-not $projectInfo.AssetsByType.ContainsKey($ext)) {
            $projectInfo.AssetsByType[$ext] = @{
                Count = 0
                TotalSize = 0
                Files = @()
            }
        }

        $projectInfo.AssetsByType[$ext].Count++
        $projectInfo.AssetsByType[$ext].TotalSize += $file.Length

        # Identificar arquivos grandes
        if ($file.Length -gt $Config.MaxFileSize) {
            $projectInfo.LargeFiles += @{
                Path = $file.FullName
                Size = $file.Length
                Type = $ext
                Severity = if ($file.Length -gt $Config.CriticalFileSize) { "Critical" }
                          elseif ($file.Length -gt 75MB) { "High" }
                          else { "Medium" }
            }
        }

        # Identificar arquivos problemáticos
        if ($ext -in $Config.CompressedFormats) {
            $projectInfo.ProblematicFiles += @{
                Path = $file.FullName
                Issue = "Compressed archive in project"
                Recommendation = "Extract and delete archive"
                Severity = "Critical"
            }
        }
    }

    return $projectInfo
}

function Analyze-Textures {
    Write-ColorOutput "`n=== ANALYZING TEXTURES ===" -Color "Info"

    $textureIssues = @()
    $texturePaths = Get-ChildItem -Path "$ProjectPath\Assets" -Recurse -Include $Config.TextureFormats.ForEach({"*$_"})

    foreach ($texture in $texturePaths) {
        # Verificar tamanho do arquivo
        if ($texture.Length -gt 10MB) {
            $textureIssues += @{
                Path = $texture.FullName
                FileSize = $texture.Length
                Issue = "Large texture file"
                Recommendation = "Reduce resolution or use compression"
                Severity = if ($texture.Length -gt 50MB) { "Critical" } else { "High" }
            }
        }

        # Verificar se está em pasta não otimizada
        if ($texture.DirectoryName -match "Resources") {
            $textureIssues += @{
                Path = $texture.FullName
                Issue = "Texture in Resources folder"
                Recommendation = "Move to regular Assets folder and use references"
                Severity = "Medium"
            }
        }

        # Analisar dimensões da imagem (se possível)
        try {
            Add-Type -AssemblyName System.Drawing -ErrorAction SilentlyContinue
            if ($texture.Extension -in @('.png', '.jpg', '.jpeg', '.bmp')) {
                $img = [System.Drawing.Image]::FromFile($texture.FullName)

                if ($img.Width -gt $Config.MaxTextureSize -or $img.Height -gt $Config.MaxTextureSize) {
                    $textureIssues += @{
                        Path = $texture.FullName
                        Dimensions = "$($img.Width)x$($img.Height)"
                        Issue = "Texture exceeds maximum recommended size"
                        Recommendation = "Resize to ${Config.MaxTextureSize}x${Config.MaxTextureSize} or smaller"
                        Severity = "High"
                    }
                }

                $img.Dispose()
            }
        } catch {
            # Silently continue if image analysis fails
        }
    }

    return $textureIssues
}

function Analyze-Models {
    Write-ColorOutput "`n=== ANALYZING 3D MODELS ===" -Color "Info"

    $modelIssues = @()
    $modelPaths = Get-ChildItem -Path "$ProjectPath\Assets" -Recurse -Include $Config.ModelFormats.ForEach({"*$_"})

    foreach ($model in $modelPaths) {
        if ($model.Length -gt $Config.MaxModelSize) {
            $modelIssues += @{
                Path = $model.FullName
                FileSize = $model.Length
                Issue = "Large model file"
                Recommendation = "Optimize mesh, reduce polygons, or split into smaller parts"
                Severity = if ($model.Length -gt 50MB) { "Critical" } else { "High" }
            }
        }

        # Verificar se está em Resources
        if ($model.DirectoryName -match "Resources") {
            $modelIssues += @{
                Path = $model.FullName
                Issue = "Model in Resources folder"
                Recommendation = "Use Addressables or Asset Bundles for large models"
                Severity = "High"
            }
        }
    }

    return $modelIssues
}

function Find-Duplicates {
    Write-ColorOutput "`n=== DETECTING DUPLICATE FILES ===" -Color "Info"

    $duplicates = @{}
    $fileHashes = @{}

    $files = Get-ChildItem -Path "$ProjectPath\Assets" -Recurse -File |
             Where-Object { $_.Length -gt 1KB } # Ignorar arquivos muito pequenos

    foreach ($file in $files) {
        # Usar combinação de tamanho e nome como hash rápido
        $quickHash = "$($file.Length)_$($file.Name)"

        if (-not $fileHashes.ContainsKey($quickHash)) {
            $fileHashes[$quickHash] = @()
        }

        $fileHashes[$quickHash] += $file.FullName
    }

    # Filtrar apenas grupos com duplicatas
    foreach ($hash in $fileHashes.Keys) {
        if ($fileHashes[$hash].Count -gt 1) {
            $duplicates[$hash] = $fileHashes[$hash]
        }
    }

    return $duplicates
}

function Check-ProjectSettings {
    Write-ColorOutput "`n=== CHECKING PROJECT SETTINGS ===" -Color "Info"

    $issues = @()

    # Verificar Library folder
    $libraryPath = Join-Path $ProjectPath "Library"
    if (Test-Path $libraryPath) {
        $librarySize = (Get-ChildItem $libraryPath -Recurse -File | Measure-Object -Property Length -Sum).Sum

        if ($librarySize -gt 5GB) {
            $issues += @{
                Issue = "Large Library folder"
                Size = $librarySize
                Recommendation = "Consider clearing Library cache and reimporting"
                Severity = "Medium"
            }
        }
    }

    # Verificar Temp folder
    $tempPath = Join-Path $ProjectPath "Temp"
    if (Test-Path $tempPath) {
        $issues += @{
            Issue = "Temp folder exists"
            Recommendation = "Delete Temp folder when Unity is closed"
            Severity = "Low"
        }
    }

    # Verificar .meta files órfãos
    $metaFiles = Get-ChildItem -Path "$ProjectPath\Assets" -Recurse -Filter "*.meta"
    foreach ($meta in $metaFiles) {
        $assetPath = $meta.FullName -replace '\.meta$', ''
        if (-not (Test-Path $assetPath)) {
            $issues += @{
                Issue = "Orphaned .meta file"
                Path = $meta.FullName
                Recommendation = "Delete orphaned .meta file"
                Severity = "Low"
            }
        }
    }

    return $issues
}

function Generate-OptimizationScript {
    param($ProjectInfo, $TextureIssues, $ModelIssues)

    Write-ColorOutput "`n=== GENERATING OPTIMIZATION SCRIPTS ===" -Color "Info"

    $scriptContent = @"
// Auto-generated Unity Optimization Script
// Generated: $(Get-Date -Format "yyyy-MM-dd HH:mm:ss")

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class AutoOptimizer : EditorWindow
{
    [MenuItem("Torre Futuro/Run Auto-Optimization")]
    public static void RunOptimization()
    {
        Debug.Log("Starting automatic optimization...");

        int optimizedCount = 0;

"@

    # Adicionar otimizações de textura
    foreach ($issue in $TextureIssues | Where-Object { $_.Severity -in @("Critical", "High") }) {
        $assetPath = $issue.Path -replace [regex]::Escape($ProjectPath), "" -replace "\\", "/"
        $scriptContent += @"

        // Optimize texture: $assetPath
        {
            string path = "$assetPath";
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer != null)
            {
                importer.maxTextureSize = 2048;
                importer.textureCompression = TextureImporterCompression.Compressed;
                importer.SaveAndReimport();
                optimizedCount++;
                Debug.Log("Optimized texture: " + path);
            }
        }
"@
    }

    $scriptContent += @"

        Debug.Log($"Optimization complete. Optimized {optimizedCount} assets.");
        AssetDatabase.Refresh();
    }
}
"@

    $scriptPath = Join-Path $ProjectPath "Assets\Editor\AutoOptimizer.cs"

    # Criar pasta Editor se não existir
    $editorPath = Join-Path $ProjectPath "Assets\Editor"
    if (-not (Test-Path $editorPath)) {
        New-Item -ItemType Directory -Path $editorPath -Force | Out-Null
    }

    # Salvar script
    $scriptContent | Out-File -FilePath $scriptPath -Encoding UTF8

    Write-ColorOutput "Optimization script generated at: $scriptPath" -Color "Success"
}

function Show-Report {
    param($ProjectInfo, $TextureIssues, $ModelIssues, $Duplicates, $SettingsIssues)

    Write-ColorOutput "`n`n" -NoNewline
    Write-ColorOutput "=" -Color "Cyan" -NoNewline
    Write-ColorOutput "=" -Color "Cyan" -NoNewline
    Write-ColorOutput "=" -Color "Cyan" -NoNewline
    Write-ColorOutput " UNITY PROJECT DIAGNOSTIC REPORT " -Color "Info" -NoNewline
    Write-ColorOutput "=" -Color "Cyan" -NoNewline
    Write-ColorOutput "=" -Color "Cyan" -NoNewline
    Write-ColorOutput "=" -Color "Cyan"

    # Resumo Geral
    Write-ColorOutput "`n--- PROJECT OVERVIEW ---" -Color "Info"
    Write-Host "Total Files: $($ProjectInfo.FileCount)"
    Write-Host "Total Size: $([Math]::Round($ProjectInfo.TotalSize / 1GB, 2)) GB"
    Write-Host "Large Files: $($ProjectInfo.LargeFiles.Count)"
    Write-Host "Problematic Files: $($ProjectInfo.ProblematicFiles.Count)"

    # Distribuição por tipo
    Write-ColorOutput "`n--- FILE TYPE DISTRIBUTION ---" -Color "Info"
    $ProjectInfo.AssetsByType.GetEnumerator() |
        Sort-Object { $_.Value.TotalSize } -Descending |
        Select-Object -First 10 |
        ForEach-Object {
            $sizeMB = [Math]::Round($_.Value.TotalSize / 1MB, 2)
            Write-Host "$($_.Key): $($_.Value.Count) files ($sizeMB MB)"
        }

    # Problemas Críticos
    $criticalCount = ($ProjectInfo.LargeFiles | Where-Object { $_.Severity -eq "Critical" }).Count
    $criticalCount += ($ProjectInfo.ProblematicFiles | Where-Object { $_.Severity -eq "Critical" }).Count
    $criticalCount += ($TextureIssues | Where-Object { $_.Severity -eq "Critical" }).Count
    $criticalCount += ($ModelIssues | Where-Object { $_.Severity -eq "Critical" }).Count

    if ($criticalCount -gt 0) {
        Write-ColorOutput "`n!!! CRITICAL ISSUES FOUND: $criticalCount !!!" -Color "Critical"

        # Listar problemas críticos
        foreach ($file in $ProjectInfo.LargeFiles | Where-Object { $_.Severity -eq "Critical" }) {
            Write-ColorOutput "  [CRITICAL] Large file: $($file.Path)" -Color "Critical"
            Write-Host "    Size: $([Math]::Round($file.Size / 1MB, 2)) MB"
        }

        foreach ($file in $ProjectInfo.ProblematicFiles | Where-Object { $_.Severity -eq "Critical" }) {
            Write-ColorOutput "  [CRITICAL] $($file.Issue): $($file.Path)" -Color "Critical"
            Write-Host "    Recommendation: $($file.Recommendation)"
        }
    }

    # Resumo de Issues
    Write-ColorOutput "`n--- ISSUES SUMMARY ---" -Color "Info"

    $allIssues = @()
    $allIssues += $TextureIssues
    $allIssues += $ModelIssues
    $allIssues += $ProjectInfo.LargeFiles
    $allIssues += $ProjectInfo.ProblematicFiles

    $issueCounts = $allIssues | Group-Object Severity

    foreach ($group in $issueCounts) {
        $color = switch($group.Name) {
            "Critical" { "Critical" }
            "High" { "High" }
            "Medium" { "Medium" }
            "Low" { "Low" }
            default { "Info" }
        }
        Write-ColorOutput "$($group.Name): $($group.Count) issues" -Color $color
    }

    # Duplicatas
    if ($Duplicates.Count -gt 0) {
        Write-ColorOutput "`n--- DUPLICATE FILES ---" -Color "Medium"
        Write-Host "Found $($Duplicates.Count) groups of duplicate files"

        $totalWaste = 0
        foreach ($group in $Duplicates.Values) {
            if ($group.Count -gt 1) {
                $fileInfo = Get-Item $group[0]
                $waste = $fileInfo.Length * ($group.Count - 1)
                $totalWaste += $waste
            }
        }

        Write-Host "Potential space savings: $([Math]::Round($totalWaste / 1MB, 2)) MB"
    }

    # Recomendações
    Write-ColorOutput "`n--- TOP RECOMMENDATIONS ---" -Color "Success"

    $recommendations = @(
        "1. Remove all compressed archives (.zip, .rar, .7z) from the project"
        "2. Optimize textures larger than 2048x2048"
        "3. Enable texture compression for all uncompressed textures"
        "4. Remove duplicate assets to save space"
        "5. Move large assets out of Resources folder"
        "6. Use Asset Bundles or Addressables for runtime loading"
        "7. Clear Library folder if it's too large (backup first)"
        "8. Disable Read/Write on textures and models unless needed"
    )

    foreach ($rec in $recommendations) {
        Write-Host "  $rec"
    }
}

function Export-DetailedReport {
    param($ProjectInfo, $TextureIssues, $ModelIssues, $Duplicates, $SettingsIssues)

    $reportPath = Join-Path $ProjectPath "DiagnosticReport_$(Get-Date -Format 'yyyyMMdd_HHmmss').txt"

    $report = @"
================================================================================
UNITY PROJECT DIAGNOSTIC REPORT - Torre Futuro
Generated: $(Get-Date -Format "yyyy-MM-dd HH:mm:ss")
Project Path: $ProjectPath
================================================================================

PROJECT STATISTICS
------------------
Total Files: $($ProjectInfo.FileCount)
Total Size: $([Math]::Round($ProjectInfo.TotalSize / 1GB, 2)) GB
Large Files (>50MB): $($ProjectInfo.LargeFiles.Count)
Problematic Files: $($ProjectInfo.ProblematicFiles.Count)

CRITICAL ISSUES
---------------
"@

    foreach ($issue in $ProjectInfo.ProblematicFiles + $TextureIssues + $ModelIssues | Where-Object { $_.Severity -eq "Critical" }) {
        $report += "`n[CRITICAL] $($issue.Issue)"
        if ($issue.Path) { $report += "`n  Path: $($issue.Path)" }
        if ($issue.Recommendation) { $report += "`n  Fix: $($issue.Recommendation)" }
        $report += "`n"
    }

    $report += @"

HIGH PRIORITY ISSUES
--------------------
"@

    foreach ($issue in $TextureIssues + $ModelIssues | Where-Object { $_.Severity -eq "High" }) {
        $report += "`n[HIGH] $($issue.Issue)"
        if ($issue.Path) { $report += "`n  Path: $($issue.Path)" }
        if ($issue.Recommendation) { $report += "`n  Fix: $($issue.Recommendation)" }
        $report += "`n"
    }

    # Salvar relatório
    $report | Out-File -FilePath $reportPath -Encoding UTF8

    Write-ColorOutput "`nDetailed report saved to: $reportPath" -Color "Success"

    return $reportPath
}

# ================================================================
# EXECUÇÃO PRINCIPAL
# ================================================================

Clear-Host
Write-ColorOutput "UNITY PROJECT DEEP ANALYZER v2.0" -Color "Cyan"
Write-ColorOutput "=================================" -Color "Cyan"
Write-ColorOutput "Project: $ProjectPath" -Color "Info"
Write-ColorOutput "Started: $(Get-Date -Format 'HH:mm:ss')`n" -Color "Info"

# Verificar se o projeto existe
if (-not (Test-Path $ProjectPath)) {
    Write-ColorOutput "ERROR: Project path not found!" -Color "Critical"
    exit 1
}

# Executar análises
$projectInfo = Analyze-ProjectStructure
$textureIssues = Analyze-Textures
$modelIssues = Analyze-Models
$duplicates = Find-Duplicates
$settingsIssues = Check-ProjectSettings

# Mostrar relatório
Show-Report -ProjectInfo $projectInfo `
           -TextureIssues $textureIssues `
           -ModelIssues $modelIssues `
           -Duplicates $duplicates `
           -SettingsIssues $settingsIssues

# Exportar relatório detalhado
$reportPath = Export-DetailedReport -ProjectInfo $projectInfo `
                                    -TextureIssues $textureIssues `
                                    -ModelIssues $modelIssues `
                                    -Duplicates $duplicates `
                                    -SettingsIssues $settingsIssues

# Gerar script de otimização se solicitado
if ($AutoFix) {
    Generate-OptimizationScript -ProjectInfo $projectInfo `
                                -TextureIssues $textureIssues `
                                -ModelIssues $modelIssues
}

Write-ColorOutput "`n=================================" -Color "Cyan"
Write-ColorOutput "Analysis Complete!" -Color "Success"
Write-ColorOutput "Time: $(Get-Date -Format 'HH:mm:ss')" -Color "Info"

# Perguntar se deseja abrir o relatório
$openReport = Read-Host "`nOpen detailed report? (Y/N)"
if ($openReport -eq 'Y' -or $openReport -eq 'y') {
    Start-Process notepad.exe $reportPath
}