namespace src.Soundboard;

public static class SoundboardDb
{
    private static readonly List<Sound> sounds = [];


    public static IReadOnlyList<Sound> GetSounds()
        => sounds.AsReadOnly();

    public static void Load()
    {
        sounds.Clear();

        Directory.CreateDirectory(App.GetPath("soundboard"));

        foreach(string file in Directory.GetFiles(App.GetPath("soundboard"), "*.json", SearchOption.AllDirectories))
            sounds.Add(Json.DeserializeFile<Sound>(file));
    }

    /// <summary>Add a file to the DB and save it to disk.</summary>
    public static void AddSound(string tempAudioPath, string displayName, f32 volume = 0.5f)
    {
        Sound sound = new(Guid.NewGuid().ToString(), displayName, volume);

        if(!File.Exists(tempAudioPath))
        {
            Console.WriteLine("Failed to load sound, as specified path does not exist.");
            return;
        }

        File.Move(tempAudioPath, sound.filePath);
        Json.SerializeFile(sound, sound.filePath + ".json");

        sounds.Add(sound);
        Console.WriteLine($"Added new sound to soundboard: \"{displayName}\"");
    }

    public static void RemoveSound(string guid)
    {
        if(!TryGetSound(guid, out Sound sound))
            return;

        File.Delete(sound.filePath);
        File.Delete(sound.filePath + ".json");
        sounds.Remove(sound);

        Console.WriteLine($"Removed sound from soundboard: \"{sound.displayName}\".");
    }

    public static void EditSound(string guid, Func<Sound, Sound> edit)
    {
        if(!TryGetSound(guid, out Sound sound))
            return;

        string originalName = sound.displayName;

        sound = edit(sound);
        sounds[sounds.FindIndex(s => s.guid == guid)] = sound;
        Json.SerializeFile(sound, sound.filePath + ".json");

        Console.WriteLine($"Edited sound \"{originalName}\" ({guid}).");
    }

    public static bool TryGetSound(string guid, out Sound sound)
    {
        sound = sounds.Find(s => s.guid == guid);
        return sound != default;
    }
}