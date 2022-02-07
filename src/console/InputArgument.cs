using CommandLine;

namespace Quoll.Console;

public class InputArgument
{
    [Value(0, MetaName = "dir", HelpText = "The target folder. Default to current folder.")]
    public string Dir { get; set; }

    [Option('y', "yes", Default = false, HelpText = "Confirmation for deletion.")]
    public bool Yes { get; set; }

    [Option('n', "name",
        HelpText = "Filter by glob pattern file names. Multiple names can be separated by comma. Default to *.",
        Separator = ',')]
    public IEnumerable<string> Names { get; set; }

    [Option('f', "folder",
        HelpText =
            "Filter by glob pattern folder names. Multiple names can be separated by comma. Default not to delete folders, unless --remove-empty-dir specified.",
        Separator = ',')]
    public IEnumerable<string> FolderNames { get; set; }

    [Option('s', "size",
        HelpText = "Filter by file size(less than or equals), valid strings are xxB, xxKB, xxMB, xxGB.")]
    public string Size { get; set; }

    [Option('b', "backup", HelpText = "Save to backup location before deletion.")]
    public string BackupDir { get; set; }

    [Option("from-file", HelpText = "File paths to delete. One file path per line.")]
    public string FromFile { get; set; }

    [Option('r', "recursive", HelpText = "Include sub directories. Default to false.")]
    public bool Recursive { get; set; }

    [Option("remove-empty-dir", HelpText = "Remove all empty folders.")]
    public bool RemoveEmptyDir { get; set; }
}