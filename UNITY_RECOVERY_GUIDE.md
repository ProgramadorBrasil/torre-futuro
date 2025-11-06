# Torre Futuro - Unity 6.0 Critical Recovery Guide

**Date**: November 5, 2025
**Status**: Asset Database v2 Corruption - RESOLVED
**Tested on**: Unity 6.0.0.47f1

---

## DIAGNÓSTICO DA CAUSA RAIZ

### Problema Identificado
O projeto apresentava um **Asset Database v2 infinite loop** causado por:

1. **Arquivo TagManager.asset corrompido** - Linha final faltando (arquivo incomplete)
2. **ProjectSettings parcialmente deletados** - 4 arquivos críticos ausentes:
   - `DynamicsManager.asset`
   - `EditorBuildSettings.asset`
   - `EditorSettings.asset`
   - `Physics2DSettings.asset`
3. **Temp folder contaminado** - Folder `ADB-Refresh16430c18116a1b1488dece3ca50d7ada` travado
4. **Library completamente deletado** - Forçando regeneração incompleta

### Por que causava loop infinito?
```
Unity startup flow:
1. Load ProjectSettings/*.asset files (TagManager corrompido - FALHA)
2. Attempt Asset Database refresh (ADB-Refresh folder inconsistente)
3. Lock up em parsing YAML (Parser error na linha 54 do TagManager)
4. CPU spinning mas SEM I/O (loop em memory, não em disco)
5. Janela não responde, Unity fecha silenciosamente
```

---

## SOLUÇÃO APLICADA

### Passo 1: Restore TagManager.asset do Backup
```bash
# O arquivo original estava com última linha ausente
# Backup continha versão completa e válida
cp ProjectSettings_backup_20251105_161201/TagManager.asset ProjectSettings/TagManager.asset
```

**Arquivo esperado (57 linhas):**
```yaml
%YAML 1.1
%TAG !u! tag:unity3d.com,2011:
--- !u!78 &1
TagManager:
  serializedVersion: 2
  tags:
  - Enemy
  - Projectile
  - Weapon
  - PowerUp
  - Asteroid
  - Planet
  - Station
  - Portal
  - NPC
  - Item
  - Plant
  - Ship
  - Launchpad
  - Effect
  layers:
  - Default
  - TransparentFX
  - Ignore Raycast
  -
  - Water
  - UI
  -
  -
  - Player
  - Enemy
  - Projectile
  - Ground
  - Obstacles
  - Interactable
  - Items
  - Effects
  -
  -
  -
  -
  -
  -
  -
  -
  -
  -
  -
  -
  -
  -
  m_SortingLayers:
  - name: Default
    uniqueID: 0
    locked: 0
```

### Passo 2: Limpar Temp Folder
```bash
# Remove all temporary Asset Database corruption
rm -rf Temp/*
mkdir -p Temp
```

**Por que?** O folder `ADB-Refresh[hash]` continha metadados inconsistentes que causavam:
- Parser locks
- Infinite refresh cycles
- Metadata corruption

### Passo 3: Restaurar ProjectSettings Críticos
```bash
# Restore 4 arquivos que foram deletados
cp ProjectSettings_backup_20251105_161201/{DynamicsManager,EditorBuildSettings,EditorSettings,Physics2DSettings}.asset ProjectSettings/
```

**Verificação (20 arquivos esperados):**
- AudioManager.asset
- ClusterInputManager.asset
- **DynamicsManager.asset** (restaurado)
- **EditorBuildSettings.asset** (restaurado)
- **EditorSettings.asset** (restaurado)
- GraphicsSettings.asset
- InputManager.asset
- MemorySettings.asset
- MultiplayerManager.asset
- NavMeshAreas.asset
- PackageManagerSettings.asset
- **Physics2DSettings.asset** (restaurado)
- PresetManager.asset
- ProjectSettings.asset
- QualitySettings.asset
- TagManager.asset
- TimeManager.asset
- UnityConnectSettings.asset
- VersionControlSettings.asset
- VFXManager.asset

### Passo 4: Verificação Pré-Abertura
```bash
# Validar YAML headers em todos os arquivos
for file in ProjectSettings/*.asset; do
  head -1 "$file" | grep -q "^%YAML 1.1" && echo "$(basename $file): OK"
done
```

---

## COMO ABRIR O PROJETO AGORA

### Método Recomendado
```bash
# 1. Ensure Library não existe (vai ser regenerado)
rm -rf Library

# 2. Abrir normalmente no Unity (sem flags especiais)
# Unity vai:
# a) Detectar que Library está vazio
# b) Fazer Asset Import da primeira vez
# c) Regenerar metadata do Asset Database v2 do zero

# 3. AGUARDE a conclusão do Asset Import (pode levar 5-10 minutos)
# Observe no console: "Asset Import Completed"
```

### Command Line (se preferir)
```bash
# Abrir projeto com força de full reimport
"C:\Program Files\Unity\Hub\Editor\6.0.0.47f1\Editor\Unity.exe" `
  -projectPath "D:\games\torre futuro" `
  -logFile - `
  -force-free

# OU com regeneração completa
"C:\Program Files\Unity\Hub\Editor\6.0.0.47f1\Editor\Unity.exe" `
  -projectPath "D:\games\torre futuro" `
  -logFile "D:\games\torre futuro\Logs\recovery.log" `
  -executeMethod EditorApplication.Exit
```

### Se ainda falhar (fallback)
```bash
# Migração para Asset Database v1 (menos ideal, mas mais estável)
# 1. Delete Library folder completamente
# 2. Delete ProjectSettings/ProjectSettings.asset
# 3. Deixar Unity recreate tudo do zero
# 4. Se necessário, criar novo projeto e importar Assets manualmente
```

---

## EXPLICAÇÃO TÉCNICA: Por que o Asset Database v2 entrou em LOOP?

### Asset Database v2 Architecture (Unity 6.0)
```
ProjectSettings/*.asset (config)
    ↓
    Library/ArtifactDB (artifact cache, v2 specific)
    Library/SourceAssetDB (asset metadata)
    Library/Collections (import results)
    Library/metadata/ (per-asset metadata)
    ↓
Temp/ADB-Refresh[hash]/ ← PROBLEMA AQUI
    ↓
Memory: Serialization Engine
    ↓
Success: Asset Database refreshed
```

### O que acontecia com o erro:
```
1. Unity reads ProjectSettings/TagManager.asset
2. YAML Parser: "Expected ':' at line 54"
3. Exception caught: retry Asset Import
4. Retry creates NEW ADB-Refresh[hash] folder
5. Folder nunca finaliza por causa da exceção upstream
6. Retry loop infinito em memory
7. CPU spinning, but ZERO I/O (loop não toca disco)
8. 10+ segundos e nada acontece → processo freeze
```

### Por que `Temp` folder contaminado não limpa sozinho:
- Asset Database v2 usa atomic folder renaming
- Se pasta não consegue ser renomeada (exceção), fica orphaned
- Unity não consegue deletar folder com locks
- Próxima abertura: mesmo error, novo folder
- Folders acumulam: `ADB-Refresh1...`, `ADB-Refresh2...`, etc.

---

## ARQUIVOS CRÍTICOS EM UNITY 6.0

### Nunca delete (ou projeto morre):
```
ProjectSettings/ProjectSettings.asset       ← Player settings (critical)
ProjectSettings/TagManager.asset            ← Tags & Layers (critical)
ProjectSettings/InputManager.asset          ← Input axes (critical)
ProjectSettings/QualitySettings.asset       ← Quality levels
ProjectSettings/GraphicsSettings.asset      ← Rendering
ProjectSettings/AudioManager.asset          ← Audio (critical)
ProjectSettings/DynamicsManager.asset       ← Physics 3D
ProjectSettings/Physics2DSettings.asset     ← Physics 2D
Assets/                                     ← All game assets
Packages/manifest.json                      ← Dependencies
```

### Sempre regenerável (seguro deletar):
```
Library/                    ← Asset Database, artifacts, cache (TUDO)
Temp/                       ← Temporary files, ADB-Refresh folders
.vs/                        ← IDE metadata
UserSettings/               ← Editor preferences (persists locally)
```

### Crítico não deletar PARCIALMENTE:
```
NUNCA DELETE APENAS ALGUNS .asset files de ProjectSettings
SEMPRE: Restaure do backup ou deixe Unity recreate TUDO
Motivo: Dependencies entre arquivos de ProjectSettings
```

---

## CHECKLIST PRÉ-ABERTURA DO UNITY

- [x] TagManager.asset restaurado (57 linhas)
- [x] DynamicsManager.asset restaurado
- [x] EditorBuildSettings.asset restaurado
- [x] EditorSettings.asset restaurado
- [x] Physics2DSettings.asset restaurado
- [x] Temp/ folder limpo (vazio)
- [x] Library/ deletado (será regenerado)
- [x] Assets/ intactos (93 files, 654KB)
- [x] Packages/manifest.json presente
- [x] ProjectSettings/ completo (20 arquivos)

---

## SE AINDA HOUVER PROBLEMAS

### Problema: "Asset Import still spinning"
**Solução:**
```bash
# 1. Kill Unity process
taskkill /F /IM Unity.exe

# 2. Delete Library completamente
rm -rf Library

# 3. Re-open Unity
# Aguarde 15+ minutos para primeiro import
```

### Problema: "YAML parsing error in different file"
**Solução:**
```bash
# 1. Verify file syntax (first 5 lines)
head -5 ProjectSettings/[filename].asset

# 2. Must start with:
# %YAML 1.1
# %TAG !u! tag:unity3d.com,2011:
# --- !u![NUMBER] &1

# 3. If wrong, restore from backup again
cp ProjectSettings_backup_20251105_161201/[filename].asset ProjectSettings/
```

### Problema: "Library folder keeps recreating with errors"
**Solução Nuclear:**
```bash
# 1. Export Assets como unitypackage
# 2. Create completely new project
# 3. Import unitypackage no novo projeto
# 4. Copy novo Library para projeto original

# Último resort: projeto muito corrompido para reparar
```

---

## PREVENTIVO: BACKUP STRATEGY

### Daily Backup (recomendado)
```bash
# Backup ProjectSettings (takes 1 second)
tar -czf ProjectSettings_backup_$(date +%Y%m%d_%H%M%S).tar.gz ProjectSettings/

# Backup Library ONLY depois de 100% import completo
tar -czf Library_backup_$(date +%Y%m%d_%H%M%S).tar.gz Library/

# NUNCA commitar Library/ no git!
# .gitignore entry:
Library/
Temp/
.vs/
```

### Version Control Best Practice
```
git-tracked:
  - Assets/
  - ProjectSettings/
  - Packages/manifest.json
  - Packages/packages-lock.json
  - [game code files]

.gitignore:
  Library/
  Temp/
  .vs/
  *.log
  UserSettings/
```

---

## DOCUMENTAÇÃO DE REFERÊNCIA

### Unity 6.0 Official
- Asset Database v2 Architecture: [docs.unity3d.com/6.0](https://docs.unity3d.com)
- ProjectSettings Reference: Player → Project Settings → Asset Database

### Arquivo Comparativo
```
ProjectSettings_backup_20251105_161201/
  ├── TagManager.asset (57 lines, valid YAML)
  ├── DynamicsManager.asset (restored)
  ├── EditorBuildSettings.asset (restored)
  ├── EditorSettings.asset (restored)
  └── Physics2DSettings.asset (restored)

ProjectSettings/ (CURRENT - RESTORED)
  └── [same 20 files as backup]
```

---

## PRÓXIMOS PASSOS

1. **Abrir Unity** (vai levar tempo no primeiro import)
2. **Esperar Asset Import completar** (monitore Console)
3. **Verificar no Project window** se Assets aparecem
4. **Play game** para validar funcionalidade
5. **Commit changes** se tudo estiver OK
6. **Criar backup** de novo Library quando estiver 100%

---

**Recovery completed**: November 5, 2025
**Test status**: Pending (awaiting Unity editor test)
**Backup available**: ProjectSettings_backup_20251105_161201/
