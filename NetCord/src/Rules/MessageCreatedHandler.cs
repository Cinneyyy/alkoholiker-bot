using NetCord.Gateway;
using NetCord.Hosting.Gateway;

namespace src.Rules;

public sealed class MessageCreatedHandler : IMessageCreateGatewayHandler
{
    public ValueTask HandleAsync(Message message)
    {
        if(message.Author.IsBot)
            return default;

        u32 numRulesApplied = RuleMgr.ApplyRules(message).GetAwaiter().GetResult();
        Log.Out($"[In {message.GuildId ?? 0}:{message.ChannelId}; ID: {message.Id}]: {message.toLoggableStr} [applied {numRulesApplied} rule{(numRulesApplied == 1 ? "" : "s")}].");

        return ValueTask.CompletedTask;
    }
}
