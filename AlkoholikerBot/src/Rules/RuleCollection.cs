using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Discord;

namespace src.Rules;

public sealed class RuleCollection
{
    private static readonly JsonSerializerOptions jsonOptions = new()
    {
        AllowTrailingCommas = true,
        IncludeFields = true,
        NumberHandling = JsonNumberHandling.AllowReadingFromString | JsonNumberHandling.AllowNamedFloatingPointLiterals,
        ReadCommentHandling = JsonCommentHandling.Skip
    };

    public readonly List<EmojiAlias> emoji = [];
    public readonly List<UserAlias> users = [];
    public readonly List<ChannelAlias> channels = [];
    public readonly List<Rule> rules = [];
    public readonly u32 defaultCooldownSeconds = 0u;


    public ChannelAlias? GetChannel(string alias)
    {
        if(alias.StartsWith('~'))
            return new() { alias = "", id = u64.Parse(alias[1..]) };
        else
            return channels.FindAll(c => c.alias == alias).First();
    }

    public UserAlias GetUser(string alias)
    {
        if(alias.StartsWith('~'))
            return new() { alias = "", id = u64.Parse(alias[1..]) };
        else
            return users.FindAll(u => u.alias == alias).First();
    }

    public IEmote GetEmoji(string alias)
    {
        if(alias is null)
            return null;

        if(alias.StartsWith('~'))
            return new EmojiAlias(alias[1..]).emote;

        return emoji.FindAll(e => e.alias == alias).FirstOrDefault().emote;
    }

    public void UnloadAll()
    {
        emoji.Clear();
        users.Clear();
        channels.Clear();
        rules.Clear();
    }

    public void Load(string json)
    {
        RuleCollectionJsonParsable collection = JsonSerializer.Deserialize<RuleCollectionJsonParsable>(json, jsonOptions);

        emoji.AddRange(collection.emoji.Select(kvp => new EmojiAlias(kvp.Key, kvp.Value)));
        users.AddRange(collection.users.Select(kvp => new UserAlias() { alias = kvp.Key, id = kvp.Value }));
        channels.AddRange(collection.channels.Select(kvp => new ChannelAlias() { alias = kvp.Key, id = kvp.Value }));
        rules.AddRange(collection.rules);
    }
}