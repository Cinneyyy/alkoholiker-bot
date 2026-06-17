using Discord;

namespace src.Rules;

public readonly record struct TextEmojiPair()
{
    public string text { get; init; }
    public string emoji { get; init; }


    public PollMediaProperties ToPollMediaProperties(RuleCollection rules)
        => new()
        {
            Text = text,
            Emoji = rules.GetEmoji(emoji)
        };

    public override string ToString()
        => string.IsNullOrEmpty(emoji) ? $"\"{text}\"" : $"[{emoji}] \"{text}\"";
}