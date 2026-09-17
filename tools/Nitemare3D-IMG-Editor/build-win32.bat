@echo off
dotnet publish Nitemare3D.ImgEditor.csproj -c Release -r win-x86 --self-contained false -p:Platform=x86
