@echo off
title Fechar LibreHardwareMonitor
echo A encerrar todas as instancias do LibreHardwareMonitor...

powershell -NoProfile -ExecutionPolicy Bypass -Command "Start-Process taskkill -ArgumentList '/F /IM LibreHardwareMonitor.Windows.Forms.exe' -Verb RunAs -Wait"

echo Concluido. Todas as instancias foram terminadas.
timeout /t 2 >nul
