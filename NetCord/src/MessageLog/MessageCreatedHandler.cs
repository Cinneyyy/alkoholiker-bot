using NetCord.Gateway;
using NetCord.Hosting.Gateway;

namespace src.MessageLog;

public sealed class MessageCreatedHandler : IMessageCreateGatewayHandler
{
    public ValueTask HandleAsync(Message message)
    {
        MessageLogMgr.MessageCreated(message.GuildId ?? 0ul, message.ChannelId, message.Id, message.toLoggableStr);
        return ValueTask.CompletedTask;
    }
}
