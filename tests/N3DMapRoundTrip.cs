// Lightweight format reference test. Compile/run separately if desired.
// MAP layout: 514-byte header, then N levels * 8192 bytes.
// Each 64x64 cell is two interleaved bytes: wall/tile then object/entity.

using MapEdit72N3D.Core;

if (args.Length != 1)
{
    Console.WriteLine("Usage: N3DMapRoundTrip <MAP.1|MAP.2|MAP.3>");
    return;
}

var source = Path.GetFullPath(args[0]);
var map = N3DMapFile.Load(source);
var temp = Path.Combine(Path.GetTempPath(), Path.GetFileName(source) + ".roundtrip");
map.Save(temp, createBackup: false);
var equal = File.ReadAllBytes(source).SequenceEqual(File.ReadAllBytes(temp));
File.Delete(temp);
Console.WriteLine($"{Path.GetFileName(source)}: {map.Levels.Count} levels, round-trip {(equal ? "PASS" : "FAIL")}");
Environment.ExitCode = equal ? 0 : 1;
