# MapEdit 7.2 - Nitemare 3-D

C#/.NET 8 WinForms reconstruction/port of the classic MapEdit workflow with native support for Gray Design Associates' **Nitemare 3-D** map files.

AI assistance disclosure
This is an AI-assisted reverse-engineering and reconstruction project. A substantial part of the analysis, research, documentation, code generation, refactoring, and interpretation of reverse-engineered material has been produced with the assistance of ChatGPT by OpenAI, under the direction and review of marek177.

Git commit authorship therefore identifies the account that committed the files and should not be interpreted as meaning that every analysis, document, or line of code was written manually and independently by the repository owner. AI-generated or AI-assisted findings may contain errors, especially where original source code or symbols are unavailable, so important reverse-engineering conclusions should be independently verified against the original executable and game data.



## Current features

- Opens and saves native `MAP.1`, `MAP.2`, `MAP.3` files.
- Preserves the original 514-byte MAP header.
- Edits all 64x64 cells in the two interleaved native layers: **Walls** and **Objects**.
- Supports Episode 1's 11 map slots and Episode 2/3's 10 slots; `E1M11` is labelled `[DEMO]`.
- Level switching without converting to Wolf3D format.
- Cell copy/paste.
- 100-step Undo/Redo for the current level.
- Save and Save As with automatic `.bak` backup when overwriting a file.
- Windows 11 x86/x64 publish scripts.
- GitHub Actions builds for `win-x86` and `win-x64`.

## MAP layout

See [FORMAT_NOTES.md](FORMAT_NOTES.md). In short: data starts at byte 514, each level is 8192 bytes, and every 64x64 cell contains two bytes in `wall, object` order.

## Build

Requires Visual Studio 2022 with .NET 8 SDK, or the .NET 8 SDK from the command line.

```bat
build-win-x64.bat
build-win-x86.bat
```

You can also open `MapEdit72N3D.sln` directly in Visual Studio 2022.

## Editing

1. Open `MAP.1`, `MAP.2` or `MAP.3`.
2. Select a level and either the Walls or Objects layer.
3. Enter a value from 0-255 and left-click a cell to paint it.
4. Right-click a cell to sample its current value.
5. Save. When overwriting an existing file, the old file is copied to `.bak` first.

## Scope

This repository contains the map editor source code only. Original commercial game assets are not redistributed. Future work can add `WALLS.1-3` / `OBJECTS.1-3` names and `IMG.1-3` sprite/texture previews without changing the native MAP codec.
