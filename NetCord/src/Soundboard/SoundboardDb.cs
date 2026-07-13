using System.Diagnostics;

namespace src.Soundboard;

public static class SoundboardDb
{
    private static readonly List<Sound> sounds = [];


    public static string path { get; private set; }


    public static IReadOnlyList<Sound> GetSounds()
        => sounds.AsReadOnly();

    public static void Load()
    {
        sounds.Clear();

        path = path;
        Directory.CreateDirectory(path);

        foreach(string file in Directory.GetFiles(path, "*.json", SearchOption.AllDirectories))
            sounds.Add(Json.DeserializeFile<Sound>(file));
    }

    public static void SetPath(string path)
        => SoundboardDb.path = path;

    /// <summary>Add a file to the DB and save it to disk.</summary>
    public static void AddSound(string tempAudioPath, string displayName, f32 volume = 0.5f, f32? start = null, f32? end = null)
    {
        Sound sound = new(Guid.NewGuid().ToString(), displayName, volume);

        if(!File.Exists(tempAudioPath))
        {
            Log.Out($"Failed to load sound, as specified path does not exist ({tempAudioPath}).");
            return;
        }


        string args = $"""
            -hide_banner
            -loglevel error
            -i {tempAudioPath}
            -vn
            -map 0:a:0
            -af "volume={volume}"
            -ac 2
            -ar 48000
            -acodec pcm_s16le
            -f s16le
        """;

        if(start is f32 _start)
        {
            args += $"\n-ss {_start}";
            Log.Out(_start.ToString());
        }

        if(end is f32 _end)
        {
            args += $"\n-to {_end}";
            Log.Out(_end.ToString());
        }

        args += "\npipe:1";

        using Process ffmpeg = Process.Start(new ProcessStartInfo()
        {
            FileName = "ffmpeg",
            Arguments = args.Replace(Environment.NewLine, " "),
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        });

        try
        {
            using FileStream fileStream = File.Create(sound.filePath);
            ffmpeg.StandardOutput.BaseStream.CopyTo(fileStream);
        }
        catch(Exception e)
        {
            Log.Out($"FFMPEG error: {e.Message}; std error: \"{ffmpeg.StandardError.ReadToEnd()}\".");
        }

        File.Delete(tempAudioPath);
        Json.SerializeFile(sound, sound.filePath + ".json");

        sounds.Add(sound);
        Log.Out($"Added new sound to soundboard: `{displayName}`");
    }

    public static void RemoveSound(string guid)
    {
        if(!TryGetSound(guid, out Sound sound))
            return;

        File.Delete(sound.filePath);
        File.Delete(sound.filePath + ".json");
        sounds.Remove(sound);

        Log.Out($"Removed sound from soundboard: `{sound.displayName}`.");
    }

    public static void EditSound(string guid, Func<Sound, Sound> edit)
    {
        if(!TryGetSound(guid, out Sound sound))
            return;

        string originalName = sound.displayName;

        sound = edit(sound);
        sounds[sounds.FindIndex(s => s.guid == guid)] = sound;
        Json.SerializeFile(sound, sound.filePath + ".json");

        Log.Out($"Edited sound `{originalName}` ({guid}).");
    }

    public static bool TryGetSound(string guid, out Sound sound)
    {
        sound = sounds.Find(s => s.guid == guid);
        return sound != default;
    }
}
