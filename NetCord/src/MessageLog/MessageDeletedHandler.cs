using NetCord.Gateway;
using NetCord.Hosting.Gateway;

namespace src.MessageLog;

public sealed class MessageDeletedHandler : IMessageDeleteGatewayHandler
{
    public async ValueTask HandleAsync(MessageDeleteEventArgs args)
        => MessageLogMgr.MessageDeleted(args.GuildId ?? 0, args.ChannelId, args.MessageId);
}
