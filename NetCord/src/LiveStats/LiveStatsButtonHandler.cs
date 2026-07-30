using NetCord.Rest;
using NetCord.Services.ComponentInteractions;

namespace src.LiveStats;

public sealed class LiveStatsButtonHandler : ComponentInteractionModule<ButtonInteractionContext>
{
    [ComponentInteraction("button_live_stats")]
    public async Task LiveStats(u64 guildId, u64 channelId, u64 messageId, u8 type)
    {
        await LiveStatsMgr.UpdateMessage(guildId, channelId, messageId, (LiveStatsType)type);

        await RespondAsync(InteractionCallback.ModifyMessage(msg => 
        {
        }));
    }
}
