using NetCord;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;
using src;

namespace src.Interactions;

public sealed partial class DebugCommands
{
    [SubSlashCommand("name-cache", "user-cache")]
    public sealed class NameCache : ApplicationCommandModule<ApplicationCommandContext>
    {
        [SubSlashCommand("get-all", "Get all cached names.")]
        public async Task GetAll(bool ephemeral = true)
        {
            IEnumerable<(u64 userId, u64? guildId, string name)> names = UserCache.GetNames();

            if(!names.Any())
            {
                await RespondAsync(InteractionCallback.Message(new()
                {
                    Content = "The name cache is currently empty.",
                    Flags = MessageFlags.Get(ephemeral: ephemeral)
                }));

                return;
            }

            static string getGuildName(u64 id)
            {
                try
                {
                    return App.restClient.GetGuildAsync(id).GetAwaiter().GetResult()?.Name;
                }
                catch
                {
                    return id.ToString();
                }
            }

            const i32 U64Pad = 20; // Length of u64.MaxValue.ToString()
            await RespondAsync(InteractionCallback.Message(new()
            {
                Embeds =
                [
                    new()
                    {
                        Title = "Name Cache",
                        Fields = names
                            .GroupBy(
                                n => n.guildId,
                                n => (
                                    userId: n.userId,
                                    name: n.name
                                ))
                            .Select(n => new EmbedFieldProperties()
                            {
                                Name = n.Key is u64 _guildId
                                    ? getGuildName(_guildId)
                                    : "Global",
                                Value = "```\n" + string.Join("\n", n.
                                    Select(n => $"{n.userId,U64Pad}  {n.name}")
                                ) + "```",
                                Inline = false 
                            }),
                        Color = new((i32)Random.Shared.NextRgb())
                    }
                ],
                Flags = MessageFlags.Get(ephemeral: ephemeral)
            }));
        }

        [SubSlashCommand("get", "Get your or someone else's cached name.")] 
        public async Task Get(User user = null, bool global = false, bool ephemeral = true)
        {
            user ??= Context.User;

            string name = UserCache.GetName(user.Id, global ? null : Context.Interaction.GuildId);

            await RespondAsync(InteractionCallback.Message(new()
            {
                Content = $"{(user.Id == Context.User.Id ? "Your" : $"<@{user.Id}>'s")} cached {(global ? "global" : "guild")} name is `{name}`",
                Flags = MessageFlags.Get(ephemeral: ephemeral)
            }));
        }

        [SubSlashCommand("update", "Update your own cached name.")] 
        public async Task Update(bool global = false, bool ephemeral = true)
        {
            await RespondAsync(InteractionCallback.DeferredMessage(MessageFlags.Get(ephemeral: ephemeral)));

            u64? guildId = global ? null : Context.Interaction.GuildId;
            string name = UserCache.GetName(Context.User.Id, guildId);
            string newName = await UserCache.UpdateNameAsync(Context.User.Id, guildId);

            await FollowupAsync(new()
            {
                Content = name == newName
                    ? $"Your cached {(global ? "global" : "guild")} name is still `{name}`."
                    : $"Your cached {(global ? "global" : "guild")} name changed from `{name}` to `{newName}`.",
                Flags = MessageFlags.Get(ephemeral: ephemeral)
            });
        }

        [SubSlashCommand("update-all", "[!] Update all cached names.")] 
        public async Task UpdateAll(bool ephemeral = true)
        {
            if(!await App.CheckForOwner(Context))
                return;

            await RespondAsync(InteractionCallback.DeferredMessage(MessageFlags.Get(ephemeral: ephemeral)));

            await UserCache.UpdateAllNamesAsync();

            await FollowupAsync(new()
            {
                Content = $"Updated all names in the cache.",
                Flags = MessageFlags.Get(ephemeral: true)
            });
        }

        [SubSlashCommand("clear", "[!] Clear all cached names.")] 
        public async Task Clear(bool ephemeral = true)
        {
            if(!await App.CheckForOwner(Context))
                return;

            Directory.Delete(UserCache.cachePath, true);

            await RespondAsync(InteractionCallback.Message(new()
            {
                Content = "Cleared name cache.",
                Flags = MessageFlags.Get(ephemeral: ephemeral)
            }));
        }
    }
}
