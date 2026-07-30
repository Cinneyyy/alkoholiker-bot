using NetCord;
using NetCord.Rest;
using src.CallHistory;
using src.Casino;

namespace src.LiveStats;

public static class LiveStatsMgr
{
    /// <summary>Returns false if the file was deleted, else it returns true</summary>
    public static async Task<bool> UpdateMessage(u64 guildId, u64 channelId, u64 messageId, LiveStatsType type)
    {
        MessageProperties messageProperties = type switch
        {
            LiveStatsType.CallStats => CallStatistics.CreateStatMessage(guildId),
            LiveStatsType.CasinoCurrency => CasinoMgr.CreateCurrencyStatMessage(guildId),
            LiveStatsType.CasinoLevels => CasinoMgr.CreateLevelStatMessage(guildId),
            _ => throw new($"Invalid LiveStatsCommands.Type: {type}.")
        };

        RestMessage message;
        try
        {
            message = await App.restClient.GetMessageAsync(channelId, messageId);
        }
        catch
        {
            return false;
        }

        if(message.Author.Id != Secrets.botUserId)
            return true;

        await message.ModifyAsync(message =>
        {
            message.Content = messageProperties.Content ?? string.Empty;
            message.Embeds = messageProperties.Embeds;

            if(type != LiveStatsType.CallStats)
            {
                message.Components =
                [
                    new ActionRowProperties()
                    {
                        new ButtonProperties($"button_live_stats:{guildId}:{channelId}:{messageId}:{(u8)type}", "Refresh", ButtonStyle.Primary)
                    }
                ];
            }
        });

        return true;
    }

    public static async Task CreateMessage(u64 guildId, TextChannel channel, LiveStatsType type)
    {
        RestMessage message = await channel.SendMessageAsync(new()
        {
            Content = $"[LiveStats::{type}]",
            Flags = MessageFlags.Get(ephemeral: false)
        });

        await UpdateMessage(guildId, channel.Id, message.Id, type);

        string path = App.GetPath($"live_stats/");
        Directory.CreateDirectory(path);
        File.WriteAllText($"{path}/{guildId}.{channel.Id}.{message.Id}.{type}", null);
    }

    public static async Task UpdateStatMessages(LiveStatsType type)
    {
        string path = App.GetPath("live_stats");

        foreach(string file in Directory.GetFiles(path, $"*.{type}", SearchOption.AllDirectories))
        {
            string f = Path.GetFileNameWithoutExtension(file);

            u64 messageId = Path.GetExtension(f)[1..].ParseU64();
            f = Path.GetFileNameWithoutExtension(f);

            u64 channelId = Path.GetExtension(f)[1..].ParseU64();
            f = Path.GetFileNameWithoutExtension(f);

            u64 guildId = f.ParseU64();

            bool messageDeleted = !await UpdateMessage(guildId, channelId, messageId, type);
            if(messageDeleted)
                File.Delete(file);
        }
    }
}
