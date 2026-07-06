using NetCord;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;

namespace src.Casino;

public sealed partial class CasinoCommands
{
    [SubSlashCommand("play", "play")]
    public sealed class Play : ApplicationCommandModule<ApplicationCommandContext>
    {
        public enum CoinFace : u8
        {
            Heads,
            Tails
        }


        [SubSlashCommand("coin-toss", "Bet a certain amount on heads or tails, double or nothing.")]
        public async Task CoinToss(u32 amount, CoinFace face)
        {
            if(!await CasinoMgr.IsValidAmount(Context, amount))
                return;

            CoinFace randomFace = Random.Shared.NextSingle() > 0.5f ? CoinFace.Heads : CoinFace.Tails;
            GuildUserPair guildUser = (Context.Guild.Id, Context.User.Id);

            bool won = face == randomFace;

            CurrencyMgr.AddCurrency(guildUser, won ? amount : -amount);
            await RespondAsync(InteractionCallback.Message(new()
            {
                Embeds =
                [
                    new()
                    {
                        Title = $"The coin landed on `{randomFace.ToString().ToLowerInvariant()}`!",
                        Description = $"You {(won ? "won" : "lost")} **[{CurrencyMgr.FormatCurrency(amount, guildUser, numberPrefix: won ? "+" : "-")}]**.\nYou now have **[{CurrencyMgr.FormatCurrency(guildUser)}]**.",
                        Color = new(won ? 0x00ff00 : 0xff0000),
                        Footer = new()
                        {
                            Text = UserCache.GetName(Context.User.Id, Context.Guild.Id),
                            IconUrl = Context.User.GetAvatarUrl().ToString(256)
                        }
                    }
                ],
                Flags = MessageFlags.Get(ephemeral: false)
            }));
        }

        [SubSlashCommand("rock-paper-scissors", "Play rock paper scissors against someone.")]
        public async Task RockPaperScissors(RpsChoice choice, User against, u32 wager)
        {
            if(!await CasinoMgr.IsValidAmount(Context, wager))
                return;

            if(against.Id == Context.User.Id || against.IsBot)
            {
                await RespondAsync(InteractionCallback.Message(new()
                {
                    Content = "Invalid opponent.",
                    Flags = MessageFlags.Get()
                }));

                return;
            }

            GuildUserPair opponent = (Context.Guild.Id, against.Id);
            if(CurrencyMgr.GetRawCurrency(opponent) < wager)
            {
                await RespondAsync(InteractionCallback.Message(new()
                {
                    Content = $"Opponent does not have enough currency (only **[{CurrencyMgr.FormatCurrency(opponent)}]**).",
                    Flags = MessageFlags.Get()
                }));

                return;
            }

            string guid = Guid.NewGuid().ToString();
            RpsButtonHandler.openGames.Add(guid, (Context.User.Id, opponent.user, choice, wager));

            GuildUserPair user = (Context.Guild.Id, Context.User.Id);
            await RespondAsync(InteractionCallback.Message(new()
            {
                Content = $"<@{opponent.user}>, <@{Context.User.Id}> challenged you to a game of rock paper scissors. The wager is **[{CurrencyMgr.FormatCurrency(wager, user)}]**.",
                Components =
                [
                    new ActionRowProperties(
                    [
                        new ButtonProperties($"button_casino_rps_accept:{guid}:{(u8)RpsChoice.Rock}", "🪨", ButtonStyle.Primary),
                        new ButtonProperties($"button_casino_rps_accept:{guid}:{(u8)RpsChoice.Paper}", "🧻", ButtonStyle.Primary),
                        new ButtonProperties($"button_casino_rps_accept:{guid}:{(u8)RpsChoice.Scissors}", "✂️", ButtonStyle.Primary),
                        new ButtonProperties($"button_casino_rps_decline:{guid}", "Decline", ButtonStyle.Primary)
                    ])
                ],
                Flags = MessageFlags.Get(ephemeral: false, silent: false)
            }));
        }
    }
}
