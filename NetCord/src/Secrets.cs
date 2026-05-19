using System.Text.Json;
using System.Text.Json.Serialization;

namespace src;

public readonly struct Secrets
{
    [JsonInclude, JsonPropertyName("token")] private readonly string _token { set => token = value; }
    [JsonInclude, JsonPropertyName("guild")] private readonly u64 _guild { set => guild = value; }


    public static string token { get; private set; } = null;
    public static u64 guild { get; private set; } = 0ul;


    public static void Load(string file)
        => _ = Json.DeserializeFile<Secrets>(file);
}