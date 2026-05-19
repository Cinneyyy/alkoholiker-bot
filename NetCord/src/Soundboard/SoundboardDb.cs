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
    public static void AddSound(string tempAudioPath, string displayName, string displayEmoji)
    {
        Sound sound = new(Guid.NewGuid().ToString(), displayName, displayEmoji);

        if(!File.Exists(tempAudioPath))
        {
            Console.WriteLine("Failed to load sound, as specified path does not exist");
            return;
        }

        File.Move(tempAudioPath, App.GetPath(sound.guid));
        Json.SerializeFile(sound, App.GetPath($"{sound.guid}.json"));
    }

    public static Sound GetSound(string guid)
        => sounds.Find(s => s.guid == guid);
}