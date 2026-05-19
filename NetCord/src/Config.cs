using System.Text.Json.Serialization;

namespace src;

public readonly struct Config
{
    [JsonInclude, JsonPropertyName("dataPath")] private readonly string _dataPath { set => dataPath = value; }


    public static string dataPath { get; private set; } = null;


    public static void Load(string file)
        => _ = Json.DeserializeFile<Config>(file);
}