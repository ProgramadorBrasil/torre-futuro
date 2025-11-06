# CHECKLIST DE TESTES FINAIS - TORRE FUTURO SPACE RPG

## STATUS: PRONTO PARA TESTE COMPLETO

Este documento contém todos os testes que devem ser executados para validar que o jogo está 100% funcional e jogável no Unity.

---

## PARTE 1: TESTES DE INICIALIZAÇÃO (10 testes)

### 1.1 - Abertura do Projeto
- [ ] **1.1.1** Unity abre o projeto sem erros
- [ ] **1.1.2** Console não mostra erros vermelhos críticos
- [ ] **1.1.3** Scene MainGame.unity carrega completamente
- [ ] **1.1.4** Hierarquia mostra GameObjects principais
- [ ] **1.1.5** Project window mostra todos os assets

### 1.2 - Compilação de Scripts
- [ ] **1.2.1** Todos os 30 scripts compilam sem erro
- [ ] **1.2.2** Nenhum warning crítico (CS0618, CS0649)
- [ ] **1.2.3** Namespaces SpaceRPG.* estão resolvidos
- [ ] **1.2.4** Assembly compila em <10 segundos
- [ ] **1.2.5** Nenhum script com "Missing Reference"

**RESULTADO PARTE 1:** ___ / 10 testes passados

---

## PARTE 2: TESTES DE SCENE SETUP (15 testes)

### 2.1 - Hierarquia de GameObjects
- [ ] **2.1.1** Main Camera existe na scene
- [ ] **2.1.2** Directional Light existe
- [ ] **2.1.3** GameManager GameObject existe
- [ ] **2.1.4** EventSystem para UI existe
- [ ] **2.1.5** Canvas para UI existe

### 2.2 - Componentes Essenciais
- [ ] **2.2.1** Main Camera tem Camera component
- [ ] **2.2.2** Main Camera tem AudioListener
- [ ] **2.2.3** GameManager tem script GameManager
- [ ] **2.2.4** Canvas tem CanvasScaler
- [ ] **2.2.5** Canvas tem GraphicRaycaster

### 2.3 - Configurações da Scene
- [ ] **2.3.1** Camera está posicionada corretamente (Y > 0)
- [ ] **2.3.2** Lighting está configurada (não totalmente preto)
- [ ] **2.3.3** Skybox está configurada (ou cor de fundo)
- [ ] **2.3.4** Physics está habilitado
- [ ] **2.3.5** Audio Settings está correto (não mutado)

**RESULTADO PARTE 2:** ___ / 15 testes passados

---

## PARTE 3: TESTES DE MOVIMENTO DA NAVE (10 testes)

### 3.1 - Movimento Básico (WASD)
- [ ] **3.1.1** Pressionar W = Nave avança
- [ ] **3.1.2** Pressionar S = Nave recua
- [ ] **3.1.3** Pressionar A = Nave vai para esquerda
- [ ] **3.1.4** Pressionar D = Nave vai para direita
- [ ] **3.1.5** Movimento é suave (sem teleport)

### 3.2 - Mouse Look e Rotação
- [ ] **3.2.1** Mover mouse = Nave rotaciona
- [ ] **3.2.2** Rotação é suave e responsiva
- [ ] **3.2.3** Mouse sensitivity está adequada
- [ ] **3.2.4** Pressionar Q = Roll para esquerda
- [ ] **3.2.5** Pressionar E = Roll para direita

### 3.3 - Controles Avançados
- [ ] **3.3.1** SHIFT = Boost (velocidade aumenta)
- [ ] **3.3.2** CTRL = Freio (velocidade diminui)
- [ ] **3.3.3** Boost consome energia/combustível
- [ ] **3.3.4** Sem boost, energia regenera
- [ ] **3.3.5** Nave respeita física (não atravessa objetos)

**RESULTADO PARTE 3:** ___ / 15 testes passados (corrigido total)

---

## PARTE 4: TESTES DE SISTEMA DE ARMAS (15 testes)

### 4.1 - Disparo Básico
- [ ] **4.1.1** Mouse Click Esquerdo dispara
- [ ] **4.1.2** Projectile é criado
- [ ] **4.1.3** Projectile se move para frente
- [ ] **4.1.4** Efeito visual de disparo aparece
- [ ] **4.1.5** Audio de disparo toca

### 4.2 - Tipos de Armas
- [ ] **4.2.1** Pressionar 1 = Seleciona Laser
- [ ] **4.2.2** Pressionar 2 = Seleciona Míssil
- [ ] **4.2.3** Pressionar 3 = Seleciona Plasma
- [ ] **4.2.4** UI mostra arma atual
- [ ] **4.2.5** Cada arma tem visual diferente

### 4.3 - Sistema de Munição
- [ ] **4.3.1** Munição diminui ao disparar
- [ ] **4.3.2** Ao chegar a 0, não dispara mais
- [ ] **4.3.3** Pressionar R = Recarrega
- [ ] **4.3.4** Reload tem animação/delay
- [ ] **4.3.5** UI mostra munição corretamente

**RESULTADO PARTE 4:** ___ / 15 testes passados

---

## PARTE 5: TESTES DE MENUS E UI (20 testes)

### 5.1 - HUD (Heads-Up Display)
- [ ] **5.1.1** Vida do jogador aparece
- [ ] **5.1.2** Energia aparece
- [ ] **5.1.3** Munição aparece
- [ ] **5.1.4** Arma atual aparece
- [ ] **5.1.5** HUD atualiza em tempo real

### 5.2 - Menu de Inventário (TAB)
- [ ] **5.2.1** Pressionar TAB = Abre inventário
- [ ] **5.2.2** Inventário renderiza corretamente
- [ ] **5.2.3** Pode ver itens (se houver)
- [ ] **5.2.4** Pode fechar inventário (TAB ou ESC)
- [ ] **5.2.5** Jogo pausa quando inventário aberto

### 5.3 - Menu de Shop (S)
- [ ] **5.3.1** Pressionar S = Abre shop
- [ ] **5.3.2** Shop mostra itens disponíveis
- [ ] **5.3.3** Pode comprar itens (se houver dinheiro)
- [ ] **5.3.4** Pode vender itens
- [ ] **5.3.5** Shop fecha corretamente

### 5.4 - Outros Menus
- [ ] **5.4.1** Pressionar I = Inventário detalhado abre
- [ ] **5.4.2** Pressionar P = Menu de plantio abre
- [ ] **5.4.3** Pressionar U = Menu de upgrades abre
- [ ] **5.4.4** Pressionar ESC = Menu de pausa abre
- [ ] **5.4.5** Todos os menus fecham sem erro

**RESULTADO PARTE 5:** ___ / 20 testes passados

---

## PARTE 6: TESTES DE SISTEMAS (15 testes)

### 6.1 - Sistema de Inventário
- [ ] **6.1.1** Inventário guarda itens
- [ ] **6.1.2** Pode adicionar itens
- [ ] **6.1.3** Pode remover itens
- [ ] **6.1.4** Limite de slots funciona
- [ ] **6.1.5** Itens têm ícones corretos

### 6.2 - Sistema de Shop
- [ ] **6.2.1** Pode comprar armas
- [ ] **6.2.2** Pode comprar upgrades
- [ ] **6.2.3** Preços são exibidos
- [ ] **6.2.4** Dinheiro diminui ao comprar
- [ ] **6.2.5** Não pode comprar sem dinheiro

### 6.3 - Sistema de Upgrades
- [ ] **6.3.1** Upgrade de velocidade funciona
- [ ] **6.3.2** Upgrade de vida funciona
- [ ] **6.3.3** Upgrade de armas funciona
- [ ] **6.3.4** Stats atualizam após upgrade
- [ ] **6.3.5** UI mostra stats corretos

**RESULTADO PARTE 6:** ___ / 15 testes passados

---

## PARTE 7: TESTES DE SISTEMAS AVANÇADOS (10 testes)

### 7.1 - Sistema de Plantio
- [ ] **7.1.1** Menu de plantio abre
- [ ] **7.1.2** Pode plantar sementes
- [ ] **7.1.3** Plantas crescem com o tempo
- [ ] **7.1.4** Pode colher plantas maduras
- [ ] **7.1.5** Recompensas são dadas ao colher

### 7.2 - Sistema de Missões
- [ ] **7.2.1** Missões são exibidas
- [ ] **7.2.2** Pode aceitar missões
- [ ] **7.2.3** Progresso de missão atualiza
- [ ] **7.2.4** Recompensa é dada ao completar
- [ ] **7.2.5** Missões completadas somem

### 7.3 - NPC Instructor
- [ ] **7.3.1** NPC aparece na scene
- [ ] **7.3.2** Pode interagir com NPC
- [ ] **7.3.3** Diálogo aparece
- [ ] **7.3.4** NPC dá dicas úteis
- [ ] **7.3.5** Pode fechar diálogo

**RESULTADO PARTE 7:** ___ / 15 testes passados (corrigido)

---

## PARTE 8: TESTES DE ÁUDIO (10 testes)

### 8.1 - Música de Fundo
- [ ] **8.1.1** Música toca ao iniciar
- [ ] **8.1.2** Música é apropriada (Space Threat)
- [ ] **8.1.3** Música faz loop corretamente
- [ ] **8.1.4** Volume está adequado
- [ ] **8.1.5** Pode ajustar volume

### 8.2 - Sound Effects (SFX)
- [ ] **8.2.1** Disparo de arma tem som
- [ ] **8.2.2** Explosão tem som
- [ ] **8.2.3** Menu abrir/fechar tem som
- [ ] **8.2.4** Boost tem som
- [ ] **8.2.5** Todos os SFX estão sincronizados

**RESULTADO PARTE 8:** ___ / 10 testes passados

---

## PARTE 9: TESTES DE EFEITOS VISUAIS (15 testes)

### 9.1 - Efeitos de Disparo
- [ ] **9.1.1** Laser tem trail visual
- [ ] **9.1.2** Míssil tem trail e fumaça
- [ ] **9.1.3** Plasma tem glow effect
- [ ] **9.1.4** Muzzle flash aparece ao disparar
- [ ] **9.1.5** Impacto tem efeito visual

### 9.2 - Efeitos de Nave
- [ ] **9.2.1** Thrusters têm partículas
- [ ] **9.2.2** Boost aumenta efeito thruster
- [ ] **9.2.3** Wing trails aparecem em velocidade
- [ ] **9.2.4** Damage effect quando nave é atingida
- [ ] **9.2.5** Explosion effect quando nave explode

### 9.3 - Efeitos de Ambiente
- [ ] **9.3.1** Skybox renderiza (estrelas/espaço)
- [ ] **9.3.2** Lighting é adequada (não muito escuro)
- [ ] **9.3.3** Corridor lighting funciona (se presente)
- [ ] **9.3.4** Neon effects brilham (se presente)
- [ ] **9.3.5** Particle effects não causam lag

**RESULTADO PARTE 9:** ___ / 15 testes passados

---

## PARTE 10: TESTES DE PERFORMANCE (15 testes)

### 10.1 - Frame Rate
- [ ] **10.1.1** FPS >= 60 em idle
- [ ] **10.1.2** FPS >= 55 durante gameplay
- [ ] **10.1.3** FPS >= 50 com muitos efeitos
- [ ] **10.1.4** Sem drops de FPS (stuttering)
- [ ] **10.1.5** Frame time consistente (<16.7ms)

### 10.2 - Memória
- [ ] **10.2.1** Memory usage < 500MB
- [ ] **10.2.2** Sem memory leaks (memory não cresce)
- [ ] **10.2.3** GC collections < 1 por segundo
- [ ] **10.2.4** Heap size estável
- [ ] **10.2.5** No GC allocations em Update()

### 10.3 - Gráficos
- [ ] **10.3.1** Draw calls < 1000
- [ ] **10.3.2** Batches < 500
- [ ] **10.3.3** Tris < 100k
- [ ] **10.3.4** SetPass calls < 100
- [ ] **10.3.5** Shadow cascades otimizadas

**RESULTADO PARTE 10:** ___ / 15 testes passados

---

## PARTE 11: TESTES DE INTEGRAÇÃO (10 testes)

### 11.1 - Comunicação Entre Sistemas
- [ ] **11.1.1** GameManager se comunica com Spaceship
- [ ] **11.1.2** WeaponSystem se comunica com GameplayUI
- [ ] **11.1.3** InventorySystem se comunica com ShopSystem
- [ ] **11.1.4** RewardSystem atualiza InventorySystem
- [ ] **11.1.5** UpgradeSystem afeta SpaceshipController

### 11.2 - Persistência de Dados
- [ ] **11.2.1** Pressionar F5 = Save funciona
- [ ] **11.2.2** Pressionar F9 = Load funciona
- [ ] **11.2.3** Save preserva estado do jogador
- [ ] **11.2.4** Save preserva inventário
- [ ] **11.2.5** Load restaura tudo corretamente

**RESULTADO PARTE 11:** ___ / 10 testes passados

---

## PARTE 12: TESTES DE NAVES (10 testes)

### 12.1 - Modelos de Naves
- [ ] **12.1.1** Space Shuttle carrega
- [ ] **12.1.2** Omega Fighter G carrega
- [ ] **12.1.3** Terceira nave carrega (se houver)
- [ ] **12.1.4** Todas as naves têm colliders
- [ ] **12.1.5** Todas as naves têm Rigidbody

### 12.2 - Troca de Naves
- [ ] **12.2.1** Pode trocar de nave no menu
- [ ] **12.2.2** Stats mudam ao trocar nave
- [ ] **12.2.3** Visual da nave muda
- [ ] **12.2.4** Controles funcionam na nova nave
- [ ] **12.2.5** Launchpad animation toca (The Courtyard)

**RESULTADO PARTE 12:** ___ / 10 testes passados

---

## PARTE 13: TESTES DE MUNDO/GALÁXIA (10 testes)

### 13.1 - WorldPortalSystem
- [ ] **13.1.1** Pode viajar entre mundos
- [ ] **13.1.2** Skybox muda ao trocar mundo
- [ ] **13.1.3** Dificuldade aumenta em mundos avançados
- [ ] **13.1.4** Teleporte tem animação (Heretic effect)
- [ ] **13.1.5** Sem erro ao trocar mundo

### 13.2 - Ambientes
- [ ] **13.2.1** Alpha Centauri (cyan) renderiza
- [ ] **13.2.2** Beta Nebula (purple) renderiza
- [ ] **13.2.3** Gamma Sector (orange) renderiza
- [ ] **13.2.4** Delta Void (red) renderiza
- [ ] **13.2.5** Epsilon Star (green) renderiza

**RESULTADO PARTE 13:** ___ / 10 testes passados

---

## PARTE 14: TESTE FINAL DE GAMEPLAY (10 testes)

### 14.1 - Sessão de Jogo Completa (5 minutos)
- [ ] **14.1.1** Iniciar novo jogo funciona
- [ ] **14.1.2** Jogar por 5 minutos sem crashes
- [ ] **14.1.3** Todos os controles respondem
- [ ] **14.1.4** Pode completar objetivo básico
- [ ] **14.1.5** Jogo é divertido/jogável

### 14.2 - Funcionalidades Combinadas
- [ ] **14.2.1** Pode disparar enquanto se move
- [ ] **14.2.2** Pode abrir menu durante gameplay
- [ ] **14.2.3** Pode usar boost e armas simultaneamente
- [ ] **14.2.4** Pode trocar arma durante combate
- [ ] **14.2.5** Save/Load funciona durante jogo

**RESULTADO PARTE 14:** ___ / 10 testes passados

---

## PARTE 15: TESTE DE QUALIDADE FINAL (10 testes)

### 15.1 - Console e Erros
- [ ] **15.1.1** Nenhum erro vermelho no Console
- [ ] **15.1.2** Warnings < 5 (não críticos)
- [ ] **15.1.3** Nenhum NullReferenceException
- [ ] **15.1.4** Nenhum IndexOutOfRangeException
- [ ] **15.1.5** Nenhum MissingReferenceException

### 15.2 - Estabilidade
- [ ] **15.2.1** Jogo não crasha em 10 minutos
- [ ] **15.2.2** Jogo não trava/freezes
- [ ] **15.2.3** Pode pausar e retomar sem problemas
- [ ] **15.2.4** Pode sair do jogo limpo (sem crash)
- [ ] **15.2.5** Pode recarregar scene sem problemas

**RESULTADO PARTE 15:** ___ / 10 testes passados

---

## RESUMO FINAL

### TOTAIS POR CATEGORIA

| Categoria | Testes | Passou | % |
|-----------|--------|--------|---|
| 1. Inicialização | 10 | ___ | ___% |
| 2. Scene Setup | 15 | ___ | ___% |
| 3. Movimento | 15 | ___ | ___% |
| 4. Armas | 15 | ___ | ___% |
| 5. Menus e UI | 20 | ___ | ___% |
| 6. Sistemas | 15 | ___ | ___% |
| 7. Sistemas Avançados | 15 | ___ | ___% |
| 8. Áudio | 10 | ___ | ___% |
| 9. Efeitos Visuais | 15 | ___ | ___% |
| 10. Performance | 15 | ___ | ___% |
| 11. Integração | 10 | ___ | ___% |
| 12. Naves | 10 | ___ | ___% |
| 13. Mundo/Galáxia | 10 | ___ | ___% |
| 14. Gameplay Final | 10 | ___ | ___% |
| 15. Qualidade Final | 10 | ___ | ___% |
| **TOTAL** | **185** | **___** | **___%** |

### CRITÉRIOS DE APROVAÇÃO

#### ✅ EXCELENTE (90-100%)
- 166-185 testes passados
- **STATUS:** Production Ready - Pode lançar!

#### ✅ BOM (75-89%)
- 139-165 testes passados
- **STATUS:** Quase pronto - Pequenos ajustes necessários

#### ⚠️ REGULAR (60-74%)
- 111-138 testes passados
- **STATUS:** Funcional mas precisa de trabalho

#### ❌ INSUFICIENTE (<60%)
- <111 testes passados
- **STATUS:** Precisa de desenvolvimento adicional

---

## OBSERVAÇÕES E NOTAS

### Problemas Encontrados:
```
[Listar aqui qualquer problema encontrado durante os testes]

1. _________________________________________________________________

2. _________________________________________________________________

3. _________________________________________________________________
```

### Melhorias Sugeridas:
```
[Listar sugestões de melhorias]

1. _________________________________________________________________

2. _________________________________________________________________

3. _________________________________________________________________
```

### Performance Notes:
```
FPS Médio: _____ fps
FPS Mínimo: _____ fps
Memory Usage: _____ MB
Load Time: _____ segundos
```

---

## ASSINATURAS

**Testador:** ___________________________ **Data:** ___/___/2025

**Aprovador:** __________________________ **Data:** ___/___/2025

**Status Final:** ☐ APROVADO  ☐ APROVADO COM RESSALVAS  ☐ REPROVADO

---

## ANEXO: COMANDOS ÚTEIS PARA TESTE

### Atalhos de Teclado (Unity Editor)
- **CTRL + P** = Play/Stop
- **CTRL + SHIFT + P** = Pause
- **CTRL + S** = Save Scene
- **CTRL + SHIFT + C** = Abrir Console

### Comandos In-Game (Play Mode)
- **F1** = Info do GameManager
- **F2** = Reload Scene
- **F3** = Toggle Debug Mode
- **F4** = Performance Stats
- **F5** = Quick Save
- **F9** = Quick Load

### Controles do Jogo
- **WASD** = Movimento
- **Mouse** = Olhar/Rotacionar
- **SHIFT** = Boost
- **CTRL** = Freio
- **Q/E** = Roll
- **Mouse Click** = Disparar
- **1/2/3** = Trocar Arma
- **R** = Reload
- **TAB** = Inventário
- **S** = Shop
- **P** = Plantio
- **ESC** = Pausa

---

**FIM DO CHECKLIST** ✅

**Total de Testes: 185**
**Tempo Estimado de Teste Completo: 2-3 horas**
**Versão do Checklist: 1.0.0 FINAL**
