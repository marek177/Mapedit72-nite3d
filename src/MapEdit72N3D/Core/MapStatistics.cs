namespace MapEdit72N3D.Core;

public sealed record MapStatistics(int OccupiedObjects,int Enemies,int Doors,int Keys,int Warps,int Starts,
    IReadOnlyDictionary<string,int> ObjectTypes,IReadOnlyDictionary<string,int> WallTypes)
{
    private static readonly string[] EnemyHints={"ENEMY","MONSTER","GUARD","GHOST","BOSS","SKELETON","ZOMB","MUMMY","DEMON","SPIDER","BAT"};
    private static bool IsEnemy(string t)=>EnemyHints.Any(h=>t.Contains(h,StringComparison.OrdinalIgnoreCase));
    private static bool IsDoor(string t)=>t.Contains("DOOR",StringComparison.OrdinalIgnoreCase)||t.Contains("ELEV",StringComparison.OrdinalIgnoreCase);
    private static bool IsWarp(string t)=>t.Contains("WARP",StringComparison.OrdinalIgnoreCase)||t.Contains("TELE",StringComparison.OrdinalIgnoreCase);

    public static MapStatistics Analyze(N3DLevel level,SymbolDefinitions? defs)
    {
        var ot=new Dictionary<string,int>(StringComparer.OrdinalIgnoreCase); var wt=new Dictionary<string,int>(StringComparer.OrdinalIgnoreCase);
        int occupied=0,enemies=0,doors=0,keys=0,warps=0,starts=0;
        for(int i=0;i<4096;i++)
        {
            var oid=level.Objects[i];
            if(oid!=0){ occupied++; var d=defs?.Find(EditLayerCompat.Objects,oid); var t=d?.Type??"UNDEFINED"; ot[t]=ot.GetValueOrDefault(t)+1;
                if(IsEnemy(t)) enemies++; if(t.Contains("KEY",StringComparison.OrdinalIgnoreCase)||t.Contains("IDCARD",StringComparison.OrdinalIgnoreCase)) keys++;
                if(t.Contains("START",StringComparison.OrdinalIgnoreCase)) starts++; if(IsWarp(t)) warps++; }
            var wid=level.Walls[i]; var w=defs?.Find(EditLayerCompat.Walls,wid); var tw=w?.Type??(wid==0?"EMPTY":"UNDEFINED"); wt[tw]=wt.GetValueOrDefault(tw)+1;
            if(IsDoor(tw)) doors++; if(IsWarp(tw)) warps++;
        }
        return new(occupied,enemies,doors,keys,warps,starts,ot,wt);
    }
}
