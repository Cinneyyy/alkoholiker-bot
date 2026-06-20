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
            string[] voiceChannels = Directory.GetDirectories(App.GetPath("vc_state/channels/"));

            if(voiceChannels is [])
            {
                await RespondAsync(InteractionCallback.Message(new()
                {
                    Content = "No ongoing voice calls.",
                    Flags = MessageFlags.Get(ephemeral: ephemeral)
                }));

                return;
            }

            // In case uncached names need to be fetched from the Discord API
            await RespondAsync(InteractionCallback.DeferredMessage(MessageFlags.Get(ephemeral: ephemeral)));

            await FollowupAsync(new()
            {
                Embeds =
                [
                    new EmbedProperties()
                    {
                        Title = "Voice Channel State",
                        Fields = voiceChannels.Select(vcPath => new EmbedFieldProperties()
                        {
                            Name = $"<#{Path.GetFileName(vcPath)}>",
                            Value = 
                                "```\n" +
                                string.Join("\n", Directory.GetFiles($"{vcPath}/history")
                                    .Select(Path.GetFileName)
                                    .Where(f => f != "session_start")
                                    .Select(u => (
                                        left: !File.Exists(App.GetPath($"{vcPath}/active/{u}")),
                                        name: src.UserCache.GetName(u64.Parse(u))
                                    ))
                                    .OrderBy(u => u.left ? 1 : 0)
                                    .Select(u => $"[ {(u.left ? 'X' : ' ')} ]  {u.name}")
                                ) +
                                "```",
                            Inline = true
                        }),
                        Color = new((i32)Random.Shared.NextRgb())

                    }
                ],
                Flags = MessageFlags.Get(ephemeral: ephemeral)
            });
        }

        [SubSlashCommand("clear", "[!] Clear the vc_state directory.")]
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
