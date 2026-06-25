using System.Text.Json.Serialization;
using src.Rules;

namespace src;

public readonly struct Config
{
    public enum Field
    {
        defaultRuleCooldown,
        logRuleFailure,
        customEmojiFallback,
        callHistoryChannel,
        logSolitaryCalls,
        activityChangeIntervalMs
    }


    private static string path;


    [JsonInclude, JsonPropertyName("defaultRuleCooldown")] private readonly string _defaultRuleCooldown
    {
        get => $"{defaultRuleCooldownSeconds}s";
        set => defaultRuleCooldownSeconds = Predicate.TimeStrToSeconds(value); 
    }
    [JsonInclude, JsonPropertyName(nameof(logRuleFailure))] private readonly bool _logRuleFailure
    {
        get => logRuleFailure;
        set => logRuleFailure = value;
    }
    [JsonInclude, JsonPropertyName(nameof(customEmojiFallback))] private readonly string _customEmojiFallback
    {
        get => customEmojiFallback;
        set => customEmojiFallback = value;
    }
    [JsonInclude, JsonPropertyName(nameof(callHistoryChannel))] private readonly string _callHistoryChannel
    {
        get => callHistoryChannel;
        set => callHistoryChannel = value;
    }
    [JsonInclude, JsonPropertyName(nameof(logSolitaryCalls))] private readonly bool _logSolitaryCalls
    {
        get => logSolitaryCalls;
        set => logSolitaryCalls = value;
    }
    [JsonInclude, JsonPropertyName(nameof(activityChangeIntervalMs))] private readonly i32 _activityChangeIntervalMs
    {
        get => activityChangeIntervalMs;
        set => activityChangeIntervalMs = value;
    }


    public static u32 defaultRuleCooldownSeconds { get; private set; } = 0u;
    public static bool logRuleFailure { get; private set; } = false;
    public static string customEmojiFallback { get; private set; } = "🇭";
    public static string callHistoryChannel { get; private set; } = null;
    public static bool logSolitaryCalls { get; private set; } = false;
    public static i32 activityChangeIntervalMs { get; private set; } = 60000;



    public static void Load()
        => _ = Json.DeserializeFile<Config>(path);

    public static void Save()
        => Json.SerializeFile(new Config(), path);

    public static void SetPath(string path)
        => Config.path = path;

    public static string GetPath()
        => path;
}
