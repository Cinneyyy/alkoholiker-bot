using System.Text.Json.Serialization;

namespace src.Soundboard;

public readonly record struct Sound
    (string guid, string displayName, f32 volume)
{
    [JsonIgnore] public string filePath => App.GetPath($"soundboard/{guid}");
}