using System.Text;
using NetCord;
using NetCord.Rest;
using NetCord.Services.ComponentInteractions;

namespace src.Soundboard.ComponentHandlers;

public sealed class SbModalHandler : ComponentInteractionModule<ModalInteractionContext>
{
    [ComponentInteraction("modal_sound_edit")]
    public async Task SoundEdit(string guid)
    {
        if(!SoundboardDb.TryGetSound(guid, out Sound sound))
        {
            await RespondAsync(InteractionCallback.Message(new()
            {
                Content = $"Failed to fetch sound ({guid}).",
                Flags = MessageFlags.Get()
            }));

            return;
        }

        IEnumerable<ILabelComponent> components = Context.Components
            .OfType<Label>()
            .Select(l => l.Component);

        string newName = components.OfType<TextInput>().First(f => f.CustomId == "text_input_sound_name").Value;
        f32 newVolume = f32.Parse(components.OfType<StringMenu>().First(f => f.CustomId == "string_menu_sound_volume").SelectedValues[0]);

        bool nameChanged = newName != sound.displayName;
        bool volumeChanged = newVolume != sound.volume;

        if(!nameChanged && !volumeChanged)
        {
            await RespondAsync(InteractionCallback.Message(new()
            {
                Content = "Nothing has changed.",
                Flags = MessageFlags.Get()
            }));

            return;
        }

        SoundboardDb.EditSound(guid, s => s with
        {
            displayName = newName,
            volume = newVolume
        });

        StringBuilder messageBuilder = new($"<@{Context.User.Id}> made the following changes to the sound `{sound.displayName}`:\n");
        if(nameChanged) messageBuilder.AppendLine($"- name: `{sound.displayName}` -> `{newName}`");
        if(volumeChanged) messageBuilder.AppendLine($"- volume: {sound.volume:0%} -> {newVolume:0%}");

        await RespondAsync(InteractionCallback.Message(new()
        {
            Content = messageBuilder.ToString(),
            Flags = MessageFlags.Get(ephemeral: false)
        }));
    }
}