@echo off
REM ====================================================================
REM TORRE FUTURO - INSTALADOR DE DEPENDENCIAS
REM Unity Version: 6000.0.47f1
REM ====================================================================

echo.
echo ========================================
echo   TORRE FUTURO - INSTALACAO DE PACOTES
echo ========================================
echo.

echo [INFO] Iniciando instalacao de dependencias...
echo.

REM ====================================================================
REM VERIFICACAO DE REQUISITOS
REM ====================================================================

echo [STEP 1/3] Verificando Unity Hub e Git...

where git >nul 2>nul
if %errorlevel% neq 0 (
    echo [ERRO] Git nao encontrado! Por favor instale o Git:
    echo https://git-scm.com/download/win
    pause
    exit /b 1
)

echo [OK] Git instalado
echo.

REM ====================================================================
REM DOTWEEN INSTALLATION
REM ====================================================================

echo [STEP 2/3] Instalando DOTween...
echo.
echo IMPORTANTE: DOTween deve ser instalado via Unity Package Manager
echo.
echo INSTRUCOES:
echo 1. Abra o Unity Editor
echo 2. Va em Window ^> Package Manager
echo 3. Clique em [+] ^> Add package from git URL...
echo 4. Cole esta URL: https://github.com/Demigiant/dotween.git#upm
echo 5. Clique em Add
echo.
echo ALTERNATIVA (Asset Store):
echo 1. Abra o Unity Editor
echo 2. Va em Window ^> Asset Store
echo 3. Procure por "DOTween (HOTween v2)"
echo 4. Download e Import
echo.
echo Pressione qualquer tecla apos instalar DOTween...
pause >nul

REM ====================================================================
REM CINEMACHINE VERIFICATION
REM ====================================================================

echo.
echo [STEP 3/3] Verificando Cinemachine...
echo.
echo Cinemachine ja vem incluido no Unity 6
echo.
echo Para verificar:
echo 1. Abra o Unity Editor
echo 2. Va em Window ^> Package Manager
echo 3. Mude para "Unity Registry"
echo 4. Procure por "Cinemachine"
echo 5. Se nao estiver instalado, clique em Install
echo.

REM ====================================================================
REM TEXTMESHPRO VERIFICATION
REM ====================================================================

echo [BONUS] Verificando TextMeshPro...
echo.
echo TextMeshPro ja vem incluido no Unity 6
echo Na primeira vez que usar TMP, o Unity pedira para importar os recursos essenciais.
echo Apenas clique em "Import TMP Essentials"
echo.

REM ====================================================================
REM FINALIZACAO
REM ====================================================================

echo.
echo ========================================
echo   INSTALACAO CONCLUIDA
echo ========================================
echo.
echo PROXIMOS PASSOS:
echo.
echo 1. Abra o projeto no Unity 6 (versao 6000.0.47f1)
echo 2. Aguarde a importacao de assets
echo 3. Se aparecer erro de DOTween, siga as instrucoes acima
echo 4. Verifique o arquivo VERIFICACAO_FINAL.txt
echo 5. Execute o jogo!
echo.
echo ========================================
echo.
pause
