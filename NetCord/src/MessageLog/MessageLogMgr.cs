namespace src.MessageLog;

public static class MessageLogMgr
{
    public static void MessageCreated(u64 guild, u64 channel, u64 message, string loggableStr)
    {
        string path = App.GetPath($"cache/messages/{guild}/{channel}/");
        Directory.CreateDirectory(path);
        File.WriteAllText($"{path}/{message}", loggableStr);
    }

    public static void MessageDeleted(u64 guild, u64 channel, u64 message)
    {
        string path = App.GetPath($"cache/messages/{guild}/{channel}/{message}");
        Log.Out(File.Exists(path)
            ? $"Cached message deleted in {guild}:{channel} ({message}): {File.ReadAllText(path)}."
            : $"Uncached message deleted in {guild}:{channel} ({message}).");
    }
}
