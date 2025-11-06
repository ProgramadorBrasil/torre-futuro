# 🎯 GUIA COMPLETO DO SISTEMA DE MISSÕES

## OVERVIEW

O Sistema de Missões (Quest System) é um sistema robusto e flexível que suporta múltiplos tipos de missões, progressão em tempo real, recompensas e integração automática com outros sistemas.

---

## 📋 TIPOS DE MISSÕES

### 1. COMBAT (Combate)
**Objetivo:** Derrotar inimigos

**Exemplo:**
```csharp
Quest combatQuest = new Quest("combat_001", "Space Cleaner", QuestType.Combat)
{
    description = "Defeat 10 enemy ships in the sector",
    targetAmount = 10,
    targetID = "enemy_fighter", // Opcional: inimigo específico
    creditsReward = 500,
    xpReward = 100,
    difficulty = QuestDifficulty.Easy
};
```

**Progressão Automática:**
```csharp
// Quando um inimigo é morto, no seu script de inimigo:
void OnDeath()
{
    QuestSystem.Instance.UpdateQuestProgressByType(
        QuestType.Combat,
        enemyID, // ID do inimigo
        1 // quantidade
    );
}
```

---

### 2. EXPLORATION (Exploração)
**Objetivo:** Visitar locais

**Exemplo:**
```csharp
Quest exploreQuest = new Quest("explore_001", "Galaxy Explorer", QuestType.Exploration)
{
    description = "Visit 5 different sectors",
    targetAmount = 5,
    creditsReward = 600,
    xpReward = 120
};
```

**Progressão:**
```csharp
// Quando o jogador entra em um setor:
void OnEnterSector(string sectorID)
{
    QuestSystem.Instance.UpdateQuestProgressByType(
        QuestType.Exploration,
        sectorID,
        1
    );
}
```

---

### 3. PLANTCARE (Cuidado de Plantas)
**Objetivo:** Regar, fertilizar ou cuidar de plantas

**Exemplo:**
```csharp
Quest plantQuest = new Quest("plant_001", "Green Thumb", QuestType.PlantCare)
{
    description = "Water 5 plants successfully",
    targetAmount = 5,
    creditsReward = 300,
    xpReward = 50
};
```

**Progressão Automática:**
Já integrado! Quando você usa `PlantCareSystemAdvanced.WaterPlant()`, a missão progride automaticamente.

---

### 4. HARVEST (Colheita)
**Objetivo:** Colher plantas maduras

**Exemplo:**
```csharp
Quest harvestQuest = new Quest("harvest_001", "Harvest Time", QuestType.Harvest)
{
    description = "Harvest 10 mature plants",
    targetAmount = 10,
    creditsReward = 400,
    xpReward = 80
};
```

**Progressão Automática:**
Quando você usa `PlantCareSystemAdvanced.HarvestPlant()`.

---

### 5. REPAIR (Reparo)
**Objetivo:** Reparar sua nave

**Exemplo:**
```csharp
Quest repairQuest = new Quest("repair_001", "Mechanic", QuestType.Repair)
{
    description = "Repair your ship 3 times",
    targetAmount = 3,
    creditsReward = 400,
    xpReward = 75
};
```

**Progressão Automática:**
Quando você usa qualquer método de `MaintenanceSystem.PerformRepair()`.

---

### 6. DELIVERY (Entrega)
**Objetivo:** Entregar itens a NPCs

**Exemplo:**
```csharp
Quest deliveryQuest = new Quest("delivery_001", "Space Courier", QuestType.Delivery)
{
    description = "Deliver cargo to Station Alpha",
    targetAmount = 1,
    targetID = "station_alpha",
    creditsReward = 800,
    xpReward = 150
};
```

**Progressão Manual:**
```csharp
void OnDeliverCargo(string stationID)
{
    QuestSystem.Instance.UpdateQuestProgressByType(
        QuestType.Delivery,
        stationID,
        1
    );
}
```

---

### 7. COLLECTION (Coleção)
**Objetivo:** Coletar itens específicos

**Exemplo:**
```csharp
Quest collectionQuest = new Quest("collection_001", "Treasure Hunter", QuestType.Collection)
{
    description = "Collect 5 rare crystals",
    targetAmount = 5,
    targetID = "item_rare_crystal",
    creditsReward = 1000,
    xpReward = 200
};
```

**Progressão Manual:**
```csharp
void OnCollectItem(string itemID)
{
    QuestSystem.Instance.UpdateQuestProgressByType(
        QuestType.Collection,
        itemID,
        1
    );
}
```

---

## 🎖️ NÍVEIS DE DIFICULDADE

```csharp
public enum QuestDifficulty
{
    Easy,      // Recompensas baixas, fácil de completar
    Medium,    // Recompensas médias
    Hard,      // Recompensas altas, desafiador
    Elite,     // Recompensas muito altas, muito desafiador
    Legendary  // Recompensas épicas, extremamente desafiador
}
```

**Sugestões de Balanceamento:**
- **Easy:** 1-5 objetivos, 100-300 créditos, 25-75 XP
- **Medium:** 5-10 objetivos, 300-600 créditos, 75-150 XP
- **Hard:** 10-20 objetivos, 600-1200 créditos, 150-300 XP
- **Elite:** 20-50 objetivos, 1200-2500 créditos, 300-600 XP
- **Legendary:** 50+ objetivos, 2500+ créditos, 600+ XP

---

## 📊 ESTADOS DE MISSÃO

```csharp
public enum QuestStatus
{
    Locked,      // Bloqueada (não cumprimos pré-requisitos)
    Available,   // Disponível para aceitar
    InProgress,  // Em progresso
    Completed,   // Completada
    Failed       // Falhada (timeout ou outro motivo)
}
```

---

## ⏱️ MISSÕES COM TEMPO LIMITE

### Criar Missão Temporizada

```csharp
Quest timedQuest = new Quest("timed_001", "Race Against Time", QuestType.Combat)
{
    description = "Defeat 5 enemies in 5 minutes",
    targetAmount = 5,
    timeLimit = 300f, // 5 minutos em segundos
    creditsReward = 800,
    xpReward = 150
};
```

### Sistema Automático
O QuestSystem automaticamente:
1. Inicia o timer quando a missão é aceita
2. Atualiza o `timeRemaining` a cada frame
3. Falha a missão quando `timeRemaining <= 0`

### Exibir Tempo Restante na UI

```csharp
Quest activeQuest = QuestSystem.Instance.GetActiveQuests()[0];

if (activeQuest.timeLimit > 0)
{
    float minutes = Mathf.Floor(activeQuest.timeRemaining / 60f);
    float seconds = activeQuest.timeRemaining % 60f;

    string timeText = $"{minutes:00}:{seconds:00}";
    questTimerText.text = $"Time: {timeText}";
}
```

---

## 🔒 PRÉ-REQUISITOS E REQUISITOS

### Missão com Requisito de Nível

```csharp
Quest advancedQuest = new Quest("advanced_001", "Veteran Mission", QuestType.Combat)
{
    description = "Advanced combat mission",
    levelRequirement = 5, // Requer nível 5
    targetAmount = 20,
    creditsReward = 1500,
    xpReward = 300
};
```

### Missão com Pré-Requisitos (Outras Missões)

```csharp
Quest chainQuest = new Quest("chain_002", "The Sequel", QuestType.Exploration)
{
    description = "Continue the adventure",
    prerequisites = new List<string> { "chain_001" }, // Requer quest anterior
    targetAmount = 3,
    creditsReward = 800,
    xpReward = 160
};
```

**Validação Automática:**
O sistema verifica automaticamente se:
- O jogador tem o nível necessário
- Todas as missões pré-requisito foram completadas

---

## 🎁 RECOMPENSAS

### Recompensas Básicas (Créditos + XP)

```csharp
Quest simpleReward = new Quest("reward_001", "Simple Task", QuestType.Repair)
{
    targetAmount = 1,
    creditsReward = 500,
    xpReward = 100
};
```

### Recompensas com Itens

```csharp
Quest itemReward = new Quest("reward_002", "Treasure Quest", QuestType.Collection)
{
    targetAmount = 5,
    creditsReward = 800,
    xpReward = 150,
    itemRewards = new List<ItemData>
    {
        rareSword,        // Item 1
        legendaryShield,  // Item 2
        megaPotion        // Item 3
    }
};
```

**Sistema Automático de Recompensas:**
Quando a missão é completada:
1. Créditos são adicionados automaticamente
2. XP é adicionado (e pode causar level up)
3. Itens são adicionados ao inventário
4. Eventos são disparados

---

## 📅 MISSÕES DIÁRIAS E SEMANAIS

### Configurar Missão Diária

```csharp
Quest dailyQuest = new Quest("daily_001", "Daily Challenge", QuestType.Combat)
{
    description = "Daily: Defeat 5 enemies",
    targetAmount = 5,
    isDailyQuest = true,
    creditsReward = 300,
    xpReward = 60
};
```

### Sistema de Geração de Diárias

O QuestSystem tem um método `GenerateDailyQuests()` que:
1. Seleciona 3-5 missões aleatórias
2. Marca como diárias
3. Podem ser resetadas a cada 24h

**Resetar Diárias:**
```csharp
void ResetDailyQuests()
{
    // Limpar quests diárias antigas
    var dailies = QuestSystem.Instance.GetDailyQuests();
    foreach (var quest in dailies)
    {
        if (quest.status == QuestStatus.InProgress)
        {
            QuestSystem.Instance.AbandonQuest(quest.questID);
        }
    }

    // Gerar novas
    // (QuestSystem.Instance já tem este método)
}

// Chamar a cada 24h (via SaveLoadSystem ou timer)
```

---

## 🔧 USO PRÁTICO

### Aceitar Missão

```csharp
bool success = QuestSystem.Instance.AcceptQuest("quest_id");

if (success)
{
    Debug.Log("Quest accepted!");
}
else
{
    Debug.Log("Cannot accept quest (full, locked, etc)");
}
```

### Atualizar Progresso (Manual)

```csharp
// Atualizar missão específica
QuestSystem.Instance.UpdateQuestProgress("quest_id", 1);

// Atualizar todas as missões de um tipo
QuestSystem.Instance.UpdateQuestProgressByType(QuestType.Combat, "enemy_id", 1);
```

### Abandonar Missão

```csharp
QuestSystem.Instance.AbandonQuest("quest_id");
```

### Listar Missões

```csharp
// Missões disponíveis
List<Quest> available = QuestSystem.Instance.GetAvailableQuests();

// Missões ativas
List<Quest> active = QuestSystem.Instance.GetActiveQuests();

// Missões completadas
List<Quest> completed = QuestSystem.Instance.GetCompletedQuests();

// Missões diárias
List<Quest> daily = QuestSystem.Instance.GetDailyQuests();
```

### Obter Missão Específica

```csharp
Quest quest = QuestSystem.Instance.GetQuestByID("quest_id");

if (quest != null)
{
    Debug.Log($"Quest: {quest.questName}");
    Debug.Log($"Progress: {quest.currentAmount}/{quest.targetAmount}");
    Debug.Log($"Percentage: {quest.GetProgress() * 100f}%");
}
```

---

## 🎨 UI DE MISSÕES

### Exibir Lista de Missões Disponíveis

```csharp
void RefreshAvailableQuests()
{
    var quests = QuestSystem.Instance.GetAvailableQuests();

    foreach (var quest in quests)
    {
        // Criar UI slot
        GameObject slot = Instantiate(questSlotPrefab, questGrid);

        // Preencher dados
        slot.GetComponent<QuestSlot>().Setup(quest);
    }
}
```

### Exibir Progresso de Missão Ativa

```csharp
void UpdateActiveQuestDisplay()
{
    var activeQuests = QuestSystem.Instance.GetActiveQuests();

    if (activeQuests.Count > 0)
    {
        Quest currentQuest = activeQuests[0];

        questNameText.text = currentQuest.questName;
        questDescText.text = currentQuest.description;
        questProgressText.text = $"{currentQuest.currentAmount}/{currentQuest.targetAmount}";

        float progress = currentQuest.GetProgress();
        questProgressBar.fillAmount = progress;

        // Cor baseada na dificuldade
        questProgressBar.color = GetDifficultyColor(currentQuest.difficulty);
    }
}

Color GetDifficultyColor(QuestDifficulty difficulty)
{
    switch (difficulty)
    {
        case QuestDifficulty.Easy: return Color.green;
        case QuestDifficulty.Medium: return Color.yellow;
        case QuestDifficulty.Hard: return Color.orange;
        case QuestDifficulty.Elite: return Color.red;
        case QuestDifficulty.Legendary: return new Color(1f, 0f, 1f); // Magenta
        default: return Color.white;
    }
}
```

### Exibir Detalhes de Missão

```csharp
void ShowQuestDetails(Quest quest)
{
    detailsPanel.SetActive(true);

    detailNameText.text = quest.questName;
    detailDescText.text = quest.description;

    string details = $"Type: {quest.type}\n";
    details += $"Difficulty: {quest.difficulty}\n";
    details += $"Objective: {quest.targetAmount}\n";
    details += $"Rewards:\n";
    details += $"  - {quest.creditsReward} Credits\n";
    details += $"  - {quest.xpReward} XP\n";

    if (quest.itemRewards.Count > 0)
    {
        details += $"  - {quest.itemRewards.Count} Items\n";
    }

    if (quest.timeLimit > 0)
    {
        float minutes = quest.timeLimit / 60f;
        details += $"Time Limit: {minutes:F0} minutes\n";
    }

    if (quest.levelRequirement > 0)
    {
        details += $"Level Required: {quest.levelRequirement}\n";
    }

    detailStatsText.text = details;

    // Botão Aceitar
    acceptButton.interactable = (quest.status == QuestStatus.Available);

    // Botão Abandonar
    abandonButton.interactable = (quest.status == QuestStatus.InProgress);
}
```

---

## 🔔 EVENTOS

### Subscrever a Eventos

```csharp
void OnEnable()
{
    QuestSystem.Instance.OnQuestAccepted += HandleQuestAccepted;
    QuestSystem.Instance.OnQuestCompleted += HandleQuestCompleted;
    QuestSystem.Instance.OnQuestFailed += HandleQuestFailed;
    QuestSystem.Instance.OnQuestProgressUpdated += HandleQuestProgress;
}

void OnDisable()
{
    QuestSystem.Instance.OnQuestAccepted -= HandleQuestAccepted;
    QuestSystem.Instance.OnQuestCompleted -= HandleQuestCompleted;
    QuestSystem.Instance.OnQuestFailed -= HandleQuestFailed;
    QuestSystem.Instance.OnQuestProgressUpdated -= HandleQuestProgress;
}

void HandleQuestAccepted(Quest quest)
{
    Debug.Log($"Quest accepted: {quest.questName}");
    ShowNotification($"New Quest: {quest.questName}");
}

void HandleQuestCompleted(Quest quest)
{
    Debug.Log($"Quest completed: {quest.questName}");
    ShowNotification($"Quest Completed!\n+{quest.creditsReward} Credits\n+{quest.xpReward} XP");

    // Tocar som de completar
    AudioManager.Instance.PlaySFX(questCompleteSound);

    // Efeito visual
    ParticleEffects.Instance.PlayLevelUp(playerPosition);
}

void HandleQuestFailed(Quest quest)
{
    Debug.Log($"Quest failed: {quest.questName}");
    ShowNotification($"Quest Failed: {quest.questName}");
}

void HandleQuestProgress(Quest quest)
{
    Debug.Log($"Quest progress: {quest.questName} ({quest.currentAmount}/{quest.targetAmount})");

    // Atualizar UI
    UpdateActiveQuestDisplay();
}
```

---

## 📊 ESTATÍSTICAS

### Obter Estatísticas Gerais

```csharp
string stats = QuestSystem.Instance.GetQuestStats();
Debug.Log(stats);

// Output:
// Active Quests: 3/5
// Completed: 15
// Credits Earned: 7500
// Player Level: 8
```

### Estatísticas Personalizadas

```csharp
int totalQuests = QuestSystem.Instance.GetCompletedQuests().Count;
int combatQuests = QuestSystem.Instance.GetCompletedQuests()
    .Where(q => q.type == QuestType.Combat).Count();

float completionRate = (float)totalQuests / allPossibleQuests * 100f;

Debug.Log($"Completion Rate: {completionRate:F1}%");
```

---

## 🎮 EXEMPLO COMPLETO: CRIAR QUEST CHAIN

```csharp
// Missão 1: Intro
Quest intro = new Quest("chain_001", "The Beginning", QuestType.Exploration)
{
    description = "Visit the training sector",
    targetAmount = 1,
    targetID = "sector_training",
    creditsReward = 100,
    xpReward = 25,
    difficulty = QuestDifficulty.Easy
};

// Missão 2: Combat Básico
Quest basicCombat = new Quest("chain_002", "First Blood", QuestType.Combat)
{
    description = "Defeat your first enemy",
    targetAmount = 1,
    prerequisites = new List<string> { "chain_001" },
    creditsReward = 200,
    xpReward = 50,
    difficulty = QuestDifficulty.Easy
};

// Missão 3: Advanced Combat
Quest advancedCombat = new Quest("chain_003", "Warrior", QuestType.Combat)
{
    description = "Defeat 10 enemies",
    targetAmount = 10,
    prerequisites = new List<string> { "chain_002" },
    levelRequirement = 3,
    timeLimit = 600f, // 10 minutos
    creditsReward = 800,
    xpReward = 200,
    itemRewards = new List<ItemData> { epicWeapon },
    difficulty = QuestDifficulty.Hard
};

// Adicionar ao sistema
QuestSystem.Instance.GetAllQuests().Add(intro);
QuestSystem.Instance.GetAllQuests().Add(basicCombat);
QuestSystem.Instance.GetAllQuests().Add(advancedCombat);
```

---

## ✅ CHECKLIST DE IMPLEMENTAÇÃO

```
□ QuestSystem inicializado
□ Missões criadas (pelo menos 5)
□ UI de lista de missões funciona
□ UI de detalhes de missão funciona
□ Botão "Accept Quest" funciona
□ Botão "Abandon Quest" funciona
□ Progresso de missões atualiza corretamente
□ Missões completadas dão recompensas
□ Missões com tempo limite funcionam
□ Missões com pré-requisitos são validadas
□ Quest tracker na HUD funciona
□ Eventos de missão são disparados
□ Integração com outros sistemas funciona
□ Missões diárias são geradas
□ Estatísticas de missões são exibidas
```

---

**SISTEMA DE MISSÕES COMPLETO!** 🎯
