using NetCord;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;
using src.ActivityStatus;

namespace src.Interactions;

public sealed partial class DebugCommands
{
    [SubSlashCommand("name-cache", "user-cache")]
    public sealed class UserCache : ApplicationCommandModule<ApplicationCommandContext>
    {
        [SubSlashCommand("get-all", "Get all cached global names.")]
        public async Task GetAll(bool ephemeral = true)
        {
            IEnumerable<(string idStr, string name)> names = src.UserCache.GetNames()
                .Select(n => (
                    idStr: n.id.ToString(),
                    name: n.name
                ));

            if(!names.Any())
            {
                await RespondAsync(InteractionCallback.Message(new()
                {
                    Content = "The global name cache is currently empty.",
                    Flags = MessageFlags.Get(ephemeral: ephemeral)
                }));

                return;
            }

            i32 idPad = names
                .OrderByDescending(n => n.idStr.Length)
                .First()
                .idStr.Length;

            await RespondAsync(InteractionCallback.Message(new()
            {
                Embeds =
                [
                    new()
                    {
                        Title = "Global Name Cache",
                        Description =
                            "```\n" +
                            string.Join("\n", names
                                .Select(n => $"{n.idStr.PadRight(idPad)}  {n.name}")) +
                            "```",
                        Color = new((i32)Random.Shared.NextRgb())
                    }
                ],
                Flags = MessageFlags.Get(ephemeral: ephemeral)
            }));
        }

        [SubSlashCommand("get", "Get your or someone else's cached global name.")] 
        public async Task Get(User user = null, bool ephemeral = true)
        {
            user ??= Context.User;

            string name = src.UserCache.GetName(user.Id);

            await RespondAsync(InteractionCallback.Message(new()
            {
                Content = $"{(user.Id == Context.User.Id ? "Your" : $"<@{user.Id}>'s")} cached global name is `{name}`",
                Flags = MessageFlags.Get(ephemeral: ephemeral)
            }));
        }

        [SubSlashCommand("update", "Update your own cached global name.")] 
        public async Task Update(bool ephemeral = true)
        {
            await RespondAsync(InteractionCallback.DeferredMessage(MessageFlags.Get(ephemeral: ephemeral)));

            string name = src.UserCache.GetName(Context.User.Id);
            string newName = await src.UserCache.UpdateNameAsync(Context.User.Id);

            await FollowupAsync(new()
            {
                Content = name == newName
                    ? $"Your cached global name is still `{name}`."
                    : $"Your cached global name changed from `{name}` to `{newName}`.",
                Flags = MessageFlags.Get(ephemeral: ephemeral)
            });
        }

        [SubSlashCommand("update-all", "[!] Update all cached global names.")] 
        public async Task UpdateAll(bool ephemeral = true)
        {
            if(!await App.CheckForOwner(Context))
                return;

            await RespondAsync(InteractionCallback.DeferredMessage(MessageFlags.Get(ephemeral: ephemeral)));

            await src.UserCache.UpdateAllNamesAsync();

            await FollowupAsync(new()
            {
                Content = $"Updated all global names in the cache.",
                Flags = MessageFlags.Get(ephemeral: true)
            });
        }

        [SubSlashCommand("clear", "[!] Clear all cached global names.")] 
        public async Task Clear(bool ephemeral = true)
        {
            if(!await App.CheckForOwner(Context))
                return;

            Directory.Delete(src.UserCache.cachePath, true);

            await RespondAsync(InteractionCallback.Message(new()
            {
                Content = "Cleared global name cache.",
                Flags = MessageFlags.Get(ephemeral: ephemeral)
            }));
        }
    }
}
