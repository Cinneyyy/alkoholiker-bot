using NetCord;
using NetCord.Rest;
using NetCord.Services.ComponentInteractions;

namespace src.Soundboard;

public class SoundboardComponentHandler : ComponentInteractionModule<ButtonInteractionContext>
{
    [ComponentInteraction("play_sound")]
    public async Task PlaySound(string guid)
    {
        await Context.Interaction.SendResponseAsync(InteractionCallback.Message(new()
        {
            Content = SoundboardDb.GetSound(guid).displayName,
            Flags = MessageFlags.Get()
        }));
    }
}