using NetCord;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;

namespace src.Interactions;

public sealed class VoteMessageCommands : ApplicationCommandModule<ApplicationCommandContext>
{
    [MessageCommand("Vote (⬆⬇)")]
    public async Task VoteUpDown(RestMessage message)
    {
        await RespondAsync(InteractionCallback.Message(new()
        {
            Content = "Adding vote reactions.",
            Flags = MessageFlags.Get()
        }));

        await message.AddReactionsAsync(["⬆️", "⬇️"]);
    }

    [MessageCommand("Vote (⬅➡)")]
    public async Task VoteLeftRight(RestMessage message)
    {
        await RespondAsync(InteractionCallback.Message(new()
        {
            Content = "Adding vote reactions.",
            Flags = MessageFlags.Get()
        }));

        await message.AddReactionsAsync(["⬅️", "➡️"]);
    }

    [MessageCommand("Vote (⬆⬇⬅➡)")]
    public async Task VoteUpDownLeftRight(RestMessage message)
    {
        await RespondAsync(InteractionCallback.Message(new()
        {
            Content = "Adding vote reactions.",
            Flags = MessageFlags.Get()
        }));

        await message.AddReactionsAsync(["⬆️", "⬇️", "⬅️", "➡️"]);
    }

    [MessageCommand("Vote (👍👎)")]
    public async Task VoteThumbsUpDown(RestMessage message)
    {
        await RespondAsync(InteractionCallback.Message(new()
        {
            Content = "Adding vote reactions.",
            Flags = MessageFlags.Get()
        }));

        await message.AddReactionsAsync(["👍", "👎"]);
    }
}
