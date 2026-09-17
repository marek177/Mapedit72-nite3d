using MapEdit72N3D.Core;

namespace MapEdit72N3D.UI;

public enum EditLayer
{
    Walls,
    Objects
}

public sealed class MapCanvas : Control
{
    private N3DLevel? _level;
    private int _selectedIndex = -1;
    private byte _paintValue = 1;

    public event EventHandler<CellEdit>? CellEdited;
    public event EventHandler? SelectionChanged;

    public EditLayer Layer { get; set; } = EditLayer.Walls;
    public int SelectedIndex => _selectedIndex;
    public byte PaintValue { get => _paintValue; set => _paintValue = value; }

    public MapCanvas()
    {
        DoubleBuffered = true;
        BackColor = Color.Black;
        ForeColor = Color.White;
        TabStop = true;
    }

    public void SetLevel(N3DLevel? level)
    {
        _level = level;
        _selectedIndex = -1;
        Invalidate();
        SelectionChanged?.Invoke(this, EventArgs.Empty);
    }

    public byte GetSelectedValue(EditLayer layer)
    {
        if (_level is null || _selectedIndex < 0) return 0;
        return layer == EditLayer.Walls ? _level.Walls[_selectedIndex] : _level.Objects[_selectedIndex];
    }

    public void ApplyEdit(CellEdit edit, bool forward)
    {
        if (_level is null) return;
        _level.Walls[edit.Index] = forward ? edit.NewWall : edit.OldWall;
        _level.Objects[edit.Index] = forward ? edit.NewObject : edit.OldObject;
        Invalidate();
    }

    public void PasteSelected(byte wall, byte obj)
    {
        if (_level is null || _selectedIndex < 0) return;
        var oldWall = _level.Walls[_selectedIndex];
        var oldObj = _level.Objects[_selectedIndex];
        if (oldWall == wall && oldObj == obj) return;
        _level.Walls[_selectedIndex] = wall;
        _level.Objects[_selectedIndex] = obj;
        CellEdited?.Invoke(this, new CellEdit(_selectedIndex, oldWall, wall, oldObj, obj));
        Invalidate();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        if (_level is null)
        {
            TextRenderer.DrawText(e.Graphics, "Open MAP.1, MAP.2 or MAP.3", Font, ClientRectangle, ForeColor,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            return;
        }

        var cell = Math.Max(2, Math.Min(ClientSize.Width / N3DMapFile.Width, ClientSize.Height / N3DMapFile.Height));
        var mapW = cell * N3DMapFile.Width;
        var mapH = cell * N3DMapFile.Height;
        var ox = Math.Max(0, (ClientSize.Width - mapW) / 2);
        var oy = Math.Max(0, (ClientSize.Height - mapH) / 2);

        using var gridPen = new Pen(Color.FromArgb(45, 255, 255, 255));
        using var selectedPen = new Pen(Color.Yellow, 2);

        for (var y = 0; y < N3DMapFile.Height; y++)
        {
            for (var x = 0; x < N3DMapFile.Width; x++)
            {
                var i = y * N3DMapFile.Width + x;
                var value = Layer == EditLayer.Walls ? _level.Walls[i] : _level.Objects[i];
                var c = value == 0 ? Color.FromArgb(20, 20, 20) : Color.FromArgb(255, value, (value * 53) & 255, (value * 97) & 255);
                using var brush = new SolidBrush(c);
                var r = new Rectangle(ox + x * cell, oy + y * cell, cell, cell);
                e.Graphics.FillRectangle(brush, r);
                if (cell >= 8) e.Graphics.DrawRectangle(gridPen, r);
                if (i == _selectedIndex) e.Graphics.DrawRectangle(selectedPen, r);
            }
        }
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);
        Focus();
        if (_level is null) return;
        var cell = Math.Max(2, Math.Min(ClientSize.Width / N3DMapFile.Width, ClientSize.Height / N3DMapFile.Height));
        var mapW = cell * N3DMapFile.Width;
        var mapH = cell * N3DMapFile.Height;
        var ox = Math.Max(0, (ClientSize.Width - mapW) / 2);
        var oy = Math.Max(0, (ClientSize.Height - mapH) / 2);
        var x = (e.X - ox) / cell;
        var y = (e.Y - oy) / cell;
        if (e.X < ox || e.Y < oy || x < 0 || y < 0 || x >= 64 || y >= 64) return;

        _selectedIndex = y * N3DMapFile.Width + x;
        SelectionChanged?.Invoke(this, EventArgs.Empty);

        if (e.Button == MouseButtons.Left)
        {
            var oldWall = _level.Walls[_selectedIndex];
            var oldObj = _level.Objects[_selectedIndex];
            var newWall = oldWall;
            var newObj = oldObj;
            if (Layer == EditLayer.Walls) newWall = PaintValue; else newObj = PaintValue;
            if (oldWall != newWall || oldObj != newObj)
            {
                _level.Walls[_selectedIndex] = newWall;
                _level.Objects[_selectedIndex] = newObj;
                CellEdited?.Invoke(this, new CellEdit(_selectedIndex, oldWall, newWall, oldObj, newObj));
            }
        }
        else if (e.Button == MouseButtons.Right)
        {
            PaintValue = Layer == EditLayer.Walls ? _level.Walls[_selectedIndex] : _level.Objects[_selectedIndex];
        }
        Invalidate();
    }
}
