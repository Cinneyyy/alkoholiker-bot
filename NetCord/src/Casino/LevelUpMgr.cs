using NetCord.Gateway;

namespace src.Casino;

public static class LevelUpMgr
{
    public enum Stat : u8
    {
        Xp,
        Level
    }


    private static readonly Dictionary<GuildUserPair, i64> lastMessageTimeStamps = [];


    public static void HandleUserMsg(Message message)
    {
        if(message.GuildId is u64 guildId)
            GiveXp((guildId, message.Author.Id), GetMessageValue(message));
    }

    public static void GiveXp(GuildUserPair guildUser, u32 amount)
    {
        u32 xp = GetStat(guildUser, Stat.Xp) + amount;
        u32 level = GetStat(guildUser, Stat.Level);

        if(xp < GetRequiredXp(level))
        {
            SetStat(guildUser, Stat.Xp, xp);
            return;
        }

        u32 reward = 0u;
        while(GetRequiredXp(level) is u32 reqXp && xp >= reqXp)
        {
            level++;
            xp -= reqXp;
            reward += CasinoMgr.GetReward(level);
        }

        SetStat(guildUser, Stat.Xp, xp);
        SetStat(guildUser, Stat.Level, level);

        _ = CasinoMgr.OnLevelUp(guildUser, level, reward);
    }

    public static u32 GetStat(GuildUserPair guildUser, Stat stat)
        => File.ReadAllText(CasinoMgr.GetPath(guildUser, stat.ToString(), "0")).Trim().ParseU32();

    public static u32 GetRequiredXp(u32 currLevel)
        => 1000u + (currLevel * 500u);

    public static (u64 user, u32 level, u32 xp)[] GetGuildUserStats(u64 guild)
        => Directory.GetFiles(App.GetPath($"casino/user_data/{guild}/"), $"*_{Stat.Xp}")
            .Select(Path.GetFileName)
            .Select(f => f[..f.IndexOf('_')])
            .Select(u64.Parse)
            .Select(u => (
                user: u,
                level: GetStat((guild, u), Stat.Level),
                xp: GetStat((guild, u), Stat.Xp)
            ))
            .ToArray();


    private static u32 GetMessageValue(Message message)
    {
        // Attachment?
        // yes => 20 points
        // no => 0 points
        u32 value = message.Attachments.Count > 0 ? 20u : 0u;

        // Message length
        // 0-100 chars ~ 0-30 points
        value += u32.Clamp((u32)message.Content.Length, 0u, 100u) * 30u / 100u;

        GuildUserPair guildUser = (message.GuildId.Value, message.Author.Id);
        i64 now = DateTime.UtcNow.Ticks;
        if(lastMessageTimeStamps.TryGetValue(guildUser, out i64 timeStamp))
        {
            i64 deltaMins = (now - timeStamp) / TimeSpan.TicksPerMinute;

            // Message pause
            // < 15mins => 0 points
            // 15-60mins ~ 0-50 points
            if(deltaMins >= 15)
                value += 50u * ((u32)i64.Clamp(deltaMins, 15, 60) - 15u) / (60u-15u);
        }

        lastMessageTimeStamps[guildUser] = now;

        return value;
    }

    private static void SetStat(GuildUserPair guildUser, Stat stat, u32 value)
        => File.WriteAllText(CasinoMgr.GetPath(guildUser, stat.ToString(), "0"), value.ToString());
}
