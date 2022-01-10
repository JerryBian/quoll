using CommandLine;

namespace Quoll.Console;

public class Options
{
    [Option("dir", HelpText = "The target folder.")]
    public string Dir { get; set; }

    [Option('y', "yes", Default = false, HelpText = "Confirmation for deletion. Default to False.")]
    public bool Yes { get; set; }

    [Option('n', "name", Group = "filter criteria",
        HelpText = "Filter by glob pattern file names. Multiple names can be separated by comma.", Separator = ',')]
    public IEnumerable<string> Names { get; set; }

    [Option('s', "size", Group = "filter criteria",
        HelpText = "Filter by file size(less than or equals), valid strings are xxB, xxKB, xxMB, xxGB.")]
    public string Size { get; set; }

    [Option("backup", HelpText = "Save to backup location before deletion.")]
    public string BackupDir { get; set; }

    public double SizeInBytes { get; set; }
}