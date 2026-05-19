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
    public static void AddSound(string tempAudioPath, string displayName)
    {
        Sound sound = new(Guid.NewGuid().ToString(), displayName);

        if(!File.Exists(tempAudioPath))
        {
            Console.WriteLine("Failed to load sound, as specified path does not exist");
            return;
        }

        File.Move(tempAudioPath, sound.filePath);
        Json.SerializeFile(sound, $"{sound.filePath}.json");

        sounds.Add(sound);
        Console.WriteLine($"Added new sound to soundboard: {displayName}");
    }

    public static Sound GetSound(string guid)
        => sounds.Find(s => s.guid == guid);
}