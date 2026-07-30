using NetCord;
using NetCord.Rest;
using NetCord.Services.ComponentInteractions;

namespace src.Casino;

public sealed class CoinTossButtonHandler : ComponentInteractionModule<ButtonInteractionContext>
{
    public static readonly Dictionary<string, (u64 challenger, u64 opponent, u32 wager)> openGames = [];


    [ComponentInteraction("button_casino_ct_accept")]
    public async Task CasinoCtAccept(string guid)
    {
        if(!openGames.TryGetValue(guid, out (u64 challenger, u64 opponent, u32 wager) gameData))
        {
            await RespondAsync(InteractionCallback.Message(new()
            {
                Content = $"The game has already concluded.",
                Flags = MessageFlags.Get()
            }));

            return;
        }

        (u64 challengerId, u64 opponentId, u32 wager) = gameData;

        if(Context.User.Id != opponentId)
        {
            await RespondAsync(InteractionCallback.Message(new()
            {
                Content = "You cannot accept a challenge directed at someone else.",
                Flags = MessageFlags.Get()
            }));

            return;
        }

        openGames.Remove(guid);

        GuildUserPair challenger = (Context.Guild.Id, challengerId);
        GuildUserPair opponent = (Context.Guild.Id, opponentId);
        if(CurrencyMgr.GetRawCurrency(opponent) < wager || CurrencyMgr.GetRawCurrency(challenger) < wager)
        {
            await RespondAsync(InteractionCallback.Message(new()
            {
                Content = $"Failed to conclude game since the wager was too great.",
                Flags = MessageFlags.Get(ephemeral: false)
            }));

            return;
        }

        CoinFace coinFace = Random.Shared.NextSingle() > 0.5f ? CoinFace.Heads : CoinFace.Tails;

        (GuildUserPair winner, GuildUserPair loser) = coinFace == CoinFace.Heads
            ? (opponent, challenger)
            : (challenger, opponent);

        CurrencyMgr.AddCurrency(winner, wager);
        CurrencyMgr.AddCurrency(loser, -wager);

        await RespondAsync(InteractionCallback.Message(new()
        {
            Embeds =
            [
                new()
                {
                    Title = "Coin toss result.",
                    Description = $"The coin landed on `{coinFace.ToString().ToLowerInvariant()}`, thus <@{winner.user}> emerged victorious.\n\n**[{CurrencyMgr.FormatCurrency(wager, winner)}]** was transferred from <@{loser.user}> to <@{winner.user}>.\n\n<@{winner.user}> now has **[{CurrencyMgr.FormatCurrency(winner)}]**.\n<@{loser.user}> now has **[{CurrencyMgr.FormatCurrency(loser)}]**.",
                    Color = new((i32)Random.Shared.NextRgb())
                }
            ],
            Flags = MessageFlags.Get(ephemeral: false)
        }));
    }

    [ComponentInteraction("button_casino_ct_decline")]
    public async Task RpsDecline(string guid)
    {
        if(!openGames.TryGetValue(guid, out (u64 challenger, u64 opponent, u32) gameData))
        {
            await RespondAsync(InteractionCallback.Message(new()
            {
                Content = $"The game has already concluded.",
                Flags = MessageFlags.Get()
            }));

            return;
        }

        if(Context.User.Id != gameData.opponent)
        {
            await RespondAsync(InteractionCallback.Message(new()
            {
                Content = "You cannot decline a challenge directed at someone else.",
                Flags = MessageFlags.Get()
            }));

            return;
        }

        openGames.Remove(guid);

        await RespondAsync(InteractionCallback.Message(new()
        {
            Embeds =
            [
                new()
                {
                    Title = $"Challenge declined",
                    Description = $"<@{gameData.challenger}>, <@{gameData.opponent}> declined your challenge.",
                    Color = new((i32)Random.Shared.NextRgb())
                }
            ],
            Flags = MessageFlags.Get(ephemeral: false, silent: false)
        }));
    }
}
