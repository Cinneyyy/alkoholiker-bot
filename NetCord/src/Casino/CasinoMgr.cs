using NetCord;
using NetCord.Rest;

namespace src.Casino;

// TODO: gamble, mafia, bonus when bot reacts, anon votes, fix seperate thread breaking bot
public static class CasinoMgr
{
    public static async Task OnLevelUp(GuildUserPair guildUser, u32 level, u32 reward)
    {
        CurrencyMgr.AddCurrency(guildUser, reward);

        RestGuild guild = await App.restClient.GetGuildAsync(guildUser.guild);
        IReadOnlyList<IGuildChannel> guildChannels = await guild.GetChannelsAsync();
        if(guildChannels.First(c => c.Name.Equals(Config.casinoChannel, StringComparison.OrdinalIgnoreCase)) is not TextGuildChannel textChannel)
            return;

        string currency = CurrencyMgr.GetUserCurrencyName(guildUser);
        await textChannel.SendMessageAsync(new()
        {
            Embeds =
            [
                new()
                {
                    Title = $"<@{guildUser.user}> achieved level {level}",
                    Description = $"You earned {CurrencyMgr.FormatCurrency(reward, currency)} and now have {CurrencyMgr.FormatCurrency(CurrencyMgr.GetRawCurrency(guildUser), currency)}",
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
}
