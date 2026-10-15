@echo off
setlocal
if "%~1"=="" (
  echo Arrastra tu APK original de Hungry Shark Evolution 6.4.2 sobre este BAT.
  pause
  exit /b 1
)
where py >nul 2>nul
if not errorlevel 1 (
    py -3 "%~dp0Preparar_Hungry_Shark_Evolution_6.4.2.py" "%~1" "%~dpn1_Para_Unity"
) else (
    python "%~dp0Preparar_Hungry_Shark_Evolution_6.4.2.py" "%~1" "%~dpn1_Para_Unity"
)
pause
