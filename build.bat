@echo off
echo ===================================
echo WWM Bober Rotations - Build Script
echo ===================================
echo.

REM Check if dotnet is installed
dotnet --version >nul 2>&1
if errorlevel 1 (
    echo ERROR: .NET SDK is not installed
    echo Please install .NET 8.0 SDK from https://dotnet.microsoft.com/download
    pause
    exit /b 1
)

echo [1/4] Restoring NuGet packages...
dotnet restore WWMBoberRotations.sln
if errorlevel 1 (
    echo ERROR: Failed to restore packages
    pause
    exit /b 1
)

echo.
echo [2/4] Building in Release mode...
dotnet build WWMBoberRotations.sln -c Release
if errorlevel 1 (
    echo ERROR: Build failed
    pause
    exit /b 1
)

echo.
echo [3/4] Publishing as single-file executable...
dotnet publish WWMBoberRotations\WWMBoberRotations.csproj -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true /p:PublishTrimmed=false -o publish
if errorlevel 1 (
    echo ERROR: Publish failed
    pause
    exit /b 1
)

echo.
echo [4/4] Copying example combos...
if not exist "publish\Examples" mkdir "publish\Examples"
copy "WWMBoberRotations\Examples\example_combos.json" "publish\Examples\" >nul

echo.
echo ===================================
echo BUILD SUCCESSFUL!
echo ===================================
echo.
echo Executable location: publish\WWMBoberRotations.exe
echo.
echo You can now distribute the entire 'publish' folder
echo or just the .exe file (it's self-contained)
echo.

pause
