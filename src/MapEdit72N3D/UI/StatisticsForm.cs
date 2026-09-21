using MapEdit72N3D.Core;
namespace MapEdit72N3D.UI;
public sealed class StatisticsForm : Form
{
    public StatisticsForm(string levelName,N3DLevel level,SymbolDefinitions? defs)
    {
        Text=$"Statistics - {levelName}";Width=620;Height=520;StartPosition=FormStartPosition.CenterParent;
        var grid=new DataGridView{Dock=DockStyle.Fill,ReadOnly=true,AllowUserToAddRows=false,AutoSizeColumnsMode=DataGridViewAutoSizeColumnsMode.Fill};
        grid.Columns.Add("layer","Layer");grid.Columns.Add("id","ID");grid.Columns.Add("type","Type");grid.Columns.Add("desc","Description");grid.Columns.Add("count","Count");
        foreach(var g in level.Walls.GroupBy(x=>x).OrderByDescending(g=>g.Count())){var d=defs?.Find(EditLayerCompat.Walls,g.Key);grid.Rows.Add("WALL",g.Key.ToString("X2"),d?.Type??"?",d?.Description??"<undefined>",g.Count());}
        foreach(var g in level.Objects.Where(x=>x!=0).GroupBy(x=>x).OrderByDescending(g=>g.Count())){var d=defs?.Find(EditLayerCompat.Objects,g.Key);grid.Rows.Add("OBJECT",g.Key.ToString("X2"),d?.Type??"?",d?.Description??"<undefined>",g.Count());}
        Controls.Add(grid);
    }
}
