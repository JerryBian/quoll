namespace Quoll.Console;

public class FileItem
{
    public FileItem(string fullPath)
    {
        FullPath = fullPath;
    }

    public string FullPath { get; }
}