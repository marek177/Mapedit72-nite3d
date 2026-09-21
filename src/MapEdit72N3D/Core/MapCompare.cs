namespace MapEdit72N3D.Core;
public sealed record MapDifference(int Index, byte OldWall, byte NewWall, byte OldObject, byte NewObject);
public static class MapCompare
{
    public static List<MapDifference> Compare(N3DLevel a, N3DLevel b)
    {
        var r=new List<MapDifference>();
        for(int i=0;i<N3DMapFile.CellCount;i++) if(a.Walls[i]!=b.Walls[i]||a.Objects[i]!=b.Objects[i])
            r.Add(new(i,a.Walls[i],b.Walls[i],a.Objects[i],b.Objects[i]));
        return r;
    }
}
