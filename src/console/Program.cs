using CommandLine;
using DotNet.Globbing;

namespace Quoll.Console;

internal class Program
{
    private static readonly CancellationTokenSource Cts = new();

    private static async Task Main(string[] args)
    {
        AppDomain.CurrentDomain.ProcessExit += OnExit;
        System.Console.CancelKeyPress += OnExit;
        TaskScheduler.UnobservedTaskException += OnExit;

        await using var outputHandler = new OutputHandler(Cts.Token);
        await Parser.Default.ParseArguments<InputArgument>(args).WithParsedAsync(async arg =>
        {
            try
            {
                var appOptions = GetAppOptions(arg);
                var mainService = new MainService(appOptions, outputHandler);
                await mainService.ExecuteAsync(Cts.Token);
            }
            catch (Exception ex)
            {
                outputHandler.Ingest(new OutputItem(ex.Message, true, true, MessageType.Error)
                    {Exception = ex.ToString()});
            }
        });
        Cts.Cancel();
    }

    private static void OnExit(object sender, EventArgs args)
    {
        Cts.Cancel();
    }

    private static AppOptions GetAppOptions(InputArgument o)
    {
        var size = 0d;
        if (!string.IsNullOrEmpty(o.Size) && !FileSizeUtil.GetSizeInBytes(o.Size, out size))
            throw new Exception($"Invalid size: {o.Size}. {FileSizeUtil.GetSizeStringPrompt()}");

        if (!string.IsNullOrEmpty(o.BackupDir) && !Directory.Exists(o.BackupDir))
            try
            {
                Directory.CreateDirectory(o.BackupDir);
            }
            catch (Exception ex)
            {
                throw new Exception($"Creating backup folder failed. {ex.Message}");
            }

        var files = new List<string>();
        if (!string.IsNullOrEmpty(o.FromFile))
        {
            if (!File.Exists(o.FromFile))
            {
                throw new Exception($"File not exist specified in from-file argument: {o.FromFile}");
            }

            foreach (var file in File.ReadLines(o.FromFile))
            {
                if (!string.IsNullOrEmpty(file))
                {
                    files.Add(Path.GetFullPath(file));
                }
            }
        }
        else
        {
            if (string.IsNullOrEmpty(o.Dir) || string.IsNullOrWhiteSpace(o.Dir))
            {
                o.Dir = Environment.CurrentDirectory;
            }

            if (!Directory.Exists(o.Dir)) throw new Exception($"Target folder not exists: {o.Dir}");
        }

        var appOptions = new AppOptions
        {
            BackupDir = o.BackupDir,
            Dir = o.Dir,
            NeedConfirmation = !o.Yes,
            IncludedFileSizeInBytes = size,
            IncludeSubDirs = o.Recursive,
            RemoveEmptyDir = o.RemoveEmptyDir
        };
        appOptions.IncludedNameGlobs.AddRange(o.Names.Select(Glob.Parse));
        appOptions.IncludedFolderNameGlobs.AddRange(o.FolderNames.Select(Glob.Parse));

        if (!appOptions.IncludedNameGlobs.Any())
        {
            appOptions.IncludedNameGlobs.Add(Glob.Parse("*"));
        }

        appOptions.IncludedFiles.AddRange(files);

        return appOptions;
    }
}