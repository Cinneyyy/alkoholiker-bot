using System.Text.Json.Serialization;

namespace src.Soundboard;

public readonly record struct Sound
    (string guid, string displayName)
{
    [JsonIgnore] public string filePath => App.GetPath($"soundboard/{guid}");
}