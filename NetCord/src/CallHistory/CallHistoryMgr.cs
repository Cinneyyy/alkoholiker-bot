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

        u64 channel = u64.Parse(File.ReadAllText(GetPath($"users/{user}")).Trim()); // Read channel from users/@
        File.Delete(GetPath($"users/{user}")); // Delete users/@
        File.Delete(GetPath($"channels/{channel}/active/{user}")); // Delete channels/#/active/@
        File.AppendAllLines(GetPath($"channels/{channel}/history/{user}"), [now.ToString()]); // Write time to channels/#/history/@

        if(Directory.GetFiles(GetPath($"channels/{channel}/active")).Length != 0) // User is the last one to leave the call.
            return;

        (u64 id, f32 partSeconds)[] participants = Directory
            .GetFiles(GetPath($"channels/{channel}/history")) // Get files in channels/#/history
            .Select(f => (
                id: u64.Parse(Path.GetFileName(f)),
                partSeconds: (f32)(File.ReadAllLines(f)
                    .Where(ln => !string.IsNullOrWhiteSpace(ln))
                    .Select(ln => ln.Trim())
                    .Select(i64.Parse)
                    .Chunk(2)
                    .Where(chunk => chunk.Length == 2)
                    .Sum(ticks => ticks[1] - ticks[0]) / TimeSpan.TicksPerSecond)
                ))
            .OrderByDescending(p => p.partSeconds)
            .ToArray();

        CallStatistics.OnVoiceCallEnd(participants.Select(p => (p.id, (u32)p.partSeconds)));

        i64 sessionStart = i64.Parse(File.ReadAllText(GetPath($"channels/{channel}/session_start")).Trim()); // Read time from channels/#/session_start
        Directory.Delete(GetPath($"channels/{channel}"), true); // Delete channels/#/

        if(participants.Length <= 1 && !Config.logSolitaryCalls)
        {
            Log.Out($"Call ended ({guildId}:{channel}, with user {user}), but skipping log, since they were the only participant. To change this behaviour, enable Config.logSolitaryCalls.");
            return;
        }
        else
            Log.Out($"Call ended in {guildId}:{channel}, with user {user}.");

        if(string.IsNullOrEmpty(Config.callHistoryChannel))
            return;

        TimeSpan time = TimeSpan.FromTicks(DateTime.UtcNow.Ticks - sessionStart);

        RestGuild guild = await App.restClient.GetGuildAsync(guildId);
        IReadOnlyList<IGuildChannel> guildChannels = await guild.GetChannelsAsync();
        TextGuildChannel textChannel = guildChannels.First(c => c.Name.Equals(Config.callHistoryChannel, StringComparison.OrdinalIgnoreCase)) as TextGuildChannel;

        IEnumerable<(string name, string hours, string percent)> partFmtData = participants
            .Select(p => (
                name: UserCache.GetName(p.id),
                hours: (p.partSeconds / 3600f).ToString("0.0h"),
                percent: (p.partSeconds / time.TotalSeconds).ToString("0%")
            ));

        (i32 hourPad, i32 percentPad) = (partFmtData.First().hours.Length, partFmtData.First().percent.Length);

        await textChannel.SendMessageAsync(new()
        {
            Embeds =
            [
                new()
                {
                    Title = $"Call ended in <#{channel}>",
                    Description =
                        "```\n" +
                        string.Join("\n", partFmtData 
                            .Select(p => $"[ {p.percent.PadLeft(percentPad)} | {p.hours.PadLeft(hourPad)} ]  {p.name}")
                        ) +
                        "```",
                    Color = new((i32)Random.Shared.NextRgb()),
                    Footer = new()
                    {
                        Text = App.GetTimeStr(time)
                    },
                    Timestamp = new(DateTime.Now.Subtract(time))
                }
            ],
            Flags = MessageFlags.Get(ephemeral: false)
        });
    }
}
