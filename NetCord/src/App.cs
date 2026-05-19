namespace src;

public static class App
{
    public static string dataPath { get; private set; }


    public static void Load()
        => dataPath = Config.dataPath.Replace("%", Path.TrimEndingDirectorySeparator(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)));

    public static string GetPath(string relPath)
        => Path.Combine(dataPath, relPath);
}