using NetCord.Gateway;
using NetCord.Hosting.Gateway;
using src.Casino;
using src.Rules;

namespace src;

public sealed class MessageCreatedHandler : IMessageCreateGatewayHandler
{
    public ValueTask HandleAsync(Message message)
    {
        string loggableStr = message.toLoggableStr;
        MessageLogMgr.MessageCreated(message.GuildId ?? 0ul, message.ChannelId, message.Id, loggableStr);

        if(Config.autoDeleteChannels.Contains(message.ChannelId) && message.Author.Id != Secrets.botUserId)
        {
            _ = message.DeleteAsync();
            return ValueTask.CompletedTask;
        }

        if(!message.Author.IsBot)
        {
            u32 numRulesApplied = RuleMgr.ApplyRules(message).GetAwaiter().GetResult();
            Log.Out($"[In {message.GuildId ?? 0}:{message.ChannelId}; ID: {message.Id}]: {loggableStr} [applied {numRulesApplied} rule{(numRulesApplied == 1 ? "" : "s")}].");

            LevelUpMgr.HandleUserMsg(message);
        }

        return ValueTask.CompletedTask;
    }
}
