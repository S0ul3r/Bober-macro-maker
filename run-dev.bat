@echo off
echo Starting WWM Bober Rotations (Development Mode)...
echo.

REM Check if dotnet is installed
dotnet --version >nul 2>&1
if errorlevel 1 (
    echo ERROR: .NET SDK is not installed
    echo Please install .NET 8.0 SDK from https://dotnet.microsoft.com/download
    pause
    exit /b 1
)

echo Restoring packages...
dotnet restore WWMBoberRotations.sln

echo.
echo Running application...
echo.
dotnet run --project WWMBoberRotations\WWMBoberRotations.csproj

pause
