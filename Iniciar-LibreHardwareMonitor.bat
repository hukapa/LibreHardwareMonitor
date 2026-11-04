@echo off
title LibreHardwareMonitor (.NET 8.0 Release - Fork hukapa)

:: Prioritizar o executavel standalone de ficheiro unico se existir (imune a bloqueios de DLLs soltas)
set "EXE_PATH=%~dp0bin\Publish-SingleFile\LibreHardwareMonitor.Windows.Forms.exe"
set "WORK_DIR=%~dp0bin\Publish-SingleFile"

if not exist "%EXE_PATH%" (
    set "EXE_PATH=%~dp0bin\Release\net8.0-windows\LibreHardwareMonitor.Windows.Forms.exe"
    set "WORK_DIR=%~dp0bin\Release\net8.0-windows"
)

:: O LibreHardwareMonitor necessita de permissoes de Administrador para ler sensores da motherboard, CPU e GPU
net session >nul 2>&1
if %errorlevel% == 0 (
    cd /d "%WORK_DIR%"
    start "" "%EXE_PATH%"
) else (
    powershell -NoProfile -ExecutionPolicy Bypass -Command "Start-Process -FilePath '%EXE_PATH%' -WorkingDirectory '%WORK_DIR%' -Verb RunAs"
)
