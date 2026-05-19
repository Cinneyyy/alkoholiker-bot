using System.Text.Json.Serialization;
using NetCord;

namespace src.Soundboard;

public readonly record struct Sound
    (string guid, string displayName, string displayEmoji)
{
    [JsonIgnore] public EmojiProperties displayEmojiProperties => u64.TryParse(displayEmoji, out u64 id)
        ? EmojiProperties.Custom(id)
        : EmojiProperties.Standard(displayEmoji);
}