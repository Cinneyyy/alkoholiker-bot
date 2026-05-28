using NetCord;
using NetCord.Gateway;
using NetCord.Hosting.Gateway;
using NetCord.Rest;

namespace src;

public sealed class CallLeaveGatewayHandler : IVoiceStateUpdateGatewayHandler
{
    // Fires when a user joins a channel, or switches. It does not call disconnect if the user switches!
    async ValueTask IVoiceStateUpdateGatewayHandler.HandleAsync(VoiceState arg)
    {
        if(arg.User.IsBot)
            return;

        try
        {
            Directory.CreateDirectory(GetPath("users"));
            Directory.CreateDirectory(GetPath("channels"));

            if(arg.ChannelId is u64 channelId) // User joined a voice channel.
            {
                if(File.Exists(GetPath($"users/{arg.UserId}"))) // User switched from another channel, if so, handle disconnect
                    await HandleDisconnect(arg.GuildId, arg.UserId);

                HandleConnect(channelId, arg.UserId);

            }
            else
                await HandleDisconnect(arg.GuildId, arg.UserId);
        }
        catch(Exception e)
        {
            Console.WriteLine($"State mismatch in vc_state (user: {arg.UserId}; channel: {arg.ChannelId ?? 0ul}; exception: {e.Message}).");
        }
    }


    private static string GetPath(string file)
        => App.GetPath($"vc_state/{file}");

    private static void HandleConnect(u64 channel, u64 user)
    {
        if(Directory.Exists(GetPath($"channels/{channel}"))) // Call is already ongoing.
        {
            File.Create(GetPath($"channels/{channel}/active/{user}"));
            File.WriteAllText(GetPath($"users/{user}"), channel.ToString());
        }
        else // User is first to join channel.
        {
            Directory.CreateDirectory(GetPath($"channels/{channel}/active"));
            Directory.CreateDirectory(GetPath($"channels/{channel}/history"));

            File.WriteAllText(GetPath($"channels/{channel}/session_start"), DateTime.UtcNow.Ticks.ToString());

            File.Create(GetPath($"channels/{channel}/active/{user}"));
            File.WriteAllText(GetPath($"users/{user}"), channel.ToString());
        }
    }

    private static async Task HandleDisconnect(u64 guildId, u64 user)
    {
        u64 channel = u64.Parse(File.ReadAllText(GetPath($"users/{user}")));
        File.Delete(GetPath($"users/{user}"));

        File.Move(GetPath($"channels/{channel}/active/{user}"), GetPath($"channels/{channel}/history/{user}"), true);

        if(Directory.GetFiles(GetPath($"channels/{channel}/active")).Length == 0) // User is the last one to leave the call
        {
            IEnumerable<u64> participants = Directory.GetFiles(GetPath($"channels/{channel}/history"))
                .Select(Path.GetFileName)
                .Select(u64.Parse);

            i64 sessionStart = i64.Parse(File.ReadAllText(GetPath($"channels/{channel}/session_start")));
            Directory.Delete(GetPath($"channels/{channel}"), true);

            TimeSpan time = TimeSpan.FromTicks(DateTime.UtcNow.Ticks - sessionStart);

            RestGuild guild = await App.client.GetGuildAsync(guildId);
            IReadOnlyList<IGuildChannel> guildChannels = await guild.GetChannelsAsync();
            TextGuildChannel textChannel = guildChannels.First(c => c.Id == Config.callHistoryChannel) as TextGuildChannel;

            await textChannel.SendMessageAsync(new()
            {
                Embeds =
                [
                    new()
                    {
                        Title = $"Call ended in <#{channel}> that lasted {time.Days*24 + time.Hours}h {time.Minutes}m {time.Seconds}s",
                        Description = $"**Participants**:\n\n{string.Join("\n", participants.Select(p => $"<@{p}>"))}",
                        Color = new((u8)Random.Shared.Next(), (u8)Random.Shared.Next(), (u8)Random.Shared.Next())
                    }
                ],
                Flags = MessageFlags.Get(ephemeral: false)
            });
        }
    }
}
