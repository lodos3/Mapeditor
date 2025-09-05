@echo off
REM Build script for Mir2 Map Editor - Windows Single File
echo Building Mir2 Map Editor for Windows x64...

cd /D "%~dp0"
cd src\Mir2.Editor

dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o "..\..\dist\windows"

if %ERRORLEVEL% == 0 (
    echo.
    echo Build completed successfully!
    echo Output location: dist\windows\Mir2.Editor.exe
    echo.
    echo To exclude debug symbols from distribution, delete the .pdb files
    echo in the dist\windows directory.
) else (
    echo.
    echo Build failed! Please check the error messages above.
)

pause