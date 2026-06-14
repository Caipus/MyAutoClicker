@echo off
setlocal

echo ===========================================
echo   Neo Auto Clicker - Build Script
echo ===========================================
echo.

REM Check if dotnet is available
where dotnet >nul 2>&1
if %ERRORLEVEL% NEQ 0 (
    echo [ERROR] .NET SDK not found. Please install .NET 10 SDK from:
    echo         https://dotnet.microsoft.com/download
    pause
    exit /b 1
)

echo [1/2] Building release...
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true

if %ERRORLEVEL% NEQ 0 (
    echo.
    echo [ERROR] Build failed. See output above.
    pause
    exit /b 1
)

echo.
echo [2/2] Copying executable...
copy /Y "bin\Release\net10.0-windows\win-x64\publish\MyAutoClicker.exe" "MyAutoClicker.exe" >nul

echo.
echo ===========================================
echo   Build successful!
echo   Output: MyAutoClicker.exe
echo ===========================================
echo.
pause
