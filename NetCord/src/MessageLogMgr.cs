using System.Net;
using NetCord.Gateway;

namespace src;

public static class MessageLogMgr
{
    public static void MessageCreated(Message message, string loggableStr)
    {
        using HttpClient httpClient = new();

        RawMessageData msgData = new()
        {
            guildId = message.GuildId ?? 0ul,
            channelId = message.ChannelId,
            messageId = message.Id,
            timestamp = DateTime.UtcNow.Ticks,
            author = message.Author.Id,
            content = message.Content,
            attachments = message.Attachments
                .Select(att => new RawMessageData.Attachment(att.FileName, httpClient.GetByteArrayAsync(att.Url).GetAwaiter().GetResult()))
                .ToArray()
        };

        u64 guild = msgData.guildId;
        u64 channel = msgData.channelId;

        string path = App.GetPath($"cache/messages/{guild}/{channel}/");
        Directory.CreateDirectory(path);

        path += message.Id.ToString();
        File.WriteAllText(path, loggableStr);
        File.WriteAllBytes($"{path}.full", msgData.GetBytes());
    }

    public static void MessageDeleted(u64 guild, u64 channel, u64 message)
    {
        string path = App.GetPath($"cache/messages/{guild}/{channel}/{message}");

        if(!File.Exists(path))
        {
            Log.Out($"Uncached message deleted in {guild}:{channel} ({message})");
            return;
        }

        string loggableContent = File.ReadAllText(path);

        path += ".full";
        if(!File.Exists(path))
            Log.Out($"Cached message deleted in {guild}:{channel} ({message}): {loggableContent}.");
        else
        {
            RawMessageData.FromBytes(File.ReadAllBytes(path)).WriteToReadable();
            Log.Out($"Fully cached message deleted in {guild}:{channel} ({message}): {loggableContent}.");
        }
    }
}
