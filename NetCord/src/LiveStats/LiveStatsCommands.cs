using NetCord;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;

namespace src.LiveStats;

[SlashCommand("live-stats", "live-stats", Contexts = [InteractionContextType.Guild])]
public sealed class LiveStatsCommands : ApplicationCommandModule<ApplicationCommandContext>
{
    [SubSlashCommand("create", "[!] Create a new live updating message.")]
    public async Task Create(LiveStatsType type)
    {
        if(!await App.CheckForOwner(Context))
            return;

        await RespondAsync(InteractionCallback.Message(new()
        {
            Content = "Creating message.",
            Flags = MessageFlags.Get()
        }));

        await LiveStatsMgr.CreateMessage(Context.Guild.Id, Context.Channel, type);
    }

    [SubSlashCommand("update", "Update existing live stat messages.")]
    public async Task Update(LiveStatsType type)
    {
        await RespondAsync(InteractionCallback.Message(new()
        {
            Content = "Updating messages.",
            Flags = MessageFlags.Get()
        }));

        await LiveStatsMgr.UpdateStatMessages(type);

        await FollowupAsync(new()
        {
            Content = $"Finished!",
            Flags = MessageFlags.Get()
        });
    }
}
