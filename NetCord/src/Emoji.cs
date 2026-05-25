using NetCord;

namespace src;

public class Emoji
{
    public string builtIn = null;
    public u64 custom = 0ul;


    public Emoji(string builtIn)
        => this.builtIn = builtIn.Trim().Trim(':');

    public Emoji(u64 custom)
        => this.custom = custom;


    public override string ToString()
        => string.IsNullOrWhiteSpace(builtIn)
            ? $"<:custom:{custom}>"
            : $":{builtIn}:";

    public EmojiProperties ToEmojiProperties()
        => string.IsNullOrWhiteSpace(builtIn)
            ? EmojiProperties.Custom(custom)
            : EmojiProperties.Standard(builtIn);
}