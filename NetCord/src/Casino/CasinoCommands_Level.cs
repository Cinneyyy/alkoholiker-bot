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
            MessageProperties message = CasinoMgr.CreateLevelStatMessage(Context.Guild.Id);

            await RespondAsync(InteractionCallback.Message(new()
            {
                Content = message.Content,
                Embeds = message.Embeds,
                Flags = MessageFlags.Get(ephemeral: ephemeral)
            }));
        }

        [SubSlashCommand("give-xp", "[!] Grant someone a certain amount of XP.")]
        public async Task GiveXp(User user, u32 raw = 0u, f32 hours = 0f, bool ephemeral = true)
        {
            if(!await App.CheckForOwner(Context))
                return;

            if(raw == 0u && hours == 0f)
            {
                await RespondAsync(InteractionCallback.Message(new()
                {
                    Content = "Please specify an amount, either raw or in call hours.",
                    Flags = MessageFlags.Get()
                }));

                return;
            }

            LevelUpMgr.GiveXp((Context.Guild.Id, user.Id), raw, hours);

            await RespondAsync(InteractionCallback.Message(new()
            {
                Content = $"Granted <@{user.Id}> **{raw + LevelUpMgr.GetXpAmountFromHours(hours)} XP**.",
                Flags = MessageFlags.Get(ephemeral: ephemeral)
            }));
        }
    }
}
