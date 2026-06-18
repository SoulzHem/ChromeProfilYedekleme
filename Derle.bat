@echo off
chcp 65001 >nul
title Chrome Profil Yedek - Derleme
cd /d "%~dp0ChromeProfilApp"

echo Derleniyor (self-contained single-file)...
dotnet publish -c Release -o "..\publish_temp"

if errorlevel 1 (
    echo Derleme basarisiz.
    pause
    exit /b 1
)

cd /d "%~dp0"
copy /Y "publish_temp\ChromeProfilYedek.exe" "ChromeProfilYedek.exe" >nul
rd /s /q "publish_temp"

echo.
echo Basarili: ChromeProfilYedek.exe olusturuldu (tek dosya, self-contained).
echo Konum: %~dp0ChromeProfilYedek.exe
pause
