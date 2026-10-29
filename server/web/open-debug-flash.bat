@echo off
cd /d "%~dp0"
set "PLAYER=%cd%\flashplayer_sa_debug.exe"
set "URL=http://127.0.0.1:41520/Client.swf"
set "LOG=%APPDATA%\Macromedia\Flash Player\Logs\flashlog.txt"

if not exist "%PLAYER%" (
  echo [ERR] missing flashplayer_sa_debug.exe
  echo path: %PLAYER%
  pause
  exit /b 1
)

echo Checking resource server 41520 ...
powershell -NoProfile -Command "try { $r=Invoke-WebRequest -Uri 'http://127.0.0.1:41520/Client.swf' -Method Head -TimeoutSec 3 -UseBasicParsing; if($r.StatusCode -ne 200){exit 2}; exit 0 } catch { exit 1 }"
if errorlevel 1 (
  echo [ERR] http://127.0.0.1:41520 not ready.
  echo Run server\start.bat first, then try again.
  pause
  exit /b 1
)

echo OK. Launching:
echo   %URL%
echo Log:
echo   %LOG%
echo.
echo After black-screen, run open-flash-log.bat
echo.

if not exist "%APPDATA%\Macromedia\Flash Player\Logs" mkdir "%APPDATA%\Macromedia\Flash Player\Logs"
echo ----- %date% %time% new session ----->> "%LOG%"

start "FlashDebug" /D "%cd%" "%PLAYER%" "%URL%"
echo Player started. You can close this window.
exit /b 0