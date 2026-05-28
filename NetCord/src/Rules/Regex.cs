using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using SysRegex = System.Text.RegularExpressions.Regex;

namespace src.Rules;

public readonly struct Regex()
{
    public readonly required string pattern { get; init; }
    public readonly bool ignoreCase { get; init; } = false;
    [JsonIgnore] public readonly RegexOptions options => RegexOptions.Multiline | (ignoreCase ? RegexOptions.IgnoreCase : 0);


    public bool Match(string text, out Match match)
    {
        match = SysRegex.Match(text, pattern, options);
        return match?.Success ?? false;
    }

    public bool IsMatch(string text)
        => SysRegex.IsMatch(text, pattern, options);
}