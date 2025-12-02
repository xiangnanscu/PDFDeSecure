#!/bin/bash
# 打包为无外部依赖的单文件 exe
# 使用方法：在 Git Bash 中运行 ./build-standalone.sh

echo "正在打包 PDFDeSecure 为单文件 exe..."

dotnet publish \
    -c Release \
    -r win-x64 \
    --self-contained true \
    -p:PublishSingleFile=true \
    -p:IncludeNativeLibrariesForSelfExtract=true \
    -p:IncludeAllContentForSelfExtract=true \
    -p:PublishTrimmed=false \
    -o "./bin/x64/Release/standalone"

echo ""
echo "打包完成！exe 文件位于: ./bin/x64/Release/standalone/PDFDeSecure.exe"
echo "该 exe 文件无需安装 .NET 运行时即可运行。"

