## This project is marked as Archive, please use [tur](https://github.com/JerryBian/tur) instead.

Command line tool to DELETE files. All supported features can be found in this [roadmap issue](https://github.com/JerryBian/quoll/issues/5).

## Install

You can go directly download from the [Release](https://github.com/JerryBian/quoll/releases) page according to your target platform.

Or you can use dotnet [global tools](https://www.nuget.org/packages/quoll/) if you have already [.NET 6](https://dotnet.microsoft.com/download) installed.

```sh
dotnet tool install --global quoll
```
For Mac users with zsh, please manually add the dotnet global tool path to `~/.zshrc`. Simply add this line as descriped in this [issue](https://github.com/dotnet/sdk/issues/9415#issuecomment-406915716).

```sh
export PATH=$HOME/.dotnet/tools:$PATH
```

If you would like to upgrade to latest version as you already installed, you can:

```sh
dotnet tool update --global quoll
```


## Usage

`quoll [options] [dir]`

```
-y, --yes             (Default: false) Confirmation for deletion.

-n, --name            Filter by glob pattern file names. Multiple names can be separated by comma. Default to *.

-f, --folder          Filter by glob pattern folder names. Multiple names can be separated by comma. Default not to delete folders, unless      
                    --remove-empty-dir specified.

-s, --size            Filter by file size(less than or equals), valid strings are xxB, xxKB, xxMB, xxGB.

-b, --backup          Save to backup location before deletion.

--from-file           File paths to delete. One file path per line.

-r, --recursive       Include sub directories. Default to false.

--remove-empty-dir    Remove all empty folders.

--help                Display this help screen.

--version             Display version information.

dir (pos. 0)          The target folder. Default to current folder.
```

## License

MIT