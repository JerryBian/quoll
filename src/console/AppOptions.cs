using DotNet.Globbing;

namespace Quoll.Console;

public class AppOptions
{
    public string Dir { get; set; }

    public bool NeedConfirmation { get; set; }

    public List<Glob> IncludedNameGlobs { get; } = new();

    public double IncludedFileSizeInBytes { get; set; }

    public string BackupDir { get; set; }

    public List<string> IncludedFiles { get; } = new();

    public bool IncludeSubDirs { get; set; }
}