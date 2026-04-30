using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace src.Rules;

public readonly partial record struct Reply()
{
    public string text { get; init; }
    public string emoji { get; init; }
    public Poll poll { get; init; }
    public string[] reactions { get; init; }
    public bool silent { get; init; } = true;
    public bool refMessage { get; init; } = false;
    public Dictionary<string, string[]> snippets { get; init; } = [];
    public f32 weight { get; init; } = 1f;
    public string[] attachments { get; init; } = [];
    public string attachment { init => attachments = [value]; }
    public readonly string randomAttachment => attachments.Length == 1 && Directory.Exists(attachments.First())
        ? Directory.GetFiles(attachments.First(), "*.*", SearchOption.AllDirectories).SelectRandom()
        : attachments.SelectRandom();


    /// <summary>@...@ => mention; :...: => emoji; $...$ => snippet; %...% => regex group.</summary>
    public string FormatText(RuleCollection rules, Match match = null)
    {
        string text = this.text;

        Dictionary<string, string[]> snippets = this.snippets;
        text = SnippetRegex().Replace(text, m =>
        {
            string[] snippetValues = snippets[m.Groups[1].Value];
            return snippetValues[Random.Shared.Next(snippetValues.Length)];
        });

        text = FormatMentionRegex().Replace(text, m => rules.GetUser(m.Groups[1].Value).mention);
        text = EmojiRegex().Replace(text, m => rules.GetEmoji(m.Groups[1].Value).ToString());

        if(match is not null)
            text = RegexRegex().Replace(text, m => match.Groups[i32.Parse(m.Groups[1].Value)].Value);

        return text;
    }

    public override string ToString()
        => ToString("");
    public string ToString(string lnPrefix)
    {
        StringBuilder sb = new();

        if(text is not null) sb.AppendLine($"{lnPrefix}text: \"{text}\"");
        if(emoji is not null) sb.AppendLine($"{lnPrefix}emoji: \"{emoji}\"");
        if(reactions is not (null or [])) sb.AppendLine($"{lnPrefix}reactions: [{string.Join(", ", reactions)}]");
        if(attachments is not []) sb.AppendLine($"{lnPrefix}attachment(s): [{string.Join(", ", attachments.Select(str => $"\"{str}\""))}]");
        if(!silent) sb.AppendLine($"{lnPrefix}silent: {silent}");
        if(snippets is not null && snippets.Count > 0) sb.AppendLine($"{lnPrefix}snippets: [{string.Join(", ", snippets.Select(kvp => $"({kvp.Key}: [{string.Join(", ", kvp.Value)}])"))}]");
        if(poll != default)
        {
            sb.AppendLine($"{lnPrefix}poll:");
            sb.Append(poll.ToString("  " + lnPrefix));
        }
        if(weight != 1f) sb.AppendLine($"{lnPrefix}weight: {weight:0.00}");

        return sb.ToString();
    }


    [GeneratedRegex("@([a-z0-9\\-_]+)@")] private static partial Regex FormatMentionRegex();
    [GeneratedRegex(":([a-z0-9\\-_~;]+):")] private static partial Regex EmojiRegex();
    [GeneratedRegex("\\$([a-z0-9\\-_]+)\\$")] private static partial Regex SnippetRegex();
    [GeneratedRegex("%([0-9]+)%")] private static partial Regex RegexRegex();
}