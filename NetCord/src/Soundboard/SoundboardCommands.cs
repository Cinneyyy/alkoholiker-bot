using System.Diagnostics;
using NetCord;
using NetCord.Gateway;
using NetCord.Gateway.Voice;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;
using src.TypeReaders;

namespace src.Soundboard;

[SlashCommand("soundboard", "All commands relating to the custom soundboard.", Contexts = [InteractionContextType.Guild])]
public class SoundboardCommands : ApplicationCommandModule<ApplicationCommandContext>
{
    [SubSlashCommand("test", "test.")]
    public async Task Test(VoiceGuildChannel channel = null)
    {
        u64 channelId = channel?.Id ?? 0;

        if(channel is null)
        {
            if(!Context.Guild.VoiceStates.TryGetValue(Context.User.Id, out VoiceState voiceState))
            {
                await RespondAsync(InteractionCallback.Message(new()
                {
                   Content = "You are not connected to a channel and did not specify one for the bot to join.",
                   Flags = MessageFlags.Get()
                }));

                return;
            }

            channelId = voiceState.ChannelId.Value;
        }

        VoiceClient voiceClient = await Context.Client.JoinVoiceChannelAsync(Context.Guild.Id, channelId);

        await RespondAsync(InteractionCallback.Message(new()
        {
            Content = "asdf",
            Flags = MessageFlags.Get()    
        }));

        await voiceClient.StartAsync();
        await voiceClient.EnterSpeakingStateAsync(new SpeakingProperties(SpeakingFlags.Microphone));
    
        Stream voiceStream = voiceClient.CreateVoiceStream();
        OpusEncodeStream opusStream = new(voiceStream, PcmFormat.Short, VoiceChannels.Stereo, OpusApplication.Audio);

        Process ffmpeg = Process.Start(new ProcessStartInfo()
        {
            FileName = "ffmpeg",
            Arguments = $"""
                -hide_banner
                -loglevel error
                -i {"/home/colin/Downloads/niekbeats.mp3"}
                -vn
                -map 0:a:0
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
            }
            catch
            {
                Console.WriteLine("Failed to flush stream");
            }

            await voiceClient.CloseAsync();
            Console.WriteLine(ffmpeg.StandardError.ReadToEnd());
        }
    }
    
    [SubSlashCommand("open", "List all the registered sounds, alongside buttons to play them.")]
    public async Task Open(bool ephemeral = true)
    {
        if(SoundboardDb.GetSounds().Count == 0)
        {
            await RespondAsync(InteractionCallback.Message(new()
            {
                Content = "Failed to open soundboard; no sounds in the database."
            }));

            return;
        }

        await RespondAsync(InteractionCallback.Message(new()
        {
            Content = "Soundboard:",
            Components =
            [
                new ActionRowProperties(SoundboardDb.GetSounds().Select(s => new ButtonProperties($"play_sound:{s.guid}", s.displayName, s.displayEmojiProperties, ButtonStyle.Primary)))
            ],
            Flags = MessageFlags.Get(ephemeral)
        }));
    }

    [SubSlashCommand("add", "Add a sound to the soundboard.")]
    public async Task Add(string name, [SlashCommandParameter(TypeReaderType = typeof(EmojiTypeReader<ApplicationCommandContext>))] EmojiProperties emoji)
    {
        await RespondAsync(InteractionCallback.Message(new()
        {
            Content = $"{name} {emoji.GetEmojiString()}",
            Flags = MessageFlags.Get()
        }));
    }
}