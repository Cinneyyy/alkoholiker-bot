using System.Text;
using System.Text.RegularExpressions;

namespace src.Casino;

public static partial class CurrencyMgr
{
    public static Dictionary<string, string[]> currencyNames { get; private set; } = [];
    public static i64[] values { get; private set; } = [];


    public static void LoadCurrencyMeta()
    {
        values = Json.DeserializeFile<i64[]>(App.GetPath("casino/currency_values.json"));
        currencyNames = Json.DeserializeFile<Dictionary<string, string[]>>(App.GetPath("casino/currency_names.json"));
    }

    public static i64 GetRawCurrency(GuildUserPair guildUser)
        => File.ReadAllText(CasinoMgr.GetPath(guildUser, "Currency", Config.startingCurrency.ToString())).Trim().ParseI64();

    public static void SetRawCurrency(GuildUserPair guildUser, i64 value)
        => File.WriteAllText(CasinoMgr.GetPath(guildUser, "Currency", Config.startingCurrency.ToString()), value.ToString());

    public static void AddCurrency(GuildUserPair guildUser, i64 addition)
        => SetRawCurrency(guildUser, GetRawCurrency(guildUser) + addition);

    public static string GetUserCurrencyName(GuildUserPair guildUser)
        => File.ReadAllText(CasinoMgr.GetPath(guildUser, "CurrencyName", "default")).Trim();

    public static void SetUserCurrencyName(GuildUserPair guildUser, string name)
        => File.WriteAllText(CasinoMgr.GetPath(guildUser, "CurrencyName", "default"), name);

    public static (u64 user, u32 currency)[] GetAllCurrency(u64 guild)
        => Directory.GetFiles(App.GetPath($"casino/user_data/{guild}/"), "*_Currency")
            .Select(f => (
                id: f.GetFileName().Split('_').First().ParseU64(),
                currency: File.ReadAllText(f).Trim().ParseU32()
            ))
            .ToArray();

    public static string FormatCurrency(i64 value, string currency, i32? displayLimit = null, bool trimEmojis = false)
    {
        if(!currencyNames.TryGetValue(currency, out string[] currencies))
        {
            Log.Out($"[Warning] Falling back to default currency, as currency `{currency}` is not registered.");
            currencies = currencyNames["default"];
        }

        if(values.Length > currencies.Length)
        {
            Log.Out($"[Error in FormatCurrency] Cannot format currency when values.Length ({values.Length}) is greater than currencies.Length ({currencies.Length}).");
            return "[Failed to format currency]";
        }

        StringBuilder sb = new();

        if(value < 0L)
            sb.Append("(debt) ");

        value = i64.Abs(value);
        bool hadGreaterCurrency = false;
        i32 displaysLeft = displayLimit ?? i32.MaxValue;

        for(i32 i = values.Length-1; i >= 0; i--)
        {
            i64 currCurrValue = value / values[i];
            value -= currCurrValue * values[i];

            if(currCurrValue == 0L && !hadGreaterCurrency && i > 0)
                continue;
            else
                hadGreaterCurrency = true;

            if(--displaysLeft < 0)
                break;

            string curr = currencies[i];
            curr = PluralRegex().Replace(curr, m => currCurrValue == 1
                ? string.Empty 
                : m.Groups[1].Value);

            curr = SingularRegex().Replace(curr, m => currCurrValue != 1
                ? string.Empty 
                : m.Groups[1].Value);

            if(trimEmojis)
                curr = Emoji.CustomEmojiRegex().Replace(curr, string.Empty).Trim();

            sb.Append($"{currCurrValue} {curr}");

            if(i > 0 && displaysLeft > 0)
                sb.Append(", ");
        }

        return sb.ToString();
    }
    public static string FormatCurrency(long amount, GuildUserPair guildUser, i32? displayLimit = null, bool trimEmojis = false)
        => FormatCurrency(amount, GetUserCurrencyName(guildUser), displayLimit, trimEmojis);
    public static string FormatCurrency(GuildUserPair guildUser, i32? displayLimit = null, bool trimEmojis = false)
        => FormatCurrency(GetRawCurrency(guildUser), GetUserCurrencyName(guildUser), displayLimit, trimEmojis);


    [GeneratedRegex(@"\(([a-zA-Z0-9]+)\)")]
    private static partial Regex PluralRegex();

    [GeneratedRegex(@"\[([a-zA-Z0-9]+)\]")]
    private static partial Regex SingularRegex();
}
