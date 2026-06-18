using NetCord;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;

namespace src.CallHistory;

[SlashCommand("call-stats", "call-stats")]
public sealed class CallStatsCommands : ApplicationCommandModule<ApplicationCommandContext>
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
        (u64 id, u32 seconds)[] stats = CallStatistics.GetAllCallSeconds();
        Array.Sort(stats, (a, b) => (i32)((i64)b.seconds - a.seconds));

        if(stats is [])
        {
            await RespondAsync(InteractionCallback.Message(new()
            {
                Content = $"No user has spent any time in voice calls.",
                Flags = MessageFlags.Get(ephemeral: ephemeral)
            }));

            return;
        }

        string[] names = new string[stats.Length];
        for(i32 i = 0; i < names.Length; i++)
            names[i] = (await App.restClient.GetUserAsync(stats[i].id)).GlobalName;

        await RespondAsync(InteractionCallback.Message(new()
        {
            Embeds =
            [
                new()
                {
                    Title = "Call statistics",
                    Fields =
                    [
                        new()
                        {
                            Name = "**User**",
                            //Value = string.Join("\n", stats.Select(stat => $"<@{stat.id}>")),
                            Value = $"```\n{string.Join("\n", names)}```",
                            Inline = true
                        },
                        new()
                        {
                            Name = "**Time**",
                            Value = $"```\n{string.Join("\n", stats.Select(stat => App.GetTimeStr(TimeSpan.FromSeconds(stat.seconds))))}```",
                            Inline = true
                        }
                    ],
                    Color = new((i32)Random.Shared.NextRgb())
                },
            ],
            Flags = MessageFlags.Get(ephemeral: ephemeral)
        }));
    }
}
