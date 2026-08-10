using NetCord.Rest;

namespace src.Casino;

public static class CasinoStatsMgr
{
    public static void OnCurrencyChange(GuildUserPair guildUser, CurrencySource source, i64 delta)
        => Set(guildUser, source, Get(guildUser, source) + delta);

    public static i64 Get(GuildUserPair guildUser, CurrencySource source)
        => File.ReadAllText(CasinoMgr.GetPath(guildUser, $"CurrencyDelta_{source}", "0")).Trim().ParseI64();

    public static void Set(GuildUserPair guildUser, CurrencySource source, i64 value)
        => File.WriteAllText(CasinoMgr.GetPath(guildUser, $"CurrencyDelta_{source}", "0"), value.ToString());

    // Count start: 2026-08-10
    public static MessageProperties CreateStatMessage(u64 guildId)
    {
        IEnumerable<(CurrencySource source, IEnumerable<(u64 user, i64 delta)> data)> stats = Enum.GetValues<CurrencySource>()
            .Select(source => (
                source: source,
                data: Directory.GetFiles(App.GetPath($"casino/user_data/{guildId}"), $"*_CurrencyDelta_{source}")
                    .Select(file => (
                        user: string.Join(null, file.GetFileName().Where(char.IsDigit)).ParseU64(),
                        delta: File.ReadAllText(file).Trim().ParseI64(
                    )))
                ));

#pragma warning disable CS0078 // The 'l' suffix is easily confused with the digit '1'
        return new()
        {
            Embeds =
            [
                new()
                {
                    Title = "Currency Deltas",
                    Fields = stats.Select(
                        stat => new EmbedFieldProperties()
                        {
                            Name = stat.source.ToString(),
                            Inline = false,
                            Value = !stat.data.Any()
                                ? "`(no data)`"
                                : string.Join("\n", stat.data
                                    .OrderByDescending(datum => datum.delta)
                                    .Select(data => $"<@{data.user}>: **[{CurrencyMgr.FormatCurrency(i64.Abs(data.delta), (guildId, data.user), numberPrefix: data.delta switch {
                                        < 0l => "-",
                                        0l => "",
                                        > 0l => "+"
                                    })}]**")
                            )
                        }
                    ),
                    Color = new((i32)Random.Shared.NextRgb())
                }
            ]
        };
    }
}
