using System.Diagnostics;
using NetCord;
using NetCord.Gateway;
using NetCord.Gateway.Voice;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;

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
    public async Task Add(string name, Attachment file)
    {
        await RespondAsync(InteractionCallback.Message(new()
        {
            Content = $"Downloading {file.FileName}.",
            Flags = MessageFlags.Get(ephemeral: false)
        }));

        string tempFile = Path.GetTempFileName();
        
        using HttpClient http = new();
        u8[] bytes = await http.GetByteArrayAsync(file.Url);
        await File.WriteAllBytesAsync(tempFile, bytes);

        await FollowupAsync(new()
        {
            Content = "Finished download; checking for file integrity.",
            Flags = MessageFlags.Get(ephemeral: false)
        });

        using Process ffprobe = Process.Start(new ProcessStartInfo()
        {
            FileName = "ffprobe",
            Arguments = $"""
                -v error
                -show_format
                -show_streams
                {tempFile}
            """,
            RedirectStandardError = true,
            RedirectStandardOutput = true,
            UseShellExecute = false
        });

        string ffprobeOut = await ffprobe.StandardError.ReadToEndAsync();
        await ffprobe.WaitForExitAsync();

        bool isValidFile = ffprobe.ExitCode == 0 && !ffprobeOut.Contains("Invalid", StringComparison.OrdinalIgnoreCase);

        if(!isValidFile)
        {
            await FollowupAsync(new()
            {
                Content = "Failed to verify the integrity of your file.",
                Flags = MessageFlags.Get(ephemeral: false)
            });

            return;
        }

        SoundboardDb.AddSound(tempFile, name, null);

        await FollowupAsync(new()
        {
            Content = $"File sucessfully validated, and uploaded to soundboard as \"{name}\". To access it, run /soundboard open.",
            Flags = MessageFlags.Get(ephemeral: false)
        });
    }
}