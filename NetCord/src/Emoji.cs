using System.Text.RegularExpressions;
using NetCord;
using NetCord.Rest;

namespace src;

public readonly partial record struct Emoji
{
    public readonly string name = null;
    public readonly u64 id = 0ul;


    public Emoji(string builtIn)
        => name = builtIn.Trim().Trim(':');

    public Emoji(string customName, u64 customId)
    {
        name = customName;
        id = customId;
    }


    public override string ToString()
        => id != 0ul
            ? $"<:{name ?? "custom"}:{id}>"
            : $"{name}";

    public EmojiProperties ToEmojiProperties()
        => id != 0ul
            ? EmojiProperties.Custom(id)
            : EmojiProperties.Standard(name);

    public ReactionEmojiProperties ToReactionProperties()
        => id != 0ul
            ? new(name, id)
            : new(name);


    public static Emoji Parse(string emoji)
    {
        if(emoji.Contains(':'))
#if DEBUG
        {
            Log.Out($"Substituting emoji {emoji} with {Config.customEmojiFallback}");
            return new("🇭");
        }
#elif RELEASE
            return new(emoji.Split(':')[0], u64.Parse(emoji.Split(':')[1]));
#endif
        else
            return new(emoji);
    }

    [GeneratedRegex(@"<a?:[a-zA-Z0-9_]+:[0-9]+>")]
    public static partial Regex CustomEmojiRegex();
}
