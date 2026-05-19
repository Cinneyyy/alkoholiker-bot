using NetCord;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;

namespace src.Debug;

[SlashCommand("debug", "Debug commands.")]
public class DebugCommands : ApplicationCommandModule<ApplicationCommandContext>
{
    [SubSlashCommand("ping", "Ping the bot and display the time it took for the bot to send a message and receive the ACK.")]
    public async Task Ping(bool ephemeral = true)
    {
        DateTime start = DateTime.UtcNow;
        await RespondAsync(InteractionCallback.Message(new()
        {
            Content = "Pong!",
            Flags = MessageFlags.Get(ephemeral)
        }));

        string newMessage = $"Pong! ({(DateTime.UtcNow - start).TotalMilliseconds:0}ms)";
        await Context.Interaction.ModifyResponseAsync(m => m.Content = newMessage);
    }
}