using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;

namespace TorreFuturo.Diagnostics
{
    /// <summary>
    /// Ferramenta completa de diagnóstico e otimização de assets para Unity 6
    /// Identifica problemas de performance e fornece soluções automatizadas
    /// </summary>
    public class UnityAssetDiagnostics : EditorWindow
    {
        // ===================== ESTRUTURAS DE DADOS =====================

        [Serializable]
        public class AssetReport
        {
            public string path;
            public string type;
            public long sizeInBytes;
            public string issue;
            public string recommendation;
            public int severity; // 1=Low, 2=Medium, 3=High, 4=Critical
        }

        [Serializable]
        public class TextureAnalysis
        {
            public string path;
            public int width;
            public int height;
            public TextureFormat format;
            public long memorySizeKB;
            public bool hasMipMaps;
            public bool isReadable;
            public string compressionType;
            public string recommendation;
        }

        [Serializable]
        public class ModelAnalysis
        {
            public string path;
            public int vertexCount;
            public int triangleCount;
            public int submeshCount;
            public int materialCount;
            public bool hasAnimations;
            public float fileSize;
            public string recommendation;
        }

        // ===================== VARIÁVEIS DE ESTADO =====================

        private List<AssetReport> problemAssets = new List<AssetReport>();
        private List<TextureAnalysis> textureProblems = new List<TextureAnalysis>();
        private List<ModelAnalysis> modelProblems = new List<ModelAnalysis>();
        private Dictionary<string, List<string>> duplicateAssets = new Dictionary<string, List<string>>();

        private bool scanInProgress = false;
        private float scanProgress = 0f;
        private string currentScanStatus = "";

        // Thresholds configuráveis
        private int maxTextureSize = 2048;
        private int maxVertexCount = 65000;
        private long maxAssetSizeBytes = 50 * 1024 * 1024; // 50MB
        private int maxMaterialsPerModel = 5;

        // ===================== UNITY EDITOR WINDOW =====================

        [MenuItem("Torre Futuro/Asset Diagnostics")]
        public static void ShowWindow()
        {
            var window = GetWindow<UnityAssetDiagnostics>("Asset Diagnostics");
            window.minSize = new Vector2(800, 600);
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Unity Asset Database Diagnostic Tool", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            // Configurações
            DrawConfigurationSection();
            EditorGUILayout.Space();

            // Botões de ação
            DrawActionButtons();
            EditorGUILayout.Space();

            // Progress bar
            if (scanInProgress)
            {
                EditorGUI.ProgressBar(EditorGUILayout.GetControlRect(GUILayout.Height(20)),
                    scanProgress, currentScanStatus);
                EditorGUILayout.Space();
            }

            // Resultados
            DrawResultsSection();
        }

        private void DrawConfigurationSection()
        {
            EditorGUILayout.LabelField("Thresholds Configuration", EditorStyles.boldLabel);
            maxTextureSize = EditorGUILayout.IntField("Max Texture Size (px)", maxTextureSize);
            maxVertexCount = EditorGUILayout.IntField("Max Vertex Count", maxVertexCount);
            maxAssetSizeBytes = EditorGUILayout.LongField("Max Asset Size (bytes)", maxAssetSizeBytes);
            maxMaterialsPerModel = EditorGUILayout.IntField("Max Materials Per Model", maxMaterialsPerModel);
        }

        private void DrawActionButtons()
        {
            EditorGUILayout.BeginHorizontal();

            GUI.enabled = !scanInProgress;
            if (GUILayout.Button("Full Diagnostic Scan", GUILayout.Height(30)))
            {
                RunFullDiagnostic();
            }

            if (GUILayout.Button("Quick Scan (Critical Only)", GUILayout.Height(30)))
            {
                RunQuickScan();
            }

            GUI.enabled = problemAssets.Count > 0;
            if (GUILayout.Button("Auto-Fix All Issues", GUILayout.Height(30)))
            {
                AutoFixAllIssues();
            }

            if (GUILayout.Button("Export Report", GUILayout.Height(30)))
            {
                ExportReport();
            }

            GUI.enabled = true;
            EditorGUILayout.EndHorizontal();
        }

        private void DrawResultsSection()
        {
            if (problemAssets.Count == 0 && !scanInProgress)
            {
                EditorGUILayout.HelpBox("No scan performed yet. Click 'Full Diagnostic Scan' to start.",
                    MessageType.Info);
                return;
            }

            // Resumo
            EditorGUILayout.LabelField($"Found {problemAssets.Count} Issues", EditorStyles.boldLabel);

            int criticalCount = problemAssets.Count(p => p.severity == 4);
            int highCount = problemAssets.Count(p => p.severity == 3);
            int mediumCount = problemAssets.Count(p => p.severity == 2);
            int lowCount = problemAssets.Count(p => p.severity == 1);

            EditorGUILayout.LabelField($"Critical: {criticalCount} | High: {highCount} | Medium: {mediumCount} | Low: {lowCount}");
            EditorGUILayout.Space();

            // Lista de problemas
            foreach (var problem in problemAssets.OrderByDescending(p => p.severity))
            {
                DrawProblemItem(problem);
            }
        }

        private void DrawProblemItem(AssetReport problem)
        {
            Color bgColor = GUI.backgroundColor;

            // Colorir baseado na severidade
            switch (problem.severity)
            {
                case 4: GUI.backgroundColor = Color.red; break;
                case 3: GUI.backgroundColor = new Color(1f, 0.5f, 0f); break;
                case 2: GUI.backgroundColor = Color.yellow; break;
                default: GUI.backgroundColor = Color.green; break;
            }

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            GUI.backgroundColor = bgColor;

            EditorGUILayout.LabelField($"[{GetSeverityLabel(problem.severity)}] {problem.type}");
            EditorGUILayout.LabelField($"Path: {problem.path}", EditorStyles.miniLabel);
            EditorGUILayout.LabelField($"Issue: {problem.issue}", EditorStyles.wordWrappedMiniLabel);
            EditorGUILayout.LabelField($"Recommendation: {problem.recommendation}", EditorStyles.wordWrappedMiniLabel);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Select", GUILayout.Width(60)))
            {
                Selection.activeObject = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(problem.path);
            }
            if (GUILayout.Button("Auto-Fix", GUILayout.Width(60)))
            {
                AutoFixAsset(problem);
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(2);
        }

        // ===================== SCANNING METHODS =====================

        private void RunFullDiagnostic()
        {
            scanInProgress = true;
            problemAssets.Clear();
            textureProblems.Clear();
            modelProblems.Clear();
            duplicateAssets.Clear();

            try
            {
                // 1. Scan for large files
                currentScanStatus = "Scanning for large files...";
                scanProgress = 0.1f;
                ScanForLargeFiles();

                // 2. Scan textures
                currentScanStatus = "Analyzing textures...";
                scanProgress = 0.3f;
                ScanTextures();

                // 3. Scan models
                currentScanStatus = "Analyzing 3D models...";
                scanProgress = 0.5f;
                Scan3DModels();

                // 4. Scan for duplicates
                currentScanStatus = "Detecting duplicate assets...";
                scanProgress = 0.7f;
                ScanForDuplicates();

                // 5. Scan for missing references
                currentScanStatus = "Checking references...";
                scanProgress = 0.85f;
                ScanForMissingReferences();

                // 6. Check import settings
                currentScanStatus = "Validating import settings...";
                scanProgress = 0.95f;
                CheckImportSettings();

                currentScanStatus = "Scan complete!";
                scanProgress = 1f;
            }
            finally
            {
                scanInProgress = false;
                EditorUtility.ClearProgressBar();
            }
        }

        private void RunQuickScan()
        {
            scanInProgress = true;
            problemAssets.Clear();

            try
            {
                currentScanStatus = "Running critical checks...";
                scanProgress = 0.5f;

                // Apenas verificações críticas
                ScanForLargeFiles();
                CheckCriticalTextures();
                CheckCriticalModels();

                currentScanStatus = "Quick scan complete!";
                scanProgress = 1f;
            }
            finally
            {
                scanInProgress = false;
            }
        }

        private void ScanForLargeFiles()
        {
            string[] allAssets = AssetDatabase.GetAllAssetPaths();

            foreach (string assetPath in allAssets)
            {
                if (!assetPath.StartsWith("Assets/")) continue;

                FileInfo fileInfo = new FileInfo(assetPath);
                if (!fileInfo.Exists) continue;

                if (fileInfo.Length > maxAssetSizeBytes)
                {
                    problemAssets.Add(new AssetReport
                    {
                        path = assetPath,
                        type = "Large File",
                        sizeInBytes = fileInfo.Length,
                        issue = $"File size is {fileInfo.Length / (1024 * 1024)}MB",
                        recommendation = "Consider compressing or splitting this asset",
                        severity = fileInfo.Length > 100 * 1024 * 1024 ? 4 : 3
                    });
                }

                // Verificar se é arquivo compactado ainda no projeto
                string extension = Path.GetExtension(assetPath).ToLower();
                if (extension == ".zip" || extension == ".rar" || extension == ".7z")
                {
                    problemAssets.Add(new AssetReport
                    {
                        path = assetPath,
                        type = "Compressed Archive",
                        sizeInBytes = fileInfo.Length,
                        issue = "Compressed archives should not be in Unity project",
                        recommendation = "Extract contents and delete the archive",
                        severity = 4
                    });
                }
            }
        }

        private void ScanTextures()
        {
            string[] texturePaths = AssetDatabase.FindAssets("t:Texture2D")
                .Select(guid => AssetDatabase.GUIDToAssetPath(guid))
                .ToArray();

            foreach (string path in texturePaths)
            {
                Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
                if (texture == null) continue;

                TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
                if (importer == null) continue;

                // Verificar tamanho
                if (texture.width > maxTextureSize || texture.height > maxTextureSize)
                {
                    problemAssets.Add(new AssetReport
                    {
                        path = path,
                        type = "Oversized Texture",
                        sizeInBytes = Profiler.GetRuntimeMemorySizeLong(texture),
                        issue = $"Texture size is {texture.width}x{texture.height}",
                        recommendation = $"Resize to maximum {maxTextureSize}x{maxTextureSize}",
                        severity = 3
                    });
                }

                // Verificar compressão
                if (importer.textureCompression == TextureImporterCompression.Uncompressed)
                {
                    problemAssets.Add(new AssetReport
                    {
                        path = path,
                        type = "Uncompressed Texture",
                        sizeInBytes = Profiler.GetRuntimeMemorySizeLong(texture),
                        issue = "Texture is not compressed",
                        recommendation = "Enable texture compression (DXT/ETC/ASTC)",
                        severity = 3
                    });
                }

                // Verificar Read/Write
                if (importer.isReadable && !path.Contains("UI"))
                {
                    problemAssets.Add(new AssetReport
                    {
                        path = path,
                        type = "Readable Texture",
                        sizeInBytes = Profiler.GetRuntimeMemorySizeLong(texture),
                        issue = "Read/Write enabled doubles memory usage",
                        recommendation = "Disable Read/Write unless necessary",
                        severity = 2
                    });
                }

                // Verificar formato não otimizado
                if (texture.format == TextureFormat.RGBA32 || texture.format == TextureFormat.ARGB32)
                {
                    problemAssets.Add(new AssetReport
                    {
                        path = path,
                        type = "Unoptimized Format",
                        sizeInBytes = Profiler.GetRuntimeMemorySizeLong(texture),
                        issue = $"Using uncompressed format: {texture.format}",
                        recommendation = "Use compressed formats (DXT5, ETC2, ASTC)",
                        severity = 2
                    });
                }
            }
        }

        private void Scan3DModels()
        {
            string[] modelPaths = AssetDatabase.FindAssets("t:GameObject")
                .Select(guid => AssetDatabase.GUIDToAssetPath(guid))
                .Where(path => path.EndsWith(".fbx") || path.EndsWith(".obj") ||
                              path.EndsWith(".3ds") || path.EndsWith(".dae"))
                .ToArray();

            foreach (string path in modelPaths)
            {
                GameObject model = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (model == null) continue;

                ModelImporter importer = AssetImporter.GetAtPath(path) as ModelImporter;
                if (importer == null) continue;

                // Contar vértices e triângulos
                int totalVertices = 0;
                int totalTriangles = 0;
                MeshFilter[] meshFilters = model.GetComponentsInChildren<MeshFilter>();

                foreach (var mf in meshFilters)
                {
                    if (mf.sharedMesh != null)
                    {
                        totalVertices += mf.sharedMesh.vertexCount;
                        totalTriangles += mf.sharedMesh.triangles.Length / 3;
                    }
                }

                // Verificar complexidade
                if (totalVertices > maxVertexCount)
                {
                    problemAssets.Add(new AssetReport
                    {
                        path = path,
                        type = "High-Poly Model",
                        sizeInBytes = new FileInfo(path).Length,
                        issue = $"Model has {totalVertices} vertices",
                        recommendation = "Reduce polygon count or create LODs",
                        severity = 3
                    });
                }

                // Verificar materiais
                Renderer[] renderers = model.GetComponentsInChildren<Renderer>();
                HashSet<Material> uniqueMaterials = new HashSet<Material>();

                foreach (var renderer in renderers)
                {
                    foreach (var mat in renderer.sharedMaterials)
                    {
                        if (mat != null) uniqueMaterials.Add(mat);
                    }
                }

                if (uniqueMaterials.Count > maxMaterialsPerModel)
                {
                    problemAssets.Add(new AssetReport
                    {
                        path = path,
                        type = "Too Many Materials",
                        sizeInBytes = new FileInfo(path).Length,
                        issue = $"Model uses {uniqueMaterials.Count} materials",
                        recommendation = "Combine materials using texture atlases",
                        severity = 2
                    });
                }

                // Verificar Read/Write em meshes
                if (importer.isReadable)
                {
                    problemAssets.Add(new AssetReport
                    {
                        path = path,
                        type = "Readable Mesh",
                        sizeInBytes = new FileInfo(path).Length,
                        issue = "Mesh Read/Write enabled doubles memory",
                        recommendation = "Disable Read/Write unless needed for runtime modification",
                        severity = 2
                    });
                }
            }
        }

        private void ScanForDuplicates()
        {
            Dictionary<string, List<string>> filesByHash = new Dictionary<string, List<string>>();
            string[] allAssets = AssetDatabase.GetAllAssetPaths();

            foreach (string assetPath in allAssets)
            {
                if (!assetPath.StartsWith("Assets/")) continue;
                if (Directory.Exists(assetPath)) continue;

                FileInfo fileInfo = new FileInfo(assetPath);
                if (!fileInfo.Exists) continue;

                // Calcular hash simples baseado em tamanho e nome
                string hash = $"{fileInfo.Length}_{Path.GetFileName(assetPath)}";

                if (!filesByHash.ContainsKey(hash))
                    filesByHash[hash] = new List<string>();

                filesByHash[hash].Add(assetPath);
            }

            // Identificar duplicatas
            foreach (var kvp in filesByHash)
            {
                if (kvp.Value.Count > 1)
                {
                    duplicateAssets[kvp.Key] = kvp.Value;

                    problemAssets.Add(new AssetReport
                    {
                        path = string.Join(", ", kvp.Value),
                        type = "Duplicate Assets",
                        sizeInBytes = new FileInfo(kvp.Value[0]).Length * (kvp.Value.Count - 1),
                        issue = $"Found {kvp.Value.Count} duplicate files",
                        recommendation = "Remove duplicates and use references",
                        severity = 2
                    });
                }
            }
        }

        private void ScanForMissingReferences()
        {
            string[] prefabPaths = AssetDatabase.FindAssets("t:Prefab")
                .Select(guid => AssetDatabase.GUIDToAssetPath(guid))
                .ToArray();

            foreach (string path in prefabPaths)
            {
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab == null) continue;

                // Verificar componentes missing
                Component[] components = prefab.GetComponentsInChildren<Component>(true);
                bool hasMissing = components.Any(c => c == null);

                if (hasMissing)
                {
                    problemAssets.Add(new AssetReport
                    {
                        path = path,
                        type = "Missing Components",
                        sizeInBytes = 0,
                        issue = "Prefab has missing script references",
                        recommendation = "Fix or remove missing components",
                        severity = 3
                    });
                }
            }
        }

        private void CheckImportSettings()
        {
            // Verificar configurações do projeto
            if (!EditorSettings.cacheServerMode.Equals("Local"))
            {
                problemAssets.Add(new AssetReport
                {
                    path = "Project Settings",
                    type = "Cache Settings",
                    sizeInBytes = 0,
                    issue = "Asset Pipeline Cache not configured",
                    recommendation = "Enable Local Cache Server for faster imports",
                    severity = 2
                });
            }
        }

        private void CheckCriticalTextures()
        {
            string[] texturePaths = AssetDatabase.FindAssets("t:Texture2D")
                .Select(guid => AssetDatabase.GUIDToAssetPath(guid))
                .Where(p => new FileInfo(p).Length > 10 * 1024 * 1024) // Só texturas > 10MB
                .ToArray();

            foreach (string path in texturePaths)
            {
                Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
                if (texture != null && (texture.width > 4096 || texture.height > 4096))
                {
                    problemAssets.Add(new AssetReport
                    {
                        path = path,
                        type = "Critical: Huge Texture",
                        sizeInBytes = new FileInfo(path).Length,
                        issue = $"Texture is {texture.width}x{texture.height} - SEVERELY impacting performance",
                        recommendation = "URGENT: Resize to 2048x2048 or smaller",
                        severity = 4
                    });
                }
            }
        }

        private void CheckCriticalModels()
        {
            string[] modelPaths = AssetDatabase.FindAssets("t:GameObject")
                .Select(guid => AssetDatabase.GUIDToAssetPath(guid))
                .Where(path => path.EndsWith(".fbx") && new FileInfo(path).Length > 20 * 1024 * 1024)
                .ToArray();

            foreach (string path in modelPaths)
            {
                problemAssets.Add(new AssetReport
                {
                    path = path,
                    type = "Critical: Large Model",
                    sizeInBytes = new FileInfo(path).Length,
                    issue = "Model file is extremely large",
                    recommendation = "URGENT: Optimize or split model",
                    severity = 4
                });
            }
        }

        // ===================== AUTO-FIX METHODS =====================

        private void AutoFixAllIssues()
        {
            if (!EditorUtility.DisplayDialog("Auto-Fix All Issues",
                "This will automatically fix all issues. This may take several minutes. Continue?",
                "Yes", "Cancel"))
            {
                return;
            }

            int fixedCount = 0;
            int totalCount = problemAssets.Count;

            foreach (var problem in problemAssets.ToList())
            {
                EditorUtility.DisplayProgressBar("Auto-Fixing Issues",
                    $"Fixing {problem.type}: {problem.path}",
                    (float)fixedCount / totalCount);

                if (AutoFixAsset(problem))
                {
                    fixedCount++;
                }
            }

            EditorUtility.ClearProgressBar();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog("Auto-Fix Complete",
                $"Successfully fixed {fixedCount} out of {totalCount} issues.",
                "OK");

            // Re-scan para atualizar lista
            RunQuickScan();
        }

        private bool AutoFixAsset(AssetReport problem)
        {
            try
            {
                switch (problem.type)
                {
                    case "Oversized Texture":
                    case "Critical: Huge Texture":
                        return FixOversizedTexture(problem.path);

                    case "Uncompressed Texture":
                        return FixUncompressedTexture(problem.path);

                    case "Readable Texture":
                        return FixReadableTexture(problem.path);

                    case "Unoptimized Format":
                        return FixTextureFormat(problem.path);

                    case "High-Poly Model":
                        return FixHighPolyModel(problem.path);

                    case "Readable Mesh":
                        return FixReadableMesh(problem.path);

                    case "Too Many Materials":
                        Debug.Log($"Material optimization for {problem.path} requires manual intervention");
                        return false;

                    case "Compressed Archive":
                        return DeleteCompressedArchive(problem.path);

                    default:
                        Debug.Log($"No auto-fix available for {problem.type}");
                        return false;
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to fix {problem.path}: {e.Message}");
                return false;
            }
        }

        private bool FixOversizedTexture(string path)
        {
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null) return false;

            importer.maxTextureSize = maxTextureSize;
            importer.SaveAndReimport();

            Debug.Log($"Resized texture: {path} to {maxTextureSize}x{maxTextureSize}");
            return true;
        }

        private bool FixUncompressedTexture(string path)
        {
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null) return false;

            importer.textureCompression = TextureImporterCompression.Compressed;
            importer.SaveAndReimport();

            Debug.Log($"Enabled compression for texture: {path}");
            return true;
        }

        private bool FixReadableTexture(string path)
        {
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null) return false;

            importer.isReadable = false;
            importer.SaveAndReimport();

            Debug.Log($"Disabled Read/Write for texture: {path}");
            return true;
        }

        private bool FixTextureFormat(string path)
        {
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null) return false;

            var platformSettings = importer.GetPlatformTextureSettings("Standalone");
            platformSettings.overridden = true;
            platformSettings.format = TextureImporterFormat.DXT5;
            importer.SetPlatformTextureSettings(platformSettings);
            importer.SaveAndReimport();

            Debug.Log($"Optimized texture format: {path}");
            return true;
        }

        private bool FixHighPolyModel(string path)
        {
            ModelImporter importer = AssetImporter.GetAtPath(path) as ModelImporter;
            if (importer == null) return false;

            // Configurar LODs se possível
            importer.optimizeMesh = true;
            importer.meshCompression = ModelImporterMeshCompression.High;
            importer.SaveAndReimport();

            Debug.Log($"Optimized model: {path}");
            return true;
        }

        private bool FixReadableMesh(string path)
        {
            ModelImporter importer = AssetImporter.GetAtPath(path) as ModelImporter;
            if (importer == null) return false;

            importer.isReadable = false;
            importer.SaveAndReimport();

            Debug.Log($"Disabled Read/Write for mesh: {path}");
            return true;
        }

        private bool DeleteCompressedArchive(string path)
        {
            if (EditorUtility.DisplayDialog("Delete Archive",
                $"Delete compressed archive: {path}?",
                "Delete", "Skip"))
            {
                AssetDatabase.DeleteAsset(path);
                Debug.Log($"Deleted archive: {path}");
                return true;
            }
            return false;
        }

        // ===================== UTILITY METHODS =====================

        private string GetSeverityLabel(int severity)
        {
            switch (severity)
            {
                case 4: return "CRITICAL";
                case 3: return "HIGH";
                case 2: return "MEDIUM";
                case 1: return "LOW";
                default: return "INFO";
            }
        }

        private void ExportReport()
        {
            string reportPath = EditorUtility.SaveFilePanel(
                "Save Diagnostic Report",
                Application.dataPath,
                "AssetDiagnosticReport_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".txt",
                "txt");

            if (string.IsNullOrEmpty(reportPath)) return;

            using (StreamWriter writer = new StreamWriter(reportPath))
            {
                writer.WriteLine("=== UNITY ASSET DIAGNOSTIC REPORT ===");
                writer.WriteLine($"Generated: {DateTime.Now}");
                writer.WriteLine($"Project: {Application.productName}");
                writer.WriteLine($"Unity Version: {Application.unityVersion}");
                writer.WriteLine();

                writer.WriteLine($"Total Issues Found: {problemAssets.Count}");
                writer.WriteLine($"Critical: {problemAssets.Count(p => p.severity == 4)}");
                writer.WriteLine($"High: {problemAssets.Count(p => p.severity == 3)}");
                writer.WriteLine($"Medium: {problemAssets.Count(p => p.severity == 2)}");
                writer.WriteLine($"Low: {problemAssets.Count(p => p.severity == 1)}");
                writer.WriteLine();

                writer.WriteLine("=== DETAILED ISSUES ===");
                writer.WriteLine();

                foreach (var problem in problemAssets.OrderByDescending(p => p.severity))
                {
                    writer.WriteLine($"[{GetSeverityLabel(problem.severity)}] {problem.type}");
                    writer.WriteLine($"  Path: {problem.path}");
                    writer.WriteLine($"  Size: {problem.sizeInBytes / (1024 * 1024)}MB");
                    writer.WriteLine($"  Issue: {problem.issue}");
                    writer.WriteLine($"  Recommendation: {problem.recommendation}");
                    writer.WriteLine();
                }

                // Seção de duplicatas
                if (duplicateAssets.Count > 0)
                {
                    writer.WriteLine("=== DUPLICATE ASSETS ===");
                    foreach (var dup in duplicateAssets)
                    {
                        writer.WriteLine($"Duplicate Group:");
                        foreach (var file in dup.Value)
                        {
                            writer.WriteLine($"  - {file}");
                        }
                        writer.WriteLine();
                    }
                }
            }

            Debug.Log($"Report exported to: {reportPath}");
            EditorUtility.RevealInFinder(reportPath);
        }
    }
}