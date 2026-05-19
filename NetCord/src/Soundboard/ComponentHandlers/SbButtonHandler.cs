using NetCord;
using NetCord.Gateway;
using NetCord.Rest;
using NetCord.Services.ComponentInteractions;

namespace src.Soundboard.ComponentHandlers;

public sealed class SbButtonHandler : ComponentInteractionModule<ButtonInteractionContext>
{
    [ComponentInteraction("button_sound_play")]
    public async Task SoundPlay(string guid)
    {
        if(!Context.Guild.VoiceStates.TryGetValue(Context.User.Id, out VoiceState userVoiceState))
        {
            await RespondAsync(InteractionCallback.Message(new()
            {
                Content = "You cannot use the soundboard while not in a voice channel.",
                Flags = MessageFlags.Get()    
            }));

            return;
        }

        if(!SoundboardDb.TryGetSound(guid, out Sound sound))
        {
            await RespondAsync(InteractionCallback.Message(new()
            {
                Content = $"Failed to fetch sound ({guid}).",
                Flags = MessageFlags.Get()
            }));

            return;
        }

        await Context.Interaction.SendResponseAsync(InteractionCallback.DeferredModifyMessage);
        await SoundboardPlayer.PlaySound(Context.Client, Context.Guild.Id, userVoiceState.ChannelId.Value, sound.filePath, sound.volume);
    }

    [ComponentInteraction("button_sound_edit")]
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
    
        await RespondAsync(InteractionCallback.Modal(new($"modal_sound_edit:{guid}", $"Edit sound {sound.displayName}")
        {
            new LabelProperties("Name", new TextInputProperties("text_input_sound_name", TextInputStyle.Short)
            {
                Value = sound.displayName,
                MinLength = 3,
                MaxLength = 32
            }),
            new LabelProperties($"Volume (current: {sound.volume:0%})", new StringMenuProperties("string_menu_sound_volume")
            {
                new("0%", "0.0"),
                new("10%", "0.1"),
                new("20%", "0.2"),
                new("30%", "0.3"),
                new("40%", "0.4"),
                new("50%", "0.5"),
                new("60%", "0.6"),
                new("70%", "0.7"),
                new("80%", "0.8"),
                new("90%", "0.9"),
                new("100%", "1.0"),
            })
        }));
    }

    [ComponentInteraction("button_sound_remove")]
    public async Task SoundRemove(string guid)
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

        await RespondAsync(InteractionCallback.Message(new()
        {
            Content = $"Are you sure you want to remove the sound \"{sound.displayName}\"?",
            Components = 
            [
                new ActionRowProperties() 
                {
                    new ButtonProperties($"button_sound_remove_confirm:{guid}", "Yes, I'm sure", ButtonStyle.Danger)
                }
            ],
            Flags = MessageFlags.Get()
        }));
    }

    [ComponentInteraction("button_sound_remove_confirm")]
    public async Task SoundRemoveConfirm(string guid)
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

        SoundboardDb.RemoveSound(guid);

        await RespondAsync(InteractionCallback.Message(new()
        {
            Content = $"<@{Context.User.Id}> removed the sound \"{sound.displayName}\".",
            Flags = MessageFlags.Get(ephemeral: false)
        }));
    }
}