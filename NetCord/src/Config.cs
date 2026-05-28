using System.Text.Json.Serialization;
using src.Rules;

namespace src;

public readonly struct Config
{
    private static string path;


    [JsonInclude, JsonPropertyName("defRuleCooldown")] private readonly string _defRuleCooldown
    {
        get => $"{defRuleCooldown}s";
        set => defRuleCooldown = Predicate.TimeStrToSeconds(value); 
    }
    [JsonInclude, JsonPropertyName("logRuleFailure")] private readonly bool _logRuleFailure
    {
        get => logRuleFailure;
        set => logRuleFailure = value;
    }
    [JsonInclude, JsonPropertyName("customEmojiFallback")] private readonly string _customEmojiFallback
    {
        get => customEmojiFallback;
        set => customEmojiFallback = value;
    }
    [JsonInclude, JsonPropertyName("callHistoryChannel")] private readonly u64 _callHistoryChannel
    {
        get => callHistoryChannel;
        set => callHistoryChannel = value;
    }


    public static u32 defRuleCooldown { get; private set; } = 0u;
    public static bool logRuleFailure { get; private set; } = false;
    public static string customEmojiFallback { get; private set; } = "🇭";
    public static u64 callHistoryChannel { get; private set; } = 0ul;



    public static void Load()
        => _ = Json.DeserializeFile<Config>(path);

    public static void Save()
        => Json.SerializeFile(new Config(), path);

    public static void SetPath(string path)
        => Config.path = path;

    public static string GetPath()
        => path;
}
