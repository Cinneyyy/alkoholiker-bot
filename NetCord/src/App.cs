using NetCord.Rest;

namespace src;

public static class App
{
    public static string dataPath { get; private set; }
    public static RestClient client { get; private set; }
    public static DateTime startTime { get; } = DateTime.UtcNow;


    public static void Load()
        => dataPath = Secrets.dataPath.Replace("%", Path.TrimEndingDirectorySeparator(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)));

    public static string GetPath(string relPath)
        => Path.Combine(dataPath, relPath);

    public static void SetClient(RestClient client)
        => App.client = client;
}
