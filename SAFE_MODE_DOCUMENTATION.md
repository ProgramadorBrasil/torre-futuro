# Sistema Profissional de Modo Seguro para Torre Futuro

## Visão Geral

O **Sistema de Modo Seguro** é uma solução profissional de carregamento de assets desenvolvida para o jogo **Torre Futuro**. Este sistema resolve problemas de carregamento de assets ao implementar um **modo de segurança** que inicializa o jogo com apenas **1 asset** e recarrega os demais **incrementalmente** durante a execução.

---

## 📋 Características Principais

✅ **Modo Seguro Inteligente**: Inicia com 1 asset seguro, evita crashes por assets problemáticos
✅ **Carregamento Incremental**: Assets são carregados gradualmente sem congelar o jogo
✅ **Assíncrono Profissional**: Usa Coroutines para não impactar performance
✅ **Callbacks de Evento**: Sistema de eventos para monitoramento em tempo real
✅ **Recarregamento em Tempo de Execução**: Recarregue categorias ou assets específicos
✅ **Validação de Integridade**: Verifica a saúde do ItemDatabase
✅ **Sistema de Testes**: Bateria completa de testes automatizados
✅ **Logging Detalhado**: Debug logs configurable para troubleshooting
✅ **Padrão Enterprise**: Segue práticas de arquitetura profissional

---

## 🏗️ Arquitetura do Sistema

### Componentes Principais

```
SafeModeInitializer (Orquestrador Principal)
    ↓
    ├─→ AssetLoadingManager (Gerenciador de Assets)
    │    ├─ Modo Seguro
    │    ├─ Carregamento Incremental
    │    └─ Recarregamento em Runtime
    │
    ├─→ ItemDatabase (Banco de Dados)
    │    └─ Validação de Integridade
    │
    └─→ GameManagerRPG (Gerenciador do Jogo)
         └─ Sincronização com Inicialização
```

### Fluxo de Inicialização

```
1. GameStartup Inicia
   ↓
2. SafeModeInitializer Cria
   ↓
3. Passo 1: Inicializar Base Managers
   ├─ ItemDatabase.Initialize()
   ├─ AssetLoadingManager.Instance
   └─ GameManagerRPG.Instance
   ↓
4. Passo 2: Configurar Modo Seguro
   ├─ SetSafeAsset(assetID)
   └─ InitializeSafeMode() → Carrega 1 Asset
   ↓
5. Passo 3: Carregamento Incremental
   ├─ StartIncrementalLoading()
   └─ WaitForLoadingComplete()
   ↓
6. Passo 4: Inicializar Sistemas do Jogo
   └─ GameManager Confirma Inicialização
   ↓
7. Jogo Pronto ✓
```

---

## 🚀 Como Usar

### Instalação Rápida

1. **Copiar os arquivos** para o seu projeto:
   - `AssetLoadingManager.cs` → `Assets/Scripts/Managers/`
   - `SafeModeInitializer.cs` → `Assets/Scripts/Core/`
   - `GameStartup.cs` → `Assets/Scripts/Core/`
   - `SafeModeTester.cs` → `Assets/Scripts/Editor/`

2. **Criar uma GameObject vazia** na cena inicial:
   - Nome: "GameStartup"
   - Adicionar o script `GameStartup.cs`

3. **Configurar no Inspector**:
   - Marcar `Enable Safe Mode On Startup`
   - Definir `Safe Asset ID` (padrão: "CREDIT")

4. **Executar o jogo** - O sistema se inicializa automaticamente!

### Uso Básico em Código

```csharp
// Acessar o gerenciador de assets
var assetManager = AssetLoadingManager.Instance;

// Verificar progresso
float progress = assetManager.LoadingProgress;
int loaded = assetManager.LoadedAssetCount;
int total = assetManager.TotalAssetCount;

// Inscrever-se em eventos
assetManager.OnLoadingProgress += (current, total, percentage) =>
{
    Debug.Log($"Progresso: {percentage:F1}%");
};

assetManager.OnLoadingComplete += () =>
{
    Debug.Log("Todos os assets carregados!");
};

// Verificar se um asset foi carregado
bool isLoaded = assetManager.IsAssetLoaded("WEAPON_01");

// Recarregar assets específicos
assetManager.ReloadAssets("WEAPON_01", "WEAPON_02");

// Recarregar uma categoria
assetManager.ReloadAssetCategory(ItemType.Weapon);

// Forçar carregamento imediato de todos
assetManager.ForceLoadAllAssets();
```

---

## ⚙️ Configuração Avançada

### SafeModeInitializer Configurações

```csharp
[SerializeField] private SafeModeInitializer.SafeModeSettings settings;
```

| Configuração | Tipo | Padrão | Descrição |
|---|---|---|---|
| `enableSafeMode` | bool | true | Ativa modo seguro |
| `safeAssetID` | string | "CREDIT" | ID do asset seguro inicial |
| `assetsPerBatch` | int | 5 | Assets carregados por lote |
| `delayBetweenBatches` | float | 0.1f | Delay (segundos) entre lotes |
| `showLoadingUI` | bool | true | Mostrar tela de carregamento |
| `loadingScreenDuration` | float | 2f | Duração da tela de loading |
| `verboseLogs` | bool | true | Ativar logs detalhados |

### AssetLoadingManager Configurações

```csharp
[SerializeField] private AssetLoadingConfig config;
```

| Configuração | Tipo | Padrão | Descrição |
|---|---|---|---|
| `enableSafeMode` | bool | true | Ativa modo seguro |
| `safeAssetID` | string | "CREDIT" | ID do asset seguro |
| `batchSize` | int | 5 | Assets por batch |
| `delayBetweenBatches` | float | 0.1f | Delay entre batches |
| `debugLogs` | bool | true | Ativar debug logs |

### Customizar Asset Seguro

```csharp
// No GameStartup.cs, altere:
public class GameStartup : MonoBehaviour
{
    private void InitializeGameWithSafeMode()
    {
        var initializer = initializerGO.AddComponent<SafeModeInitializer>();

        // Customizar segurança
        initializer.SetSafeAsset("SEU_ASSET_ID");
    }
}

// Ou em runtime:
AssetLoadingManager.Instance.SetSafeAsset("NOVO_ASSET_ID");
```

---

## 📊 Monitoramento e Debug

### Obter Estatísticas

```csharp
var stats = AssetLoadingManager.Instance.GetLoadingStats();
Debug.Log(stats);

// Output:
// === Asset Loading Statistics ===
// Estado: Complete
// Assets Carregados: 45/50
// Progresso: 90.0%
// Modo Seguro Ativo: Sim
// Asset Seguro: CREDIT
```

### Sistema de Eventos

```csharp
var assetManager = AssetLoadingManager.Instance;

// Quando carregamento começa
assetManager.OnLoadingStarted += () =>
{
    Debug.Log("Carregamento iniciado!");
};

// Progresso em tempo real
assetManager.OnLoadingProgress += (current, total, percentage) =>
{
    progressBar.fillAmount = percentage;
    progressText.text = $"{percentage:F1}%";
};

// Quando completa
assetManager.OnLoadingComplete += () =>
{
    Debug.Log("Carregamento completo!");
};

// Em caso de erro
assetManager.OnLoadingError += (error) =>
{
    Debug.LogError($"Erro: {error}");
};
```

### Validação do Database

```csharp
bool isValid = ItemDatabase.Instance.ValidateDatabase();
if (isValid)
{
    Debug.Log("Database está íntegro!");
}
else
{
    Debug.LogError("Problemas encontrados no database");
}
```

---

## 🧪 Testes Automatizados

### Executar Bateria de Testes

1. **Adicionar SafeModeTester na cena**:
   - GameObject vazia
   - Adicionar script `SafeModeTester.cs`
   - Marcar `Run Tests On Start`

2. **Configurar Testes**:
   ```csharp
   [SerializeField] private SafeModeTester.TestConfiguration config;
   ```

3. **Testes Inclusos**:
   - ✓ Inicialização do Modo Seguro
   - ✓ Carregamento de Assets
   - ✓ Carregamento Incremental
   - ✓ Recarregamento de Assets
   - ✓ Validação de Integridade

### Output dos Testes

```
╔════════════════════════════════════╗
║   INICIANDO BATERIA DE TESTES     ║
╚════════════════════════════════════╝

➤ Teste 1: Inicialização do Modo Seguro
═════════════════════════════════════
✓ Modo Seguro foi ativado
✓ Pelo menos 1 asset foi carregado
✓ Asset Seguro Carregado: 1 asset(s)

...

═════════════════════════════════════
RESUMO:
  Total de Testes: 5
  ✓ Aprovados: 5
  ✗ Falhados: 0
  Taxa de Sucesso: 100.0%
═════════════════════════════════════
```

---

## 🔧 Troubleshooting

### Problema: Assets não carregam

**Solução**:
1. Verifique se o `ItemDatabase` está inicializado
2. Verifique se o `safeAssetID` existe no banco
3. Ative `debugLogs` para ver mensagens detalhadas
4. Execute `SafeModeTester` para diagnosticar

```csharp
// Debug
var mgr = AssetLoadingManager.Instance;
Debug.Log($"Assets Carregados: {mgr.LoadedAssetCount}");
Debug.Log($"Total Esperado: {mgr.TotalAssetCount}");
Debug.Log(mgr.GetLoadingStats());
```

### Problema: Modo Seguro não ativa

**Solução**:
1. Certifique-se que `enableSafeMode = true`
2. Verifique se `GameStartup` está na cena
3. Confirme que `ItemDatabase` está funcionando
4. Veja console para mensagens de erro

### Problema: Jogo congela durante carregamento

**Solução**:
1. Aumente `delayBetweenBatches` (ex: 0.2f)
2. Diminua `batchSize` (ex: 3)
3. Use `ForceLoadAllAssets()` como último recurso

```csharp
// Ajuste em runtime
var config = new AssetLoadingManager.AssetLoadingConfig
{
    batchSize = 3,
    delayBetweenBatches = 0.2f
};
```

---

## 🎯 Casos de Uso

### Caso 1: Inicialização Segura (Padrão)
```
GameStartup → SafeModeInitializer → Carrega Asset Seguro → Carrega Demais Assets
```
✅ Recomendado para produção

### Caso 2: Carregamento Rápido
```
AssetLoadingManager.Instance.ForceLoadAllAssets()
```
✅ Quando você já validou que todos os assets funcionam

### Caso 3: Carregamento Sob Demanda
```
// Quando entrada em loja
AssetLoadingManager.Instance.ReloadAssetCategory(ItemType.Weapon);

// Quando entrada em área de cultivo
AssetLoadingManager.Instance.ReloadAssetCategory(ItemType.Seed);
```
✅ Otimiza memória em jogo grande

### Caso 4: Recuperação de Erro
```
try
{
    assetManager.StartIncrementalLoading();
}
catch (Exception e)
{
    Debug.LogError($"Erro: {e.Message}");
    assetManager.ResetLoading();
    assetManager.InitializeSafeMode();
}
```
✅ Resiliência contra falhas

---

## 📈 Performance

### Benchmarks (Simulado)

| Operação | Tempo | Notas |
|---|---|---|
| Inicializar Modo Seguro | 100ms | Carrega 1 asset |
| Carregar 50 Assets | 5.2s | Incrementalmente, batchSize=5 |
| Recarregar Categoria | 300ms | Depende da categoria |
| Validar Database | 50ms | Com 100+ itens |

### Otimizações Implementadas

- **Batch Loading**: Evita carregar tudo de uma vez
- **Async Coroutines**: Não bloqueia thread principal
- **Caching**: Assets carregados são cacheados
- **Lazy Loading**: Assets só carregam quando necessário
- **Event-Driven**: Sem polling, apenas eventos

---

## 🔐 Segurança e Integridade

### Validação Automática

```csharp
// Validação que ocorre automaticamente
ItemDatabase.Instance.ValidateDatabase();

// Verifica:
// ✓ Assets null
// ✓ IDs duplicados
// ✓ Preços inválidos
// ✓ Assets sem ícones (warning)
```

### Detecção de Corrupção

```csharp
// Detectar problema em tempo real
assetManager.OnLoadingError += (error) =>
{
    // Registrar erro
    LogErrorToServer(error);

    // Tentar recuperar
    assetManager.ResetLoading();
};
```

---

## 📚 Referência de API

### AssetLoadingManager

```csharp
// Propriedades
public bool IsLoading { get; }
public bool IsSafeModeActive { get; }
public float LoadingProgress { get; }
public int LoadedAssetCount { get; }
public int TotalAssetCount { get; }

// Métodos
public void InitializeSafeMode()
public void StartIncrementalLoading()
public IEnumerator WaitForLoadingComplete()
public void ReloadAssets(params string[] assetIDs)
public void ReloadAssetCategory(ItemType category)
public string GetLoadingStats()
public List<ItemData> GetLoadedAssets()
public bool IsAssetLoaded(string assetID)
public void ForceLoadAllAssets()
public void SetSafeAsset(string assetID)
public void ResetLoading()

// Eventos
public event LoadingEventHandler OnLoadingStarted
public event LoadingProgressHandler OnLoadingProgress
public event LoadingEventHandler OnLoadingComplete
public event LoadingErrorHandler OnLoadingError
```

### SafeModeInitializer

```csharp
public class SafeModeInitializer : MonoBehaviour
{
    // Configurações
    [SerializeField] private SafeModeSettings settings;

    // Propriedade
    public bool IsInitializationComplete { get; }
}
```

### GameStartup

```csharp
public class GameStartup : MonoBehaviour
{
    [SerializeField] private bool enableSafeModeOnStartup = true;
    [SerializeField] private bool enableDebugMode = true;

    private void InitializeGameWithSafeMode()
    private void InitializeGameNormally()
    private void OnGameInitializationComplete()
    private void PrintGameStatus()
}
```

---

## 🎓 Exemplos Práticos

### Exemplo 1: Monitorar Carregamento com UI

```csharp
public class LoadingScreenUI : MonoBehaviour
{
    [SerializeField] private Image progressBar;
    [SerializeField] private Text percentageText;
    [SerializeField] private Text assetCountText;

    private void Start()
    {
        var mgr = AssetLoadingManager.Instance;
        mgr.OnLoadingProgress += OnLoadingProgress;
        mgr.OnLoadingComplete += OnLoadingComplete;
    }

    private void OnLoadingProgress(int current, int total, float percentage)
    {
        progressBar.fillAmount = percentage / 100f;
        percentageText.text = $"{percentage:F1}%";
        assetCountText.text = $"{current}/{total}";
    }

    private void OnLoadingComplete()
    {
        // Esconder tela de loading
        gameObject.SetActive(false);
    }
}
```

### Exemplo 2: Recarregar Assets Contextuais

```csharp
public class ShopSystem : MonoBehaviour
{
    public void OpenShop()
    {
        // Garantir que armas estão carregadas
        AssetLoadingManager.Instance.ReloadAssetCategory(ItemType.Weapon);

        // Esperar carregamento
        StartCoroutine(WaitAndOpenShop());
    }

    private IEnumerator WaitAndOpenShop()
    {
        yield return new WaitForSeconds(0.5f);

        // Validar que temos armas
        var weapons = ItemDatabase.Instance.GetItemsByType(ItemType.Weapon);
        if (weapons.Count > 0)
        {
            ShowShopUI();
        }
    }
}
```

### Exemplo 3: Recuperação de Erro

```csharp
public class RobustAssetLoader : MonoBehaviour
{
    private AssetLoadingManager mgr;

    private void Start()
    {
        mgr = AssetLoadingManager.Instance;
        mgr.OnLoadingError += OnAssetLoadError;
    }

    private void OnAssetLoadError(string error)
    {
        Debug.LogError($"Falha ao carregar assets: {error}");

        // Tentar recuperar
        mgr.ResetLoading();
        mgr.InitializeSafeMode();
        mgr.ForceLoadAllAssets(); // Carregar tudo rapidinho

        // Notificar usuário
        ShowErrorMessage("Assets recarregados. Continuar?");
    }
}
```

---

## 📝 Checklist de Implementação

- [ ] Copiar arquivos para o projeto
- [ ] Criar GameObject "GameStartup" na cena inicial
- [ ] Adicionar script GameStartup ao GameObject
- [ ] Configurar Safe Asset ID no Inspector
- [ ] Testar com SafeModeTester
- [ ] Verificar logs de inicialização
- [ ] Validar que todos os assets carregam
- [ ] Testar recarregamento em tempo de execução
- [ ] Implementar UI de carregamento
- [ ] Fazer deploy em produção

---

## 🤝 Contribuição e Suporte

Esse sistema foi desenvolvido seguindo padrões profissionais de:
- **Clean Code** (Robert C. Martin)
- **Design Patterns** (Gang of Four)
- **SOLID Principles**
- **Unity Best Practices**

---

## 📄 Licença

Parte do projeto Torre Futuro - Space RPG

---

## 🎉 Conclusão

Parabéns! Você agora tem um **sistema profissional de carregamento de assets com modo seguro**. Use-o para:

✅ Iniciar o jogo sem erros de assets
✅ Carregar incrementalmente sem congelar
✅ Recarregar em tempo de execução sob demanda
✅ Validar integridade de dados
✅ Monitorar progresso em tempo real

**Bom desenvolvimento! 🚀**
