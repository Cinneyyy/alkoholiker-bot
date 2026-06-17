using System;
using Discord;

namespace src;

public readonly record struct EmojiAlias()
{
    public string alias { get; init; }
    public string name { get; init; }
    public ulong customId { get; init; } = 0ul;
    public bool animated { get; init; } = false;

    public IEmote emote => this == default ? null : (customId == 0ul ? Emoji.Parse($":{name}:") : new Emote(customId, name, animated));


    public EmojiAlias(string alias, string value) : this()
    {
        this.alias = alias;

        value = value.Replace(';', ':');

        if(!value.Contains(':'))
            name = value;
        else
        {
            string[] customEmojiArgs = value.Split(':');
            animated = customEmojiArgs[0] == "a";
            name = customEmojiArgs[1];
            customId = u64.Parse(customEmojiArgs[2]);
        }
    }

    public EmojiAlias(string value) : this("", value)
    {
    }
}