# ✅ CHECKLIST COMPLETO DE TESTES - SPACE RPG SYSTEM

## INSTRUÇÕES DE USO

1. Imprima ou mantenha este checklist aberto
2. Teste cada item na ordem
3. Marque [x] quando passar no teste
4. Anote bugs encontrados na seção de bugs
5. Não pule testes - todos são importantes!

---

## 🗂️ 1. SISTEMA DE DADOS (ItemDatabase)

### 1.1 Inicialização
```
[ ] ItemDatabase é criado automaticamente ao iniciar o jogo
[ ] ItemDatabase.Initialize() é chamado sem erros
[ ] Console mostra "ItemDatabase initialized with X items"
[ ] Nenhum erro de NullReference no Console
```

### 1.2 Criação de Itens
```
[ ] Pode criar ItemData via Create > Space RPG > Item Data
[ ] ItemData tem todos os campos visíveis no Inspector
[ ] Pode configurar Item ID, Name, Description
[ ] Pode configurar Type, Rarity
[ ] Pode configurar Buy/Sell Price
[ ] Pode arrastar ícone (Sprite) para o campo Icon
[ ] GetRarityColor() retorna cores corretas:
    [ ] Common = Cinza
    [ ] Uncommon = Verde
    [ ] Rare = Azul
    [ ] Epic = Roxo
    [ ] Legendary = Laranja
```

### 1.3 População do Database
```
[ ] ItemDatabase GameObject existe na cena
[ ] Pode arrastar itens para as listas no Inspector:
    [ ] Weapons
    [ ] Ship Parts
    [ ] Consumables
    [ ] Quest Items
    [ ] Materials
    [ ] Seeds
    [ ] Plant Care Items
    [ ] Tools
[ ] Itens padrão estão configurados:
    [ ] Default Weapon
    [ ] Repair Kit
    [ ] Water Item
    [ ] Fertilizer Item
```

### 1.4 Busca de Itens
```
[ ] GetItemByID() retorna item correto
[ ] GetItemByID() com ID inválido retorna null
[ ] GetItemByName() retorna item correto
[ ] GetItemsByType() retorna lista filtrada
[ ] SearchItems() encontra itens por nome parcial
[ ] FilterItems() aplica múltiplos filtros corretamente
```

---

## 📦 2. SISTEMA DE INVENTÁRIO (InventorySystem)

### 2.1 Inicialização
```
[ ] InventorySystem é criado automaticamente
[ ] Console mostra "Inventory initialized"
[ ] Inventário começa vazio (ou com item padrão)
[ ] Créditos iniciais = 1000
[ ] Peso inicial = 0kg
[ ] Max slots = 50
[ ] Max weight = 500kg
```

### 2.2 Adicionar Itens
```
[ ] AddItem() adiciona item ao inventário
[ ] Item aparece na lista allItems
[ ] Quantidade é atualizada corretamente
[ ] Peso total é calculado corretamente
[ ] Items stackable empilham corretamente
[ ] Items não-stackable criam slots separados
[ ] Adicionar item além do limite de peso falha
[ ] Adicionar item com inventário cheio falha
[ ] Evento OnItemAdded é disparado
[ ] Console mostra "Added to inventory: [item name]"
```

### 2.3 Remover Itens
```
[ ] RemoveItem() remove quantidade correta
[ ] Item é removido quando quantidade = 0
[ ] RemoveItem() com quantidade insuficiente falha
[ ] RemoveItem() com ID inválido falha
[ ] Peso total é atualizado após remover
[ ] Evento OnItemRemoved é disparado
```

### 2.4 Usar Itens
```
[ ] UseItem() funciona para consumíveis
[ ] Consumível reduz quantidade em 1
[ ] UseItem() funciona para equipáveis
[ ] UseItem() falha para items não-usáveis
[ ] Som de uso é tocado (se configurado)
[ ] Efeito visual é spawnado (se configurado)
[ ] Evento OnItemUsed é disparado
```

### 2.5 Equipar/Desequipar
```
[ ] EquipItem() equipa arma
[ ] Item equipado tem isEquipped = true
[ ] Item equipado aparece em equippedItems
[ ] Equipar nova arma desequipa a anterior
[ ] UnequipItem() desequipa corretamente
[ ] Som de equipar é tocado
[ ] Eventos OnItemEquipped/Unequipped são disparados
```

### 2.6 Créditos
```
[ ] AddCredits() adiciona créditos corretamente
[ ] RemoveCredits() remove créditos corretamente
[ ] RemoveCredits() com valor insuficiente falha
[ ] GetCurrentCredits() retorna valor correto
[ ] Evento OnCreditsChanged é disparado
```

### 2.7 Categorização
```
[ ] Weapons são adicionados à lista weapons
[ ] ShipParts são adicionados à lista shipParts
[ ] Consumables são adicionados à lista consumables
[ ] QuestItems são adicionados à lista questItems
[ ] GetItemsByCategory() retorna lista correta
```

### 2.8 Filtros e Ordenação
```
[ ] FilterInventory() filtra por nome
[ ] SortInventory() ordena por nome
[ ] SortInventory() ordena por preço
[ ] HasItem() verifica existência corretamente
[ ] GetItemQuantity() retorna quantidade correta
```

---

## 🖼️ 3. UI DO INVENTÁRIO (InventoryUI)

### 3.1 Abertura/Fechamento
```
[ ] Inventário abre com TAB
[ ] Inventário fecha com TAB novamente
[ ] Inventário fecha com botão X
[ ] Som de abertura toca
[ ] Som de fechamento toca
[ ] Tempo é pausado quando aberto (Time.timeScale = 0)
[ ] Tempo volta ao normal quando fechado
```

### 3.2 Exibição de Itens
```
[ ] Grid exibe todos os itens
[ ] Ícone do item é exibido
[ ] Nome do item é exibido
[ ] Quantidade é exibida (se > 1)
[ ] Borda colorida baseada em raridade
[ ] Indicador [E] aparece em items equipados
[ ] Itens são categorizados corretamente
```

### 3.3 Tabs de Categorias
```
[ ] Tab "All Items" mostra todos os itens
[ ] Tab "Weapons" mostra apenas armas
[ ] Tab "Ship Parts" mostra apenas peças
[ ] Tab "Consumables" mostra apenas consumíveis
[ ] Tab "Quest Items" mostra apenas items de quest
[ ] Tab "Equipped" mostra apenas equipados
[ ] Atalhos numéricos (1-6) funcionam
```

### 3.4 Detalhes de Item
```
[ ] Clicar em item mostra painel de detalhes
[ ] Ícone grande é exibido
[ ] Nome com cor de raridade é exibido
[ ] Descrição completa é exibida
[ ] Stats são exibidos (damage, defense, etc)
[ ] Peso e valor são exibidos
[ ] Durabilidade é exibida (se aplicável)
[ ] Botões corretos são mostrados (Use/Equip/Drop/Sell)
```

### 3.5 Ações de Item
```
[ ] Botão "Use" usa consumível
[ ] Botão "Equip" equipa item
[ ] Botão "Unequip" desequipa item
[ ] Botão "Drop" descarta item
[ ] Botão "Sell" vende item (abre diálogo)
[ ] Som de clique é tocado
```

### 3.6 Busca e Filtros
```
[ ] Campo de busca filtra itens por nome
[ ] Busca é case-insensitive
[ ] Busca atualiza em tempo real
[ ] Dropdown de ordenação funciona
[ ] Dropdown de filtro de raridade funciona
```

### 3.7 Stats Display
```
[ ] Créditos são exibidos corretamente
[ ] Peso atual/máximo é exibido
[ ] Barra de peso é atualizada visualmente
[ ] Slots usados/total são exibidos
[ ] Valores atualizam em tempo real
```

### 3.8 Drag & Drop (se implementado)
```
[ ] Pode arrastar item
[ ] Item fica semi-transparente ao arrastar
[ ] Pode soltar item em outro slot
[ ] Itens trocam de posição
[ ] Som de drag é tocado
[ ] Som de drop é tocado
```

---

## 🛒 4. SISTEMA DE LOJA (ShopSystem)

### 4.1 Inicialização
```
[ ] ShopSystem é criado automaticamente
[ ] Loja é populada com itens compráveis
[ ] Estoque inicial está correto
[ ] Ofertas especiais são geradas (3-5 itens)
[ ] Console mostra "Shop initialized with X items"
```

### 4.2 Compra de Itens
```
[ ] BuyItem() compra item com sucesso
[ ] Créditos são reduzidos corretamente
[ ] Item é adicionado ao inventário
[ ] Estoque é reduzido (se limitado)
[ ] Compra sem créditos suficientes falha
[ ] Compra com inventário cheio falha
[ ] Compra sem estoque suficiente falha
[ ] Evento OnItemPurchased é disparado
[ ] Histórico de compras é atualizado
```

### 4.3 Venda de Itens
```
[ ] SellItem() vende item do inventário
[ ] Créditos são adicionados corretamente
[ ] Item é removido do inventário
[ ] Estoque da loja aumenta (se limitado)
[ ] Venda de item não-vendável falha
[ ] Preço de venda = 50% do valor de compra
[ ] Evento OnItemSold é disparado
```

### 4.4 Descontos
```
[ ] Ofertas especiais têm desconto visível
[ ] Desconto é aplicado no preço final
[ ] Desconto de lealdade aumenta com compras
[ ] CalculatePurchasePrice() aplica descontos
[ ] Descontos acumulam corretamente
```

### 4.5 Wishlist
```
[ ] AddToWishlist() adiciona item
[ ] Item já na wishlist não é adicionado de novo
[ ] RemoveFromWishlist() remove item
[ ] IsInWishlist() verifica corretamente
[ ] GetWishlistItems() retorna lista correta
[ ] Eventos de wishlist são disparados
```

### 4.6 Reabastecimento
```
[ ] RestockShop() reabastece estoque
[ ] Estoque é renovado aleatoriamente
[ ] Novas ofertas especiais são criadas
[ ] Evento OnShopRestocked é disparado
[ ] Reabastecimento automático ocorre após 24h (testar com timer modificado)
```

### 4.7 Busca e Filtros
```
[ ] SearchShop() encontra itens por nome
[ ] FilterByPrice() filtra por faixa de preço
[ ] GetItemsByCategory() retorna categoria correta
[ ] SortShopItems() ordena corretamente
```

---

## 🛒 5. UI DA LOJA (ShopUI)

### 5.1 Abertura/Fechamento
```
[ ] Loja abre com tecla S
[ ] Loja fecha com tecla S novamente
[ ] Loja fecha com botão X
[ ] Som de abertura toca
[ ] Som de fechamento toca
[ ] Tempo é pausado quando aberta
```

### 5.2 Exibição de Itens
```
[ ] Grid exibe itens da loja
[ ] Ícone é exibido
[ ] Nome é exibido
[ ] Preço é exibido
[ ] Estoque é exibido (se limitado)
[ ] Tag de desconto é exibida (se houver)
[ ] Tag NEW é exibida (se novo)
[ ] Indicador de wishlist é exibido
```

### 5.3 Tabs
```
[ ] Tab "All Items" mostra todos
[ ] Tab "Weapons" mostra armas
[ ] Tab "Ship Parts" mostra peças
[ ] Tab "Consumables" mostra consumíveis
[ ] Tab "Special Offers" mostra ofertas
[ ] Tab "Wishlist" mostra itens desejados
```

### 5.4 Detalhes e Compra
```
[ ] Clicar em item mostra detalhes
[ ] Preço com desconto é exibido
[ ] Preço original é riscado (se com desconto)
[ ] Campo de quantidade funciona
[ ] Botão "Buy" compra item
[ ] Botão "Wishlist" adiciona/remove
[ ] Mensagem de sucesso é exibida
[ ] Mensagem de erro é exibida (se falhar)
[ ] Som de compra toca
[ ] Som de erro toca (se falhar)
```

### 5.5 Stats do Jogador
```
[ ] Créditos do jogador são exibidos
[ ] Desconto de lealdade é exibido
[ ] Total de compras é exibido
[ ] Valores atualizam após compra
```

### 5.6 Busca e Filtros
```
[ ] Campo de busca funciona
[ ] Dropdown de ordenação funciona
[ ] Slider de preço mínimo funciona
[ ] Slider de preço máximo funciona
[ ] Range de preço é exibido
```

---

## 🎯 6. SISTEMA DE MISSÕES (QuestSystem)

### 6.1 Inicialização
```
[ ] QuestSystem é criado automaticamente
[ ] Missões de exemplo são criadas
[ ] Missões diárias são geradas
[ ] Console mostra "Quest System initialized"
```

### 6.2 Aceitar Missões
```
[ ] AcceptQuest() aceita missão disponível
[ ] Missão muda status para InProgress
[ ] Missão é adicionada às ativas
[ ] Aceitar missão bloqueada falha
[ ] Aceitar com 5 ativas já falha
[ ] Aceitar missão com nível insuficiente falha
[ ] Evento OnQuestAccepted é disparado
```

### 6.3 Progresso de Missões
```
[ ] UpdateQuestProgress() atualiza corretamente
[ ] Progresso é incrementado
[ ] Progresso não excede target
[ ] UpdateQuestProgressByType() funciona
[ ] Missão completa quando atinge target
[ ] Evento OnQuestProgressUpdated é disparado
```

### 6.4 Completar Missões
```
[ ] Missão completa automaticamente ao atingir target
[ ] Status muda para Completed
[ ] Missão é removida das ativas
[ ] Missão é adicionada às completadas
[ ] Recompensas são dadas:
    [ ] Créditos são adicionados
    [ ] XP é adicionado
    [ ] Items são adicionados ao inventário
[ ] Evento OnQuestCompleted é disparado
[ ] Console mostra "Quest completed: [name]"
```

### 6.5 Missões com Tempo
```
[ ] Timer inicia quando missão é aceita
[ ] TimeRemaining diminui a cada frame
[ ] Missão falha quando timer = 0
[ ] Evento OnQuestFailed é disparado
[ ] Console mostra "Quest failed: [name]"
```

### 6.6 Pré-requisitos
```
[ ] Missão com nível maior que o jogador fica bloqueada
[ ] Missão com prerequisite não completado fica bloqueada
[ ] Missão é desbloqueada ao cumprir requisitos
```

### 6.7 Tipos de Missões
```
[ ] Combat: Progride ao matar inimigos
[ ] Exploration: Progride ao visitar locais
[ ] PlantCare: Progride ao cuidar de plantas (automático)
[ ] Harvest: Progride ao colher plantas (automático)
[ ] Repair: Progride ao reparar nave (automático)
[ ] Delivery: Progride manualmente
[ ] Collection: Progride ao coletar itens
```

### 6.8 Abandonar Missões
```
[ ] AbandonQuest() remove missão das ativas
[ ] Status volta para Available
[ ] Progresso é resetado
```

### 6.9 Missões Diárias
```
[ ] GetDailyQuests() retorna lista de diárias
[ ] 3-5 missões diárias são geradas
[ ] isDailyQuest = true nas diárias
```

---

## 🚀 7. SISTEMA DE NAVES (ShipSystem)

### 7.1 Inicialização
```
[ ] ShipSystem é criado automaticamente
[ ] 3 naves são criadas:
    [ ] Space Shuttle
    [ ] Omega Fighter
    [ ] Star Cruiser
[ ] Nave inicial (Shuttle) é equipada
[ ] Console mostra "Ship System initialized with 3 ships"
```

### 7.2 Trocar Naves
```
[ ] ChangeShip() troca para nave possuída
[ ] Instância anterior é destruída
[ ] Nova instância é criada (se tem prefab)
[ ] currentShip é atualizado
[ ] Saúde é resetada para máximo da nova nave
[ ] ChangeShip() para nave não-possuída falha
[ ] Evento OnShipChanged é disparado
```

### 7.3 Comprar/Vender Naves
```
[ ] PurchaseShip() compra nave disponível
[ ] Créditos são reduzidos
[ ] Nave é adicionada às possuídas
[ ] isUnlocked = true
[ ] Compra sem créditos falha
[ ] Compra de nave já possuída falha
[ ] SellShip() vende nave possuída
[ ] Créditos são adicionados
[ ] Nave é removida das possuídas
[ ] Venda da última nave falha
[ ] Eventos são disparados
```

### 7.4 Sistema de Dano
```
[ ] TakeDamage() reduz saúde
[ ] Shield é reduzido primeiro
[ ] Health é reduzido depois
[ ] DamageLevel é atualizado (0-100%)
[ ] Evento OnShipDamaged é disparado
[ ] Saúde não fica negativa
[ ] Console mostra "Ship destroyed!" ao chegar a 0
```

### 7.5 Sistema de Reparo
```
[ ] RepairShip() restaura saúde
[ ] Saúde não excede máximo
[ ] DamageLevel é atualizado
[ ] Evento OnShipRepaired é disparado
[ ] Reparar nave com saúde cheia falha
```

### 7.6 Customização
```
[ ] SetShipColor() define cores primary/secondary
[ ] Cores são aplicadas ao modelo (se implementado)
```

### 7.7 Stats e Getters
```
[ ] GetCurrentShip() retorna nave correta
[ ] GetCurrentHealth() retorna saúde correta
[ ] GetCurrentShield() retorna shield correto
[ ] GetDamageLevel() retorna 0-100%
[ ] GetOwnedShips() retorna lista correta
[ ] GetShipStats() retorna string formatada
```

---

## 🔧 8. SISTEMA DE MANUTENÇÃO (MaintenanceSystem)

### 8.1 Tipos de Reparo
```
[ ] PerformQuickRepair() restaura 25 HP por 100 créditos
[ ] PerformStandardRepair() restaura 50 HP por 300 créditos
[ ] PerformFullRepair() restaura 100 HP por 500 créditos
[ ] PerformEmergencyRepair() restaura 100 HP por 800 créditos
[ ] Emergency Repair requer canivete
[ ] Emergency Repair degrada canivete
```

### 8.2 Validações
```
[ ] Reparo sem créditos falha
[ ] Reparo sem Repair Kit falha (exceto Emergency)
[ ] Reparo com nave full HP falha
[ ] Reparo sem canivete (Emergency) falha
[ ] Eventos de falha são disparados
```

### 8.3 Histórico
```
[ ] Registro é adicionado ao histórico
[ ] Histórico tem data/hora
[ ] Histórico tem tipo de manutenção
[ ] Histórico tem HP restaurado
[ ] Histórico tem custo
[ ] GetMaintenanceHistory() retorna lista
[ ] Histórico limitado a 50 registros
```

### 8.4 Upgrade de Durabilidade
```
[ ] UpgradeDurability() aumenta nível
[ ] Custo aumenta com cada nível
[ ] Multiplicador aumenta 10% por nível
[ ] Reparos futuros são mais eficientes
[ ] Evento OnDurabilityUpgraded é disparado
```

### 8.5 Diagnóstico
```
[ ] GetDamageType() retorna tipo correto:
    [ ] None (0-20% dano)
    [ ] Light (20-40% dano)
    [ ] Moderate (40-60% dano)
    [ ] Heavy (60-80% dano)
    [ ] Critical (80-100% dano)
[ ] GetDamageDescription() retorna texto apropriado
[ ] GetRecommendedRepairCost() sugere custo correto
```

### 8.6 Integração com Missões
```
[ ] Reparar nave progride missões de Repair
[ ] Missão é atualizada automaticamente
```

---

## 🌱 9. SISTEMA DE PLANTAS (PlantCareSystemAdvanced)

### 9.1 Plantar
```
[ ] PlantSeed() planta semente
[ ] Prefab é instanciado (se disponível)
[ ] Planta é adicionada à lista
[ ] Estado inicial = Seed
[ ] Saúde inicial = 100%
[ ] Água inicial = 100%
[ ] Nutrientes iniciais = 100%
[ ] Evento OnPlantAdded é disparado
[ ] Missões de PlantCare são atualizadas
```

### 9.2 Crescimento
```
[ ] Planta cresce com o tempo (se saudável)
[ ] GrowthProgress aumenta
[ ] Estado muda conforme progresso:
    [ ] Seed (0-20%)
    [ ] Sprout (20-40%)
    [ ] Growing (40-60%)
    [ ] Mature (60-80%)
    [ ] Flowering (80-100%)
    [ ] Harvestable (100%)
[ ] Evento OnPlantStateChanged é disparado
```

### 9.3 Recursos
```
[ ] WaterLevel diminui com o tempo
[ ] NutrientLevel diminui com o tempo
[ ] Pragas aparecem aleatoriamente
[ ] Saúde diminui se precisar de cuidados
```

### 9.4 Cuidados
```
[ ] WaterPlant() restaura água
[ ] Requer Water item no inventário
[ ] Water item é consumido
[ ] Saúde aumenta levemente
[ ] timesWatered é incrementado
[ ] Evento OnPlantWatered é disparado
[ ] Missões de PlantCare são atualizadas
```

```
[ ] FertilizePlant() restaura nutrientes
[ ] Requer Fertilizer item
[ ] Fertilizer é consumido
[ ] Growth rate aumenta temporariamente
[ ] timesFertilized é incrementado
[ ] Evento OnPlantFertilized é disparado
```

```
[ ] UsePesticide() remove pragas
[ ] hasPests = false
[ ] Saúde aumenta
[ ] timesPesticideUsed é incrementado
[ ] Evento OnPesticideUsed é disparado
```

### 9.5 Colheita
```
[ ] HarvestPlant() colhe planta madura
[ ] Harvest item é adicionado ao inventário
[ ] Quantidade correta é dada
[ ] Planta é removida
[ ] Prefab é destruído
[ ] totalHarvests é incrementado
[ ] Evento OnPlantHarvested é disparado
[ ] Missões de Harvest são atualizadas
```

### 9.6 Morte de Plantas
```
[ ] Planta morre se saúde = 0
[ ] Estado muda para Dead
[ ] totalPlantDeaths é incrementado
[ ] Evento OnPlantDied é disparado
```

### 9.7 Stats e Listas
```
[ ] GetAllPlants() retorna todas
[ ] GetHealthyPlants() retorna apenas saudáveis
[ ] GetHarvestablePlants() retorna apenas colhíveis
[ ] GetPlantCareStats() retorna estatísticas
```

---

## 🔊 10. SISTEMA DE ÁUDIO (AudioManager)

### 10.1 Música
```
[ ] PlayMusic() toca música
[ ] Música faz loop
[ ] Trocar música para a mesma não reinicia
[ ] StopMusic() para música
[ ] PauseMusic() pausa
[ ] ResumeMusic() retoma
```

### 10.2 SFX
```
[ ] PlaySFX() toca som
[ ] Múltiplos SFX podem tocar simultaneamente
[ ] Pool de AudioSources funciona
[ ] StopAllSFX() para todos os sons
```

### 10.3 Ambiente
```
[ ] PlayAmbient() toca som ambiente
[ ] Som ambiente faz loop
[ ] StopAmbient() para ambiente
```

### 10.4 Volumes
```
[ ] SetMasterVolume() ajusta volume geral
[ ] SetMusicVolume() ajusta volume da música
[ ] SetSFXVolume() ajusta volume dos SFX
[ ] SetAmbientVolume() ajusta volume ambiente
[ ] Valores são clamped entre 0-1
[ ] Volumes afetam playback imediatamente
```

---

## 💾 11. SISTEMA DE SAVE/LOAD (SaveLoadSystem)

### 11.1 Salvar
```
[ ] SaveGame() cria arquivo de save
[ ] Arquivo é salvo em persistentDataPath
[ ] Dados do inventário são salvos
[ ] Dados de missões são salvos
[ ] Dados de naves são salvos
[ ] Dados da loja são salvos
[ ] Configurações de áudio são salvas
[ ] Tempo de jogo é salvo
[ ] Console mostra "Game saved successfully"
```

### 11.2 Carregar
```
[ ] LoadGame() carrega arquivo
[ ] Inventário é restaurado
[ ] Créditos são restaurados
[ ] Items equipados são restaurados
[ ] Durabilidade de items é restaurada
[ ] Missões são restauradas (se implementado)
[ ] Console mostra "Game loaded successfully"
[ ] LoadGame() sem arquivo retorna false
```

### 11.3 Utilidades
```
[ ] SaveExists() verifica corretamente
[ ] DeleteSave() apaga arquivo
[ ] GetSaveInfo() retorna informações:
    [ ] Data do save
    [ ] Tempo de jogo
    [ ] Nível do jogador
    [ ] Créditos
    [ ] Número de itens
```

### 11.4 Auto-Save
```
[ ] Auto-save está ativado por padrão
[ ] Auto-save ocorre a cada 5 minutos
[ ] Auto-save ocorre ao sair do jogo
```

---

## 🎨 12. MENU MANAGER (MenuManager)

### 12.1 Menu Principal
```
[ ] Botão "Start Game" inicia novo jogo
[ ] Botão "Load Game" carrega save
[ ] Botão "Settings" abre configurações
[ ] Botão "Quit" fecha o jogo
```

### 12.2 Menu de Pausa
```
[ ] ESC pausa o jogo
[ ] Time.timeScale = 0 quando pausado
[ ] Botão "Resume" retoma o jogo
[ ] Botão "Save Game" salva
[ ] Botão "Settings" abre configurações
[ ] Botão "Main Menu" volta ao menu
[ ] Botão "Quit" fecha o jogo
```

### 12.3 Configurações
```
[ ] Slider de Master Volume funciona
[ ] Slider de Music Volume funciona
[ ] Slider de SFX Volume funciona
[ ] Dropdown de Quality funciona
[ ] Dropdown de Resolution funciona
[ ] Toggle de Fullscreen funciona
[ ] Configurações são salvas em PlayerPrefs
[ ] Botão "Back" fecha configurações
```

### 12.4 HUD
```
[ ] Créditos são exibidos
[ ] Saúde da nave é exibida
[ ] Barra de saúde é atualizada visualmente
[ ] Quest tracker mostra missão ativa
[ ] Progresso da missão é exibido
[ ] HUD atualiza em tempo real
```

### 12.5 Notificações
```
[ ] ShowNotification() exibe mensagem
[ ] Notificação desaparece após 3 segundos
[ ] Múltiplas notificações não sobrepõem
```

### 12.6 Loading Screen
```
[ ] ShowLoadingScreen() exibe tela
[ ] Barra de progresso funciona
[ ] UpdateLoadingProgress() atualiza barra
[ ] HideLoadingScreen() esconde tela
```

---

## ✨ 13. EFEITOS (ParticleEffects & UIAnimator)

### 13.1 Efeitos de Partículas
```
[ ] PlayExplosion() spawna explosão
[ ] PlayImpact() spawna impacto
[ ] PlayHeal() spawna efeito de cura
[ ] PlayPowerUp() spawna power-up
[ ] PlayPurchase() spawna efeito de compra
[ ] PlayLevelUp() spawna level up
[ ] PlayWater() spawna efeito de água
[ ] PlayPlantGrow() spawna crescimento
[ ] Efeitos são desativados após lifetime
[ ] Pool de efeitos funciona (reutiliza objetos)
```

### 13.2 Animações de UI
```
[ ] FadeIn() faz fade in de CanvasGroup
[ ] FadeOut() faz fade out
[ ] ScalePulse() anima scale (bounce)
[ ] SlideIn() desliza painel para dentro
[ ] SlideOut() desliza painel para fora
```

---

## 🎮 14. GAME MANAGER (GameManagerRPG)

### 14.1 Inicialização
```
[ ] GameManager é criado automaticamente
[ ] Console mostra "=== Initializing Space RPG Systems ==="
[ ] Todos os sistemas são inicializados na ordem:
    [ ] Item Database
    [ ] Inventory System
    [ ] Shop System
    [ ] Quest System
    [ ] Ship System
    [ ] Maintenance System
    [ ] Plant Care System
    [ ] Audio Manager
    [ ] Save/Load System
    [ ] Menu Manager
[ ] Console mostra "=== Space RPG Initialization Complete ==="
[ ] isGameInitialized = true
```

### 14.2 XP e Level Up
```
[ ] AddXP() adiciona XP ao jogador
[ ] XP acumula corretamente
[ ] Level up ocorre ao atingir XP necessário
[ ] XP necessário aumenta (x1.5)
[ ] Notificação de level up é exibida
[ ] Console mostra "Player leveled up to X!"
```

### 14.3 Event Handlers
```
[ ] Inventário cheio exibe notificação
[ ] Peso excedido exibe notificação
[ ] Transação falhada exibe razão
[ ] Quest completada adiciona XP
[ ] Quest completada exibe notificação
[ ] Dano crítico na nave exibe aviso
[ ] Planta morta exibe notificação
```

### 14.4 Debug Keys
```
[ ] F1 adiciona 1000 créditos
[ ] F2 repara nave completamente
[ ] F3 adiciona 100 XP
[ ] F4 completa quest ativa
[ ] F5 salva o jogo (Quick Save)
[ ] F9 carrega o jogo (Quick Load)
[ ] Console mostra mensagens de debug
```

### 14.5 Stats Gerais
```
[ ] GetGameStats() retorna estatísticas de:
    [ ] Player Level
    [ ] Player XP
    [ ] Inventory Stats
    [ ] Shop Stats
    [ ] Quest Stats
    [ ] Ship Stats
    [ ] Maintenance Stats
    [ ] Plant Care Stats
```

---

## 🔗 15. INTEGRAÇÃO ENTRE SISTEMAS

### 15.1 Inventário ↔ Loja
```
[ ] Comprar item adiciona ao inventário
[ ] Vender item remove do inventário
[ ] Créditos são compartilhados
```

### 15.2 Inventário ↔ Manutenção
```
[ ] Usar Repair Kit do inventário funciona
[ ] Canivete do inventário é usado em Emergency Repair
[ ] Durabilidade do canivete diminui ao usar
```

### 15.3 Inventário ↔ Plantas
```
[ ] Water item é consumido ao regar
[ ] Fertilizer item é consumido ao fertilizar
[ ] Harvest item é adicionado ao inventário
```

### 15.4 Missões ↔ Outros Sistemas
```
[ ] Missões de Combat progridem com kills
[ ] Missões de PlantCare progridem ao regar
[ ] Missões de Harvest progridem ao colher
[ ] Missões de Repair progridem ao reparar
```

### 15.5 Quest Rewards ↔ Inventário
```
[ ] Recompensas de créditos são adicionadas
[ ] Recompensas de items são adicionadas
[ ] XP é adicionado ao jogador
```

---

## ⚠️ 16. BUGS ENCONTRADOS

### Registre bugs aqui durante os testes:

```
BUG 1:
Descrição:
Passos para reproduzir:
Severidade: [Crítico/Alto/Médio/Baixo]

BUG 2:
Descrição:
Passos para reproduzir:
Severidade:

BUG 3:
Descrição:
Passos para reproduzir:
Severidade:
```

---

## 📊 17. PERFORMANCE

### 17.1 FPS
```
[ ] FPS estável em 60 (Window > Analysis > Profiler)
[ ] CPU usage < 16ms por frame
[ ] GC.Alloc mínimo (< 1KB por frame)
```

### 17.2 Memória
```
[ ] Uso de memória estável
[ ] Sem memory leaks
[ ] Sem objetos sendo instanciados infinitamente
```

### 17.3 Console
```
[ ] Sem erros no Console
[ ] Sem warnings críticos
[ ] Sem NullReferenceExceptions
```

---

## ✅ 18. CHECKLIST FINAL

```
[ ] Todos os sistemas funcionam independentemente
[ ] Todos os sistemas integram entre si
[ ] Todas as UIs funcionam
[ ] Todos os eventos são disparados
[ ] Save/Load funciona perfeitamente
[ ] Debug keys funcionam
[ ] Performance está boa (60fps)
[ ] Sem erros no Console
[ ] Documentação está completa
[ ] Projeto está pronto para build
```

---

## 🎉 CONCLUSÃO

Se TODOS os itens acima estão marcados como [x]:

**PARABÉNS! SEU SISTEMA RPG ESPACIAL ESTÁ COMPLETO E FUNCIONAL!** 🚀

Próximos passos:
1. Polir assets visuais
2. Adicionar mais conteúdo (items, quests, ships)
3. Balancear economia e progressão
4. Adicionar mais SFX e música
5. Criar tutorial para jogadores
6. Fazer build e distribuir!

---

*Este checklist foi criado para garantir qualidade AAA no sistema Space RPG*
