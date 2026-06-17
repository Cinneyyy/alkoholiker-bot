using NetCord;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;

namespace src.Interactions;

public sealed partial class DebugCommands
{
    [SubSlashCommand("vc-state", "vc-state")]
    public sealed class VcState : ApplicationCommandModule<ApplicationCommandContext>
    {
        [SubSlashCommand("print", "Print the contents of the vc_state directory.")]
        public async Task Print(bool ephemeral = true)
        {
            if(!await App.CheckForOwner(Context))
                return;

            string[] voiceChannels = Directory.GetDirectories(App.GetPath("vc_state/channels/"));

            if(voiceChannels is [])
            {
                await RespondAsync(InteractionCallback.Message(new()
                {
                    Content = "No active voice calls.",
                    Flags = MessageFlags.Get(ephemeral: ephemeral)
                }));

                return;
            }

            await RespondAsync(InteractionCallback.Message(new()
            {
                Embeds =
                [
                    new EmbedProperties()
                    {
                        Title = "Voice Channel State",
                        Fields = voiceChannels.Select(vc => new EmbedFieldProperties()
                        {
                            Name = $"<#{Path.GetFileName(vc)}>",
                            Value = string.Join("\n", Directory.GetFiles($"{vc}/history")
                                .Select(Path.GetFileName)
                                .Where(f => f != "session_start")
                                .Select(user => $"<@{user}> ({(File.Exists(App.GetPath($"{vc}/active/{user}")) ? "active" : "left")})")
                            ),
                            Inline = true
                        }),
                        Color = new((i32)Random.Shared.NextRgb())

                    }
                ],
                Flags = MessageFlags.Get(ephemeral: ephemeral)
            }));
        }

        [SubSlashCommand("clear", "Clear the vc_state directory.")]
        public async Task Clear(bool ephemeral = true)
        {
            if(!await App.CheckForOwner(Context))
                return;

            Directory.Delete(App.GetPath("vc_state/channels/"), true);
            Directory.Delete(App.GetPath("vc_state/users/"), true);

            await RespondAsync(InteractionCallback.Message(new()
            {
                Content = $"Removed vc_state/channels/ and vc_state/users/",
                Flags = MessageFlags.Get(ephemeral: ephemeral)
            }));
        }
    }
}
