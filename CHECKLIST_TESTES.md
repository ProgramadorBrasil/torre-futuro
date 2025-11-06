# ✅ CHECKLIST DE TESTES - SPACESHIP TOWER FUTURO

## 🎯 OBJETIVO
Verificar que TODOS os sistemas do jogo estão funcionando corretamente antes do lançamento.

---

## 🚀 TESTES DE MOVIMENTO E FÍSICA

### SpaceshipController - Controles Básicos
- [ ] **Teste 1.1:** Pressionar W acelera a nave para frente
- [ ] **Teste 1.2:** Pressionar A gira a nave para esquerda
- [ ] **Teste 1.3:** Pressionar D gira a nave para direita
- [ ] **Teste 1.4:** Pressionar S reduz a velocidade
- [ ] **Teste 1.5:** Mouse controla pitch (cima/baixo)
- [ ] **Teste 1.6:** Mouse controla yaw (esquerda/direita)
- [ ] **Teste 1.7:** Q faz roll para esquerda
- [ ] **Teste 1.8:** E faz roll para direita

### SpaceshipController - Boost e Freio
- [ ] **Teste 2.1:** SHIFT ativa boost (velocidade aumenta)
- [ ] **Teste 2.2:** Boost consome energia (barra diminui)
- [ ] **Teste 2.3:** Boost desativa quando energia acaba
- [ ] **Teste 2.4:** Trail effects aparecem durante boost
- [ ] **Teste 2.5:** CTRL ativa freio (velocidade diminui rápido)

### SpaceshipController - Limites e Física
- [ ] **Teste 3.1:** Velocidade máxima é respeitada
- [ ] **Teste 3.2:** Nave desacelera quando solta W
- [ ] **Teste 3.3:** Colisão com obstáculo causa dano
- [ ] **Teste 3.4:** Física de rotação é suave
- [ ] **Teste 3.5:** Rigidbody não atravessa objetos

### SpaceshipController - Recursos
- [ ] **Teste 4.1:** Barra de vida diminui ao receber dano
- [ ] **Teste 4.2:** Barra de energia regenera com tempo
- [ ] **Teste 4.3:** Barra de combustível diminui ao mover
- [ ] **Teste 4.4:** Barra de armadura absorve dano
- [ ] **Teste 4.5:** Avisos aparecem quando recursos baixos
- [ ] **Teste 4.6:** Nave morre quando vida chega a 0
- [ ] **Teste 4.7:** Nave respawna após morte (3 segundos)
- [ ] **Teste 4.8:** Recursos restauram ao respawnar

---

## 🔫 TESTES DE SISTEMA DE ARMAS

### WeaponSystem - Disparar
- [ ] **Teste 5.1:** Left Click dispara arma atual
- [ ] **Teste 5.2:** SPACE também dispara arma
- [ ] **Teste 5.3:** Fire rate é respeitado (não atira muito rápido)
- [ ] **Teste 5.4:** Projétil aparece e se move para frente
- [ ] **Teste 5.5:** Efeito de muzzle flash aparece
- [ ] **Teste 5.6:** Som de disparo toca

### WeaponSystem - Munição
- [ ] **Teste 6.1:** Munição diminui a cada tiro
- [ ] **Teste 6.2:** Não dispara quando munição = 0
- [ ] **Teste 6.3:** Som de "click" toca quando sem munição
- [ ] **Teste 6.4:** R inicia recarga
- [ ] **Teste 6.5:** Indicador de "Reloading" aparece
- [ ] **Teste 6.6:** Munição restaura após reload
- [ ] **Teste 6.7:** Não pode atirar durante reload
- [ ] **Teste 6.8:** Reload time varia por arma

### WeaponSystem - Tipos de Armas
- [ ] **Teste 7.1:** Laser (1): rápido, baixo dano, azul
- [ ] **Teste 7.2:** Míssil (2): lento, alto dano, vermelho, guiado
- [ ] **Teste 7.3:** Plasma (3): médio, splash damage, verde
- [ ] **Teste 7.4:** Cada arma tem munição separada
- [ ] **Teste 7.5:** Cada arma tem fire rate diferente
- [ ] **Teste 7.6:** Cada arma consome energia diferente

### WeaponSystem - Troca de Armas
- [ ] **Teste 8.1:** Tecla 1 seleciona Laser
- [ ] **Teste 8.2:** Tecla 2 seleciona Míssil (se desbloqueado)
- [ ] **Teste 8.3:** Tecla 3 seleciona Plasma (se desbloqueado)
- [ ] **Teste 8.4:** Mouse wheel up troca para próxima arma
- [ ] **Teste 8.5:** Mouse wheel down troca para arma anterior
- [ ] **Teste 8.6:** Som de troca de arma toca
- [ ] **Teste 8.7:** UI atualiza para mostrar arma atual
- [ ] **Teste 8.8:** Não pode trocar arma durante reload

### WeaponSystem - Dano e Impacto
- [ ] **Teste 9.1:** Laser causa 10 de dano base
- [ ] **Teste 9.2:** Míssil causa 50 de dano base
- [ ] **Teste 9.3:** Plasma causa 30 de dano + splash
- [ ] **Teste 9.4:** Efeito de impacto aparece ao acertar
- [ ] **Teste 9.5:** Inimigo morre após receber dano fatal
- [ ] **Teste 9.6:** Splash damage do plasma afeta área
- [ ] **Teste 9.7:** Míssil segue alvo se houver targeting

### WeaponSystem - Heat Management
- [ ] **Teste 10.1:** Heat bar aumenta ao atirar
- [ ] **Teste 10.2:** Heat dissipa com tempo
- [ ] **Teste 10.3:** Overheating impede disparos
- [ ] **Teste 10.4:** Aviso de overheat aparece
- [ ] **Teste 10.5:** Som de overheat toca

---

## 🔧 TESTES DE SISTEMA DE UPGRADE

### UpgradeSystem - Menu
- [ ] **Teste 11.1:** TAB abre menu de upgrades
- [ ] **Teste 11.2:** TAB novamente fecha menu
- [ ] **Teste 11.3:** Lista de upgrades aparece
- [ ] **Teste 11.4:** Custo de cada upgrade é mostrado
- [ ] **Teste 11.5:** Nível atual de upgrade é mostrado
- [ ] **Teste 11.6:** Upgrades bloqueados aparecem cinza

### UpgradeSystem - Compra
- [ ] **Teste 12.1:** Click em upgrade compra se tem créditos
- [ ] **Teste 12.2:** Créditos são deduzidos corretamente
- [ ] **Teste 12.3:** Nível do upgrade aumenta
- [ ] **Teste 12.4:** Não compra se créditos insuficientes
- [ ] **Teste 12.5:** Mensagem de "insuficiente" aparece
- [ ] **Teste 12.6:** Custo aumenta a cada nível
- [ ] **Teste 12.7:** Max level impede mais compras
- [ ] **Teste 12.8:** Som de compra toca

### UpgradeSystem - Upgrades de Nave
- [ ] **Teste 13.1:** Speed upgrade aumenta velocidade máxima
- [ ] **Teste 13.2:** Health upgrade aumenta vida máxima
- [ ] **Teste 13.3:** Armor upgrade aumenta armadura
- [ ] **Teste 13.4:** Energy upgrade aumenta energia máxima
- [ ] **Teste 13.5:** Efeitos são aplicados imediatamente
- [ ] **Teste 13.6:** Stats na UI atualizam

### UpgradeSystem - Upgrades de Armas
- [ ] **Teste 14.1:** Laser damage upgrade aumenta dano
- [ ] **Teste 14.2:** Laser fire rate upgrade aumenta taxa
- [ ] **Teste 14.3:** Laser ammo upgrade aumenta munição max
- [ ] **Teste 14.4:** Missile damage upgrade aumenta dano
- [ ] **Teste 14.5:** Plasma damage upgrade aumenta dano
- [ ] **Teste 14.6:** Upgrades aplicam multiplicadores corretos

### UpgradeSystem - Tech Tree
- [ ] **Teste 15.1:** Missile System desbloqueia míssil
- [ ] **Teste 15.2:** Plasma Cannon desbloqueia plasma
- [ ] **Teste 15.3:** Prerequisites impedem compra prematura
- [ ] **Teste 15.4:** Comprar prereq desbloqueia dependentes
- [ ] **Teste 15.5:** Notificação de "Unlocked!" aparece

### UpgradeSystem - Persistência
- [ ] **Teste 16.1:** Upgrades salvam ao comprar
- [ ] **Teste 16.2:** Upgrades carregam ao iniciar jogo
- [ ] **Teste 16.3:** Créditos salvam
- [ ] **Teste 16.4:** XP e nível salvam
- [ ] **Teste 16.5:** Save file criado em persistentDataPath

---

## 🌱 TESTES DE SISTEMA DE PLANTIO

### PlantingSystem - Ativar Modo
- [ ] **Teste 17.1:** P ativa modo de plantio
- [ ] **Teste 17.2:** P novamente desativa modo
- [ ] **Teste 17.3:** Reticle de plantio aparece
- [ ] **Teste 17.4:** Cursor do mouse controla reticle
- [ ] **Teste 17.5:** Mensagem "Planting Mode" aparece

### PlantingSystem - Validação de Posição
- [ ] **Teste 18.1:** Reticle fica verde em posição válida
- [ ] **Teste 18.2:** Reticle fica vermelho em posição inválida
- [ ] **Teste 18.3:** Não planta muito perto de outra planta
- [ ] **Teste 18.4:** Não planta fora do range
- [ ] **Teste 18.5:** Não planta fora do terreno plantável

### PlantingSystem - Plantar Sementes
- [ ] **Teste 19.1:** Left Click planta semente
- [ ] **Teste 19.2:** Créditos são deduzidos (custo da semente)
- [ ] **Teste 19.3:** Planta aparece na posição
- [ ] **Teste 19.4:** Efeito de plantio aparece
- [ ] **Teste 19.5:** Som de plantio toca
- [ ] **Teste 19.6:** Contador de plantas aumenta
- [ ] **Teste 19.7:** Não planta se créditos insuficientes
- [ ] **Teste 19.8:** Não planta se atingiu limite (50)

### PlantingSystem - Tipos de Plantas
- [ ] **Teste 20.1:** [ key] troca para próximo tipo
- [ ] **Teste 20.2:** ] key troca para tipo anterior
- [ ] **Teste 20.3:** Energy Flower (azul) restaura energia
- [ ] **Teste 20.4:** Credit Fruit (amarelo) dá créditos
- [ ] **Teste 20.5:** Healing Herb (verde) restaura vida
- [ ] **Teste 20.6:** Rare Crystal (roxo) restaura tudo
- [ ] **Teste 20.7:** UI mostra tipo selecionado
- [ ] **Teste 20.8:** UI mostra custo da semente

### PlantingSystem - Crescimento
- [ ] **Teste 21.1:** Planta inicia pequena (escala 0)
- [ ] **Teste 21.2:** Planta cresce gradualmente
- [ ] **Teste 21.3:** Growth stages visíveis (5 estágios)
- [ ] **Teste 21.4:** Partículas aparecem ao crescer
- [ ] **Teste 21.5:** Tempo de crescimento varia por tipo
- [ ] **Teste 21.6:** Planta pulsa quando madura (emissão)
- [ ] **Teste 21.7:** Growth curve smooth (não linear)

### PlantingSystem - Colheita
- [ ] **Teste 22.1:** H coleta plantas próximas
- [ ] **Teste 22.2:** Só coleta plantas maduras
- [ ] **Teste 22.3:** Recompensa é concedida
- [ ] **Teste 22.4:** Efeito de colheita aparece
- [ ] **Teste 22.5:** Som de colheita toca
- [ ] **Teste 22.6:** Planta desaparece após coleta
- [ ] **Teste 22.7:** Contador de plantas diminui
- [ ] **Teste 22.8:** Mensagem de reward aparece

---

## 👤 TESTES DE NPC INSTRUTOR

### NPCInstructor - Interação
- [ ] **Teste 23.1:** NPC aparece na scene
- [ ] **Teste 23.2:** Prompt "Press E" aparece ao aproximar
- [ ] **Teste 23.3:** E inicia diálogo
- [ ] **Teste 23.4:** Diálogo aparece na UI
- [ ] **Teste 23.5:** Diálogo some após duração
- [ ] **Teste 23.6:** Não pode interagir durante diálogo ativo

### NPCInstructor - Tutorial
- [ ] **Teste 24.1:** Welcome dialogue ao iniciar jogo
- [ ] **Teste 24.2:** Movement tutorial ao interagir (stage 1)
- [ ] **Teste 24.3:** Weapons tutorial (stage 2)
- [ ] **Teste 24.4:** Planting tutorial (stage 3)
- [ ] **Teste 24.5:** Upgrades tutorial (stage 4)
- [ ] **Teste 24.6:** Training complete ao finalizar
- [ ] **Teste 24.7:** Recompensa ao completar tutorial
- [ ] **Teste 24.8:** Tutorial não repete após completo

### NPCInstructor - Animação
- [ ] **Teste 25.1:** NPC anima ao falar (Talk animation)
- [ ] **Teste 25.2:** NPC acena ao jogador aproximar (Wave)
- [ ] **Teste 25.3:** NPC volta a idle ao terminar
- [ ] **Teste 25.4:** NPC olha para jogador (head tracking)
- [ ] **Teste 25.5:** NPC rotaciona corpo para jogador

### NPCInstructor - Quests
- [ ] **Teste 26.1:** Quest marker aparece quando tem quest
- [ ] **Teste 26.2:** Quest marker roda
- [ ] **Teste 26.3:** Luz muda de cor com quest ativo
- [ ] **Teste 26.4:** Diálogo de quest oferecido
- [ ] **Teste 26.5:** Aceitar quest inicia objetivo
- [ ] **Teste 26.6:** Completar quest dá recompensa
- [ ] **Teste 26.7:** Som de quest complete toca

---

## 🏆 TESTES DE SISTEMA DE RECOMPENSAS

### RewardSystem - Kills
- [ ] **Teste 27.1:** Matar inimigo dá créditos
- [ ] **Teste 27.2:** Matar inimigo dá XP
- [ ] **Teste 27.3:** Popup de reward aparece
- [ ] **Teste 27.4:** Som de reward toca
- [ ] **Teste 27.5:** Elite enemy dá mais rewards
- [ ] **Teste 27.6:** Boss dá muito mais rewards
- [ ] **Teste 27.7:** Contador de kills aumenta

### RewardSystem - Streak System
- [ ] **Teste 28.1:** Kill streak aumenta matando rápido
- [ ] **Teste 28.2:** UI mostra streak atual
- [ ] **Teste 28.3:** Streak reseta após 5 segundos sem kill
- [ ] **Teste 28.4:** Streak reseta ao morrer
- [ ] **Teste 28.5:** Milestones (5, 10, 20) dão bônus
- [ ] **Teste 28.6:** Notificação de milestone aparece
- [ ] **Teste 28.7:** Som especial toca em milestone
- [ ] **Teste 28.8:** Streak multiplier aplica corretamente

### RewardSystem - Combo System
- [ ] **Teste 29.1:** Combo aumenta matando rapidamente
- [ ] **Teste 29.2:** UI mostra "x2 COMBO!", "x3 COMBO!"
- [ ] **Teste 29.3:** Combo reseta após 3 segundos
- [ ] **Teste 29.4:** Combo multiplier aplica rewards
- [ ] **Teste 29.5:** Combo max é registrado

### RewardSystem - Achievements
- [ ] **Teste 30.1:** "First Blood" desbloqueia no 1º kill
- [ ] **Teste 30.2:** "Killing Spree" desbloqueia em streak 5
- [ ] **Teste 30.3:** "Rampage" desbloqueia em streak 10
- [ ] **Teste 30.4:** "Hunter" desbloqueia em 100 kills
- [ ] **Teste 30.5:** Painel de achievement aparece
- [ ] **Teste 30.6:** Som de achievement toca
- [ ] **Teste 30.7:** Recompensa de achievement concedida
- [ ] **Teste 30.8:** Achievement não desbloqueia 2x

### RewardSystem - XP e Level
- [ ] **Teste 31.1:** XP bar enche gradualmente
- [ ] **Teste 31.2:** Level up ao atingir XP necessário
- [ ] **Teste 31.3:** Notificação "LEVEL UP!" aparece
- [ ] **Teste 31.4:** Bônus de créditos ao level up
- [ ] **Teste 31.5:** XP necessário aumenta por level
- [ ] **Teste 31.6:** Som de level up toca

### RewardSystem - Daily Bonus
- [ ] **Teste 32.1:** Daily bonus ao logar pela 1ª vez do dia
- [ ] **Teste 32.2:** Notificação de bonus aparece
- [ ] **Teste 32.3:** 100 créditos concedidos
- [ ] **Teste 32.4:** Não dá bonus 2x no mesmo dia
- [ ] **Teste 32.5:** Reseta no dia seguinte

### RewardSystem - Persistência
- [ ] **Teste 33.1:** Total kills salva
- [ ] **Teste 33.2:** Total deaths salva
- [ ] **Teste 33.3:** Max streak salva
- [ ] **Teste 33.4:** Achievements salvam
- [ ] **Teste 33.5:** Play time salva
- [ ] **Teste 33.6:** Estatísticas carregam ao iniciar

---

## 🖥️ TESTES DE UI

### GameplayUI - HUD Elements
- [ ] **Teste 34.1:** Health bar visível e atualiza
- [ ] **Teste 34.2:** Armor bar visível e atualiza
- [ ] **Teste 34.3:** Energy bar visível e atualiza
- [ ] **Teste 34.4:** Fuel bar visível e atualiza
- [ ] **Teste 34.5:** Ammo counter atualiza
- [ ] **Teste 34.6:** Weapon name mostra arma atual
- [ ] **Teste 34.7:** Speed mostra velocidade em m/s
- [ ] **Teste 34.8:** Credits atualiza em tempo real
- [ ] **Teste 34.9:** XP bar atualiza
- [ ] **Teste 34.10:** Level mostra corretamente

### GameplayUI - Avisos e Indicadores
- [ ] **Teste 35.1:** Low fuel warning pisca quando < 20%
- [ ] **Teste 35.2:** Overheated warning aparece
- [ ] **Teste 35.3:** Reloading indicator aparece
- [ ] **Teste 35.4:** Boost indicator acende ao booster
- [ ] **Teste 35.5:** Damage vignette pulsa quando < 30% vida

### GameplayUI - Minimap
- [ ] **Teste 36.1:** Minimap mostra área ao redor
- [ ] **Teste 36.2:** Ícone do player aparece
- [ ] **Teste 36.3:** Ícone rotaciona com nave
- [ ] **Teste 36.4:** Inimigos aparecem no minimap
- [ ] **Teste 36.5:** Plantas aparecem no minimap (opcional)

### GameplayUI - Crosshair e Targeting
- [ ] **Teste 37.1:** Crosshair aparece no centro
- [ ] **Teste 37.2:** Crosshair fica vermelho ao mirar inimigo
- [ ] **Teste 37.3:** Target reticle aparece em inimigo
- [ ] **Teste 37.4:** Nome do inimigo aparece
- [ ] **Teste 37.5:** Barra de vida do inimigo aparece

### GameplayUI - Menus
- [ ] **Teste 38.1:** ESC abre pause menu
- [ ] **Teste 38.2:** Time.timeScale = 0 ao pausar
- [ ] **Teste 38.3:** Resume button funciona
- [ ] **Teste 38.4:** Quit button funciona
- [ ] **Teste 38.5:** TAB abre upgrade menu
- [ ] **Teste 38.6:** I abre inventário
- [ ] **Teste 38.7:** Cursor aparece nos menus
- [ ] **Teste 38.8:** Cursor some no gameplay

### GameplayUI - Notificações
- [ ] **Teste 39.1:** Notificação de reward aparece
- [ ] **Teste 39.2:** Notificação some após 3 segundos
- [ ] **Teste 39.3:** Múltiplas notificações não sobrepõem
- [ ] **Teste 39.4:** Achievement notification especial
- [ ] **Teste 39.5:** Wave notification aparece

### GameplayUI - Dialogue
- [ ] **Teste 40.1:** Painel de diálogo aparece
- [ ] **Teste 40.2:** Nome do falante aparece
- [ ] **Teste 40.3:** Texto do diálogo aparece
- [ ] **Teste 40.4:** Portrait aparece (se houver)
- [ ] **Teste 40.5:** Diálogo some após duração

---

## 🎮 TESTES DE GAME MANAGER

### GameManager - Inicialização
- [ ] **Teste 41.1:** GameManager é Singleton
- [ ] **Teste 41.2:** Jogo inicia automaticamente
- [ ] **Teste 41.3:** Todas as referências encontradas
- [ ] **Teste 41.4:** Spawn points encontrados
- [ ] **Teste 41.5:** Checkpoint inicial definido

### GameManager - Wave System
- [ ] **Teste 42.1:** Wave 1 inicia ao começar jogo
- [ ] **Teste 42.2:** Notificação "Wave 1" aparece
- [ ] **Teste 42.3:** Inimigos spawnam gradualmente
- [ ] **Teste 42.4:** Contador de inimigos correto
- [ ] **Teste 42.5:** Wave completa ao matar todos
- [ ] **Teste 42.6:** Notificação "Wave Complete" aparece
- [ ] **Teste 42.7:** Bônus concedido ao completar wave
- [ ] **Teste 42.8:** Próxima wave inicia após 15 segundos
- [ ] **Teste 42.9:** Dificuldade aumenta por wave
- [ ] **Teste 42.10:** Enemy stats escalam corretamente

### GameManager - Enemy Spawning
- [ ] **Teste 43.1:** Inimigos spawnam em spawn points
- [ ] **Teste 43.2:** Máximo de 10 inimigos simultâneos
- [ ] **Teste 43.3:** Spawn interval de 10 segundos
- [ ] **Teste 43.4:** Não spawna se max atingido
- [ ] **Teste 43.5:** Inimigos mortos são limpos da lista

### GameManager - Checkpoints
- [ ] **Teste 44.1:** Checkpoint salva posição
- [ ] **Teste 44.2:** Checkpoint salva rotação
- [ ] **Teste 44.3:** Notificação de checkpoint aparece
- [ ] **Teste 44.4:** Respawn vai para checkpoint
- [ ] **Teste 44.5:** Stats restauram ao respawn

### GameManager - Save System
- [ ] **Teste 45.1:** Auto-save a cada 60 segundos
- [ ] **Teste 45.2:** Save file criado
- [ ] **Teste 45.3:** Checkpoint salvo automaticamente
- [ ] **Teste 45.4:** Sistemas salvam dados próprios

### GameManager - Game States
- [ ] **Teste 46.1:** Estado inicial: MainMenu ou Tutorial
- [ ] **Teste 46.2:** Transição para Gameplay funciona
- [ ] **Teste 46.3:** Paused state para tempo
- [ ] **Teste 46.4:** GameOver state ao morrer (opcional)
- [ ] **Teste 46.5:** Victory state ao completar objetivo
- [ ] **Teste 46.6:** Eventos de state change disparam

### GameManager - Adaptive Difficulty
- [ ] **Teste 47.1:** Dificuldade ajusta com performance
- [ ] **Teste 47.2:** KDR alto aumenta dificuldade
- [ ] **Teste 47.3:** KDR baixo diminui dificuldade
- [ ] **Teste 47.4:** Multiplier aplica em rewards
- [ ] **Teste 47.5:** Performance rating salva

---

## 🔊 TESTES DE ÁUDIO (Se implementado)

### Audio - Engine
- [ ] **Teste 48.1:** Som de motor toca continuamente
- [ ] **Teste 48.2:** Pitch varia com velocidade
- [ ] **Teste 48.3:** Volume varia com velocidade
- [ ] **Teste 48.4:** Som de boost ao ativar SHIFT

### Audio - Weapons
- [ ] **Teste 49.1:** Som diferente para cada arma
- [ ] **Teste 49.2:** Som de reload toca
- [ ] **Teste 49.3:** Som de empty magazine toca
- [ ] **Teste 49.4:** Som de troca de arma toca

### Audio - UI
- [ ] **Teste 50.1:** Som de click nos botões
- [ ] **Teste 50.2:** Som de notificação
- [ ] **Teste 50.3:** Som de achievement
- [ ] **Teste 50.4:** Som de level up

### Audio - Music
- [ ] **Teste 51.1:** Música de gameplay toca
- [ ] **Teste 51.2:** Música de menu diferente
- [ ] **Teste 51.3:** Música de boss (se houver)
- [ ] **Teste 51.4:** Volume ajustável

---

## ✨ TESTES DE EFEITOS VISUAIS

### VFX - Armas
- [ ] **Teste 52.1:** Muzzle flash ao atirar
- [ ] **Teste 52.2:** Projectile trail visível
- [ ] **Teste 52.3:** Impact effect ao acertar
- [ ] **Teste 52.4:** Explosion effect ao matar inimigo

### VFX - Nave
- [ ] **Teste 53.1:** Engine particles sempre ativos
- [ ] **Teste 53.2:** Boost trail ao booster
- [ ] **Teste 53.3:** Damage sparks ao receber dano
- [ ] **Teste 53.4:** Explosion ao morrer

### VFX - Plantas
- [ ] **Teste 54.1:** Partículas ao plantar
- [ ] **Teste 54.2:** Partículas ao crescer
- [ ] **Teste 54.3:** Glow quando madura
- [ ] **Teste 54.4:** Partículas ao colher

---

## 🐛 TESTES DE BUGS CONHECIDOS

### Bug Testing
- [ ] **Teste 55.1:** Nave não atravessa chão
- [ ] **Teste 55.2:** Projéteis não duplicam
- [ ] **Teste 55.3:** UI não some inesperadamente
- [ ] **Teste 55.4:** Memory leaks não ocorrem (profiler)
- [ ] **Teste 55.5:** Framerate estável (> 60 FPS)
- [ ] **Teste 55.6:** Sem warnings no console
- [ ] **Teste 55.7:** Save/Load não corrompe dados
- [ ] **Teste 55.8:** Múltiplos sistemas não conflitam

---

## 🎯 TESTES DE INTEGRAÇÃO

### Integration Testing
- [ ] **Teste 56.1:** Todos os scripts compilam sem erros
- [ ] **Teste 56.2:** Todas as referências assignadas
- [ ] **Teste 56.3:** Eventos conectam corretamente
- [ ] **Teste 56.4:** Upgrade aplica em nave e armas
- [ ] **Teste 56.5:** Reward integra com upgrade
- [ ] **Teste 56.6:** UI reflete todos os sistemas
- [ ] **Teste 56.7:** GameManager controla tudo
- [ ] **Teste 56.8:** Save/Load preserva tudo

---

## 📊 TESTES DE PERFORMANCE

### Performance Testing
- [ ] **Teste 57.1:** FPS > 60 com 10 inimigos
- [ ] **Teste 57.2:** FPS > 30 com 50 inimigos
- [ ] **Teste 57.3:** RAM < 500 MB
- [ ] **Teste 57.4:** Draw calls < 1000
- [ ] **Teste 57.5:** No GC spikes durante gameplay
- [ ] **Teste 57.6:** Loading time < 5 segundos

---

## 🎓 TESTES DE UX (User Experience)

### UX Testing
- [ ] **Teste 58.1:** Controles são intuitivos
- [ ] **Teste 58.2:** Tutorial é claro
- [ ] **Teste 58.3:** UI é legível
- [ ] **Teste 58.4:** Feedback visual imediato
- [ ] **Teste 58.5:** Progressão é satisfatória
- [ ] **Teste 58.6:** Dificuldade balanceada
- [ ] **Teste 58.7:** Recompensas são motivadoras
- [ ] **Teste 58.8:** Jogo é divertido! 🎮

---

## 📝 RESULTADO FINAL

**Total de Testes:** 300+

**Aprovados:** _____ / 300+

**Reprovados:** _____ / 300+

**Cobertura:** _____ %

**Status:** [ ] APROVADO  [ ] REPROVADO  [ ] EM PROGRESSO

---

## 🚀 CRITÉRIOS DE LANÇAMENTO

Para lançar o jogo, é necessário:
- ✅ 95%+ dos testes aprovados
- ✅ 0 bugs críticos (que impedem gameplay)
- ✅ Performance estável (60 FPS mínimo)
- ✅ Todos os sistemas principais funcionando
- ✅ Save/Load sem corrupção
- ✅ Tutorial completo e funcional

**BOA SORTE NOS TESTES!** 🎯✅
