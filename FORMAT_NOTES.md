# Nitemare 3-D MAP format notes

This project deliberately treats `MAP.1`, `MAP.2` and `MAP.3` as Nitemare 3-D data, not Wolfenstein 3-D maps.

* Header: **514 bytes**, preserved byte-for-byte.
* Map dimensions: **64 x 64** cells = 4096 cells.
* Cell storage: **2 interleaved bytes** per cell.
  * byte 0: wall / door / floor-zone / control tile ID
  * byte 1: object / item / enemy / entity ID
* One level: **8192 bytes**.
* Data begins at offset **514**.
* `MAP.1`: normally **11** levels (the 11th slot is treated as the demo map in the UI).
* `MAP.2`: normally **10** levels.
* `MAP.3`: normally **10** levels.

The editor performs a lossless read/write of the header and every cell byte. Semantic names and graphics from `WALLS.1-3`, `OBJECTS.1-3` and `IMG.1-3` are intentionally a separate next step; the current editor is already safe for raw native MAP editing.
