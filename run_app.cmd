@echo off
SETLOCAL EnableDelayedExpansion

echo ==========================================================
echo       Vendor Resource Portal - Full Stack Build
echo ==========================================================

:: Step 1: Build Angular Frontend
echo [1/3] Building Angular Application (vendor-ui)...
cd vendor-ui
:: Use call to ensure script continues after npm
call npm install
call npm run build
if !errorlevel! neq 0 (
    echo.
    echo [ERROR] Angular build failed. Aborting.
    pause
    exit /b !errorlevel!
)
cd ..

echo.
echo [2/3] Building .NET API Application (vendor-api)...
cd vendor-api
dotnet build
if !errorlevel! neq 0 (
    echo.
    echo [ERROR] .NET build failed. Aborting.
    pause
    exit /b !errorlevel!
)

echo.
echo [3/3] Starting Application Service...
echo The application will be hosted at https://localhost:7291
echo Opening browser...
start https://localhost:7291/resources

:: Run the project
dotnet run --launch-profile "https"

pause
