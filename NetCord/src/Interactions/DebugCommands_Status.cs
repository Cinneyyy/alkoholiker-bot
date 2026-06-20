using NetCord;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;
using src.ActivityStatus;

namespace src.Interactions;

public sealed partial class DebugCommands
{
    [SubSlashCommand("status", "status")]
    public sealed class Status : ApplicationCommandModule<ApplicationCommandContext>
    {
        [SubSlashCommand("reload", "[!] Reload statuses.")]
        public async Task Reload(bool ephemeral = true)
        {
            if(!await App.CheckForOwner(Context))
                return;

            ActivityStatusMgr.Load();

            await RespondAsync(InteractionCallback.Message(new()
            {
                Content = $"Reloaded statuses from `statuses.json`",
                Flags = MessageFlags.Get(ephemeral: ephemeral)
            }));
        }

        [SubSlashCommand("print", "Print the possible statuses.")]
        public async Task Print(bool ephemeral = true)
            => await RespondAsync(InteractionCallback.Message(new()
            {
                Attachments =
                [
                    new("statuses.json", File.OpenRead(App.GetPath("statuses.json")))
                ],
                Flags = MessageFlags.Get(ephemeral: ephemeral)
            }));
    }
}
