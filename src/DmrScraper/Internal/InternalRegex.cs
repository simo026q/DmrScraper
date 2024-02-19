using System.Text.RegularExpressions;

namespace DmrScraper.Internal;

internal static partial class InternalRegex
{
    [GeneratedRegex("e(?<e>\\d+)s(?<s>\\d+)", RegexOptions.Compiled)]
    public static partial Regex GetDmrExecutionRegex();
}
