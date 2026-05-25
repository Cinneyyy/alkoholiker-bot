using System.Text.RegularExpressions;

namespace src.Rules;

public readonly struct Regex()
{
    public readonly required string regex { get; init; }
    public readonly bool ignoreCase { get; init; } = false;


    public bool Match(string text, out Match match)
    {
        match = System.Text.RegularExpressions.Regex.Match(text, regex, RegexOptions.Multiline | (ignoreCase ? RegexOptions.IgnoreCase : 0));
        return match?.Success ?? false;
    }
}