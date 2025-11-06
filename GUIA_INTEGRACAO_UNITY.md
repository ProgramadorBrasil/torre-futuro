# 🎮 GUIA DE INTEGRAÇÃO COM UNITY

## PASSO A PASSO COMPLETO PARA CONFIGURAR O SISTEMA NO UNITY

---

## 📦 1. IMPORTAÇÃO DOS SCRIPTS

### 1.1 Organizar Scripts no Unity

1. Abra o Unity
2. No Project, crie a estrutura:
```
Assets/
└── SpaceRPG/
    ├── Scripts/
    │   ├── Core/
    │   ├── Data/
    │   ├── Systems/
    │   ├── UI/
    │   ├── Managers/
    │   └── Effects/
    ├── Prefabs/
    ├── UI/
    ├── ScriptableObjects/
    │   └── Items/
    ├── Audio/
    └── Sprites/
```

3. Copie os scripts de `D:\games\torre futuro\Scripts\` para as pastas correspondentes

---

## 🔧 2. INSTALAÇÃO DE DEPENDÊNCIAS

### 2.1 TextMesh Pro
```
1. Window > Package Manager
2. Procure "TextMesh Pro"
3. Clique em "Install"
4. Import TMP Essential Resources
```

### 2.2 Input System (Opcional, para controles avançados)
```
1. Window > Package Manager
2. Procure "Input System"
3. Clique em "Install"
```

---

## 🎨 3. CRIAR SCRIPTABLE OBJECTS (ITEMS)

### 3.1 Configurar Menu de Criação

O script `ItemData.cs` já tem o atributo `[CreateAssetMenu]`

### 3.2 Criar Itens de Exemplo

**Arma Laser:**
```
1. Project > Create > Space RPG > Item Data
2. Nomeie: "Laser_Blaster"
3. Configure:
   - Item ID: "weapon_laser_001"
   - Item Name: "Laser Blaster"
   - Type: Weapon
   - Rarity: Uncommon
   - Buy Price: 500
   - Sell Price: 250
   - Damage Bonus: 25
   - Fire Rate: 3
   - Range: 50
```

**Repair Kit:**
```
1. Create > Space RPG > Item Data
2. Nome: "Repair_Kit"
3. Configure:
   - Item ID: "consumable_repair_kit"
   - Item Name: "Repair Kit"
   - Type: Consumable
   - Rarity: Common
   - Buy Price: 100
   - Sell Price: 50
   - Is Consumable: TRUE
```

**Semente:**
```
1. Create > Space RPG > Item Data
2. Nome: "Seed_Cosmic_Flower"
3. Configure:
   - Item ID: "seed_cosmic_001"
   - Item Name: "Cosmic Flower Seed"
   - Type: Seed
   - Rarity: Rare
   - Buy Price: 50
```

**Água:**
```
1. Create > Space RPG > Item Data
2. Nome: "Water"
3. Configure:
   - Item ID: "plantcare_water"
   - Item Name: "Water"
   - Type: PlantCare
   - Is Stackable: TRUE
   - Max Stack Size: 99
```

**Canivete:**
```
1. Create > Space RPG > Item Data
2. Nome: "Knife_Tool"
3. Configure:
   - Item ID: "tool_knife"
   - Item Name: "Swiss Army Knife"
   - Type: Tool
   - Rarity: Uncommon
   - Buy Price: 200
```

### 3.3 Adicionar Ícones

1. Importe sprites para `Assets/SpaceRPG/Sprites/Icons/`
2. Configure como Sprite (2D and UI)
3. Arraste para o campo `Icon` do ItemData

---

## 🎯 4. CONFIGURAR ITEM DATABASE

### 4.1 Criar GameObject do Database

```
1. Hierarchy > Create Empty
2. Nome: "ItemDatabase"
3. Add Component > ItemDatabase
4. Add to prefab folder
```

### 4.2 Popular o Database

```
1. Selecione ItemDatabase no Hierarchy
2. No Inspector, expanda as listas:
   - All Items (deixar vazio, preenchido automaticamente)
   - Weapons (arrastar armas criadas)
   - Ship Parts (arrastar peças)
   - Consumables (arrastar consumíveis)
   - Seeds (arrastar sementes)
   - Plant Care Items (arrastar água, fertilizante, pesticida)
   - Tools (arrastar canivete)

3. Configurar itens padrão:
   - Default Currency: (criar item de moeda)
   - Default Weapon: Laser Blaster
   - Repair Kit: Repair Kit
   - Water Item: Water
   - Fertilizer Item: Fertilizer
```

---

## 🖼️ 5. CRIAR UI DO INVENTÁRIO

### 5.1 Criar Canvas Principal

```
1. Hierarchy > UI > Canvas
2. Nome: "InventoryCanvas"
3. Canvas Scaler:
   - UI Scale Mode: Scale With Screen Size
   - Reference Resolution: 1920x1080
4. Add Event System (automático)
```

### 5.2 Criar Painel de Inventário

```
1. InventoryCanvas > Create > Panel
2. Nome: "InventoryPanel"
3. Configure:
   - Anchor: Stretch (ocupar tela toda)
   - Alpha: 0.9 (semi-transparente)
```

### 5.3 Criar Header

```
1. InventoryPanel > Create > Text - TextMeshPro
2. Nome: "TitleText"
3. Text: "INVENTORY"
4. Font Size: 48
5. Alignment: Center, Top
```

### 5.4 Criar Tabs de Categorias

```
Para cada categoria (All Items, Weapons, Ship Parts, etc):

1. Create > Button - TextMeshPro
2. Nome: "Tab_AllItems" (etc)
3. Text: "ALL ITEMS"
4. Posicionar em linha horizontal no topo
```

### 5.5 Criar Grid de Itens

```
1. InventoryPanel > Create > Scroll View
2. Nome: "ItemGrid"
3. Content > Add Component > Grid Layout Group
   - Cell Size: 100x100
   - Spacing: 10x10
   - Constraint: Fixed Column Count = 6
```

### 5.6 Criar Item Slot Prefab

```
1. Content > Create > Image
2. Nome: "ItemSlot"
3. Estrutura:
   ItemSlot (Image - fundo)
   ├── Icon (Image - ícone do item)
   ├── QuantityText (TMP - quantidade)
   ├── RarityBorder (Image - borda colorida)
   └── EquippedIndicator (Image - [E] se equipado)

4. Add Component: Button
5. Add Component: CanvasGroup (para drag)
6. Salvar como Prefab: "ItemSlotPrefab"
7. Deletar da hierarquia
```

### 5.7 Criar Painel de Detalhes

```
1. InventoryPanel > Create > Panel
2. Nome: "DetailsPanel"
3. Posição: Lado direito
4. Estrutura:
   DetailsPanel
   ├── DetailIcon (Image - ícone grande)
   ├── DetailName (TMP - nome)
   ├── DetailDescription (TMP - descrição)
   ├── DetailStats (TMP - stats)
   ├── UseButton (Button)
   ├── EquipButton (Button)
   ├── DropButton (Button)
   └── SellButton (Button)
```

### 5.8 Criar Search & Filter

```
1. Create > InputField - TMP
2. Nome: "SearchField"
3. Placeholder: "Search items..."

4. Create > Dropdown - TMP
5. Nome: "SortDropdown"
6. Options: Name (A-Z), Price (Low-High), etc

7. Create > Dropdown - TMP
8. Nome: "RarityFilterDropdown"
9. Options: All Rarities, Common, Uncommon, etc
```

### 5.9 Criar Stats Display

```
1. Create > Text - TMP
2. Nome: "CreditsText"
3. Text: "1000 Credits"

4. Create > Text - TMP
5. Nome: "WeightText"
6. Text: "Weight: 50/500 kg"

7. Create > Image (Slider)
8. Nome: "WeightBar"
9. Slider Type: Filled
```

### 5.10 Atribuir Referências

```
1. Selecione InventoryCanvas
2. Add Component > InventoryUI
3. Arrastar todos os elementos criados para os campos correspondentes:
   - Inventory Panel
   - Close Button
   - All Items Tab
   - Weapons Tab
   - etc...
   - Item Grid Parent (Content do ScrollView)
   - Item Slot Prefab
   - Details Panel (todos os campos)
   - Search Field
   - Sort Dropdown
   - Rarity Filter Dropdown
   - Credits Text
   - Weight Text
   - Weight Bar
```

---

## 🛒 6. CRIAR UI DA LOJA (SIMILAR AO INVENTÁRIO)

Repita o processo acima, mas com ajustes para a loja:
- ShopPanel ao invés de InventoryPanel
- ShopItemSlot com preço e estoque
- Wishlist Button
- Discount Tags
- New Tags

---

## 🎮 7. CONFIGURAR GAME MANAGER

### 7.1 Criar Game Manager GameObject

```
1. Hierarchy > Create Empty
2. Nome: "GameManagerRPG"
3. Add Component > GameManagerRPG
```

### 7.2 Verificar Inicialização

```
1. Play Mode
2. Console deve mostrar:
   === Initializing Space RPG Systems ===
   ✓ Item Database initialized
   ✓ Inventory System initialized
   ✓ Shop System initialized
   ✓ Quest System initialized
   ✓ Ship System initialized
   ✓ Maintenance System initialized
   ✓ Plant Care System initialized
   === Space RPG Initialization Complete ===
```

---

## 🚢 8. CONFIGURAR NAVES

### 8.1 Importar Modelos 3D

```
1. Importe Space Shuttle FBX
2. Importe Space Ship Omega Fighter FBX
3. Configure:
   - Scale Factor: Ajustar
   - Generate Colliders: TRUE
   - Materials > Extract Materials
```

### 8.2 Criar Prefabs de Naves

```
Para cada nave:

1. Arrastar modelo para cena
2. Add Component > Rigidbody (se necessário)
3. Add Component > Spaceship Controller (seu script de controle)
4. Ajustar colliders
5. Salvar como Prefab
6. Remover da cena
```

### 8.3 Configurar Ship System

```
1. O ShipSystem cria dados automaticamente no Start
2. Ou criar manualmente via Inspector
3. Atribuir prefabs aos ShipData correspondentes
```

---

## 🎵 9. CONFIGURAR ÁUDIO

### 9.1 Importar Audio Clips

```
1. Importe Space Threat.mp3 para Assets/SpaceRPG/Audio/Music/
2. Importe SFX para Assets/SpaceRPG/Audio/SFX/
3. Configure:
   - Load Type: Compressed in Memory (música)
   - Load Type: Decompress on Load (SFX)
```

### 9.2 Configurar Audio Manager

```
1. AudioManager é criado automaticamente
2. Ou criar GameObject manualmente:
   - Nome: "AudioManager"
   - Add Component > AudioManager

3. Arrastar clips para os campos:
   - Main Theme: Space Threat
   - Shop Theme: (sua música de loja)
   - Combat Theme: (música de combate)
```

---

## 🌱 10. CONFIGURAR SISTEMA DE PLANTAS

### 10.1 Criar Container de Plantas

```
1. Hierarchy > Create Empty
2. Nome: "PlantContainer"
3. Este será o pai de todas as plantas
```

### 10.2 Atribuir ao PlantCareSystem

```
1. PlantCareSystemAdvanced > Inspector
2. Plant Container: Arrastar PlantContainer
```

### 10.3 Criar Prefabs de Plantas

```
1. Importar modelo 3D da planta
2. Criar variações para cada estado (Seed, Sprout, Growing, etc)
3. Salvar como prefabs
4. Atribuir aos ItemData (Seeds)
```

---

## 🎯 11. CRIAR MISSÕES

### 11.1 Via Inspector (Testing)

```
O QuestSystem cria missões de exemplo automaticamente.
Para criar novas:

1. No script QuestSystem.cs > CreateSampleQuests()
2. Adicionar:

Quest newQuest = new Quest("combat_002", "Space Warrior", QuestType.Combat)
{
    description = "Defeat 20 enemy ships",
    targetAmount = 20,
    creditsReward = 1000,
    xpReward = 200,
    difficulty = QuestDifficulty.Medium
};
allQuests.Add(newQuest);
```

### 11.2 Via ScriptableObjects (Avançado)

Criar QuestData.cs similar ao ItemData.cs e configurar via Inspector.

---

## 🔑 12. TESTES E VALIDAÇÃO

### 12.1 Checklist de Testes

```
□ Inventário abre com TAB
□ Pode adicionar/remover itens
□ Peso é calculado corretamente
□ Itens podem ser equipados
□ Loja abre com S
□ Pode comprar itens (créditos reduzem)
□ Pode vender itens (créditos aumentam)
□ Descontos funcionam
□ Missões podem ser aceitas
□ Progresso de missões atualiza
□ Missões completadas dão recompensas
□ Pode trocar de nave
□ Dano na nave funciona
□ Reparo funciona
□ Canivete pode ser usado para reparo
□ Plantas podem ser plantadas
□ Regar/Fertilizar funciona
□ Plantas crescem
□ Plantas podem ser colhidas
□ Save Game funciona (F5)
□ Load Game funciona (F9)
□ Áudio toca
□ Todos os UIs abrem corretamente
```

### 12.2 Testar Debug Keys

```
F1 - Deve adicionar 1000 créditos
F2 - Deve reparar nave completamente
F3 - Deve adicionar 100 XP
F4 - Deve completar missão ativa
F5 - Deve salvar jogo
F9 - Deve carregar jogo
```

---

## ⚠️ 13. TROUBLESHOOTING

### Problema: "NullReferenceException no ItemDatabase"
**Solução:** Certifique-se de que ItemDatabase.Initialize() é chamado primeiro

### Problema: "UI não aparece"
**Solução:** Verificar se Canvas Scaler está configurado, Event System existe

### Problema: "Items não aparecem no inventário"
**Solução:** Verificar se ItemSlotPrefab está atribuído no InventoryUI

### Problema: "Não consigo comprar itens"
**Solução:** Verificar se tem créditos suficientes, inventário não está cheio

### Problema: "Plantas não crescem"
**Solução:** Verificar se PlantCareSystemAdvanced está no modo Play, não pausado

### Problema: "Save não funciona"
**Solução:** Verificar permissões de escrita em Application.persistentDataPath

---

## 📊 14. PERFORMANCE E OTIMIZAÇÃO

### 14.1 Object Pooling

Os sistemas já usam pooling para:
- Efeitos de partículas (ParticleEffects)
- AudioSources (AudioManager)
- UI Slots podem usar pooling adicional

### 14.2 Otimizações Recomendadas

```csharp
// Cachear referências
private InventorySystem inventory;

void Start()
{
    inventory = InventorySystem.Instance; // Cache
}

// Usar em vez de chamar Instance toda vez
inventory.AddItem(item);
```

### 14.3 Profiling

```
1. Window > Analysis > Profiler
2. Play Mode
3. Verificar:
   - CPU Usage (deve ser < 16ms para 60fps)
   - Memory (GC.Alloc deve ser mínimo)
   - Rendering (batches, tris)
```

---

## 🎨 15. CUSTOMIZAÇÃO E EXTENSÃO

### 15.1 Adicionar Novo Tipo de Item

```csharp
// Em ItemData.cs > ItemType enum
public enum ItemType
{
    // ... existentes
    Decoration, // NOVO
}

// Em ItemDatabase.cs
[SerializeField] private List<ItemData> decorations = new List<ItemData>();

// Adicionar lógica de handling
```

### 15.2 Adicionar Novo Tipo de Missão

```csharp
// Em QuestSystem.cs > QuestType enum
public enum QuestType
{
    // ... existentes
    Crafting, // NOVO
}

// Implementar lógica de progresso
```

### 15.3 Criar Sistema de Crafting

```csharp
// Novo arquivo: CraftingSystem.cs
public class CraftingSystem : MonoBehaviour
{
    // Integrar com InventorySystem e ItemDatabase
}
```

---

## ✅ 16. CHECKLIST FINAL DE INTEGRAÇÃO

```
□ Todos os scripts compilam sem erros
□ Item Database populado com pelo menos 10 itens
□ UI do Inventário funcional
□ UI da Loja funcional
□ 3 naves configuradas
□ Sistema de Missões funcional
□ Sistema de Plantas funcional
□ Sistema de Manutenção funcional
□ Áudio configurado e tocando
□ Save/Load funcional
□ GameManager inicializa todos os sistemas
□ Sem NullReferenceExceptions no Console
□ Performance estável (60fps)
□ Todos os UIs têm animações
□ Efeitos visuais funcionam
□ Debug keys funcionam
□ Documentação revisada
```

---

## 🚀 17. BUILD E DISTRIBUIÇÃO

### 17.1 Preparar para Build

```
1. File > Build Settings
2. Adicionar cenas necessárias
3. Player Settings:
   - Company Name
   - Product Name
   - Icon
   - Splash Screen
4. Configurar Platform (PC, WebGL, Mobile)
```

### 17.2 Build

```
1. Build Settings > Build
2. Escolher pasta de output
3. Testar o executável
```

---

**SISTEMA PRONTO PARA PRODUÇÃO!** 🎉

Para suporte adicional, consulte os outros guias:
- README_SISTEMA_COMPLETO.md
- GUIA_INVENTARIO.md
- GUIA_LOJA.md
- GUIA_MISSOES.md
