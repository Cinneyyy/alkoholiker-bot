using NetCord.Gateway;
using NetCord.Hosting.Gateway;

namespace src.Rules;

public sealed class MessageHandler : IMessageCreateGatewayHandler
{
    public ValueTask HandleAsync(Message message)
    {
        if(message.Author.IsBot)
            return default;

        u32 numRulesApplied = RuleMgr.ApplyRules(message).GetAwaiter().GetResult();
        Console.WriteLine($"[{DateTime.Now:yyyy'-'MM'-'dd' 'HH':'mm':'ss}] <{message.Author.Username}> \"{message.Content}\" ({message.Attachments.Count} attachments) [applied {numRulesApplied} rules].");

        return ValueTask.CompletedTask;
    }
}
