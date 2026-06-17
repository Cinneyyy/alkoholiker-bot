using NetCord;
using NetCord.Rest;
using NetCord.Services.ComponentInteractions;

namespace src.RockPaperScissors;

public sealed class RpsButtonHandler : ComponentInteractionModule<ButtonInteractionContext>
{
    private static readonly Dictionary<u64, RpsSelection> selections = []; // TODO: guild-specific counter


    [ComponentInteraction("button_rps_select")]
    public async Task RpsSelect(u8 selectionValue)
    {
        RpsSelection selection = (RpsSelection)selectionValue;
        selections[Context.User.Id] = selection;

        await RespondAsync(InteractionCallback.Message(new()
        {
            Content = $"Locked in {FormatSelection(selection)}.",
            Flags = MessageFlags.Get()
        }));

        await Context.Channel.SendMessageAsync(new()
        {
            Content = $"{Context.User.GlobalName} locked something in.",
            Flags = MessageFlags.Get(ephemeral: false)
        });
    }

    [ComponentInteraction("button_rps_finish")]
    public async Task RpsFinish()
    {
        if(selections.Count == 0)
        {
            await RespondAsync(InteractionCallback.Message(new()
            {
                Content = "No selections have been made.",
                Flags = MessageFlags.Get()
            }));

            return;
        }

        // TODO: anounce winners

        InteractionMessageProperties response = new()
        {
            Embeds =
            [
                new EmbedProperties()
                {
                    Title = "Results",
                    Description = string.Join("\n", selections.OrderBy(kvp => kvp.Value).Select(kvp => $"<@{kvp.Key}> chose **{FormatSelection(kvp.Value)}**")),
                    Color = new((i32)Random.Shared.NextRgb())
                }
            ]
        };

        selections.Clear();
        await RespondAsync(InteractionCallback.Message(response));
    }


    private static string FormatSelection(RpsSelection sel)
        => sel switch
        {
            RpsSelection.Rock => "rock 🪨",
            RpsSelection.Paper => "paper 🧻",
            RpsSelection.Scissors => "scissors ✂️",
            _ => throw new($"Invalid RpsSelection ({sel}).")
        };
}
