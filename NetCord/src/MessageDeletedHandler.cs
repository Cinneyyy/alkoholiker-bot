using NetCord.Gateway;
using NetCord.Hosting.Gateway;

namespace src;

public sealed class MessageDeletedHandler : IMessageDeleteGatewayHandler
{
    public async ValueTask HandleAsync(MessageDeleteEventArgs args)
        => MessageLogMgr.MessageDeleted(args.GuildId ?? 0ul, args.ChannelId, args.MessageId);
}
