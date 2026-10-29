@echo off
chcp 65001 >nul
setlocal EnableDelayedExpansion
cd /d "%~dp0"

echo ========================================
echo  Nieo backend one-click start
echo  GM=29180 RES=41520 LOGIN=22973 GAME=28610
echo  MySQL root@127.0.0.1/niao
echo ========================================

if not exist "configs\advertise_host.txt" (
  echo 127.0.0.1> "configs\advertise_host.txt"
)

echo [patch] ServerR.xml login IP/port ...
powershell -NoProfile -ExecutionPolicy Bypass -File "scripts\patch_serverr_local.ps1"
if errorlevel 1 (
  echo [warn] ServerR patch failed
)

echo [dll] NieoData\dll is authoritative (no overwrite from tmp)
if not exist "..\NieoData\dll\NieoCore.swf" (
  echo [warn] missing ..\NieoData\dll\NieoCore.swf
  echo [warn] put incremental dll into BOTH NieoData\dll AND tmp\dll
) else (
  echo [ok] NieoData\dll\NieoCore.swf present - left untouched
)

if not exist "bin\nieoserver.exe" (
  echo [build] bin\nieoserver.exe missing, compiling ...
  if not exist output mkdir output
  if not exist bin mkdir bin
  set "TAGS="
  set "LINK=cmd\nieoserver\boundgod_link.go"
  if exist "boundgod\register.go" (
    >"!LINK!" echo //go:build boundgod
    >>"!LINK!" echo.
    >>"!LINK!" echo package main
    >>"!LINK!" echo.
    >>"!LINK!" echo import _ "niaohao/server/boundgod"
    set "TAGS=-tags boundgod"
  ) else (
    if exist "!LINK!" del /f /q "!LINK!"
  )
  go build !TAGS! -ldflags "-s -w" -o output\nieoserver.exe .\cmd\nieoserver
  if errorlevel 1 (
    echo [fail] build failed
    pause
    exit /b 1
  )
  copy /y output\nieoserver.exe bin\nieoserver.exe >nul
  echo [ok] built bin\nieoserver.exe
)

echo [hint] MySQL must be running
echo [run] GM  http://127.0.0.1:29180/
echo [run] RES http://127.0.0.1:41520/
echo [log] [RES]=resource  [CMD] OK/UNIMPL=protocol
echo.
bin\nieoserver.exe -root "%cd%"
pause
