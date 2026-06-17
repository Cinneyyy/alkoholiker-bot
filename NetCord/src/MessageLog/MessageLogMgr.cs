using NetCord.Gateway;

namespace src.MessageLog;

public static class MessageLogMgr
{
    public static void MessageCreated(Message message)
    {
        string path = App.GetPath($"msg_log/{message.GuildId}/{message.ChannelId}/");
        Directory.CreateDirectory(path);
        File.WriteAllText($"{path}/{message}", message.toLoggableStr);
    }

    public static void MessageDeleted(u64 guild, u64 channel, u64 message)
    {
        string path = App.GetPath($"msg_log/{guild}/{channel}/{message}");
        Log.Out(File.Exists(path)
            ? $"Cached message deleted in {guild}:{channel}: {File.ReadAllText(path)}."
            : $"Uncached message deleted in {guild}:{channel} ({message}).");
    }
}
