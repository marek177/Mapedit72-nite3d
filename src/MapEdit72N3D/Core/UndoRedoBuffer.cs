namespace MapEdit72N3D.Core;

public sealed record CellEdit(int Index, byte OldWall, byte NewWall, byte OldObject, byte NewObject);

public sealed class UndoRedoBuffer
{
    private readonly Stack<CellEdit> _undo = new();
    private readonly Stack<CellEdit> _redo = new();
    private readonly int _capacity;

    public UndoRedoBuffer(int capacity = 100) => _capacity = Math.Max(1, capacity);

    public bool CanUndo => _undo.Count > 0;
    public bool CanRedo => _redo.Count > 0;

    public void Clear()
    {
        _undo.Clear();
        _redo.Clear();
    }

    public void Push(CellEdit edit)
    {
        _undo.Push(edit);
        _redo.Clear();
        Trim(_undo);
    }

    public CellEdit? Undo()
    {
        if (_undo.Count == 0) return null;
        var edit = _undo.Pop();
        _redo.Push(edit);
        return edit;
    }

    public CellEdit? Redo()
    {
        if (_redo.Count == 0) return null;
        var edit = _redo.Pop();
        _undo.Push(edit);
        return edit;
    }

    private void Trim(Stack<CellEdit> stack)
    {
        if (stack.Count <= _capacity) return;
        var kept = stack.Take(_capacity).Reverse().ToArray();
        stack.Clear();
        foreach (var edit in kept) stack.Push(edit);
    }
}
