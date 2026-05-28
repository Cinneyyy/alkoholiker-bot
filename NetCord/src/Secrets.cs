using System.Text.Json;
using System.Text.Json.Serialization;

namespace src;

public readonly struct Secrets
{
    [JsonInclude, JsonPropertyName("owner")] private readonly u64 _owner { set => owner = value; }
    [JsonInclude, JsonPropertyName("token")] private readonly string _token { set => token = value; }
    [JsonInclude, JsonPropertyName("guild")] private readonly u64 _guild { set => guild = value; }
    [JsonInclude, JsonPropertyName("botUserId")] private readonly u64 _botUserId { set => botUserId = value; }
    [JsonInclude, JsonPropertyName("dataPath")] private readonly string _dataPath { set => dataPath = value; }


    public static u64 owner { get; private set; } = 0ul;
    public static string token { get; private set; } = null;
    public static u64 guild { get; private set; } = 0ul;
    public static u64 botUserId { get; private set; } = 0ul;
    public static string dataPath { get; private set; } = "";


    public static void Load(string file)
        => _ = Json.DeserializeFile<Secrets>(file);
}