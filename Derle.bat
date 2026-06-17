@echo off
chcp 65001 >nul
title Chrome Profil Yedek - Derleme
cd /d "%~dp0ChromeProfilApp"

echo Derleniyor...
dotnet publish -c Release -o "..\publish"

if errorlevel 1 (
    echo Derleme basarisiz.
    pause
    exit /b 1
)

copy /Y "..\publish\ChromeProfilYedek.exe" "..\ChromeProfilYedek.exe" >nul
echo.
echo Basarili: ChromeProfilYedek.exe olusturuldu.
echo Konum: %~dp0ChromeProfilYedek.exe
pause
