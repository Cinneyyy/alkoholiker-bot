using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Audio;
using Discord.WebSocket;

namespace src.Soundboard;

public static class SoundboardMgr
{
    public static readonly List<SbEntry> sounds = [];

    private static Dictionary<u64, IAudioClient> audioClients = [];


    public static string dirPath => $"{Program.dataPath}/sounds";


    public static void LoadSounds()
    {
        if(!Directory.Exists(dirPath))
            return;

        sounds.Clear();

        foreach(string file in Directory.GetFiles(dirPath, "*.sb", SearchOption.AllDirectories))
        {
            string[] data = File.ReadAllLines(file);
            SbEntry entry = new(Path.GetFileNameWithoutExtension(file), data[0], data[1]);
            sounds.Add(entry);
        }
    }

    public static void SaveSounds()
    {
        foreach(SbEntry entry in sounds)
            File.WriteAllLines($"{dirPath}/{entry.name}.sb", [entry.displayName, entry.displayEmoji]);
    }

    public static async Task PlaySound(SbEntry entry, u64 channel)
    {
        if(!audioClients.TryGetValue(channel, out IAudioClient client))
            return;

        if(client.ConnectionState != Discord.ConnectionState.Connected)
            audioClients.Remove(channel);
Console.WriteLine("asdf");
        Process ffmpeg = CreateFfmpegStream($"{Program.dataPath}/{entry.name}");
        Stream ffmpegOut = ffmpeg.StandardOutput.BaseStream;
        AudioOutStream discordStream = client.CreatePCMStream(AudioApplication.Music);

        try
        {
            await ffmpegOut.CopyToAsync(discordStream);
        }
        finally
        {
            await discordStream.FlushAsync();
        }
    }

    public static async Task JoinVoiceChannelAsync(IVoiceChannel channel)
    {
        u64 botId = u64.Parse(Environment.GetEnvironmentVariable("BOT_USER_ID"));
        audioClients[channel.Id] = await channel.ConnectAsync();
    }

    
    private static Process CreateFfmpegStream(string path)
        => Process.Start(new ProcessStartInfo
        {
            FileName = "ffmpeg",
            Arguments = $"-hide_banner -loglevel panic -i \"{path}\" -ac 2 -f s16le -ar 48000 -vn -acodec pcm_s16le pipe:1",
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        });
}