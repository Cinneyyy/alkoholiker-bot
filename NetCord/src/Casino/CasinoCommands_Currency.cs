using NetCord;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;

namespace src.Casino;

public sealed partial class CasinoCommands
{
    [SubSlashCommand("currency", "currency")]
    public sealed class Currency : ApplicationCommandModule<ApplicationCommandContext>
    {
        [SubSlashCommand("reload", "[!] Reload currency meta.")]
        public async Task Reload(bool ephemeral = true)
        {
            if(!await App.CheckForOwner(Context))
                return; 

            CurrencyMgr.LoadCurrencyMeta();

            await RespondAsync(InteractionCallback.Message(new()
            {
                Content = "Reloaded currency meta.",
                Flags = MessageFlags.Get(ephemeral: ephemeral)
            }));
        }

        [SubSlashCommand("set", "[!] Set the specified user's currency to the specified value.")]
        public async Task SetCurrency(i64 value, User user = null, bool ephemeral = true)
        {
            if(!await App.CheckForOwner(Context))
                return; 

            user ??= Context.User;

            if(user.IsBot)
            {
                await RespondAsync(InteractionCallback.Message(new()
                {
                    Content = "A bot does not contain user data.",
                    Flags = MessageFlags.Get(ephemeral: ephemeral)
                }));

                return;
            }

            GuildUserPair guildUser = (Context.Interaction.GuildId.Value, user.Id);
            CurrencyMgr.SetRawCurrency(guildUser, value);

            await RespondAsync(InteractionCallback.Message(new()
            {
                Content = $"<@{user.Id}> now has {CurrencyMgr.FormatCurrency(value, CurrencyMgr.GetUserCurrencyName(guildUser))}.",
                Flags = MessageFlags.Get(ephemeral: ephemeral)
            }));
        }

        [SubSlashCommand("get", "Get the specified user's currency.")]
        public async Task GetCurrency(User user = null, bool ephemeral = true)
        {
            user ??= Context.User;

            if(user.IsBot)
            {
                await RespondAsync(InteractionCallback.Message(new()
                {
                    Content = "A bot does not contain user data.",
                    Flags = MessageFlags.Get(ephemeral: ephemeral)
                }));

                return;
            }

            GuildUserPair guildUser = (Context.Interaction.GuildId.Value, user.Id);
            i64 value = CurrencyMgr.GetRawCurrency(guildUser);

            await RespondAsync(InteractionCallback.Message(new()
            {
                Content = $"<@{user.Id}> has {CurrencyMgr.FormatCurrency(value, CurrencyMgr.GetUserCurrencyName(guildUser))}.",
                Flags = MessageFlags.Get(ephemeral: ephemeral)
            }));
        }

        [SubSlashCommand("get-all", "Display all users' currency.")]
        public async Task GetAll(bool ephemeral = true)
        {
            MessageProperties message = CasinoMgr.CreateCurrencyStatMessage(Context.Guild.Id);

            await RespondAsync(InteractionCallback.Message(new()
            {
                Content = message.Content,
                Embeds = message.Embeds,
                Flags = MessageFlags.Get(ephemeral: ephemeral)
            }));
        }

        [SubSlashCommand("format", "See what a certain amount of currency looks like when formatted.")]
        public async Task Format(u32 amount, string currency, bool trimEmojis = false, i32? displayLimit = null, string numberPrefix = null, bool ephemeral = true)
        {
            if(!CurrencyMgr.currencyNames.ContainsKey(currency))
            {
                await RespondAsync(InteractionCallback.Message(new()
                {
                    Content = $"Invalid currency; see available currencies via `/casino currency list`.",
                    Flags = MessageFlags.Get()
                }));

                return;
            }

            await RespondAsync(InteractionCallback.Message(new()
            {
                Content = CurrencyMgr.FormatCurrency(amount, currency, displayLimit, trimEmojis, numberPrefix),
                Flags = MessageFlags.Get(ephemeral: ephemeral)
            }));
        }

        [SubSlashCommand("donate", "Donate some money to a poor soul.")]
        public async Task Donate(User user, u32 amount)
        {
            if(user.Id == Context.User.Id)
            {
                await RespondAsync(InteractionCallback.Message(new()
                {
                    Content = "You cannot donate money to yourself!",
                    Flags = MessageFlags.Get(ephemeral: false)
                }));

                return;
            }

            if(user.IsBot)
            {
                await RespondAsync(InteractionCallback.Message(new()
                {
                    Content = "A bot does not contain user data.",
                    Flags = MessageFlags.Get(ephemeral: false)
                }));

                return;
            }

            GuildUserPair self = (Context.Guild.Id, Context.User.Id);
            GuildUserPair target = (Context.Guild.Id, user.Id);
            if(CurrencyMgr.GetRawCurrency(self) < amount)
            {
                await RespondAsync(InteractionCallback.Message(new()
                {
                    Content = "You cannot donate more money than you own!",
                    Flags = MessageFlags.Get(ephemeral: false)
                }));

                return;
            }

            CurrencyMgr.AddCurrency(target, amount);
            CurrencyMgr.AddCurrency(self, -amount);

            await RespondAsync(InteractionCallback.Message(new()
            {
                Content = $"Donated **[{CurrencyMgr.FormatCurrency(amount, CurrencyMgr.GetUserCurrencyName(self))}**] to <@{target.user}> (you now have **[{CurrencyMgr.FormatCurrency(CurrencyMgr.GetRawCurrency(self), CurrencyMgr.GetUserCurrencyName(self))}]**; they now have **[{CurrencyMgr.FormatCurrency(CurrencyMgr.GetRawCurrency(target), CurrencyMgr.GetUserCurrencyName(target))}]**).",
                Flags = MessageFlags.Get(ephemeral: false)
            }));
        }

        [SubSlashCommand("set-preference", "Set which currency names you'd like to use.")]
        public async Task SetPreference(string name, bool ephemeral = true)
        {
            if(!CurrencyMgr.currencyNames.ContainsKey(name))
            {
                await RespondAsync(InteractionCallback.Message(new()
                {
                    Content = $"The currency `{name}` does not exist. Use `/casino currency list` to see which ones are available.",
                    Flags = MessageFlags.Get()
                }));

                return;
            }

            CurrencyMgr.SetUserCurrencyName((Context.Guild.Id, Context.User.Id), name);

            await RespondAsync(InteractionCallback.Message(new()
            {
                Content = $"Set currency preference to `{name}`.",
                Flags = MessageFlags.Get(ephemeral: ephemeral)
            }));
        }

        [SubSlashCommand("get-preference", "Get your or someone else's currency names.")]
        public async Task SetPreference(User user = null, bool ephemeral = true)
        {
            user ??= Context.User;

            if(user.IsBot)
            {
                await RespondAsync(InteractionCallback.Message(new()
                {
                    Content = "A bot does not contain user data.",
                    Flags = MessageFlags.Get(ephemeral: ephemeral)
                }));

                return;
            }

            await RespondAsync(InteractionCallback.Message(new()
            {
                Content = $"<@{user.Id}>'s currency preference is `{CurrencyMgr.GetUserCurrencyName((Context.Guild.Id, user.Id))}`.",
                Flags = MessageFlags.Get(ephemeral: ephemeral)
            }));
        }

        [SubSlashCommand("list", "List all available currency names.")]
        public async Task List(bool ephemeral = true)
        {
            await RespondAsync(InteractionCallback.Message(new()
            {
                Embeds = 
                [
                    new()
                    {
                        Title = "Currencies",
                        Fields = CurrencyMgr.currencyNames.Select(kvp => new EmbedFieldProperties()
                        {
                            Name = kvp.Key,
                            Value = string.Join("\n", kvp.Value),
                            Inline = true
                        }),
                        Color = new((i32)Random.Shared.NextRgb())
                    }
                ],
                Flags = MessageFlags.Get(ephemeral: ephemeral)
            }));
        }

        [SubSlashCommand("set-user-preference", "[!] Set someone else's currency names.")]
        public async Task SetPreference(User user, string name, bool ephemeral = true)
        {
            if(!await App.CheckForOwner(Context))
                return;

            if(user.IsBot)
            {
                await RespondAsync(InteractionCallback.Message(new()
                {
                    Content = "A bot does not contain user data.",
                    Flags = MessageFlags.Get(ephemeral: ephemeral)
                }));

                return;
            }

            if(!CurrencyMgr.currencyNames.ContainsKey(name))
            {
                await RespondAsync(InteractionCallback.Message(new()
                {
                    Content = $"The currency `{name}` does not exist. Use `/casino currency list` to see which ones are available.",
                    Flags = MessageFlags.Get()
                }));

                return;
            }

            CurrencyMgr.SetUserCurrencyName((Context.Guild.Id, user.Id), name);

            await RespondAsync(InteractionCallback.Message(new()
            {
                Content = $"Set <@{user.Id}>'s currency preference to `{name}`.",
                Flags = MessageFlags.Get(ephemeral: ephemeral)
            }));
        }
    }
}
