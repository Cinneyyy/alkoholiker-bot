using NetCord;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;

namespace src.CallHistory;

[SlashCommand("call-stats", "call-stats")]
public sealed class CallStatisticsCommands : ApplicationCommandModule<ApplicationCommandContext>
{
    [SubSlashCommand("get", "Get your or someone else's call statistics.")]
    public async Task Get(User user = null, bool ephemeral = true)
    {
        user ??= Context.User;

        u32 totalSeconds = CallStatistics.GetUserCallSeconds(user.Id);
        TimeSpan time = TimeSpan.FromSeconds(totalSeconds);

        await RespondAsync(InteractionCallback.Message(new()
        {
            Content = $"{(user.Id == Context.User.Id ? $"You [<@{user.Id}>] have" : $"<@{user.Id}> has")} spent {App.GetTimeStr(time)} in voice calls.",
            Flags = MessageFlags.Get(ephemeral: ephemeral)
        }));
    }

    [SubSlashCommand("get-all", "Get all users' call statistics.")]
    public async Task GetAll(bool ephemeral = true)
    {
        await RespondAsync(InteractionCallback.DeferredMessage(MessageFlags.Get(ephemeral: ephemeral)));

        MessageProperties message = CallStatistics.CreateStatMessage(Context.Guild?.Id ?? 0ul);
        await FollowupAsync(new()
        {
            Content = message.Content,
            Embeds = message.Embeds,
            Flags = MessageFlags.Get(ephemeral: ephemeral)
        });
    }

    [SubSlashCommand("remove", "[!] Remove a user from the call statistics.")]
    public async Task Remove(User user, bool ephemeral = true)
    {
        if(!await App.CheckForOwner(Context))
            return;

        CallStatistics.RemoveUser(user.Id);

        await RespondAsync(InteractionCallback.Message(new()
        {
            Content = $"Successfully removed <@{user.Id}> from the call statistics.",
            Flags = MessageFlags.Get(ephemeral: ephemeral)
        }));
    }
}
