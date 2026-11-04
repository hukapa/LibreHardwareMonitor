@echo off
setlocal

:: 1. Se a tarefa agendada existir, arranca instantaneamente em 40ms sem prompt UAC
schtasks /query /tn "LibreHardwareMonitor" >nul 2>&1
if %errorlevel% == 0 (
    taskkill /F /IM LibreHardwareMonitor.Windows.Forms.exe >nul 2>&1
    schtasks /run /tn "LibreHardwareMonitor" >nul 2>&1
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
