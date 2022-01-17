namespace Quoll.Console;

public static class FileSizeUtil
{
    private static readonly string[] Units = {"KB", "MB", "GB", "B"};

    public static bool GetSizeInBytes(string str, out double bytes)
    {
        bytes = 0;
        if (string.IsNullOrEmpty(str))
        {
            return false;
        }

        var valid = false;
        foreach (var unit in Units)
        {
            if (str.EndsWith(unit, StringComparison.InvariantCulture))
            {
                if (!double.TryParse(str.AsSpan(0, str.IndexOf(unit, StringComparison.InvariantCulture)),
                        out var d))
                {
                    return false;
                }

                valid = true;
                switch (unit)
                {
                    case "B":
                        bytes = d;
                        break;
                    case "KB":
                        bytes = 1024 * d;
                        break;
                    case "MB":
                        bytes = 1024 * 1024 * d;
                        break;
                    case "GB":
                        bytes = 1024 * 1024 * 1024 * d;
                        break;
                }

                break;
            }
        }

        return valid;
    }

    public static string GetSizeStringPrompt()
    {
        return "Valid file size strings are: xxB, xxKB, xxMB, xxGB. xx represents double digit.";
    }
}