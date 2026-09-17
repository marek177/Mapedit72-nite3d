namespace MapEdit72N3D.Core;

public sealed class N3DMapFile
{
    public const int HeaderSize = 514;
    public const int Width = 64;
    public const int Height = 64;
    public const int CellCount = Width * Height;
    public const int BytesPerCell = 2;
    public const int BytesPerLevel = CellCount * BytesPerCell;

    public byte[] Header { get; }
    public List<N3DLevel> Levels { get; }
    public string? SourcePath { get; private set; }

    private N3DMapFile(byte[] header, List<N3DLevel> levels, string? sourcePath)
    {
        Header = header;
        Levels = levels;
        SourcePath = sourcePath;
    }

    public static N3DMapFile Load(string path)
    {
        var bytes = File.ReadAllBytes(path);
        if (bytes.Length < HeaderSize)
            throw new InvalidDataException($"File is too small to be a Nitemare 3-D MAP file ({bytes.Length} bytes).");

        var payload = bytes.Length - HeaderSize;
        if (payload % BytesPerLevel != 0)
            throw new InvalidDataException($"Unexpected MAP size. Payload {payload} is not a multiple of {BytesPerLevel} bytes.");

        var levelCount = payload / BytesPerLevel;
        if (levelCount <= 0)
            throw new InvalidDataException("MAP file contains no levels.");

        var header = bytes[..HeaderSize];
        var levels = new List<N3DLevel>(levelCount);
        var offset = HeaderSize;
        for (var levelIndex = 0; levelIndex < levelCount; levelIndex++)
        {
            var level = new N3DLevel();
            for (var i = 0; i < CellCount; i++)
            {
                level.Walls[i] = bytes[offset++];
                level.Objects[i] = bytes[offset++];
            }
            levels.Add(level);
        }

        return new N3DMapFile(header, levels, path);
    }

    public void Save(string path, bool createBackup = true)
    {
        if (createBackup && File.Exists(path))
        {
            var backup = path + ".bak";
            File.Copy(path, backup, overwrite: true);
        }

        using var fs = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None);
        fs.Write(Header);
        foreach (var level in Levels)
        {
            for (var i = 0; i < CellCount; i++)
            {
                fs.WriteByte(level.Walls[i]);
                fs.WriteByte(level.Objects[i]);
            }
        }
        SourcePath = path;
    }

    public static int ExpectedLevelCountForEpisode(int episode) => episode == 1 ? 11 : 10;

    public static int TryGetEpisodeFromFileName(string path)
    {
        var ext = Path.GetExtension(path);
        return ext.Length == 2 && int.TryParse(ext[1..], out var episode) && episode is >= 1 and <= 3 ? episode : 0;
    }
}

public sealed class N3DLevel
{
    public byte[] Walls { get; } = new byte[N3DMapFile.CellCount];
    public byte[] Objects { get; } = new byte[N3DMapFile.CellCount];

    public N3DLevel Clone()
    {
        var copy = new N3DLevel();
        Buffer.BlockCopy(Walls, 0, copy.Walls, 0, Walls.Length);
        Buffer.BlockCopy(Objects, 0, copy.Objects, 0, Objects.Length);
        return copy;
    }
}
