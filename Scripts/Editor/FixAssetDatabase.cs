using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using UnityEngine;
using UnityEditor;

namespace TorreFuturo.Emergency
{
    /// <summary>
    /// Script de Emergência para Resolver Problemas de Carregamento do Asset Database
    /// Executa múltiplas estratégias para recuperar o projeto
    /// </summary>
    public class FixAssetDatabase : EditorWindow
    {
        private enum FixStrategy
        {
            QuickFix,        // Correções rápidas
            DeepClean,       // Limpeza profunda
            Rebuild,         // Reconstrução completa
            Nuclear          // Opção nuclear - deletar tudo e reconstruir
        }

        private FixStrategy currentStrategy = FixStrategy.QuickFix;
        private bool isProcessing = false;
        private string statusMessage = "";
        private float progress = 0f;
        private List<string> actionLog = new List<string>();

        // ===================== WINDOW SETUP =====================

        [MenuItem("Torre Futuro/Emergency Fix Asset Database", priority = -100)]
        [MenuItem("Assets/Fix Asset Database Loading Issues", priority = -100)]
        public static void ShowWindow()
        {
            var window = GetWindow<FixAssetDatabase>("FIX ASSET DATABASE");
            window.minSize = new Vector2(600, 400);
            window.ShowUtility(); // Mostra como janela prioritária
        }

        private void OnGUI()
        {
            DrawHeader();
            DrawStrategySelection();
            DrawActionButtons();
            DrawProgressBar();
            DrawActionLog();
            DrawEmergencyActions();
        }

        private void DrawHeader()
        {
            EditorGUILayout.Space(10);

            GUIStyle titleStyle = new GUIStyle(EditorStyles.boldLabel);
            titleStyle.fontSize = 16;
            titleStyle.alignment = TextAnchor.MiddleCenter;

            EditorGUILayout.LabelField("ASSET DATABASE EMERGENCY FIX", titleStyle);

            EditorGUILayout.Space(5);

            EditorGUILayout.HelpBox(
                "Este utilitário resolve problemas críticos de carregamento do Asset Database. " +
                "Selecione uma estratégia baseada na severidade do problema.",
                MessageType.Warning);

            EditorGUILayout.Space(10);
        }

        private void DrawStrategySelection()
        {
            EditorGUILayout.LabelField("Estratégia de Correção", EditorStyles.boldLabel);

            currentStrategy = (FixStrategy)EditorGUILayout.EnumPopup("Selecione:", currentStrategy);

            string description = currentStrategy switch
            {
                FixStrategy.QuickFix => "Correções rápidas: Limpa cache, corrige meta files, remove temporários",
                FixStrategy.DeepClean => "Limpeza profunda: Reimporta assets problemáticos, otimiza configurações",
                FixStrategy.Rebuild => "Reconstrução: Deleta Library, força reimportação completa",
                FixStrategy.Nuclear => "NUCLEAR: Remove TUDO exceto Assets e ProjectSettings, reconstrução total",
                _ => ""
            };

            EditorGUILayout.HelpBox(description, MessageType.Info);
            EditorGUILayout.Space(10);
        }

        private void DrawActionButtons()
        {
            EditorGUILayout.BeginHorizontal();

            GUI.enabled = !isProcessing;

            // Botão principal com cor baseada na estratégia
            Color originalColor = GUI.backgroundColor;

            GUI.backgroundColor = currentStrategy switch
            {
                FixStrategy.Nuclear => Color.red,
                FixStrategy.Rebuild => new Color(1f, 0.5f, 0f),
                FixStrategy.DeepClean => Color.yellow,
                _ => Color.green
            };

            if (GUILayout.Button($"EXECUTAR {currentStrategy}", GUILayout.Height(40)))
            {
                if (currentStrategy == FixStrategy.Nuclear)
                {
                    if (EditorUtility.DisplayDialog("AVISO CRÍTICO",
                        "A opção NUCLEAR irá DELETAR completamente Library, Temp, e outras pastas. " +
                        "Isso forçará uma reimportação COMPLETA de TODOS os assets. " +
                        "Este processo pode levar HORAS.\n\n" +
                        "Tem certeza ABSOLUTA?",
                        "SIM, EXECUTAR NUCLEAR",
                        "CANCELAR"))
                    {
                        ExecuteStrategy(currentStrategy);
                    }
                }
                else
                {
                    ExecuteStrategy(currentStrategy);
                }
            }

            GUI.backgroundColor = originalColor;

            if (GUILayout.Button("Analisar Problema", GUILayout.Height(40), GUILayout.Width(150)))
            {
                AnalyzeProblem();
            }

            GUI.enabled = true;
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(10);
        }

        private void DrawProgressBar()
        {
            if (isProcessing)
            {
                EditorGUI.ProgressBar(
                    EditorGUILayout.GetControlRect(GUILayout.Height(20)),
                    progress,
                    statusMessage);

                EditorGUILayout.Space(5);
            }
        }

        private void DrawActionLog()
        {
            if (actionLog.Count > 0)
            {
                EditorGUILayout.LabelField("Log de Ações:", EditorStyles.boldLabel);

                // Scroll view para o log
                GUILayout.BeginScrollView(Vector2.zero, GUILayout.Height(150));
                foreach (string log in actionLog.TakeLast(20))
                {
                    EditorGUILayout.LabelField(log, EditorStyles.miniLabel);
                }
                GUILayout.EndScrollView();
            }
        }

        private void DrawEmergencyActions()
        {
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Ações de Emergência", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Forçar Salvamento"))
            {
                AssetDatabase.SaveAssets();
                EditorUtility.DisplayDialog("Sucesso", "Assets salvos com sucesso!", "OK");
            }

            if (GUILayout.Button("Limpar Console"))
            {
                var logEntries = System.Type.GetType("UnityEditor.LogEntries, UnityEditor.dll");
                var clearMethod = logEntries.GetMethod("Clear", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
                clearMethod.Invoke(null, null);
            }

            if (GUILayout.Button("Abrir Project Settings"))
            {
                EditorApplication.ExecuteMenuItem("Edit/Project Settings...");
            }

            EditorGUILayout.EndHorizontal();
        }

        // ===================== ANÁLISE DO PROBLEMA =====================

        private void AnalyzeProblem()
        {
            actionLog.Clear();
            actionLog.Add($"[{DateTime.Now:HH:mm:ss}] Iniciando análise do problema...");

            try
            {
                // 1. Verificar espaço em disco
                string projectPath = Application.dataPath.Replace("/Assets", "");
                DriveInfo driveInfo = new DriveInfo(Path.GetPathRoot(projectPath));
                long freeSpace = driveInfo.AvailableFreeSpace / (1024 * 1024 * 1024); // GB

                actionLog.Add($"Espaço livre em disco: {freeSpace} GB");
                if (freeSpace < 10)
                {
                    actionLog.Add("⚠️ AVISO: Pouco espaço em disco! Isso pode causar problemas.");
                }

                // 2. Verificar Library
                string libraryPath = Path.Combine(projectPath, "Library");
                if (Directory.Exists(libraryPath))
                {
                    long librarySize = GetDirectorySize(libraryPath) / (1024 * 1024); // MB
                    actionLog.Add($"Tamanho da Library: {librarySize} MB");

                    if (librarySize > 5000)
                    {
                        actionLog.Add("⚠️ Library muito grande! Considere limpeza profunda.");
                    }
                }

                // 3. Verificar arquivos problemáticos
                string[] problematicExtensions = { ".zip", ".rar", ".7z", ".tar", ".gz" };
                int problematicCount = 0;

                foreach (string ext in problematicExtensions)
                {
                    string[] files = Directory.GetFiles(Application.dataPath, "*" + ext, SearchOption.AllDirectories);
                    problematicCount += files.Length;

                    if (files.Length > 0)
                    {
                        actionLog.Add($"Encontrados {files.Length} arquivos {ext}");
                    }
                }

                if (problematicCount > 0)
                {
                    actionLog.Add($"⚠️ CRÍTICO: {problematicCount} arquivos compactados encontrados!");
                }

                // 4. Verificar meta files órfãos
                int orphanedMeta = 0;
                string[] metaFiles = Directory.GetFiles(Application.dataPath, "*.meta", SearchOption.AllDirectories);

                foreach (string meta in metaFiles)
                {
                    string assetPath = meta.Replace(".meta", "");
                    if (!File.Exists(assetPath) && !Directory.Exists(assetPath))
                    {
                        orphanedMeta++;
                    }
                }

                if (orphanedMeta > 0)
                {
                    actionLog.Add($"Encontrados {orphanedMeta} arquivos .meta órfãos");
                }

                // 5. Verificar configurações do Editor
                if (!EditorSettings.cacheServerMode.Equals("Local"))
                {
                    actionLog.Add("Cache Server não está configurado como Local");
                }

                // Diagnóstico final
                actionLog.Add("----------------------------------------");
                if (problematicCount > 0 || librarySize > 5000)
                {
                    actionLog.Add("🔴 RECOMENDAÇÃO: Use 'Deep Clean' ou 'Rebuild'");
                }
                else if (orphanedMeta > 10)
                {
                    actionLog.Add("🟡 RECOMENDAÇÃO: Use 'Quick Fix'");
                }
                else
                {
                    actionLog.Add("🟢 Nenhum problema crítico detectado");
                }

                actionLog.Add($"[{DateTime.Now:HH:mm:ss}] Análise concluída");
            }
            catch (Exception e)
            {
                actionLog.Add($"❌ Erro na análise: {e.Message}");
            }

            Repaint();
        }

        // ===================== EXECUÇÃO DAS ESTRATÉGIAS =====================

        private void ExecuteStrategy(FixStrategy strategy)
        {
            isProcessing = true;
            actionLog.Clear();
            progress = 0f;

            EditorApplication.delayCall += () =>
            {
                try
                {
                    switch (strategy)
                    {
                        case FixStrategy.QuickFix:
                            ExecuteQuickFix();
                            break;
                        case FixStrategy.DeepClean:
                            ExecuteDeepClean();
                            break;
                        case FixStrategy.Rebuild:
                            ExecuteRebuild();
                            break;
                        case FixStrategy.Nuclear:
                            ExecuteNuclear();
                            break;
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"Erro ao executar estratégia: {e}");
                    actionLog.Add($"❌ ERRO: {e.Message}");
                }
                finally
                {
                    isProcessing = false;
                    progress = 1f;
                    statusMessage = "Processo concluído!";
                    Repaint();
                }
            };
        }

        private void ExecuteQuickFix()
        {
            string projectPath = Application.dataPath.Replace("/Assets", "");

            // 1. Limpar Temp
            UpdateProgress(0.1f, "Limpando arquivos temporários...");
            string tempPath = Path.Combine(projectPath, "Temp");
            if (Directory.Exists(tempPath))
            {
                try
                {
                    Directory.Delete(tempPath, true);
                    actionLog.Add("✓ Pasta Temp removida");
                }
                catch
                {
                    actionLog.Add("⚠️ Não foi possível remover Temp (pode estar em uso)");
                }
            }

            // 2. Limpar meta files órfãos
            UpdateProgress(0.3f, "Removendo meta files órfãos...");
            int removedMeta = 0;
            string[] metaFiles = Directory.GetFiles(Application.dataPath, "*.meta", SearchOption.AllDirectories);

            foreach (string meta in metaFiles)
            {
                string assetPath = meta.Replace(".meta", "");
                if (!File.Exists(assetPath) && !Directory.Exists(assetPath))
                {
                    File.Delete(meta);
                    removedMeta++;
                }
            }

            if (removedMeta > 0)
            {
                actionLog.Add($"✓ {removedMeta} meta files órfãos removidos");
            }

            // 3. Limpar cache do Asset Database
            UpdateProgress(0.5f, "Limpando cache do Asset Database...");
            string cacheDbPath = Path.Combine(projectPath, "Library", "AssetDatabase3");
            if (File.Exists(cacheDbPath))
            {
                try
                {
                    File.Delete(cacheDbPath);
                    actionLog.Add("✓ Cache do AssetDatabase limpo");
                }
                catch
                {
                    actionLog.Add("⚠️ Não foi possível limpar cache (em uso)");
                }
            }

            // 4. Otimizar import settings
            UpdateProgress(0.7f, "Otimizando configurações de importação...");
            EditorSettings.cacheServerMode = CacheServerMode.Local;
            EditorSettings.asyncShaderCompilation = true;
            actionLog.Add("✓ Configurações otimizadas");

            // 5. Forçar refresh
            UpdateProgress(0.9f, "Atualizando Asset Database...");
            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);

            actionLog.Add("✓ Quick Fix concluído!");
        }

        private void ExecuteDeepClean()
        {
            string projectPath = Application.dataPath.Replace("/Assets", "");

            // 1. Executar Quick Fix primeiro
            UpdateProgress(0.1f, "Executando correções rápidas...");
            ExecuteQuickFix();

            // 2. Limpar ShaderCache
            UpdateProgress(0.3f, "Limpando shader cache...");
            string shaderCachePath = Path.Combine(projectPath, "Library", "ShaderCache");
            if (Directory.Exists(shaderCachePath))
            {
                try
                {
                    Directory.Delete(shaderCachePath, true);
                    actionLog.Add("✓ Shader cache removido");
                }
                catch
                {
                    actionLog.Add("⚠️ Não foi possível remover shader cache");
                }
            }

            // 3. Reimportar assets problemáticos
            UpdateProgress(0.5f, "Reimportando assets grandes...");

            string[] largeAssets = AssetDatabase.FindAssets("t:Texture2D")
                .Select(guid => AssetDatabase.GUIDToAssetPath(guid))
                .Where(path => new FileInfo(path).Length > 5 * 1024 * 1024) // > 5MB
                .ToArray();

            foreach (string asset in largeAssets)
            {
                AssetDatabase.ImportAsset(asset, ImportAssetOptions.ForceUpdate);
            }

            actionLog.Add($"✓ {largeAssets.Length} assets grandes reimportados");

            // 4. Compactar Asset Database
            UpdateProgress(0.7f, "Compactando Asset Database...");
            System.Type assetDatabaseType = typeof(AssetDatabase);
            var method = assetDatabaseType.GetMethod("CompactDatabase",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);

            if (method != null)
            {
                method.Invoke(null, null);
                actionLog.Add("✓ Asset Database compactado");
            }

            // 5. Limpar caches adicionais
            UpdateProgress(0.9f, "Limpando caches adicionais...");
            string[] cacheFolders = {
                "Library/Artifacts",
                "Library/BuildCache",
                "Library/StateCache"
            };

            foreach (string folder in cacheFolders)
            {
                string fullPath = Path.Combine(projectPath, folder);
                if (Directory.Exists(fullPath))
                {
                    try
                    {
                        Directory.Delete(fullPath, true);
                        actionLog.Add($"✓ {folder} removido");
                    }
                    catch
                    {
                        actionLog.Add($"⚠️ Não foi possível remover {folder}");
                    }
                }
            }

            actionLog.Add("✓ Deep Clean concluído!");
        }

        private void ExecuteRebuild()
        {
            if (!EditorUtility.DisplayDialog("Confirmar Rebuild",
                "Isso deletará a pasta Library completamente e forçará reimportação total. " +
                "O processo pode levar de 30 minutos a várias horas.\n\n" +
                "Continuar?",
                "Sim, Rebuild",
                "Cancelar"))
            {
                isProcessing = false;
                return;
            }

            string projectPath = Application.dataPath.Replace("/Assets", "");

            // 1. Salvar tudo
            UpdateProgress(0.1f, "Salvando projeto...");
            AssetDatabase.SaveAssets();
            EditorApplication.SaveProject();

            // 2. Deletar Library
            UpdateProgress(0.3f, "Removendo pasta Library...");
            string libraryPath = Path.Combine(projectPath, "Library");

            if (Directory.Exists(libraryPath))
            {
                try
                {
                    Directory.Delete(libraryPath, true);
                    actionLog.Add("✓ Library completamente removida");
                }
                catch (Exception e)
                {
                    actionLog.Add($"❌ Erro ao remover Library: {e.Message}");
                    actionLog.Add("Feche o Unity e delete manualmente");
                    return;
                }
            }

            // 3. Deletar Temp
            UpdateProgress(0.5f, "Removendo Temp...");
            string tempPath = Path.Combine(projectPath, "Temp");
            if (Directory.Exists(tempPath))
            {
                try
                {
                    Directory.Delete(tempPath, true);
                    actionLog.Add("✓ Temp removido");
                }
                catch
                {
                    actionLog.Add("⚠️ Temp não pôde ser removido");
                }
            }

            // 4. Criar arquivo de sinalização
            UpdateProgress(0.7f, "Preparando para reimportação...");
            File.WriteAllText(Path.Combine(projectPath, "REBUILD_IN_PROGRESS.txt"),
                $"Rebuild iniciado em: {DateTime.Now}\n" +
                "NÃO DELETE ESTE ARQUIVO!\n" +
                "Ele será removido automaticamente quando o processo terminar.");

            actionLog.Add("✓ Rebuild preparado");

            // 5. Solicitar restart
            UpdateProgress(0.9f, "Preparando restart...");
            actionLog.Add("⚠️ IMPORTANTE: Reinicie o Unity agora!");
            actionLog.Add("O Unity reconstruirá todo o projeto ao abrir");

            EditorUtility.DisplayDialog("Ação Necessária",
                "Rebuild preparado com sucesso!\n\n" +
                "PRÓXIMOS PASSOS:\n" +
                "1. Feche o Unity completamente\n" +
                "2. Abra o projeto novamente\n" +
                "3. Aguarde a reimportação completa (pode levar horas)\n" +
                "4. NÃO interrompa o processo!",
                "Entendi");
        }

        private void ExecuteNuclear()
        {
            // Segunda confirmação para Nuclear
            if (!EditorUtility.DisplayDialog("ÚLTIMA CONFIRMAÇÃO",
                "NUCLEAR removerá:\n" +
                "• Library (completamente)\n" +
                "• Temp (completamente)\n" +
                "• Logs\n" +
                "• obj\n" +
                "• Build outputs\n\n" +
                "Isso é IRREVERSÍVEL e levará MUITO tempo para reconstruir.\n\n" +
                "TEM CERTEZA ABSOLUTA?",
                "SIM, EXECUTAR NUCLEAR",
                "CANCELAR"))
            {
                isProcessing = false;
                return;
            }

            string projectPath = Application.dataPath.Replace("/Assets", "");

            // 1. Backup crítico
            UpdateProgress(0.05f, "Criando backup de emergência...");
            string backupPath = Path.Combine(projectPath, $"BACKUP_BEFORE_NUCLEAR_{DateTime.Now:yyyyMMdd_HHmmss}");
            Directory.CreateDirectory(backupPath);

            // Copiar ProjectSettings
            string settingsSource = Path.Combine(projectPath, "ProjectSettings");
            string settingsDest = Path.Combine(backupPath, "ProjectSettings");
            CopyDirectory(settingsSource, settingsDest);

            actionLog.Add($"✓ Backup criado em: {backupPath}");

            // 2. Salvar absolutamente tudo
            UpdateProgress(0.1f, "Salvando tudo...");
            AssetDatabase.SaveAssets();
            EditorApplication.SaveProject();

            // 3. NUCLEAR - Deletar tudo exceto essenciais
            string[] foldersToNuke = {
                "Library",
                "Temp",
                "Logs",
                "obj",
                "Build",
                "Builds",
                ".vs",
                ".idea",
                "UserSettings"
            };

            float progressStep = 0.8f / foldersToNuke.Length;
            float currentProgress = 0.1f;

            foreach (string folder in foldersToNuke)
            {
                currentProgress += progressStep;
                UpdateProgress(currentProgress, $"Removendo {folder}...");

                string fullPath = Path.Combine(projectPath, folder);
                if (Directory.Exists(fullPath))
                {
                    try
                    {
                        Directory.Delete(fullPath, true);
                        actionLog.Add($"✓ {folder} DESTRUÍDO");
                    }
                    catch (Exception e)
                    {
                        actionLog.Add($"❌ Falha ao destruir {folder}: {e.Message}");
                    }
                }
            }

            // 4. Limpar arquivos residuais
            UpdateProgress(0.95f, "Limpeza final...");
            string[] filesToDelete = Directory.GetFiles(projectPath, "*.csproj")
                .Concat(Directory.GetFiles(projectPath, "*.sln"))
                .Concat(Directory.GetFiles(projectPath, "*.suo"))
                .Concat(Directory.GetFiles(projectPath, "*.user"))
                .ToArray();

            foreach (string file in filesToDelete)
            {
                try
                {
                    File.Delete(file);
                }
                catch { }
            }

            // 5. Criar arquivo de sinalização NUCLEAR
            File.WriteAllText(Path.Combine(projectPath, "NUCLEAR_REBUILD_REQUIRED.txt"),
                $"NUCLEAR executado em: {DateTime.Now}\n" +
                $"Backup em: {backupPath}\n\n" +
                "INSTRUÇÕES:\n" +
                "1. FECHE O UNITY IMEDIATAMENTE\n" +
                "2. Abra o projeto novamente\n" +
                "3. Espere VÁRIAS HORAS para reimportação\n" +
                "4. NÃO INTERROMPA O PROCESSO!");

            actionLog.Add("🔴 NUCLEAR COMPLETO!");
            actionLog.Add($"Backup salvo em: {backupPath}");

            EditorUtility.DisplayDialog("NUCLEAR EXECUTADO",
                "NUCLEAR foi executado com sucesso!\n\n" +
                "AÇÃO IMEDIATA NECESSÁRIA:\n\n" +
                "1. FECHE O UNITY AGORA (File > Exit)\n" +
                "2. NÃO salve nada ao fechar\n" +
                "3. Reabra o projeto\n" +
                "4. Aguarde reconstrução completa (HORAS!)\n\n" +
                $"Backup de segurança em:\n{backupPath}",
                "FECHAR UNITY AGORA");

            EditorApplication.Exit(0);
        }

        // ===================== MÉTODOS AUXILIARES =====================

        private void UpdateProgress(float value, string message)
        {
            progress = value;
            statusMessage = message;
            actionLog.Add($"[{DateTime.Now:HH:mm:ss}] {message}");
            Repaint();

            // Forçar atualização da UI
            if (Event.current != null)
            {
                Event.current.Use();
            }
        }

        private long GetDirectorySize(string path)
        {
            try
            {
                return Directory.GetFiles(path, "*", SearchOption.AllDirectories)
                    .Sum(file => new FileInfo(file).Length);
            }
            catch
            {
                return 0;
            }
        }

        private void CopyDirectory(string sourceDir, string destDir)
        {
            Directory.CreateDirectory(destDir);

            foreach (string file in Directory.GetFiles(sourceDir))
            {
                string destFile = Path.Combine(destDir, Path.GetFileName(file));
                File.Copy(file, destFile, true);
            }

            foreach (string dir in Directory.GetDirectories(sourceDir))
            {
                string destSubDir = Path.Combine(destDir, Path.GetFileName(dir));
                CopyDirectory(dir, destSubDir);
            }
        }
    }

    /// <summary>
    /// Classe auxiliar para executar correções na inicialização
    /// </summary>
    [InitializeOnLoad]
    public static class StartupFixer
    {
        static StartupFixer()
        {
            // Verificar se há rebuild pendente
            string projectPath = Application.dataPath.Replace("/Assets", "");
            string rebuildFlag = Path.Combine(projectPath, "REBUILD_IN_PROGRESS.txt");
            string nuclearFlag = Path.Combine(projectPath, "NUCLEAR_REBUILD_REQUIRED.txt");

            if (File.Exists(nuclearFlag))
            {
                Debug.LogWarning("NUCLEAR REBUILD EM PROGRESSO - Aguarde conclusão completa!");
                File.Delete(nuclearFlag);
            }
            else if (File.Exists(rebuildFlag))
            {
                Debug.Log("Rebuild em progresso - Aguarde reimportação completa");
                File.Delete(rebuildFlag);
            }

            // Auto-otimizações na inicialização
            EditorApplication.delayCall += () =>
            {
                // Configurar cache local se não estiver
                if (!EditorSettings.cacheServerMode.Equals("Local"))
                {
                    EditorSettings.cacheServerMode = CacheServerMode.Local;
                    Debug.Log("Cache Server configurado para Local automaticamente");
                }

                // Habilitar compilação assíncrona de shaders
                if (!EditorSettings.asyncShaderCompilation)
                {
                    EditorSettings.asyncShaderCompilation = true;
                    Debug.Log("Compilação assíncrona de shaders habilitada");
                }
            };
        }
    }
}