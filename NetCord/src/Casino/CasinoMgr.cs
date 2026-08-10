using NetCord;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;

namespace src.Casino;

// TODO: gamble, mafia, bonus when bot reacts, anon votes, fix seperate thread breaking bot
public static class CasinoMgr
{
    public static async Task OnLevelUp(GuildUserPair guildUser, u32 level, u32 reward)
    {
        CurrencyMgr.AddCurrency(guildUser, reward);
        CasinoStatsMgr.OnCurrencyChange(guildUser, CurrencySource.LevelUp, reward);

        RestGuild guild = await App.restClient.GetGuildAsync(guildUser.guild);
        IReadOnlyList<IGuildChannel> guildChannels = await guild.GetChannelsAsync();
        if(guildChannels.First(c => c.Name.Equals(Config.casinoChannel, StringComparison.OrdinalIgnoreCase)) is not TextGuildChannel textChannel)
            return;

        await textChannel.SendMessageAsync(new()
        {
            Embeds =
            [
                new()
                {
                    Title = $"Level up!",
                    Description = $"<@{guildUser.user}> achieved level **{level}**!\n\nThey earned **[{CurrencyMgr.FormatCurrency(reward, guildUser)}]**.\nThey now have **[{CurrencyMgr.FormatCurrency(guildUser)}]**.\n\nXP required for level **{level+1}**: **{LevelUpMgr.GetRequiredXp(level)}**",
                    Color = new((i32)Random.Shared.NextRgb())
                }
            ],
            Flags = MessageFlags.Get(ephemeral: false)
        });
    }

    public static string GetPath(GuildUserPair guildUser, string suffix, string defaultValue)
    {
        string path = App.GetPath($"casino/user_data/{guildUser.guild}/");
        Directory.CreateDirectory(path);

        path += $"{guildUser.user}_{suffix}";
        if(!File.Exists(path))
            File.WriteAllText(path, defaultValue);

        return path;
    }

    public static u32 GetReward(u32 newLevel)
    {
        u32 reward = LevelUpMgr.GetRequiredXp(newLevel-1)*10;
        reward += (u32)Random.Shared.Next(-(i32)reward / 8, (i32)reward / 8);
        return reward;
    }

    public static async Task<bool> IsValidAmount(ApplicationCommandContext context, u32 amount)
    {
        async Task<bool> fail(string err)
        {
            await context.Interaction.SendResponseAsync(InteractionCallback.Message(new()
            {
                Content = err,
                Flags = MessageFlags.Get()
            }));

            return false;
        }

        if(context.User.IsBot)
            return await fail("Bots cannot contain user data.");

        i64 ownedCurrency = CurrencyMgr.GetRawCurrency((context.Guild.Id, context.User.Id));

        if(amount > ownedCurrency)
            return await fail("You cannot bet more than you own.");

        return true;
    }

    public static MessageProperties CreateLevelStatMessage(u64 guildId)
    {

        IEnumerable<(string name, string xp, string reqXp, string level)> levelStats = LevelUpMgr.GetGuildUserStats(guildId)
            .OrderByDescending(stat => stat.xp)
            .OrderByDescending(stat => stat.level)
            .Select(stat => (
                name: UserCache.GetName(stat.user, guildId),
                xp: stat.xp.ToString(),
                reqXp: LevelUpMgr.GetRequiredXp(stat.level).ToString(),
                level: stat.level.ToString()
            ));

        if(!levelStats.Any())
        {
            return new()
            {
                Content = $"No user has any level progress."
            };
        }

        i32 reqXpPad = levelStats.Max(stat => stat.reqXp.Length);
        i32 levelPad = levelStats.Max(stat => stat.level.Length);

        return new() 
        {
            Embeds =
            [
                new()
                {
                    Title = "Levels",
                    Description =
                        "```\n" +
                        string.Join("\n", levelStats
                            .Select(stat => $"[ level {stat.level.PadLeft(levelPad)} ~ {stat.xp.PadLeft(stat.reqXp.Length, '0').PadLeft(reqXpPad - stat.reqXp.Length)}/{stat.reqXp.PadRight(reqXpPad)} XP ]  {stat.name}")) +
                        "```",
                    Color = new((i32)Random.Shared.NextRgb())
                }
            ]
        };
    }

    public static MessageProperties CreateCurrencyStatMessage(u64 guildId)
    {
        IEnumerable<(u64 user, string currency)> currStats = CurrencyMgr.GetAllCurrency(guildId)
            .OrderByDescending(stat => stat.currency)
            .Select(c => (
                user: c.user,
                currency: CurrencyMgr.FormatCurrency(c.currency, (guildId, c.user))
            ));

        if(!currStats.Any())
        {
            return new()
            {
                Content = "Nobody has any currency."
            };
        }

        return new()
        {
            Embeds =
            [
                new()
                {
                    Title = "Currency",
                    Description = string.Join("\n", currStats
                        .Select(stat => $"<@{stat.user}>: **[{stat.currency}]**")
                    ),
                    Color = new((i32)Random.Shared.NextRgb())
                }
            ]
        };
    }
}
