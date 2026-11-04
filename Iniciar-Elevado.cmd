@echo off
taskkill /F /IM LibreHardwareMonitor.Windows.Forms.exe >nul 2>&1
taskkill /F /IM LibreHardwareMonitor.exe >nul 2>&1
timeout /t 1 /nobreak >nul

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

cd /d "%WORK_DIR%"
start "" "%EXE_PATH%"
exit
