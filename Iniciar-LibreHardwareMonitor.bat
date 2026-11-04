@echo off
setlocal

set "EXE_PATH=%~dp0bin\Publish\LibreHardwareMonitor.Windows.Forms.exe"
set "WORK_DIR=%~dp0bin\Publish"

if not exist "%EXE_PATH%" (
    set "EXE_PATH=%~dp0bin\Publish-SingleFile\LibreHardwareMonitor.Windows.Forms.exe"
    set "WORK_DIR=%~dp0bin\Publish-SingleFile"
)

if not exist "%EXE_PATH%" (
    set "EXE_PATH=%~dp0bin\Release\net8.0-windows\LibreHardwareMonitor.Windows.Forms.exe"
    set "WORK_DIR=%~dp0bin\Release\net8.0-windows"
)

:: 1. Se a tarefa agendada existir, arranca instantaneamente em 40ms sem prompt UAC
schtasks /query /tn "LibreHardwareMonitor" >nul 2>&1
if %errorlevel% == 0 (
    schtasks /run /tn "LibreHardwareMonitor" >nul 2>&1
    exit /b 0
)

:: 2. Se ja for Administrador, arranca diretamente sem atraso
net session >nul 2>&1
if %errorlevel% == 0 (
    cd /d "%WORK_DIR%"
    start "" "%EXE_PATH%"
    exit /b 0
)

:: 3. Caso contrario, eleva via ShellExecute e fecha imediatamente a consola
start "" wscript //nologo "%~dp0launch.vbs"
exit /b 0
