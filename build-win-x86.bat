@echo off
setlocal
cd /d %~dp0
dotnet publish src\MapEdit72N3D\MapEdit72N3D.csproj -c Release -r win-x86 --self-contained false -p:Platform=x86 -o publish\win-x86
