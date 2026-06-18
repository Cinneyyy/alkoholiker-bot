using NetCord.Gateway;
using NetCord.Hosting.Gateway;

namespace src.CallHistory;

public sealed class VoiceStateUpdateGatewayHandler : IVoiceStateUpdateGatewayHandler
{
    // Fires when a user joins a channel, or switches. It does not call disconnect if the user switches!
    async ValueTask IVoiceStateUpdateGatewayHandler.HandleAsync(VoiceState arg)
    {
        if(arg.User.IsBot)
            return;

        try
        {
            Directory.CreateDirectory(CallHistoryMgr.GetPath("users"));
            Directory.CreateDirectory(CallHistoryMgr.GetPath("channels"));

            if(arg.ChannelId is u64 channelId) // User joined a voice channel.
            {// User switched over from another channel, if so, handle disconnect.

                if(File.Exists(CallHistoryMgr.GetPath($"users/{arg.UserId}")))
                {
                    u64 prevChannel = u64.Parse(File.ReadAllText(CallHistoryMgr.GetPath($"users/{arg.UserId}")).Trim());

                    if(prevChannel == channelId) // User muted or deafened themself
                        return;

                    // Else: user switched voice channel, so disconnect them before reconnecting
                    await CallHistoryMgr.HandleDisconnect(arg.GuildId, arg.UserId);
                }

                CallHistoryMgr.HandleConnect(channelId, arg.UserId);
            }
            else
                await CallHistoryMgr.HandleDisconnect(arg.GuildId, arg.UserId);
        }
        catch(Exception e)
        {
#if DEBUG
            Log.Out($"State mismatch in vc_state (user: {arg.UserId}; channel: {arg.ChannelId ?? 0ul}; exception: {e}).");
#elif RELEASE
            Log.Out($"State mismatch in vc_state (user: {arg.UserId}; channel: {arg.ChannelId ?? 0ul}; exception: {e.Message}).");
#endif
        }
    }
}
