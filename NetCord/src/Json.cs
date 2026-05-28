using System.Text.Json;
using System.Text.Json.Serialization;

namespace src;

public static class Json
{
    private static readonly JsonSerializerOptions jsonOptions = new()
    {
        AllowTrailingCommas = true,
        IncludeFields = true,
        NumberHandling = JsonNumberHandling.AllowReadingFromString | JsonNumberHandling.AllowNamedFloatingPointLiterals,
        ReadCommentHandling = JsonCommentHandling.Skip,
        WriteIndented = true,
        IndentSize = 4,
        PropertyNameCaseInsensitive = true,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow
    };


    public static T Deserialize<T>(string json)
        => JsonSerializer.Deserialize<T>(json, jsonOptions);

    public static string Serialize<T>(T obj)
        => JsonSerializer.Serialize(obj, jsonOptions);

    public static T DeserializeFile<T>(string filePath)
        => JsonSerializer.Deserialize<T>(File.ReadAllText(filePath), jsonOptions);

    public static void SerializeFile<T>(T obj, string filePath)
        => File.WriteAllText(filePath, JsonSerializer.Serialize(obj, jsonOptions));
}
