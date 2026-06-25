using NetCord;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;

namespace src.Interactions;

public sealed partial class MiscCommands : ApplicationCommandModule<ApplicationCommandContext>
{
    [SlashCommand("say", "Say something as the bot.")]
    public async Task Say(string message)
    {
        if(string.IsNullOrWhiteSpace(message))
        {
            await RespondAsync(InteractionCallback.Message(new()
            {
                Content = "Message cannot be empty!",
                Flags = MessageFlags.Get(ephemeral: true)
            }));

            return;
        }

        await RespondAsync(InteractionCallback.Message(new()
        {
            Content = $"Sending message.",
            Flags = MessageFlags.Get(ephemeral: true)
        }));

        await Context.Channel.SendMessageAsync(new()
        {
            Content = message,
            Flags = MessageFlags.Get(ephemeral: false, silent: false)
        });
    }
}
