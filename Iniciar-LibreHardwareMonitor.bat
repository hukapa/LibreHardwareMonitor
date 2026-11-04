@echo off
title LibreHardwareMonitor (.NET 8.0 Release - Fork hukapa)
cd /d "%~dp0bin\Release\net8.0-windows"

:: O LibreHardwareMonitor necessita de permissoes de Administrador para ler sensores da motherboard, CPU e GPU
net session >nul 2>&1
if %errorlevel% == 0 (
    start "" "%~dp0bin\Release\net8.0-windows\LibreHardwareMonitor.Windows.Forms.exe"
) else (
    powershell -NoProfile -ExecutionPolicy Bypass -Command "Start-Process -FilePath '%~dp0bin\Release\net8.0-windows\LibreHardwareMonitor.Windows.Forms.exe' -WorkingDirectory '%~dp0bin\Release\net8.0-windows' -Verb RunAs"
)