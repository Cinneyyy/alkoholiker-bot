using NetCord;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;

namespace src.Casino;

public sealed partial class CasinoCommands
{
    [SubSlashCommand("challenge", "challenge")]
    public sealed class Challenge : ApplicationCommandModule<ApplicationCommandContext>
    {
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

            await RespondAsync(InteractionCallback.Message(new()
            {
                Content = $"Sending challenge to <@{against.Id}>.",
                Flags = MessageFlags.Get()
            }));

            string guid = Guid.NewGuid().ToString();
            RpsButtonHandler.openGames.Add(guid, (Context.User.Id, opponent.user, choice, wager));

            GuildUserPair user = (Context.Guild.Id, Context.User.Id);
            await Context.Channel.SendMessageAsync(new()
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
            });
        }

        [SubSlashCommand("coin-toss", "Challenge someone to a coin toss.")]
        public async Task CoinToss(User against, u32 wager)
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

            await RespondAsync(InteractionCallback.Message(new()
            {
                Content = $"Sending challenge to <@{against.Id}>.",
                Flags = MessageFlags.Get()
            }));

            string guid = Guid.NewGuid().ToString();
            CoinTossButtonHandler.openGames.Add(guid, (Context.User.Id, opponent.user, wager));

            GuildUserPair user = (Context.Guild.Id, Context.User.Id);
            await Context.Channel.SendMessageAsync(new()
            {
                Content = $"<@{opponent.user}>, <@{Context.User.Id}> challenged you to a coin toss. You win on heads, they win on tails. The wager is **[{CurrencyMgr.FormatCurrency(wager, user)}]**.",
                Components =
                [
                    new ActionRowProperties(
                    [
                        new ButtonProperties($"button_casino_ct_accept:{guid}", "Accept", ButtonStyle.Primary),
                        new ButtonProperties($"button_casino_ct_decline:{guid}", "Decline", ButtonStyle.Primary)
                    ])
                ],
                Flags = MessageFlags.Get(ephemeral: false, silent: false)
            });
        }
    }
}
