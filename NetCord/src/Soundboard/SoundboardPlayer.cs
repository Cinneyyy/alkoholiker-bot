using NetCord.Gateway;
using NetCord.Gateway.Voice;

namespace src.Soundboard;

public static class SoundboardPlayer
{
    private static readonly Dictionary<u64, VoiceClient> voiceClients = [];


    public static async Task PlaySound(GatewayClient gatewayClient, u64 guildId, u64 channelId, string filePath, f32 volume)
    {
        if(!voiceClients.TryGetValue(guildId, out VoiceClient voiceClient))
        {
            voiceClient = await gatewayClient.JoinVoiceChannelAsync(guildId, channelId);
            await voiceClient.StartAsync();

            await gatewayClient.UpdateVoiceStateAsync(new(guildId, channelId));

            Log.Out($"Voice client opened ({guildId}:{channelId})");
            voiceClients[guildId] = voiceClient;

            voiceClient.Disconnect += args =>
            {
                Log.Out($"Voice client closed ({guildId}:{channelId}).");
                voiceClients.Remove(guildId);
                voiceClient.Dispose();
                return default;
            };
        }

        if(voiceClient.ChannelId != channelId)
        {
            await Disconnect(guildId);
            await Task.Delay(1000);
            await PlaySound(gatewayClient, guildId, channelId, filePath, volume);
            return;
        }

        await voiceClient.EnterSpeakingStateAsync(new(SpeakingFlags.Soundshare));

        using Stream voiceStream = voiceClient.CreateVoiceStream();
        using OpusEncodeStream opusStream = new(voiceStream, PcmFormat.Short, VoiceChannels.Stereo, OpusApplication.Audio);
        using FileStream audioFileStream = File.OpenRead(filePath);

        try
        {
            await audioFileStream.CopyToAsync(opusStream);
            await opusStream.FlushAsync();
        }
        catch(Exception e) 
        {
            Log.Out($"Failed to flush opus stream ({filePath}; {e.Message}).");
        }
    }

    public static async Task<bool> Disconnect(u64 guildId)
    {
        if(!voiceClients.TryGetValue(guildId, out VoiceClient voiceClient))
            return false;

        voiceClients.Remove(guildId);

        try
        {
            await voiceClient.CloseAsync();
        }
        finally
        {
            await App.gatewayClient.UpdateVoiceStateAsync(new(guildId, null));
            voiceClient.Dispose();
        }

        return true;
    }
}
