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

:: 1. Se existir a tarefa agendada especifica deste projeto, arranca instantaneamente em 40ms sem prompt UAC
schtasks /query /tn "LibreHardwareMonitor-CrystalStudio" >nul 2>&1
if %errorlevel% == 0 (
    taskkill /F /IM LibreHardwareMonitor.Windows.Forms.exe >nul 2>&1
    taskkill /F /IM LibreHardwareMonitor.exe >nul 2>&1
    schtasks /run /tn "LibreHardwareMonitor-CrystalStudio" >nul 2>&1
    exit /b 0
)

:: 2. Se ja for Administrador, executa Iniciar-Elevado.cmd diretamente
net session >nul 2>&1
if %errorlevel% == 0 (
    call "%~dp0Iniciar-Elevado.cmd"
    exit /b 0
)

:: 3. Caso contrario, eleva via launch.vbs e fecha imediatamente a consola
start "" wscript //nologo "%~dp0launch.vbs"
exit /b 0
