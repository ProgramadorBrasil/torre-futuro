# Guia Rápido: Sistema de Modo Seguro

## 5 Passos para Implementação

### 1️⃣ Preparar a Cena
```
- Abrir Assets/Scenes/MainGame.unity
- Criar GameObject vazia: nome "GameStartup"
- NÃO adicionar script ainda (espere passo 2)
```

### 2️⃣ Adicionar Script GameStartup
```
- Selecionar GameObject "GameStartup"
- Arrastar script "GameStartup.cs" para Inspector
- OU usar Add Component > GameStartup
```

### 3️⃣ Configurar no Inspector
```
GameStartup (Script)
├─ Enable Safe Mode On Startup: ON
├─ Enable Debug Mode: ON
└─ Safe Mode Settings
   ├─ Enable Safe Mode: ON
   ├─ Safe Asset ID: CREDIT
   ├─ Assets Per Batch: 5
   ├─ Delay Between Batches: 0.1
   └─ Verbose Logs: ON
```

### 4️⃣ Testar Funcionamento
```
▶ Clicar Play no Unity
    ↓ Observar Console (Ctrl+Shift+C)
    ↓ Deve aparecer:

[SafeModeInitializer] === Iniciando Modo Seguro de Carregamento ===
[AssetLoadingManager] ✓ Modo Seguro Ativado
[AssetLoadingManager] Carregando incrementalmente X assets...
[AssetLoadingManager] === Carregamento de Assets Completo ===
```

### 5️⃣ Validar com Testes
```
- Criar GameObject vazia: "Tester"
- Adicionar script SafeModeTester.cs
- Marcar "Run Tests On Start"
- Play novamente
- Verificar resultado dos testes no Console
```

---

## ✅ Checklist de Verificação

- [ ] GameStartup adicionado à cena
- [ ] Modo Seguro está ativado (ON)
- [ ] Asset Seguro ("CREDIT") existe no ItemDatabase
- [ ] Console mostra mensagens de inicialização
- [ ] Testes passam com sucesso (100%)
- [ ] Nenhum erro aparece no Console

---

## 🔍 Verificar Status

Copie este código em qualquer script:

```csharp
void CheckStatus()
{
    var mgr = AssetLoadingManager.Instance;
    Debug.Log(mgr.GetLoadingStats());

    // Output esperado:
    // === Asset Loading Statistics ===
    // Estado: Complete
    // Assets Carregados: X/Y
    // Progresso: 100.0%
    // Modo Seguro Ativo: Sim
}
```

---

## 🐛 Problemas Comuns

### Erro: "NullReferenceException em GameStartup"
- Solução: Certifique-se que GameStartup está na cena

### Assets não carregam
- Solução: Verifique se ItemDatabase tem itens
- Console: Veja mensagens de erro

### Jogo congela
- Solução: Reduza `batchSize` para 3 ou 2

---

## 📞 Próximos Passos

1. Ler documentação completa: `SAFE_MODE_DOCUMENTATION.md`
2. Implementar UI de carregamento (veja exemplos)
3. Integrar com save/load system
4. Otimizar batch size conforme necessário

---

## 🎯 O Sistema Garante

✅ **Inicialização Segura**: Jogo sempre inicia com 1 asset seguro
✅ **Sem Freezes**: Assets carregam incrementalmente
✅ **Rastreável**: Eventos e logs de progresso
✅ **Recuperável**: Sistema de erro com fallbacks
✅ **Extensível**: Fácil de customizar e integrar

---

## Estrutura de Arquivos Criada

```
D:\games\torre futuro\
├─ Assets/Scripts/
│  ├─ Managers/
│  │  └─ AssetLoadingManager.cs ✓ NOVO
│  ├─ Core/
│  │  ├─ SafeModeInitializer.cs ✓ NOVO
│  │  └─ GameStartup.cs ✓ NOVO
│  └─ Editor/
│     └─ SafeModeTester.cs ✓ NOVO
│
└─ (Root)
   ├─ SAFE_MODE_DOCUMENTATION.md ✓ NOVO
   └─ SAFE_MODE_QUICK_START.md ✓ NOVO
```

---

## Fim do Guia Rápido! 🚀

Você está pronto para usar o sistema. Qualquer dúvida, consulte a documentação completa.
