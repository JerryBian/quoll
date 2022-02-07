using System.Diagnostics;

namespace Quoll.Console;

public class MainService
{
    private readonly List<string> _emptyFolders;
    private readonly List<string> _files;
    private readonly List<string> _folders;
    private readonly AppOptions _options;
    private readonly IOutputHandler _outputHandler;

    public MainService(AppOptions options, IOutputHandler outputHandler)
    {
        _options = options;
        _outputHandler = outputHandler;
        _files = new List<string>();
        _folders = new List<string>();
        _emptyFolders = new List<string>();
    }

    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        var sw = Stopwatch.StartNew();
        _files.Clear();
        _folders.Clear();

        if (!string.IsNullOrEmpty(_options.Dir))
        {
            _outputHandler.Ingest(new OutputItem("Scanning folder:", false));
            _outputHandler.Ingest(new OutputItem($" {_options.Dir}", true, messageType: MessageType.Success));
        }

        _files.AddRange(_options.IncludedFiles);
        ScanFolders(_options.Dir);

        if (_folders.Any())
        {
            _outputHandler.Ingest(new OutputItem());
            _outputHandler.Ingest(new OutputItem("=== Folders to Delete ===", messageType: MessageType.DarkVerbose));
            foreach (var folder in _folders)
            {
                if (cancellationToken.IsCancellationRequested) break;

                _outputHandler.Ingest(new OutputItem("\u2192 ", false,
                    messageType: MessageType.DarkSuccess));
                _outputHandler.Ingest(new OutputItem(folder, true, messageType: MessageType.Success));
            }
        }

        if (_files.Any())
        {
            _outputHandler.Ingest(new OutputItem());
            _outputHandler.Ingest(new OutputItem("=== Files to Delete ===", messageType: MessageType.DarkVerbose));
            foreach (var file in _files)
            {
                if (cancellationToken.IsCancellationRequested) break;

                _outputHandler.Ingest(new OutputItem("\u2192 ", false,
                    messageType: MessageType.DarkSuccess));
                _outputHandler.Ingest(new OutputItem(file, true, messageType: MessageType.Success));
            }
        }

        if (!_files.Any() && !_folders.Any())
        {
            _outputHandler.Ingest(new OutputItem("Nothing to delete.", true, messageType: MessageType.Warning));
            RemoveEmptyDir(_options.Dir, cancellationToken);
            return;
        }


        if (_options.NeedConfirmation)
        {
            _outputHandler.Ingest(new OutputItem(
                $"Are you sure to delete all these items({_files.Count} files, {_folders.Count} folders)? Y/y for yes, others for no: ",
                false,
                messageType: MessageType.Warning));
            if (!string.Equals(System.Console.ReadLine(), "y", StringComparison.InvariantCultureIgnoreCase))
            {
                RemoveEmptyDir(_options.Dir, cancellationToken);
                return;
            }
        }

        foreach (var folder in _folders)
        {
            if (cancellationToken.IsCancellationRequested) break;

            var stopwatch = Stopwatch.StartNew();
            _outputHandler.Ingest(new OutputItem("\u2192 ", false,
                messageType: MessageType.DarkSuccess));
            _outputHandler.Ingest(new OutputItem("Deleting ", false,
                messageType: MessageType.DarkWarning));
            _outputHandler.Ingest(new OutputItem(folder, false));
            try
            {
                var dir = Path.GetDirectoryName(folder);
                var backupDir = _options.BackupDir;
                if (!string.IsNullOrEmpty(backupDir) && _options.Dir != dir)
                {
                    backupDir = Path.Combine(backupDir, Path.GetRelativePath(_options.Dir, dir));
                }

                if (!string.IsNullOrEmpty(backupDir))
                {
                    var destFolder = Path.Combine(backupDir, Path.GetFileName(folder));
                    Directory.CreateDirectory(destFolder);
                    CopyFilesRecursively(folder, destFolder);
                }

                Directory.Delete(folder, true);
            }
            catch (Exception ex)
            {
                _outputHandler.Ingest(new OutputItem(ex.Message, true, true, MessageType.DarkError));
            }

            stopwatch.Stop();

            _outputHandler.Ingest(new OutputItem(" \u2713", false,
                messageType: MessageType.DarkSuccess));
            _outputHandler.Ingest(new OutputItem($" ({stopwatch.Elapsed.TotalMilliseconds:0.00}ms)"));
        }

        foreach (var file in _files)
        {
            if (cancellationToken.IsCancellationRequested) break;

            var stopwatch = Stopwatch.StartNew();
            _outputHandler.Ingest(new OutputItem("\u2192 ", false,
                messageType: MessageType.DarkSuccess));
            _outputHandler.Ingest(new OutputItem("Deleting ", false,
                messageType: MessageType.DarkWarning));
            _outputHandler.Ingest(new OutputItem(file, false));
            try
            {
                if (File.Exists(file))
                {
                    if (!string.IsNullOrEmpty(_options.Dir))
                    {
                        // from-file does not support backup
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
                    }

                    File.Delete(file);
                }
            }
            catch (Exception ex)
            {
                _outputHandler.Ingest(new OutputItem(ex.Message, true, true, MessageType.DarkError));
            }

            stopwatch.Stop();

            _outputHandler.Ingest(new OutputItem(" \u2713", false,
                messageType: MessageType.DarkSuccess));
            _outputHandler.Ingest(new OutputItem($" ({stopwatch.Elapsed.TotalMilliseconds:0.00}ms)"));
        }

        RemoveEmptyDir(_options.Dir, cancellationToken);
        sw.Stop();
        _outputHandler.Ingest(new OutputItem("Done. ", false, messageType: MessageType.DarkSuccess));
        _outputHandler.Ingest(new OutputItem($"Elapsed {sw.Elapsed:hh\\:mm\\:ss}. ", false));
        await Task.CompletedTask;
    }

    private void ScanEmptyDir(string folder)
    {
        if (!Directory.EnumerateFiles(folder, "*", SearchOption.AllDirectories).Any())
        {
            _emptyFolders.Add(folder);
        }

        foreach (var dir in Directory.EnumerateDirectories(folder, "*", SearchOption.AllDirectories))
        {
            ScanEmptyDir(dir);
        }
    }

    private void RemoveEmptyDir(string folder, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(folder) || !_options.RemoveEmptyDir)
        {
            return;
        }

        ScanEmptyDir(folder);
        if (!_emptyFolders.Any())
        {
            return;
        }

        _outputHandler.Ingest(new OutputItem());
        _outputHandler.Ingest(new OutputItem("=== Empty folders to Delete ===", messageType: MessageType.DarkVerbose));
        foreach (var emptyFolder in _emptyFolders)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }

            _outputHandler.Ingest(new OutputItem("\u2192 ", false,
                messageType: MessageType.DarkSuccess));
            _outputHandler.Ingest(new OutputItem(emptyFolder, true, messageType: MessageType.Success));
        }

        if (_options.NeedConfirmation)
        {
            _outputHandler.Ingest(new OutputItem(
                $"Are you sure to delete all these {_emptyFolders.Count} empty folders? Y/y for yes, others for no: ",
                false,
                messageType: MessageType.Warning));
            if (!string.Equals(System.Console.ReadLine(), "y", StringComparison.InvariantCultureIgnoreCase))
            {
                _outputHandler.Ingest(new OutputItem("Skip deleting empty folders."));
                return;
            }
        }

        foreach (var emptyFolder in _emptyFolders)
        {
            if (cancellationToken.IsCancellationRequested) break;

            var stopwatch = Stopwatch.StartNew();
            _outputHandler.Ingest(new OutputItem("\u2192 ", false,
                messageType: MessageType.DarkSuccess));
            _outputHandler.Ingest(new OutputItem("Deleting ", false,
                messageType: MessageType.DarkWarning));
            _outputHandler.Ingest(new OutputItem(emptyFolder, false));
            try
            {
                if (Directory.Exists(emptyFolder))
                {
                    Directory.Delete(emptyFolder, true);
                }
            }
            catch (Exception ex)
            {
                _outputHandler.Ingest(new OutputItem(ex.Message, true, true, MessageType.DarkError));
            }

            stopwatch.Stop();

            _outputHandler.Ingest(new OutputItem(" \u2713", false,
                messageType: MessageType.DarkSuccess));
            _outputHandler.Ingest(new OutputItem($" ({stopwatch.Elapsed.TotalMilliseconds:0.00}ms)"));
        }
    }


    private void CopyFilesRecursively(string src, string dest)
    {
        foreach (var dirPath in Directory.GetDirectories(src, "*", SearchOption.AllDirectories))
            Directory.CreateDirectory(dirPath.Replace(src, dest));

        foreach (var newPath in Directory.GetFiles(src, "*", SearchOption.AllDirectories))
            File.Copy(newPath, newPath.Replace(src, dest), true);
    }

    private void ScanFolders(string folder)
    {
        if (string.IsNullOrEmpty(folder)) return;

        foreach (var dir in Directory.EnumerateDirectories(folder, "*", SearchOption.TopDirectoryOnly))
        {
            if (_folders.Contains(dir)) continue;

            if (_options.IncludedFolderNameGlobs.Any(x => x.IsMatch(Path.GetFileName(dir))))
                _folders.Add(dir);
            else if (_options.IncludeSubDirs) ScanFolders(dir);
        }

        foreach (var file in Directory.EnumerateFiles(folder, "*", SearchOption.TopDirectoryOnly))
        {
            if (_options.IncludedNameGlobs.Any(x => x.IsMatch(Path.GetFileName(file))) ||
                _options.IncludedFileSizeInBytes > 0 && new FileInfo(file).Length <= _options.IncludedFileSizeInBytes)
            {
                _files.Add(file);
            }
        }
    }
}