using MapEdit72N3D.Core;

namespace MapEdit72N3D.UI;

public sealed class MainForm : Form
{
    private readonly MapCanvas _canvas = new() { Dock = DockStyle.Fill };
    private readonly ComboBox _levels = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 155 };
    private readonly ComboBox _layer = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 95 };
    private readonly NumericUpDown _value = new() { Minimum = 0, Maximum = 255, Width = 70 };
    private readonly ToolStripStatusLabel _status = new() { Spring = true, TextAlign = ContentAlignment.MiddleLeft };
    private readonly UndoRedoBuffer _history = new(100);

    private N3DMapFile? _map;
    private byte? _copyWall;
    private byte? _copyObject;
    private bool _dirty;

    public MainForm()
    {
        Text = "MapEdit 7.2 - Nitemare 3-D";
        Width = 1050;
        Height = 850;
        StartPosition = FormStartPosition.CenterScreen;

        var menu = BuildMenu();
        var toolbar = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 38, Padding = new Padding(6), FlowDirection = FlowDirection.LeftToRight };
        toolbar.Controls.AddRange(new Control[]
        {
            new Label { Text = "Level:", AutoSize = true, Margin = new Padding(0, 7, 4, 0) }, _levels,
            new Label { Text = "Layer:", AutoSize = true, Margin = new Padding(12, 7, 4, 0) }, _layer,
            new Label { Text = "Value:", AutoSize = true, Margin = new Padding(12, 7, 4, 0) }, _value
        });

        var status = new StatusStrip();
        status.Items.Add(_status);
        Controls.Add(_canvas);
        Controls.Add(toolbar);
        Controls.Add(menu);
        Controls.Add(status);
        MainMenuStrip = menu;

        _layer.Items.AddRange(new object[] { "Walls", "Objects" });
        _layer.SelectedIndex = 0;
        _layer.SelectedIndexChanged += (_, _) =>
        {
            _canvas.Layer = _layer.SelectedIndex == 0 ? EditLayer.Walls : EditLayer.Objects;
            _canvas.Invalidate();
            UpdateStatus();
        };
        _value.ValueChanged += (_, _) => _canvas.PaintValue = (byte)_value.Value;
        _levels.SelectedIndexChanged += (_, _) => SelectLevel(_levels.SelectedIndex);
        _canvas.CellEdited += (_, edit) => { _history.Push(edit); _dirty = true; UpdateTitle(); UpdateStatus(); };
        _canvas.SelectionChanged += (_, _) => UpdateStatus();
        FormClosing += OnFormClosing;
    }

    private MenuStrip BuildMenu()
    {
        var menu = new MenuStrip();
        var file = new ToolStripMenuItem("&File");
        file.DropDownItems.Add(new ToolStripMenuItem("&Open...", null, (_, _) => OpenMap(), Keys.Control | Keys.O));
        file.DropDownItems.Add(new ToolStripMenuItem("&Save", null, (_, _) => SaveMap(false), Keys.Control | Keys.S));
        file.DropDownItems.Add(new ToolStripMenuItem("Save &As...", null, (_, _) => SaveMap(true), Keys.Control | Keys.Shift | Keys.S));
        file.DropDownItems.Add(new ToolStripSeparator());
        file.DropDownItems.Add(new ToolStripMenuItem("E&xit", null, (_, _) => Close()));

        var edit = new ToolStripMenuItem("&Edit");
        edit.DropDownItems.Add(new ToolStripMenuItem("&Undo", null, (_, _) => Undo(), Keys.Control | Keys.Z));
        edit.DropDownItems.Add(new ToolStripMenuItem("&Redo", null, (_, _) => Redo(), Keys.Control | Keys.Y));
        edit.DropDownItems.Add(new ToolStripSeparator());
        edit.DropDownItems.Add(new ToolStripMenuItem("&Copy cell", null, (_, _) => CopyCell(), Keys.Control | Keys.C));
        edit.DropDownItems.Add(new ToolStripMenuItem("&Paste cell", null, (_, _) => PasteCell(), Keys.Control | Keys.V));

        var help = new ToolStripMenuItem("&Help");
        help.DropDownItems.Add(new ToolStripMenuItem("&About", null, (_, _) => MessageBox.Show(this,
            "MapEdit 7.2 - Nitemare 3-D\nC#/.NET 8 WinForms port\nNative MAP.1-MAP.3 editing",
            "About", MessageBoxButtons.OK, MessageBoxIcon.Information)));

        menu.Items.AddRange(new ToolStripItem[] { file, edit, help });
        return menu;
    }

    private void OpenMap()
    {
        if (!ConfirmDiscard()) return;
        using var dlg = new OpenFileDialog
        {
            Filter = "Nitemare 3-D MAP files (MAP.1;MAP.2;MAP.3)|MAP.1;MAP.2;MAP.3|All files (*.*)|*.*",
            Title = "Open Nitemare 3-D map file"
        };
        if (dlg.ShowDialog(this) != DialogResult.OK) return;

        try
        {
            _map = N3DMapFile.Load(dlg.FileName);
            _levels.Items.Clear();
            var episode = N3DMapFile.TryGetEpisodeFromFileName(dlg.FileName);
            for (var i = 0; i < _map.Levels.Count; i++)
            {
                var label = episode > 0 ? $"E{episode}M{i + 1}" : $"Level {i + 1}";
                if (episode == 1 && i == 10) label += " [DEMO]";
                _levels.Items.Add(label);
            }
            _levels.SelectedIndex = 0;
            _history.Clear();
            _dirty = false;
            UpdateTitle();
            UpdateStatus();

            if (episode > 0 && _map.Levels.Count != N3DMapFile.ExpectedLevelCountForEpisode(episode))
                MessageBox.Show(this, $"Loaded {_map.Levels.Count} levels. Expected {N3DMapFile.ExpectedLevelCountForEpisode(episode)} for MAP.{episode}.",
                    "Level count warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, "Open failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void SelectLevel(int index)
    {
        if (_map is null || index < 0 || index >= _map.Levels.Count) { _canvas.SetLevel(null); return; }
        _history.Clear();
        _canvas.SetLevel(_map.Levels[index]);
        UpdateStatus();
    }

    private void SaveMap(bool saveAs)
    {
        if (_map is null) return;
        var path = _map.SourcePath;
        if (saveAs || string.IsNullOrWhiteSpace(path))
        {
            using var dlg = new SaveFileDialog
            {
                Filter = "Nitemare 3-D MAP files (MAP.1;MAP.2;MAP.3)|MAP.1;MAP.2;MAP.3|All files (*.*)|*.*",
                FileName = path is null ? "MAP.1" : Path.GetFileName(path),
                Title = "Save Nitemare 3-D map file"
            };
            if (dlg.ShowDialog(this) != DialogResult.OK) return;
            path = dlg.FileName;
        }

        try
        {
            _map.Save(path!, createBackup: true);
            _dirty = false;
            UpdateTitle();
            UpdateStatus("Saved. Existing file was backed up as .bak.");
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, "Save failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void Undo()
    {
        var edit = _history.Undo();
        if (edit is null) return;
        _canvas.ApplyEdit(edit, forward: false);
        _dirty = true;
        UpdateTitle();
        UpdateStatus();
    }

    private void Redo()
    {
        var edit = _history.Redo();
        if (edit is null) return;
        _canvas.ApplyEdit(edit, forward: true);
        _dirty = true;
        UpdateTitle();
        UpdateStatus();
    }

    private void CopyCell()
    {
        if (_canvas.SelectedIndex < 0) return;
        _copyWall = _canvas.GetSelectedValue(EditLayer.Walls);
        _copyObject = _canvas.GetSelectedValue(EditLayer.Objects);
        UpdateStatus($"Copied cell: wall={_copyWall}, object={_copyObject}");
    }

    private void PasteCell()
    {
        if (_copyWall is null || _copyObject is null) return;
        _canvas.PasteSelected(_copyWall.Value, _copyObject.Value);
    }

    private void UpdateTitle()
    {
        var name = _map?.SourcePath is null ? "untitled" : Path.GetFileName(_map.SourcePath);
        Text = $"MapEdit 7.2 - Nitemare 3-D - {name}{(_dirty ? " *" : string.Empty)}";
    }

    private void UpdateStatus(string? prefix = null)
    {
        if (prefix is not null) { _status.Text = prefix; return; }
        if (_map is null) { _status.Text = "Ready"; return; }
        var idx = _canvas.SelectedIndex;
        if (idx < 0)
        {
            _status.Text = $"{_levels.SelectedItem} | {_map.Levels.Count} levels | right-click samples a value";
            return;
        }
        var x = idx % 64;
        var y = idx / 64;
        _status.Text = $"{_levels.SelectedItem} | x={x}, y={y} | wall={_canvas.GetSelectedValue(EditLayer.Walls)} | object={_canvas.GetSelectedValue(EditLayer.Objects)} | paint={_canvas.PaintValue}";
    }

    private bool ConfirmDiscard()
    {
        if (!_dirty) return true;
        var r = MessageBox.Show(this, "Save changes before continuing?", "Unsaved changes", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
        if (r == DialogResult.Cancel) return false;
        if (r == DialogResult.Yes)
        {
            SaveMap(false);
            return !_dirty;
        }
        return true;
    }

    private void OnFormClosing(object? sender, FormClosingEventArgs e)
    {
        if (!ConfirmDiscard()) e.Cancel = true;
    }
}
