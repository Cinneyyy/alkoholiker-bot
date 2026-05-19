using NetCord;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;
using NetCord.Services.Commands;

namespace src.Soundboard;

[SlashCommand("soundboard", "All commands relating to the custom soundboard.", Contexts = [InteractionContextType.Guild])]
public sealed class SoundboardCommands : ApplicationCommandModule<ApplicationCommandContext>
{
    [SubSlashCommand("open", "List all the registered sounds, alongside buttons to play them.")]
    public async Task Open(bool ephemeral = true)
    {
        if(SoundboardDb.GetSounds().Count == 0)
        {
            await RespondAsync(InteractionCallback.Message(new()
            {
                Content = "Failed to open soundboard; no sounds in the database.",
                Flags = MessageFlags.Get()
            }));

            return;
        }

        await RespondAsync(InteractionCallback.Message(new()
        {
            Components = BuildSoundboardGrid("button_sound_play"),
            Flags = MessageFlags.Get(ephemeral: ephemeral)
        }));
    }

    [SubSlashCommand("add", "Add a sound to the soundboard.")]
    public async Task Add(string name, Attachment file, [SlashCommandParameter(MinValue = 0.0, MaxValue = 1.0)] f32 volume = 0.5f)
    {
        await RespondAsync(InteractionCallback.Message(new()
        {
            Content = $"Downloading {file.FileName}.",
            Flags = MessageFlags.Get(ephemeral: false)
        }));

        string tempFile = Path.GetTempFileName();
        
        using HttpClient http = new();
        u8[] bytes = await http.GetByteArrayAsync(file.Url);
        await File.WriteAllBytesAsync(tempFile, bytes);

        SoundboardDb.AddSound(tempFile, name, f32.Round(volume, 1));

        await FollowupAsync(new()
        {
            Content = $"File sucessfully downloaded and added to soundboard as \"{name}\". To access it, run /soundboard open.",
            Flags = MessageFlags.Get(ephemeral: false)
        });
    }

    [SubSlashCommand("remove", "Remove a sound from the soundboard.")]
    public async Task Remove()
    {
        if(SoundboardDb.GetSounds().Count == 0)
        {
            await RespondAsync(InteractionCallback.Message(new()
            {
                Content = "Failed to display soundboard; no sounds in the database.",
                Flags = MessageFlags.Get()
            }));

            return;
        }
        
        await RespondAsync(InteractionCallback.Message(new()
        {
            Content = "Select a sound to remove.",
            Components = BuildSoundboardGrid("button_sound_remove", ButtonStyle.Danger),
            Flags = MessageFlags.Get()
        }));
    }

    [SubSlashCommand("edit", "Edit a sound in the soundboard.")]
    public async Task Edit()
    {
        if(SoundboardDb.GetSounds().Count == 0)
        {
            await RespondAsync(InteractionCallback.Message(new()
            {
                Content = "Failed to display soundboard; no sounds in the database.",
                Flags = MessageFlags.Get()
            }));

            return;
        }

        await RespondAsync(InteractionCallback.Message(new()
        {
            Content = "Select a sound to edit.",
            Components = BuildSoundboardGrid("button_sound_edit"),
            Flags = MessageFlags.Get()
        }));
    }


    private static IEnumerable<IMessageComponentProperties> BuildSoundboardGrid(string buttonId, ButtonStyle buttonStyle = ButtonStyle.Primary)
        => SoundboardDb
            .GetSounds()
            .Chunk(5)
            .Select(sounds => new ActionRowProperties(sounds
                .Select(sound => new ButtonProperties($"{buttonId}:{sound.guid}", sound.displayName, buttonStyle)))
            );
}