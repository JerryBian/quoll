using System.Diagnostics;

namespace Quoll.Console;

public class MainService
{
    private readonly AppOptions _options;
    private readonly IOutputHandler _outputHandler;

    public MainService(AppOptions options, IOutputHandler outputHandler)
    {
        _options = options;
        _outputHandler = outputHandler;
    }

    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        var sw = Stopwatch.StartNew();
        _outputHandler.Ingest(new OutputItem("Preparing file lists to deleted for folder:", false));
        _outputHandler.Ingest(new OutputItem($" {_options.Dir}", true, messageType: MessageType.Success));
        var files = GetTargetFiles();

        if (files.Any())
        {
            _outputHandler.Ingest(new OutputItem(""));
            _outputHandler.Ingest(new OutputItem("=== Files Affected ==="));

            foreach (var file in files)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }

                _outputHandler.Ingest(new OutputItem("\u2192 ", false,
                    messageType: MessageType.DarkSuccess));
                _outputHandler.Ingest(new OutputItem(file, true, messageType: MessageType.Success));
            }

            _outputHandler.Ingest(new OutputItem(""));

            if (_options.NeedConfirmation)
            {
                _outputHandler.Ingest(new OutputItem(
                    $"Are you sure to delete all these {files.Count} files? Y/y for yes, others for no: ", false,
                    messageType: MessageType.Warning));
                if (!string.Equals(System.Console.ReadLine(), "y", StringComparison.InvariantCultureIgnoreCase))
                {
                    _outputHandler.Ingest(new OutputItem("Process terminated.", false));
                    return;
                }
            }

            foreach (var file in files)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }

                var stopwatch = Stopwatch.StartNew();
                _outputHandler.Ingest(new OutputItem("\u2192 ", false,
                    messageType: MessageType.DarkSuccess));
                _outputHandler.Ingest(new OutputItem("Deleting ", false,
                    messageType: MessageType.DarkWarning));
                _outputHandler.Ingest(new OutputItem(file, false));
                var dir = Path.GetDirectoryName(file);
                var backupDir = _options.BackupDir;
                if (!string.IsNullOrEmpty(backupDir))
                {
                    backupDir = Path.Combine(backupDir, Path.GetRelativePath(_options.Dir, dir));
                }

                if (!string.IsNullOrEmpty(backupDir))
                {
                    Directory.CreateDirectory(backupDir);
                    var destFile = Path.Combine(backupDir, Path.GetFileName(file));
                    File.Copy(file, destFile, true);
                }

                File.Delete(file);
                stopwatch.Stop();

                _outputHandler.Ingest(new OutputItem(" \u2713", false,
                    messageType: MessageType.DarkSuccess));
                _outputHandler.Ingest(new OutputItem($" ({stopwatch.Elapsed.TotalMilliseconds:0:00}ms)"));
            }

            _outputHandler.Ingest(new OutputItem(""));
        }
        else
        {
            _outputHandler.Ingest(new OutputItem("No files to delete.", true, messageType: MessageType.Warning));
        }

        sw.Stop();
        _outputHandler.Ingest(new OutputItem("Done. ", false, messageType: MessageType.DarkSuccess));
        _outputHandler.Ingest(new OutputItem($"Elapsed {sw.Elapsed:hh\\:mm\\:ss}. ", false));
        await Task.CompletedTask;
    }

    private List<string> GetTargetFiles()
    {
        var result = new HashSet<string>(_options.IncludedFiles);
        foreach (var file in Directory.EnumerateFiles(_options.Dir, "*",
                     _options.IncludeSubDirs ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly))
        {
            var found = false;
            if (_options.IncludedNameGlobs.Any())
            {
                foreach (var includedNameGlob in _options.IncludedNameGlobs)
                {
                    if (includedNameGlob.IsMatch(Path.GetFileName(file)))
                    {
                        found = true;
                        break;
                    }
                }
            }

            if (_options.IncludedFileSizeInBytes > 0)
            {
                if (new FileInfo(file).Length <= _options.IncludedFileSizeInBytes)
                {
                    found = true;
                }
            }

            if (found && !result.Contains(file))
            {
                result.Add(file);
            }
        }

        return result.ToList();
    }
}