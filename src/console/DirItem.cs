namespace Quoll.Console;

public class DirItem
{
    public DirItem(string fullPath)
    {
        SubItems = new List<DirItem>();
        Files = new List<FileItem>();
        FullPath = fullPath;
    }

    public string FullPath { get; }

    public List<DirItem> SubItems { get; }

    public List<FileItem> Files { get; }
}