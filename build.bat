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

echo [1/3] Building release (self-contained)...
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:EnableCompressionInSingleFile=true -o publish_output_temp

if %ERRORLEVEL% NEQ 0 (
    echo.
    echo [ERROR] Build failed. See output above.
    pause
    exit /b 1
)

echo.
echo [2/3] Copying executable to root...
powershell -Command "$src='publish_output_temp\NeoAutoClicker.exe'; $dst='NeoAutoClicker.exe'; for ($i=1; $i -le 10; $i++) { try { if (Test-Path $dst) { Remove-Item $dst -Force -ErrorAction SilentlyContinue }; Copy-Item -Path $src -Destination $dst -Force -ErrorAction Stop; break } catch { Start-Sleep -Seconds 3 } }"

if not exist "NeoAutoClicker.exe" (
    echo [ERROR] Failed to copy NeoAutoClicker.exe.
    pause
    exit /b 1
)

echo.
echo [3/3] Creating ZIP archive...
if exist "NeoAutoClicker.zip" del /F /Q "NeoAutoClicker.zip"
powershell -Command "Compress-Archive -Path NeoAutoClicker.exe -DestinationPath NeoAutoClicker.zip -Force"

REM Clean up temp publish output directory
rmdir /S /Q publish_output_temp

echo.
echo ===========================================
echo   Build successful!
echo   Output:
echo     - NeoAutoClicker.exe (Bundled)
echo     - NeoAutoClicker.zip
echo ===========================================
echo.
pause
