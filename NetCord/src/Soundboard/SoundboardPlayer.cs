using System.Diagnostics;
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

            Console.WriteLine($"Voice client opened ({guildId}:{channelId})");
            voiceClients[guildId] = voiceClient;

            voiceClient.Disconnect += args =>
            {
                Console.WriteLine($"Voice client closed ({guildId}:{channelId}).");
                voiceClients.Remove(guildId);
                voiceClient.Dispose();
                return default;
            };
        }

        if(voiceClient.ChannelId != channelId)
        {
            await voiceClient.CloseAsync();
            await PlaySound(gatewayClient, guildId, channelId, filePath, volume);
            return;
        }

        await voiceClient.EnterSpeakingStateAsync(new(SpeakingFlags.Soundshare));

        using Stream voiceStream = voiceClient.CreateVoiceStream();
        using OpusEncodeStream opusStream = new(voiceStream, PcmFormat.Short, VoiceChannels.Stereo, OpusApplication.Audio);

        using Process ffmpeg = Process.Start(new ProcessStartInfo()
        {
            FileName = "ffmpeg",
            Arguments = $"""
                -hide_banner
                -loglevel error
                -i {filePath}
                -vn
                -map 0:a:0
                -af "volume={volume}"
                -ac 2
                -ar 48000
                -acodec pcm_s16le
                -f s16le
                pipe:1
            """.Replace(Environment.NewLine, " "),
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        });

        try
        {
            await ffmpeg.StandardOutput.BaseStream.CopyToAsync(opusStream);
        }
        finally
        {
            try
            {
                await opusStream.FlushAsync();    
                Console.WriteLine("Flushed opusStream.");
            }
            catch
            {
                Console.WriteLine("Failed to flush opusStream.");
            }

            Console.WriteLine(ffmpeg.StandardError.ReadToEnd());
        }
    }

    public static async Task<bool> Disconnect(u64 guildId)
    {
        if(!voiceClients.TryGetValue(guildId, out VoiceClient voiceClient))
            return false;

        await voiceClient.CloseAsync();
        return true;
    }
}