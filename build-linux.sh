#!/bin/bash
# Build script for Mir2 Map Editor - Linux Single File
echo "Building Mir2 Map Editor for Linux x64..."

# Get the script directory
SCRIPT_DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" &> /dev/null && pwd )"
cd "$SCRIPT_DIR/src/Mir2.Editor"

dotnet publish -c Release -r linux-x64 --self-contained true -p:PublishSingleFile=true -o "../../dist/linux"

if [ $? -eq 0 ]; then
    echo ""
    echo "Build completed successfully!"
    echo "Output location: dist/linux/Mir2.Editor"
    echo ""
    echo "To exclude debug symbols from distribution, delete the .pdb files"
    echo "in the dist/linux directory."
    echo ""
    echo "Make the executable runnable with: chmod +x dist/linux/Mir2.Editor"
else
    echo ""
    echo "Build failed! Please check the error messages above."
    exit 1
fi