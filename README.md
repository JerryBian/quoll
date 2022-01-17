# quoll
Command line tool to DELETE files

## Options

```
-y, --yes          (Default: false) Confirmation for deletion.

-n, --name         (Group: filter criteria) Filter by glob pattern file names. Multiple names can be separated by
                    comma.

-s, --size         (Group: filter criteria) Filter by file size(less than or equals), valid strings are xxB, xxKB,
                    xxMB, xxGB.

--backup           Save to backup location before deletion.

--from-file        File paths to delete. One file path per line.

-r, --recursive    Include sub directories. Default to false.

--help             Display this help screen.

--version          Display version information.

dir (pos. 0)       The target folder. Default to current folder.
```

## License

[MIT](./LICENSE)