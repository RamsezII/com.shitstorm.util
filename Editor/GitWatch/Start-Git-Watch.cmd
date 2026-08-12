@echo off
start "SHITSTORM Git Watch" powershell.exe -NoProfile -WindowStyle Hidden -ExecutionPolicy Bypass -File "%~dp0Binaries\GitWatch.ps1" -Root "%~dp0..\..\..\.."
