using CommandLine;
using DotNet.Globbing;
using Quoll.Console;

var fileNamePatterns = new List<Glob>();
var fileSizeBytesLimit = -1d;

await Parser.Default.ParseArguments<Options>(args).WithParsedAsync(async o =>
{
    try
    {
        if (await ValidateOptionsAsync(o))
        {
            await Console.Out.WriteLineAsync($"Checking folder: {Path.GetFullPath(o.Dir)} ...");
            await Console.Out.WriteLineAsync();
            await Console.Out.WriteLineAsync("=== Files affected ===");

            foreach (var name in o.Names)
            {
                fileNamePatterns.Add(Glob.Parse(name));
            }

            var items = await GetFileItemsAsync(o.Dir, o.Dir);
            await Console.Out.WriteLineAsync();
            if (!o.Yes)
            {
                await Console.Out.WriteAsync(
                    $"Are you sure to delete all these {items.Count} files? Y/y for yes, others for no: ");
                if (!string.Equals(Console.ReadLine(), "y", StringComparison.InvariantCultureIgnoreCase))
                {
                    await Console.Out.WriteLineAsync("Process terminated.");
                    return;
                }
            }

            foreach (var item in items)
            {
                await Console.Out.WriteAsync($"File \"{Path.GetRelativePath(o.Dir, item)}\": ");
                var dir = Path.GetDirectoryName(item);
                var backupDir = o.BackupDir;
                if (!string.IsNullOrEmpty(dir))
                {
                    backupDir = Path.Combine(backupDir, Path.GetRelativePath(o.Dir, dir));
                }

                if (!string.IsNullOrEmpty(backupDir))
                {
                    Directory.CreateDirectory(backupDir);
                    var destFile = Path.Combine(backupDir, Path.GetFileName(item));
                    File.Copy(item, destFile, true);
                    await Console.Out.WriteAsync("Backup done. ");
                }

                File.Delete(item);
                await Console.Out.WriteLineAsync("Delete done.");
            }

            await Console.Out.WriteLineAsync();
            await Console.Out.WriteAsync("Done.");
        }
    }
    catch (Exception ex)
    {
        await Console.Error.WriteLineAsync($"Unexpected error. {ex.Message}");
    }
});

async Task<List<string>> GetFileItemsAsync(string dirFullPath, string basePath)
{
    var items = new List<string>();
    foreach (var dir in Directory.EnumerateDirectories(dirFullPath, "*", SearchOption.TopDirectoryOnly))
    {
        items.AddRange(await GetFileItemsAsync(dir, basePath));
    }

    foreach (var file in Directory.EnumerateFiles(dirFullPath, "*", SearchOption.TopDirectoryOnly))
    {
        var found = false;

        if (fileNamePatterns.Any())
        {
            foreach (var fileNamePattern in fileNamePatterns)
            {
                if (fileNamePattern.IsMatch(Path.GetFileName(file)))
                {
                    found = true;
                    break;
                }
            }
        }

        if (fileSizeBytesLimit >= 0)
        {
            if (new FileInfo(file).Length <= fileSizeBytesLimit)
            {
                found = true;
            }
        }

        if (found)
        {
            await Console.Out.WriteLineAsync(Path.GetRelativePath(basePath, file));
            items.Add(file);
        }
    }

    return items;
}

async Task<bool> ValidateOptionsAsync(Options o)
{
    if (string.IsNullOrEmpty(o.Dir) || string.IsNullOrWhiteSpace(o.Dir))
    {
        o.Dir = Environment.CurrentDirectory;
    }

    if (!Directory.Exists(o.Dir))
    {
        await Console.Error.WriteLineAsync($"Target folder not exists: {o.Dir}");
        return false;
    }

    if (!string.IsNullOrEmpty(o.Size) && !FileSizeUtil.ValidateSizeString(o.Size, out fileSizeBytesLimit))
    {
        await Console.Error.WriteLineAsync($"Invalid size: {o.Size}. {FileSizeUtil.GetSizeStringPrompt()}");
        return false;
    }

    if (!string.IsNullOrEmpty(o.BackupDir) && !Directory.Exists(o.BackupDir))
    {
        try
        {
            Directory.CreateDirectory(o.BackupDir);
        }
        catch (Exception ex)
        {
            await Console.Error.WriteLineAsync($"Creating backup folder failed. {ex.Message}");
            return false;
        }
    }

    return true;
}