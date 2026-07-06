using NetCord;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;

namespace src.Casino;

[SlashCommand("casino", "casino", Contexts = [InteractionContextType.Guild])]
public sealed class CasinoCommands : ApplicationCommandModule<ApplicationCommandContext>
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
            IEnumerable<(string name, string currency)> currStats = CurrencyMgr.GetAllCurrency(Context.Guild.Id)
                .OrderByDescending(stat => stat.currency)
                .Select(c => (
                    name: UserCache.GetName(c.user, Context.Guild.Id),
                    currency: CurrencyMgr.FormatCurrency(c.currency, CurrencyMgr.GetUserCurrencyName((Context.Guild.Id, c.user)), 2, true)
                ));

            if(!currStats.Any())
            {
                await RespondAsync(InteractionCallback.Message(new()
                {
                    Content = "Nobody has any currency.",
                    Flags = MessageFlags.Get(ephemeral: ephemeral)
                }));

                return;
            }

            i32 namePad = currStats.Max(stat => stat.name.Length);
            i32 currPad = currStats.Max(stat => stat.currency.Length);

            await RespondAsync(InteractionCallback.Message(new()
            {
                Embeds =
                [
                    new()
                    {
                        Title = "Currency",
                        Description =
                            "```\n" +
                            string.Join("\n", currStats
                                .Select(stat => $"{stat.currency.PadRight(currPad)} ~ {stat.name.PadRight(namePad)}")) +
                            "```",
                        Color = new((i32)Random.Shared.NextRgb())
                    }
                ],
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
                    Flags = MessageFlags.Get()
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
                    Flags = MessageFlags.Get()
                }));

                return;
            }

            CurrencyMgr.AddCurrency(target, amount);
            CurrencyMgr.AddCurrency(self, -amount);
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

    [SubSlashCommand("level", "level")]
    public sealed class Level : ApplicationCommandModule<ApplicationCommandContext>
    {
        [SubSlashCommand("get", "Get your or someone's current level and xp.")]
        public async Task Get(User user = null, bool ephemeral = true)
        {
            user ??= Context.User;
            GuildUserPair guildUser = (Context.Guild.Id, user.Id);

            if(user.IsBot)
            {
                await RespondAsync(InteractionCallback.Message(new()
                {
                    Content = "A bot does not contain user data.",
                    Flags = MessageFlags.Get(ephemeral: ephemeral)
                }));

                return;
            }

            u32 xp = LevelUpMgr.GetStat(guildUser, LevelUpMgr.Stat.Xp);
            u32 level = LevelUpMgr.GetStat(guildUser, LevelUpMgr.Stat.Level);

            await RespondAsync(InteractionCallback.Message(new()
            {
                Content = $"<@{user.Id}>'s progess to level `{level+1}`: `{xp}`/`{LevelUpMgr.GetRequiredXp(level)}` XP.",
                Flags = MessageFlags.Get(ephemeral: ephemeral)
            }));
        }

        [SubSlashCommand("get-all", "Get all users' level progress.")]
        public async Task GetAll(bool ephemeral = true)
        {
            IEnumerable<(string name, string xp, string reqXp, string level)> levelStats = LevelUpMgr.GetGuildUserStats(Context.Guild.Id)
                .OrderByDescending(stat => stat.xp)
                .OrderByDescending(stat => stat.level)
                .Select(stat => (
                    name: UserCache.GetName(stat.user, Context.Guild.Id),
                    xp: stat.xp.ToString(),
                    reqXp: LevelUpMgr.GetRequiredXp(stat.level).ToString(),
                    level: stat.level.ToString()
                ));

            if(!levelStats.Any())
            {
                await RespondAsync(InteractionCallback.Message(new()
                {
                    Content = $"No user has any level progress.",
                    Flags = MessageFlags.Get(ephemeral: ephemeral)
                }));

                return;
            }

            i32 xpPad = levelStats.Max(stat => stat.xp.Length);
            i32 reqXpPad = levelStats.Max(stat => stat.reqXp.Length);
            i32 levelPad = levelStats.Max(stat => stat.level.Length);

            await RespondAsync(InteractionCallback.Message(new()
            {
                Embeds =
                [
                    new()
                    {
                        Title = "Levels",
                        Description =
                            "```\n" +
                            string.Join("\n", levelStats
                                .Select(stat => $"[ level {stat.level.PadRight(levelPad)} ~ {stat.xp.PadLeft(xpPad)}/{stat.reqXp.PadRight(reqXpPad)} XP ]  {stat.name}")) +
                            "```",
                        Color = new((i32)Random.Shared.NextRgb())
                    }
                ],
                Flags = MessageFlags.Get(ephemeral: ephemeral)
            }));
        }
    }
}
