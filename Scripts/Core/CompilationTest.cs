using UnityEngine;
using SpaceRPG.Systems;
using SpaceRPG.Data;
using SpaceRPG.UI;

namespace SpaceRPG.Core
{
    /// <summary>
    /// Script de teste para verificar se todos os namespaces e classes estão acessíveis
    /// Remove este arquivo após verificar que tudo compila
    /// </summary>
    public class CompilationTest : MonoBehaviour
{
    // Este script apenas testa se todas as referências estão corretas
    private void TestReferences()
    {
        // Test Core
        var tweenHelper = typeof(TweenHelper);
        var enemyController = typeof(EnemyController);
        var enemySpawner = typeof(EnemySpawner);

        // Test Systems
        var audioManager = AudioManager.Instance;
        var questSystem = QuestSystem.Instance;
        var itemDatabase = ItemDatabase.Instance;
        var inventorySystem = InventorySystem.Instance;

        // Test UI
        var eyeMissionUI = EyeMissionUI.Instance;
        var modernMenu = ModernMenuIntegration.Instance;

        // Test Data
        var itemData = typeof(ItemData);
        var itemType = typeof(ItemType);

        Debug.Log("All references are valid!");
    }
    }
}
