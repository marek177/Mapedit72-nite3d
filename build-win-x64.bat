@echo off
setlocal
cd /d %~dp0
dotnet publish src\MapEdit72N3D\MapEdit72N3D.csproj -c Release -r win-x64 --self-contained false -p:Platform=x64 -o publish\win-x64
