using src.Rules.Language.Ast;

namespace src.Rules.Language;

public static class Runtime
{
    public static Dictionary<string, u64> userAliases = [];
    public static Dictionary<string, u64> channelAliases = [];
    public static Dictionary<string, Emoji> emojiAliases = [];
    public static List<Function> functions =
    [
    ];


    public static Emoji GetEmoji(object value)
    {
        if(value is string str)
        {
            if(str.StartsWith('~'))
                return emojiAliases[str[1..]];
            else
                return new(str);
        }
        else if(value is u64 id)
            return new(id);
        else
            throw new($"Invalid emoji ({value}).");
    }

    public static u64 GetChannel(object value)
    {
        if(value is string str)
            return channelAliases[str];
        else if(value is u64 id)
            return id;
        else
            throw new($"Invalid channel ({value}).");
    }

    public static u64 GetUser(object value)
    {
        if(value is string str)
            return channelAliases[str];
        else if(value is u64 id)
            return id;
        else
            throw new($"Invalid user ({value}).");
    }
}