using NetCord;
using NetCord.Rest;
using NetCord.Services.ComponentInteractions;

namespace src.Casino;

public sealed class RpsButtonHandler : ComponentInteractionModule<ButtonInteractionContext>
{
    public static readonly Dictionary<string, (u64 challenger, u64 opponent, RpsChoice challengerValue, u32 wager)> openGames = [];


    [ComponentInteraction("button_casino_rps_accept")]
    public async Task CasinoRpsAccept(string guid, u8 opponentValueU8)
    {
        if(!openGames.TryGetValue(guid, out (u64 challenger, u64 opponent, RpsChoice challengerValue, u32 wager) gameData))
        {
            await RespondAsync(InteractionCallback.Message(new()
            {
                Content = $"The game has already concluded.",
                Flags = MessageFlags.Get()
            }));

            return;
        }

        (u64 challengerId, u64 opponentId, RpsChoice challengerValue, u32 wager) = gameData;

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

        RpsChoice opponentValue = (RpsChoice)opponentValueU8;
        if(challengerValue == opponentValue)
        {
            await RespondAsync(InteractionCallback.Message(new()
            {
                Content = $"Both players chose `{FormatChoice(challengerValue)}`.\nNothing has changed.",
                Flags = MessageFlags.Get(ephemeral: false)
            }));

            return;
        }

        (GuildUserPair winner, GuildUserPair loser, RpsChoice winningChoice, RpsChoice losingChoice) = (u8)challengerValue == ((u8)opponentValue+1)%3
            ? (challenger, opponent, challengerValue, opponentValue)
            : (opponent, challenger, opponentValue, challengerValue);

        CurrencyMgr.AddCurrency(winner, wager);
        CurrencyMgr.AddCurrency(loser, -wager);

        LevelUpMgr.OnGamble(winner, wager);
        LevelUpMgr.OnGamble(loser, wager);

        await RespondAsync(InteractionCallback.Message(new()
        {
            Embeds =
            [
                new()
                {
                    Title = "Rock paper scissors result.",
                    Description = $"<@{winner.user}> won with `{FormatChoice(winningChoice)}`.\n<@{loser.user}> lost with `{FormatChoice(losingChoice)}`.\n\n**[{CurrencyMgr.FormatCurrency(wager, winner)}]** was transferred from <@{loser.user}> to <@{winner.user}>.\n\n<@{winner.user}> now has **[{CurrencyMgr.FormatCurrency(winner)}]**.\n<@{loser.user}> now has **[{CurrencyMgr.FormatCurrency(loser)}]**.",
                    Color = new((i32)Random.Shared.NextRgb())
                }
            ],
            Flags = MessageFlags.Get(ephemeral: false)
        }));
    }

    [ComponentInteraction("button_casino_rps_decline")]
    public async Task RpsDecline(string guid)
    {
        if(!openGames.TryGetValue(guid, out (u64 challenger, u64 opponent, RpsChoice, u32) gameData))
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


    private static string FormatChoice(RpsChoice choice)
        => choice switch
        {
            RpsChoice.Rock => "rock 🪨",
            RpsChoice.Paper => "paper 🧻",
            RpsChoice.Scissors => "scissors ✂️",
            _ => throw new($"Invalid RpsSelection ({choice}).")
        };
}
