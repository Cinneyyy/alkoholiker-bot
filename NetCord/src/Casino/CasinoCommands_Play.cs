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

        // [SubSlashCommand("lootbox", "Buy a lootbox and hope you get something good.")]
        // public async Task Lootbox()
        // {
        // }

    }
}
