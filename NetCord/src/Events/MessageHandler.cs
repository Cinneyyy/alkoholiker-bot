using NetCord.Gateway;
using NetCord.Hosting.Gateway;

namespace src.Events;

public class MessageHandler : IMessageCreateGatewayHandler
{
    public ValueTask HandleAsync(Message message)
    {
        if(message.Author.IsBot)
            return default;
        
        return ValueTask.CompletedTask;
    }
}