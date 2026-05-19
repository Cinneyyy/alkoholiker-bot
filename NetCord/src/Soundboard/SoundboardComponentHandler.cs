using NetCord;
using NetCord.Gateway;
using NetCord.Rest;
using NetCord.Services.ComponentInteractions;

namespace src.Soundboard;

public class SoundboardComponentHandler : ComponentInteractionModule<ButtonInteractionContext>
{
    [ComponentInteraction("play_sound")]
    public async Task PlaySoundButtonHandler(string guid)
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

        Sound sound = SoundboardDb.GetSound(guid);
        await SoundboardPlayer.PlaySound(Context.Client, Context.Guild.Id, userVoiceState.ChannelId.Value, sound.filePath);
    }
}