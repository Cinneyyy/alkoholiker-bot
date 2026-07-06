using NetCord;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;

namespace src.Casino;

public sealed partial class CasinoCommands
{
    [SubSlashCommand("level", "level")]
    public sealed class Level : ApplicationCommandModule<ApplicationCommandContext>
    {
        [SubSlashCommand("get", "Get your or someone's current level and xp.")]
        public async Task Get(User user = null, bool ephemeral = true)
        {
            user ??= Context.User;
            GuildUserPair guildUser = (Context.Guild.Id, user.Id);

            if(user.IsBot)
            {
                await RespondAsync(InteractionCallback.Message(new()
                {
                    Content = "A bot does not contain user data.",
                    Flags = MessageFlags.Get(ephemeral: ephemeral)
                }));

                return;
            }

            u32 xp = LevelUpMgr.GetStat(guildUser, LevelUpMgr.Stat.Xp);
            u32 level = LevelUpMgr.GetStat(guildUser, LevelUpMgr.Stat.Level);

            await RespondAsync(InteractionCallback.Message(new()
            {
                Content = $"<@{user.Id}>'s progess to level `{level+1}`: `{xp}`/`{LevelUpMgr.GetRequiredXp(level)}` XP.",
                Flags = MessageFlags.Get(ephemeral: ephemeral)
            }));
        }

        [SubSlashCommand("get-all", "Get all users' level progress.")]
        public async Task GetAll(bool ephemeral = true)
        {
            IEnumerable<(string name, string xp, string reqXp, string level)> levelStats = LevelUpMgr.GetGuildUserStats(Context.Guild.Id)
                .OrderByDescending(stat => stat.xp)
                .OrderByDescending(stat => stat.level)
                .Select(stat => (
                    name: UserCache.GetName(stat.user, Context.Guild.Id),
                    xp: stat.xp.ToString(),
                    reqXp: LevelUpMgr.GetRequiredXp(stat.level).ToString(),
                    level: stat.level.ToString()
                ));

            if(!levelStats.Any())
            {
                await RespondAsync(InteractionCallback.Message(new()
                {
                    Content = $"No user has any level progress.",
                    Flags = MessageFlags.Get(ephemeral: ephemeral)
                }));

                return;
            }

            i32 xpPad = levelStats.Max(stat => stat.xp.Length);
            i32 reqXpPad = levelStats.Max(stat => stat.reqXp.Length);
            i32 levelPad = levelStats.Max(stat => stat.level.Length);

            await RespondAsync(InteractionCallback.Message(new()
            {
                Embeds =
                [
                    new()
                    {
                        Title = "Levels",
                        Description =
                            "```\n" +
                            string.Join("\n", levelStats
                                .Select(stat => $"[ level {stat.level.PadRight(levelPad)} ~ {stat.xp.PadLeft(xpPad)}/{stat.reqXp.PadRight(reqXpPad)} XP ]  {stat.name}")) +
                            "```",
                        Color = new((i32)Random.Shared.NextRgb())
                    }
                ],
                Flags = MessageFlags.Get(ephemeral: ephemeral)
            }));
        }
    }
}
