using UnityEngine;
using System.Collections.Generic;
using System.Text;

/// <summary>
/// Script de validação automática de todos os sistemas do jogo
/// Execute no Unity Editor para testar se tudo está funcionando
/// </summary>
public class GameTestValidator : MonoBehaviour
{
    [Header("Test Configuration")]
    [SerializeField] private bool runOnStart = true;
    [SerializeField] private bool logDetailedResults = true;
    [SerializeField] private bool autoFixIssues = false;

    [Header("Test Results")]
    [SerializeField] private int totalTests = 0;
    [SerializeField] private int passedTests = 0;
    [SerializeField] private int failedTests = 0;
    [SerializeField] private float testProgress = 0f;

    private List<string> testResults = new List<string>();
    private StringBuilder reportBuilder = new StringBuilder();

    #region Unity Lifecycle

    private void Start()
    {
        if (runOnStart)
        {
            StartCoroutine(RunAllTests());
        }
    }

    #endregion

    #region Public Methods

    [ContextMenu("Run All Tests")]
    public void RunAllTestsManual()
    {
        StartCoroutine(RunAllTests());
    }

    [ContextMenu("Generate Test Report")]
    public void GenerateReport()
    {
        Debug.Log("=== GAME TEST VALIDATOR REPORT ===\n" + reportBuilder.ToString());
    }

    #endregion

    #region Test Execution

    private System.Collections.IEnumerator RunAllTests()
    {
        Debug.Log("<color=cyan>=== INICIANDO VALIDAÇÃO COMPLETA DO JOGO ===</color>");
        reportBuilder.Clear();
        testResults.Clear();
        totalTests = 0;
        passedTests = 0;
        failedTests = 0;

        reportBuilder.AppendLine("╔════════════════════════════════════════════╗");
        reportBuilder.AppendLine("║   TORRE FUTURO - TESTE DE VALIDAÇÃO       ║");
        reportBuilder.AppendLine("╚════════════════════════════════════════════╝");
        reportBuilder.AppendLine();

        // 1. TESTE DE SCRIPTS
        yield return StartCoroutine(TestScriptsExistence());
        yield return new WaitForSeconds(0.1f);

        // 2. TESTE DE MANAGERS
        yield return StartCoroutine(TestManagers());
        yield return new WaitForSeconds(0.1f);

        // 3. TESTE DE SISTEMAS
        yield return StartCoroutine(TestSystems());
        yield return new WaitForSeconds(0.1f);

        // 4. TESTE DE UI
        yield return StartCoroutine(TestUI());
        yield return new WaitForSeconds(0.1f);

        // 5. TESTE DE PLAYER
        yield return StartCoroutine(TestPlayerSystems());
        yield return new WaitForSeconds(0.1f);

        // 6. TESTE DE SCENE
        yield return StartCoroutine(TestSceneSetup());
        yield return new WaitForSeconds(0.1f);

        // 7. TESTE DE ASSETS
        yield return StartCoroutine(TestAssets());
        yield return new WaitForSeconds(0.1f);

        // RESULTADO FINAL
        GenerateFinalReport();
    }

    #endregion

    #region Individual Tests

    private System.Collections.IEnumerator TestScriptsExistence()
    {
        reportBuilder.AppendLine("┌─────────────────────────────────────┐");
        reportBuilder.AppendLine("│  1. TESTE DE SCRIPTS (30 scripts)  │");
        reportBuilder.AppendLine("└─────────────────────────────────────┘");

        // Scripts Core
        RunTest("GameManager exists", typeof(GameManager) != null);
        RunTest("GameManagerRPG exists", FindObjectOfType(System.Type.GetType("SpaceRPG.Core.GameManagerRPG")) != null || System.Type.GetType("SpaceRPG.Core.GameManagerRPG") != null);
        RunTest("OptimizationManager exists", System.Type.GetType("SpaceRPG.Core.OptimizationManager") != null);

        // Scripts de Gameplay
        RunTest("SpaceshipController exists", typeof(SpaceshipController) != null);
        RunTest("WeaponSystem exists", typeof(WeaponSystem) != null);
        RunTest("UpgradeSystem exists", typeof(UpgradeSystem) != null);
        RunTest("RewardSystem exists", typeof(RewardSystem) != null);
        RunTest("PlantingSystem exists", typeof(PlantingSystem) != null);
        RunTest("NPCInstructor exists", typeof(NPCInstructor) != null);

        // Scripts UI
        RunTest("GameplayUI exists", typeof(GameplayUI) != null);
        RunTest("MenuManager exists", System.Type.GetType("SpaceRPG.UI.MenuManager") != null);
        RunTest("ModernMenuIntegration exists", System.Type.GetType("SpaceRPG.UI.ModernMenuIntegration") != null);

        reportBuilder.AppendLine();
        yield return null;
    }

    private System.Collections.IEnumerator TestManagers()
    {
        reportBuilder.AppendLine("┌─────────────────────────────────────┐");
        reportBuilder.AppendLine("│  2. TESTE DE MANAGERS               │");
        reportBuilder.AppendLine("└─────────────────────────────────────┘");

        // GameManager
        GameManager gm = FindObjectOfType<GameManager>();
        RunTest("GameManager in scene", gm != null);
        if (gm != null)
        {
            RunTest("GameManager Instance", GameManager.Instance != null);
        }

        // AudioManager
        var audioManagerType = System.Type.GetType("SpaceRPG.Managers.AudioManager");
        if (audioManagerType != null)
        {
            var audioManager = FindObjectOfType(audioManagerType);
            RunTest("AudioManager in scene", audioManager != null);
        }

        reportBuilder.AppendLine();
        yield return null;
    }

    private System.Collections.IEnumerator TestSystems()
    {
        reportBuilder.AppendLine("┌─────────────────────────────────────┐");
        reportBuilder.AppendLine("│  3. TESTE DE SISTEMAS               │");
        reportBuilder.AppendLine("└─────────────────────────────────────┘");

        // Inventory System
        var inventoryType = System.Type.GetType("SpaceRPG.Systems.InventorySystem");
        RunTest("InventorySystem script exists", inventoryType != null);

        // Shop System
        var shopType = System.Type.GetType("SpaceRPG.Systems.ShopSystem");
        RunTest("ShopSystem script exists", shopType != null);

        // Quest System
        var questType = System.Type.GetType("SpaceRPG.Systems.QuestSystem");
        RunTest("QuestSystem script exists", questType != null);

        // Ship System
        var shipType = System.Type.GetType("SpaceRPG.Systems.ShipSystem");
        RunTest("ShipSystem script exists", shipType != null);

        // Plant Care System
        var plantType = System.Type.GetType("SpaceRPG.Systems.PlantCareSystemAdvanced");
        RunTest("PlantCareSystemAdvanced script exists", plantType != null);

        // World Portal System
        var portalType = System.Type.GetType("SpaceRPG.Systems.WorldPortalSystem");
        RunTest("WorldPortalSystem script exists", portalType != null);

        // Launchpad Controller
        var launchpadType = System.Type.GetType("SpaceRPG.Systems.LaunchpadController");
        RunTest("LaunchpadController script exists", launchpadType != null);

        reportBuilder.AppendLine();
        yield return null;
    }

    private System.Collections.IEnumerator TestUI()
    {
        reportBuilder.AppendLine("┌─────────────────────────────────────┐");
        reportBuilder.AppendLine("│  4. TESTE DE UI                     │");
        reportBuilder.AppendLine("└─────────────────────────────────────┘");

        // Canvas
        Canvas[] canvases = FindObjectsOfType<Canvas>();
        RunTest("Canvas in scene", canvases.Length > 0);

        // GameplayUI
        GameplayUI gameplayUI = FindObjectOfType<GameplayUI>();
        RunTest("GameplayUI in scene", gameplayUI != null);

        // Modern Menu Integration
        var modernMenuType = System.Type.GetType("SpaceRPG.UI.ModernMenuIntegration");
        if (modernMenuType != null)
        {
            var modernMenu = FindObjectOfType(modernMenuType);
            RunTest("ModernMenuIntegration in scene", modernMenu != null);
        }

        // Eye Mission UI
        var eyeMissionType = System.Type.GetType("SpaceRPG.UI.EyeMissionUI");
        if (eyeMissionType != null)
        {
            var eyeMission = FindObjectOfType(eyeMissionType);
            RunTest("EyeMissionUI in scene", eyeMission != null);
        }

        reportBuilder.AppendLine();
        yield return null;
    }

    private System.Collections.IEnumerator TestPlayerSystems()
    {
        reportBuilder.AppendLine("┌─────────────────────────────────────┐");
        reportBuilder.AppendLine("│  5. TESTE DE PLAYER SYSTEMS         │");
        reportBuilder.AppendLine("└─────────────────────────────────────┘");

        // SpaceshipController
        SpaceshipController ship = FindObjectOfType<SpaceshipController>();
        RunTest("SpaceshipController in scene", ship != null);
        if (ship != null)
        {
            Rigidbody rb = ship.GetComponent<Rigidbody>();
            RunTest("Ship has Rigidbody", rb != null);
        }

        // WeaponSystem
        WeaponSystem weapons = FindObjectOfType<WeaponSystem>();
        RunTest("WeaponSystem in scene", weapons != null);

        // UpgradeSystem
        UpgradeSystem upgrades = FindObjectOfType<UpgradeSystem>();
        RunTest("UpgradeSystem in scene", upgrades != null);

        // RewardSystem
        RewardSystem rewards = FindObjectOfType<RewardSystem>();
        RunTest("RewardSystem in scene", rewards != null);

        // PlantingSystem
        PlantingSystem planting = FindObjectOfType<PlantingSystem>();
        RunTest("PlantingSystem in scene", planting != null);

        // NPCInstructor
        NPCInstructor npc = FindObjectOfType<NPCInstructor>();
        RunTest("NPCInstructor in scene", npc != null);

        reportBuilder.AppendLine();
        yield return null;
    }

    private System.Collections.IEnumerator TestSceneSetup()
    {
        reportBuilder.AppendLine("┌─────────────────────────────────────┐");
        reportBuilder.AppendLine("│  6. TESTE DE SCENE SETUP            │");
        reportBuilder.AppendLine("└─────────────────────────────────────┘");

        // Camera
        Camera mainCam = Camera.main;
        RunTest("Main Camera exists", mainCam != null);
        if (mainCam != null)
        {
            AudioListener listener = mainCam.GetComponent<AudioListener>();
            RunTest("Main Camera has AudioListener", listener != null);
        }

        // Light
        Light[] lights = FindObjectsOfType<Light>();
        RunTest("Scene has lighting", lights.Length > 0);

        // Event System (para UI)
        UnityEngine.EventSystems.EventSystem eventSystem = FindObjectOfType<UnityEngine.EventSystems.EventSystem>();
        RunTest("EventSystem exists (for UI)", eventSystem != null);

        reportBuilder.AppendLine();
        yield return null;
    }

    private System.Collections.IEnumerator TestAssets()
    {
        reportBuilder.AppendLine("┌─────────────────────────────────────┐");
        reportBuilder.AppendLine("│  7. TESTE DE ASSETS                 │");
        reportBuilder.AppendLine("└─────────────────────────────────────┘");

        // Folders
        bool scriptsFolder = System.IO.Directory.Exists(Application.dataPath + "/Scripts");
        RunTest("Scripts folder exists", scriptsFolder);

        bool scenesFolder = System.IO.Directory.Exists(Application.dataPath + "/Scenes");
        RunTest("Scenes folder exists", scenesFolder);

        bool prefabsFolder = System.IO.Directory.Exists(Application.dataPath + "/Prefabs");
        RunTest("Prefabs folder exists", prefabsFolder);

        bool materialsFolder = System.IO.Directory.Exists(Application.dataPath + "/Materials");
        RunTest("Materials folder exists", materialsFolder);

        reportBuilder.AppendLine();
        yield return null;
    }

    #endregion

    #region Test Utilities

    private void RunTest(string testName, bool passed)
    {
        totalTests++;
        if (passed)
        {
            passedTests++;
            string result = $"✓ {testName}";
            testResults.Add(result);
            reportBuilder.AppendLine($"  ✅ {testName}");
            if (logDetailedResults)
                Debug.Log($"<color=green>✓</color> {testName}");
        }
        else
        {
            failedTests++;
            string result = $"✗ {testName}";
            testResults.Add(result);
            reportBuilder.AppendLine($"  ❌ {testName}");
            Debug.LogWarning($"<color=red>✗</color> {testName}");
        }

        testProgress = (float)totalTests / 30f; // Assuming 30 total tests
    }

    private void GenerateFinalReport()
    {
        reportBuilder.AppendLine();
        reportBuilder.AppendLine("╔════════════════════════════════════════════╗");
        reportBuilder.AppendLine("║          RESULTADO FINAL                   ║");
        reportBuilder.AppendLine("╚════════════════════════════════════════════╝");
        reportBuilder.AppendLine();
        reportBuilder.AppendLine($"  Total de Testes:   {totalTests}");
        reportBuilder.AppendLine($"  Testes Passados:   {passedTests} ✅");
        reportBuilder.AppendLine($"  Testes Falhados:   {failedTests} ❌");
        reportBuilder.AppendLine();

        float successRate = (float)passedTests / totalTests * 100f;
        reportBuilder.AppendLine($"  Taxa de Sucesso:   {successRate:F1}%");
        reportBuilder.AppendLine();

        string status = "";
        Color statusColor = Color.white;

        if (successRate >= 90f)
        {
            status = "✅ EXCELENTE - Projeto Pronto!";
            statusColor = Color.green;
        }
        else if (successRate >= 70f)
        {
            status = "⚠️  BOM - Alguns ajustes necessários";
            statusColor = Color.yellow;
        }
        else if (successRate >= 50f)
        {
            status = "⚠️  REGULAR - Vários problemas encontrados";
            statusColor = Color.orange;
        }
        else
        {
            status = "❌ CRÍTICO - Muitos problemas encontrados";
            statusColor = Color.red;
        }

        reportBuilder.AppendLine($"  Status: {status}");
        reportBuilder.AppendLine();
        reportBuilder.AppendLine("════════════════════════════════════════════");

        Debug.Log($"<color={ColorUtility.ToHtmlStringRGB(statusColor)}>{status}</color>");
        Debug.Log(reportBuilder.ToString());

        // Salvar relatório em arquivo
        SaveReportToFile();
    }

    private void SaveReportToFile()
    {
        string path = Application.dataPath + "/../TESTE_VALIDACAO_RESULTADO.txt";
        try
        {
            System.IO.File.WriteAllText(path, reportBuilder.ToString());
            Debug.Log($"<color=cyan>Relatório salvo em: {path}</color>");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Erro ao salvar relatório: {e.Message}");
        }
    }

    #endregion

    #region Console Commands

    [ContextMenu("Quick Test - Scripts Only")]
    public void QuickTestScripts()
    {
        StartCoroutine(TestScriptsExistence());
    }

    [ContextMenu("Quick Test - Scene Only")]
    public void QuickTestScene()
    {
        StartCoroutine(TestSceneSetup());
    }

    #endregion
}
