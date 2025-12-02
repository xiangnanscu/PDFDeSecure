@echo off
REM 处理指定文件夹下所有PDF（含子文件夹）并覆盖源文件
REM 使用方法: process-folder.bat "C:\路径\到\文件夹"
chcp 936 >nul 2>&1

if "%~1"=="" (
    echo 错误：请指定要处理的文件夹路径
    echo.
    echo 使用方法: process-folder.bat "C:\路径\到\文件夹"
    echo.
    echo 示例: process-folder.bat "C:\Users\xnscu\Documents\5.【2025】数学思维提升课（L1-L6）"
    pause
    exit /b 1
)

set "TARGET_DIR=%~1"
set "EXE_PATH=C:\Users\xnscu\PDFDeSecure\bin\x64\Release\standalone\PDFDeSecure.exe"

if not exist "%EXE_PATH%" (
    echo 错误：找不到 PDFDeSecure.exe
    echo 请确认路径: %EXE_PATH%
    pause
    exit /b 1
)

if not exist "%TARGET_DIR%" (
    echo 错误：找不到目标文件夹: %TARGET_DIR%
    pause
    exit /b 1
)

echo ========================================
echo 开始处理文件夹
echo ========================================
echo 目标: %TARGET_DIR%
echo ========================================
echo.

"%EXE_PATH%" "%TARGET_DIR%"

pause

