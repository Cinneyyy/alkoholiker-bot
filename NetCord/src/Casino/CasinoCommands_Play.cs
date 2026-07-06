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
            if(face == randomFace)
            {
                CurrencyMgr.AddCurrency(guildUser, amount);

                await RespondAsync(InteractionCallback.Message(new()
                {
                    Content = $"The coin landed on {randomFace}; you won {CurrencyMgr.FormatCurrency(amount, guildUser)} (you now have {CurrencyMgr.FormatCurrency(guildUser)})!",
                    Flags = MessageFlags.Get(ephemeral: false)
                }));
            }
            else
            {
                CurrencyMgr.AddCurrency((Context.Guild.Id, Context.User.Id), -amount);

                await RespondAsync(InteractionCallback.Message(new()
                {
                    Content = $"The coin landed on {randomFace}; you lost {CurrencyMgr.FormatCurrency(amount, guildUser)} (you now have {CurrencyMgr.FormatCurrency(guildUser)}).",
                    Flags = MessageFlags.Get(ephemeral: false)
                }));
            }
        }
    }
}
