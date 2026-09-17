#!/usr/bin/env python3
import pathlib, sys
HEADER=514
LEVEL=8192
if len(sys.argv)!=2:
    raise SystemExit('usage: n3d_dump.py MAP.1')
p=pathlib.Path(sys.argv[1]); b=p.read_bytes()
payload=len(b)-HEADER
if len(b)<HEADER or payload%LEVEL:
    raise SystemExit(f'invalid size: {len(b)}')
levels=payload//LEVEL
print(f'{p.name}: {len(b)} bytes, header={HEADER}, levels={levels}')
for n in range(levels):
    d=b[HEADER+n*LEVEL:HEADER+(n+1)*LEVEL]
    walls=d[0::2]; objs=d[1::2]
    print(f'  level {n+1:2}: nonzero walls={sum(x!=0 for x in walls):4}, nonzero objects={sum(x!=0 for x in objs):4}')
