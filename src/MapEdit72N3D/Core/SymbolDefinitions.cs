using System.Globalization;
using System.Text.RegularExpressions;

namespace MapEdit72N3D.Core;

public sealed record SymbolDefinition(byte Id, string Symbol, string Resource, string Type, string Description)
{
    public override string ToString() => $"{Id:X2}  {Symbol,-4}  {Description}";
}

public sealed class SymbolDefinitions
{
    public IReadOnlyList<SymbolDefinition> Walls { get; }
    public IReadOnlyList<SymbolDefinition> Objects { get; }
    private readonly Dictionary<byte, SymbolDefinition> _walls;
    private readonly Dictionary<byte, SymbolDefinition> _objects;

    private SymbolDefinitions(List<SymbolDefinition> walls, List<SymbolDefinition> objects)
    {
        Walls=walls; Objects=objects; _walls=walls.ToDictionary(x=>x.Id); _objects=objects.ToDictionary(x=>x.Id);
    }

    public SymbolDefinition? Find(EditLayerCompat layer, byte id) =>
        (layer==EditLayerCompat.Walls?_walls:_objects).GetValueOrDefault(id);

    public static SymbolDefinitions LoadForMap(string mapPath)
    {
        var episode=N3DMapFile.TryGetEpisodeFromFileName(mapPath);
        if(episode<=0) return new([],[]);
        var dir=Path.GetDirectoryName(mapPath)!;
        return new(Parse(FindSidecar(dir,"WALLS",episode)),Parse(FindSidecar(dir,"OBJECTS",episode)));
    }

    private static string? FindSidecar(string dir,string stem,int episode)
    {
        var exact=Path.Combine(dir,$"{stem}.{episode}"); if(File.Exists(exact)) return exact;
        return Directory.EnumerateFiles(dir).FirstOrDefault(p=>Regex.IsMatch(Path.GetFileName(p),$"^{stem}(?:\\(\\d+\\))?\\.{episode}$",RegexOptions.IgnoreCase));
    }

    private static List<SymbolDefinition> Parse(string? path)
    {
        var result=new List<SymbolDefinition>(); if(path is null) return result;
        foreach(var raw in File.ReadLines(path))
        {
            if(string.IsNullOrWhiteSpace(raw)) continue;
            var m=Regex.Match(raw,@"^\s*([0-9A-Fa-f]{4})\s+(\S+)\s+(\S+)\s+(\S+)\s+(.*)$"); if(!m.Success) continue;
            var n=int.Parse(m.Groups[1].Value,NumberStyles.HexNumber,CultureInfo.InvariantCulture); if(n>255) continue;
            result.Add(new((byte)n,m.Groups[2].Value,m.Groups[3].Value,m.Groups[4].Value,m.Groups[5].Value.Trim()));
        }
        return result;
    }
}
public enum EditLayerCompat { Walls, Objects }
