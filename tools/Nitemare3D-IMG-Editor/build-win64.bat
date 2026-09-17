@echo off
dotnet publish Nitemare3D.ImgEditor.csproj -c Release -r win-x64 --self-contained false -p:Platform=x64
