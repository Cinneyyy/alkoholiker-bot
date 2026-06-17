using NetCord;
using NetCord.Rest;

namespace src.CallHistory;

public static class CallHistoryMgr
{
    public static string GetPath(string file)
        => App.GetPath($"vc_state/{file}");

    public static void HandleConnect(u64 channel, u64 user)
    {
        Log.Out($"User {user} ({App.restClient.GetUserAsync(user).GetAwaiter().GetResult().Username}) joined voice channel {channel}");
        string now = DateTime.UtcNow.Ticks.ToString();

        if(!Directory.Exists(GetPath($"channels/{channel}"))) // User is first to join voice channel
        {
            Directory.CreateDirectory(GetPath($"channels/{channel}/active"));
            Directory.CreateDirectory(GetPath($"channels/{channel}/history"));

            File.WriteAllText(GetPath($"channels/{channel}/session_start"), now); // Write time to channels/#/session_start
        }

        File.WriteAllText(GetPath($"channels/{channel}/active/{user}"), string.Empty); // Create channels/#/active/@
        File.AppendAllLines(GetPath($"channels/{channel}/history/{user}"), [now]); // Append time to channels/#/history/@
        File.WriteAllText(GetPath($"users/{user}"), channel.ToString()); // Write channel to users/@
    }

    public static async Task HandleDisconnect(u64 guildId, u64 user)
    {
        Log.Out($"User {user} ({App.restClient.GetUserAsync(user).GetAwaiter().GetResult().Username}) left voice channel in guild {guildId}");
        i64 now = DateTime.UtcNow.Ticks;

        u64 channel = u64.Parse(File.ReadAllText(GetPath($"users/{user}"))); // Read channel from users/@
        File.Delete(GetPath($"users/{user}")); // Delete users/@
        File.Delete(GetPath($"channels/{channel}/active/{user}")); // Delete channels/#/active/@
        File.AppendAllLines(GetPath($"channels/{channel}/history/{user}"), [now.ToString()]); // Write time to channels/#/history/@

        if(Directory.GetFiles(GetPath($"channels/{channel}/active")).Length == 0) // User is the last one to leave the call.
        {
            (u64 id, f64 partSeconds)[] participants = Directory
                .GetFiles(GetPath($"channels/{channel}/history")) // Get files in channels/#/history
                .Select(f => (
                    id: u64.Parse(Path.GetFileName(f)),
                    partSeconds: File.ReadAllLines(f)
                        .Select(i64.Parse)
                        .Chunk(2)
                        .Sum(ticks => ticks[1] - ticks[0]) / (f64)TimeSpan.TicksPerSecond
                    ))
                .OrderByDescending(p => p.partSeconds)
                .ToArray();

            i64 sessionStart = i64.Parse(File.ReadAllText(GetPath($"channels/{channel}/session_start"))); // Read time from channels/#/session_start
            Directory.Delete(GetPath($"channels/{channel}"), true); // Delete channels/#/

            if(participants.Length <= 1 && !Config.logSolitaryCalls)
            {
                Log.Out($"Call ended ({guildId}:{channel}, with user {user}), but skipping log, since they were the only participant. To change this behaviour, enable Config.logSolitaryCalls.");
                return;
            }

            TimeSpan time = TimeSpan.FromTicks(DateTime.UtcNow.Ticks - sessionStart);

            RestGuild guild = await App.restClient.GetGuildAsync(guildId);
            IReadOnlyList<IGuildChannel> guildChannels = await guild.GetChannelsAsync();
            TextGuildChannel textChannel = guildChannels.First(c => c.Id == Config.callHistoryChannel) as TextGuildChannel;

            await textChannel.SendMessageAsync(new()
            {
                Embeds =
                [
                    new()
                    {
                        Title = $"Call ended in <#{channel}> that lasted {time.Days*24 + time.Hours}h {time.Minutes}m {time.Seconds}s",
                        Fields =
                        [
                            new()
                            {
                                Name = "**Participants**",
                                Value = string.Join("\n", participants.Select(p => $"<@{p.id}>")),
                                Inline = true
                            },
                            new()
                            {
                                Name = "**Presence**",
                                Value = string.Join("\n", participants.Select(p => (p.partSeconds/time.TotalSeconds).ToString("0%"))),
                                Inline = true
                            }
                        ],
                        Color = new((i32)Random.Shared.NextRgb())
                    }
                ],
                Flags = MessageFlags.Get(ephemeral: false)
            });
        }
    }
}
