@echo off
REM Build script for Mir2 Map Editor - Windows Release (no debug symbols)
echo Building Mir2 Map Editor for Windows x64 (Release)...

cd /D "%~dp0"
cd src\Mir2.Editor

dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:DebugType=None -p:DebugSymbols=false -o "..\..\dist\windows-release"

if %ERRORLEVEL% == 0 (
    echo.
    echo Build completed successfully!
    echo Output location: dist\windows-release\Mir2.Editor.exe
    echo File size optimized for distribution (no debug symbols)
) else (
    echo.
    echo Build failed! Please check the error messages above.
)

pause