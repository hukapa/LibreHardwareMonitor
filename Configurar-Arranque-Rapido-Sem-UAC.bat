@echo off
title Configurar Arranque Sem UAC - LibreHardwareMonitor
echo =======================================================================
echo   Configuracao de Arranque Instantaneo sem Confirmacoes UAC
echo =======================================================================
echo.

set "EXE_PATH=%~dp0bin\Publish-v2\LibreHardwareMonitor.Windows.Forms.exe"
if not exist "%EXE_PATH%" (
    set "EXE_PATH=%~dp0bin\Publish\LibreHardwareMonitor.Windows.Forms.exe"
)
if not exist "%EXE_PATH%" (
    set "EXE_PATH=%~dp0bin\Publish-SingleFile\LibreHardwareMonitor.Windows.Forms.exe"
)
if not exist "%EXE_PATH%" (
    set "EXE_PATH=%~dp0bin\Release\net8.0-windows\LibreHardwareMonitor.Windows.Forms.exe"
)

net session >nul 2>&1
if %errorlevel% neq 0 (
    echo A solicitar elevacao de administrador para registar a tarefa no Agendador...
    powershell -NoProfile -ExecutionPolicy Bypass -Command "Start-Process '%~f0' -Verb RunAs"
    exit /b 0
)

:: Regista como LibreHardwareMonitor-CrystalStudio para nunca colidir com tarefas de outros diretorios
schtasks /create /tn "LibreHardwareMonitor-CrystalStudio" /tr "\"%EXE_PATH%\"" /sc onlogon /rl highest /f >nul 2>&1
if %errorlevel% == 0 (
    echo Sucesso: Tarefa 'LibreHardwareMonitor-CrystalStudio' registada com sucesso!
    echo.
    echo A partir de agora, o LibreHardwareMonitor deste projeto arranca em ~40ms:
    echo 1. Sem caixas de confirmacao do Controlo de Conta de Utilizador (UAC).
    echo 2. Sem consolas pretas abertas.
    echo.
    echo Pode executar via Iniciar-LibreHardwareMonitor.bat ou launch.vbs.
) else (
    echo Erro ao registar a tarefa no Agendador de Tarefas do Windows.
)

echo.
pause
