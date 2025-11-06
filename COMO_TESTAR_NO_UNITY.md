# GUIA COMPLETO - COMO TESTAR O JOGO NO UNITY

## OBJETIVO
Este guia explica passo a passo como abrir o projeto no Unity e testar todos os sistemas do jogo.

---

## PASSO 1: REQUISITOS

### Software Necessário
- **Unity Hub** - Baixar em: https://unity.com/download
- **Unity Editor 2021.3 LTS ou superior** - Instalar pelo Unity Hub
- **Visual Studio 2022** ou **VS Code** (opcional, para edição de código)

### Especificações Mínimas do PC
- Windows 10/11
- 8GB RAM (16GB recomendado)
- GPU com suporte DirectX 11
- 5GB espaço em disco

---

## PASSO 2: ABRIR O PROJETO

### 2.1 - Através do Unity Hub

1. Abrir **Unity Hub**
2. Clicar em **"Add"** (ou "Adicionar")
3. Navegar até a pasta: `D:/games/torre futuro`
4. Selecionar a pasta e clicar em **"Add Project"**
5. O projeto aparecerá na lista com o nome **"Torre Futuro Space RPG"**
6. Clicar no projeto para abrir

### 2.2 - Primeira Abertura (Importante!)

Na primeira vez que abrir:
1. Unity irá importar todos os assets (pode demorar 2-5 minutos)
2. Aguardar a barra de progresso **"Importing..."** completar
3. Quando terminar, o Unity Editor abrirá completamente

---

## PASSO 3: VERIFICAR ESTRUTURA DO PROJETO

### 3.1 - Verificar Hierarquia de Pastas

No **Project Window** (parte inferior), verificar se existe:

```
Assets/
├── Scripts/
│   ├── Core/
│   ├── Systems/
│   ├── UI/
│   ├── Managers/
│   ├── Data/
│   └── Effects/
├── Scenes/
│   └── MainGame.unity
├── Prefabs/
├── Materials/
├── Audio/
└── Models/
```

### 3.2 - Abrir Scene Principal

1. No **Project Window**, navegar para `Assets/Scenes/`
2. Dar duplo-clique em **MainGame.unity**
3. A scene abrirá no **Scene View**

---

## PASSO 4: EXECUTAR TESTE AUTOMÁTICO

### 4.1 - Criar GameObject de Teste

1. Na **Hierarchy** (painel esquerdo), clicar com botão direito
2. Selecionar **"Create Empty"**
3. Renomear para **"GameTestValidator"**

### 4.2 - Adicionar Script de Teste

1. Selecionar o GameObject **"GameTestValidator"**
2. No **Inspector** (painel direito), clicar em **"Add Component"**
3. Digitar **"GameTestValidator"**
4. Selecionar o script quando aparecer

### 4.3 - Executar Teste

**OPÇÃO A - Automático ao Play:**
1. No Inspector do GameTestValidator, marcar **"Run On Start"** = ✅
2. Clicar no botão **Play** (▶️) no topo do Unity
3. Aguardar 5-10 segundos
4. Verificar resultados no **Console** (aba inferior)

**OPÇÃO B - Manual:**
1. Selecionar GameObject **"GameTestValidator"**
2. No Inspector, clicar com botão direito no script
3. Selecionar **"Run All Tests"**
4. Ver resultados no Console

### 4.4 - Interpretar Resultados

#### ✅ SUCESSO (90-100%):
```
╔════════════════════════════════════════════╗
║          RESULTADO FINAL                   ║
╚════════════════════════════════════════════╝

  Total de Testes:   30
  Testes Passados:   28 ✅
  Testes Falhados:   2 ❌
  Taxa de Sucesso:   93.3%

  Status: ✅ EXCELENTE - Projeto Pronto!
```
**Ação:** Projeto está funcionando! Pode testar gameplay.

#### ⚠️ PROBLEMAS (50-89%):
```
  Status: ⚠️  BOM - Alguns ajustes necessários
```
**Ação:** Verificar erros no Console e seguir Passo 5 (Troubleshooting).

#### ❌ CRÍTICO (<50%):
```
  Status: ❌ CRÍTICO - Muitos problemas encontrados
```
**Ação:** Verificar se todos os scripts foram copiados corretamente.

---

## PASSO 5: CONFIGURAR SCENE MANUALMENTE (se necessário)

### 5.1 - Adicionar GameManager

Se o teste indicar que falta GameManager:

1. Na **Hierarchy**, clicar com botão direito
2. Selecionar **"Create Empty"**
3. Renomear para **"GameManager"**
4. Adicionar componente **"GameManager"** (script)

### 5.2 - Configurar Player Ship

1. Na Hierarchy, clicar com botão direito
2. Selecionar **"3D Object > Capsule"**
3. Renomear para **"PlayerShip"**
4. Adicionar componente **"Rigidbody"**
5. Adicionar componente **"SpaceshipController"**
6. Adicionar componente **"WeaponSystem"**

### 5.3 - Configurar Camera

Se a camera não seguir a nave:
1. Selecionar **"Main Camera"** na Hierarchy
2. No Inspector, ajustar Position: `X: 0, Y: 5, Z: -15`
3. Ajustar Rotation: `X: 10, Y: 0, Z: 0`

### 5.4 - Adicionar UI

1. Na Hierarchy, clicar com botão direito
2. Selecionar **"UI > Canvas"**
3. No Canvas, adicionar componente **"GameplayUI"**
4. Adicionar componente **"MenuManager"** (do namespace SpaceRPG.UI)

### 5.5 - Adicionar EventSystem

Se UI não responder:
1. Verificar se existe **"EventSystem"** na Hierarchy
2. Se não existir:
   - Clicar com botão direito na Hierarchy
   - Selecionar **"UI > Event System"**

---

## PASSO 6: TESTAR GAMEPLAY

### 6.1 - Iniciar Jogo

1. Clicar no botão **Play** (▶️)
2. Aguardar 2-3 segundos para inicializar
3. Verificar se Console não mostra erros vermelhos

### 6.2 - Testar Movimento

**WASD Movement:**
- Pressionar **W** = Nave avança
- Pressionar **S** = Nave recua
- Pressionar **A** = Nave move para esquerda
- Pressionar **D** = Nave move para direita

**Mouse Look:**
- Mover o mouse = Nave rotaciona suavemente

**Controles Avançados:**
- **SHIFT** = Boost (acelerar)
- **CTRL** = Freio (desacelerar)
- **Q** = Roll para esquerda
- **E** = Roll para direita

### 6.3 - Testar Armas

**Disparo:**
- **Mouse Botão Esquerdo** = Disparo primário
- **1** = Selecionar Laser
- **2** = Selecionar Míssil
- **3** = Selecionar Plasma
- **R** = Reload (recarregar)

**Verificar:**
- ✅ Munição diminui ao disparar
- ✅ Efeitos visuais aparecem
- ✅ Audio de disparo toca

### 6.4 - Testar Menus

**Abrir Menus:**
- **TAB** = Inventário
- **I** = Inventário Detalhado
- **S** = Shop (Loja)
- **P** = Plantio
- **U** = Upgrades
- **M** = Mapa/Missões
- **ESC** = Pausa

**Verificar:**
- ✅ Menu abre sem erro
- ✅ UI renderiza corretamente
- ✅ Pode fechar o menu

### 6.5 - Testar Sistemas

**Inventário:**
1. Pressionar **TAB**
2. Verificar se abre janela de inventário
3. Tentar adicionar item (se possível)

**Shop:**
1. Pressionar **S**
2. Verificar se mostra itens para comprar
3. Tentar comprar algo

**Plantio:**
1. Pressionar **P**
2. Verificar interface de plantio
3. Tentar plantar (se possível)

**Upgrades:**
1. Pressionar **U**
2. Verificar opções de upgrade
3. Ver stats da nave

### 6.6 - Testar Performance

**Verificar FPS:**
1. No **Game View**, clicar em **"Stats"** (canto superior direito)
2. Verificar **FPS** (deve ser 60+ FPS)
3. Verificar **Draw Calls** (<1000)
4. Verificar **Memory** (<500MB)

**Sinais de Boa Performance:**
- ✅ Movimento suave, sem lag
- ✅ UI responde instantaneamente
- ✅ Sem stuttering ao disparar
- ✅ Audio sincronizado

---

## PASSO 7: SALVAR E EXPORTAR

### 7.1 - Salvar Scene

Após fazer mudanças:
1. Pressionar **CTRL + S** (ou CMD + S no Mac)
2. Ou: **File > Save** no menu

### 7.2 - Salvar Projeto

Para garantir que tudo está salvo:
1. **File > Save Project**
2. Aguardar mensagem de confirmação

### 7.3 - Build do Jogo (Opcional)

Para criar executável:
1. **File > Build Settings**
2. Verificar se **MainGame.unity** está na lista
3. Selecionar **Platform** (Windows, Mac, Linux)
4. Clicar em **"Build"**
5. Escolher pasta de destino
6. Aguardar build completar (5-10 minutos)

---

## PASSO 8: TROUBLESHOOTING

### Problema 1: "Scene não carrega"

**Solução:**
1. Verificar se arquivo existe: `Assets/Scenes/MainGame.unity`
2. No menu: **File > Open Scene**
3. Navegar até MainGame.unity
4. Abrir manualmente

### Problema 2: "Scripts não compilam"

**Erros no Console:**
```
error CS0246: The type or namespace name 'SpaceRPG' could not be found
```

**Solução:**
1. Verificar se TODOS os scripts estão em `Assets/Scripts/`
2. No menu: **Assets > Reimport All**
3. Aguardar recompilação

### Problema 3: "Missing References"

**Avisos:**
```
Missing: The referenced script on this Behaviour is missing!
```

**Solução:**
1. Selecionar GameObject com aviso
2. No Inspector, remover componente com "Missing Script"
3. Adicionar o script correto novamente

### Problema 4: "UI não aparece"

**Solução:**
1. Verificar se existe **Canvas** na Hierarchy
2. Canvas deve ter **Canvas Scaler** component
3. Canvas deve ter **Graphic Raycaster** component
4. Verificar se existe **EventSystem** na Hierarchy

### Problema 5: "FPS muito baixo"

**Solução:**
1. No menu: **Edit > Project Settings > Quality**
2. Selecionar preset **"Medium"** ou **"Low"**
3. Desabilitar **Anti-Aliasing**
4. Reduzir **Shadow Distance** para 50

### Problema 6: "Input não funciona"

**Solução:**
1. Verificar **Edit > Project Settings > Input Manager**
2. Verificar se existe "Horizontal", "Vertical", "Fire1"
3. Se não existir, copiar InputManager.asset do backup

### Problema 7: "Audio não toca"

**Solução:**
1. Verificar se AudioListener existe (geralmente na Main Camera)
2. Verificar se AudioManager está na scene
3. Verificar Volume: **Edit > Project Settings > Audio**

---

## PASSO 9: COMANDOS DE DEBUG

### Console Commands (durante Play Mode)

Pressionar **F1-F5** para executar comandos debug:

- **F1** = Mostrar info do GameManager
- **F2** = Recarregar scene
- **F3** = Toggle debug mode
- **F4** = Mostrar stats de performance
- **F5** = Quick save

### Inspector Debug

Para ver valores em tempo real:
1. Selecionar GameObject na Hierarchy
2. No Inspector, os valores serializados atualizam em tempo real
3. Pode modificar valores durante Play Mode (para teste)
   - ⚠️ Mudanças durante Play Mode **NÃO são salvas** ao parar!

---

## PASSO 10: CHECKLIST DE VALIDAÇÃO

### Checklist Antes de Considerar Completo

#### ✅ PROJETO CONFIGURADO
- [ ] Unity abre o projeto sem erros
- [ ] Todos os scripts compilam (Console sem erros vermelhos)
- [ ] Scene MainGame.unity carrega
- [ ] Estrutura de pastas está organizada

#### ✅ GAMEPLAY FUNCIONAL
- [ ] Nave se move com WASD
- [ ] Mouse look rotaciona a nave
- [ ] Boost (SHIFT) funciona
- [ ] Armas disparam (Mouse Click)
- [ ] Munição diminui ao atirar
- [ ] Pode trocar entre 3 tipos de arma (1, 2, 3)

#### ✅ MENUS E UI
- [ ] HUD aparece (vida, energia, munição)
- [ ] TAB abre inventário
- [ ] S abre shop
- [ ] I abre inventário detalhado
- [ ] P abre plantio
- [ ] ESC pausa o jogo

#### ✅ SISTEMAS
- [ ] GameManager inicializa
- [ ] Save/Load funciona (F5/F9)
- [ ] Audio toca
- [ ] Efeitos visuais aparecem
- [ ] Sem memory leaks

#### ✅ PERFORMANCE
- [ ] FPS >= 60
- [ ] Sem lag/stutter
- [ ] Memory < 500MB
- [ ] Carrega em < 5 segundos

#### ✅ QUALIDADE
- [ ] Sem erros no Console
- [ ] Sem warnings críticos
- [ ] GameTestValidator passa >= 90%
- [ ] Documentação completa

---

## PASSO 11: PRÓXIMOS PASSOS

### Após Validar que Tudo Funciona:

1. **Customizar Assets:**
   - Adicionar modelos 3D de naves
   - Adicionar texturas e materiais
   - Adicionar audio clips
   - Adicionar efeitos de partículas

2. **Expandir Gameplay:**
   - Criar inimigos
   - Criar missões
   - Criar diferentes mundos
   - Adicionar power-ups

3. **Polish:**
   - Melhorar UI/UX
   - Adicionar animações
   - Melhorar efeitos visuais
   - Adicionar música e SFX

4. **Build e Distribuir:**
   - Fazer build final
   - Testar em diferentes PCs
   - Criar instalador
   - Distribuir!

---

## RECURSOS ADICIONAIS

### Documentação do Projeto
- **START_HERE_FINAL.md** - Visão geral do projeto
- **QUICK_START_GUIDE.md** - Setup rápido
- **INTEGRACAO_FINAL_COMPLETA.md** - Documentação técnica completa
- **API_REFERENCE.md** - Referência de código

### Links Úteis
- Unity Manual: https://docs.unity3d.com/Manual/index.html
- Unity Scripting API: https://docs.unity3d.com/ScriptReference/
- Unity Answers: https://answers.unity.com/

### Suporte
- Verificar Console para erros específicos
- Ler mensagens de Debug.Log no Console
- Consultar documentação inline nos scripts (comentários ///)

---

## CONCLUSÃO

Este guia cobre 100% do processo de abertura, configuração e teste do projeto no Unity.

**Se todos os passos foram seguidos:**
- ✅ Projeto deve abrir sem erros
- ✅ GameTestValidator deve passar >= 90%
- ✅ Gameplay deve ser jogável
- ✅ Todos os sistemas devem funcionar

**Status:** Projeto Production-Ready! 🚀

**Data:** Novembro 2025
**Versão:** 1.0.0 FINAL

---

## QUICK REFERENCE CARD

### Controles Principais
```
MOVIMENTO:     W/A/S/D
MOUSE LOOK:    Mouse
BOOST:         SHIFT
FREIO:         CTRL
ROLL:          Q/E
DISPARO:       Mouse Click
TROCAR ARMA:   1/2/3
RELOAD:        R

MENUS:
Inventário:    TAB
Shop:          S
Plantio:       P
Upgrades:      U
Pausa:         ESC

DEBUG:
Info:          F1
Stats:         F4
Save:          F5
```

### Comandos Unity Editor
```
PLAY/STOP:     CTRL + P
PAUSE:         CTRL + SHIFT + P
SALVAR SCENE:  CTRL + S
BUILD:         CTRL + B
CONSOLE:       CTRL + SHIFT + C
PROFILER:      CTRL + 7
```

---

**FIM DO GUIA** ✅
