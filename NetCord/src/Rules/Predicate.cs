using System.Text.Json.Serialization;

namespace src.Rules;

public readonly record struct Predicate()
{
    [JsonIgnore] public readonly string guid = Guid.NewGuid().ToString();


    public f32? chance { get; init; } = null;
    public Regex? regex { get; init; } = null;
    public string channel { get; init; } = null;
    public string user { get; init; } = null;
    public u32? cooldownSeconds { get; init; } = null;
    public bool? refMessage { get; init; } = null;
    public bool? hasAttachment { get; init; } = null;

    public u32? cooldownMinutes { init => cooldownSeconds = 60 * value; }
    public u32? cooldownHours { init => cooldownSeconds = 60*60 * value; }
    public string cooldown { init => cooldownSeconds = (value[^1] switch
    {
        's' => 1u,
        'm' => 60,
        'h' => 60u*60u,
        'd' => 24u*60u*60,
        _ => throw new($"Invalid cooldown unit (\"{value}\"); must be s, m, h, or d.")
    }) * u32.Parse(value[..^1]); }


    public u32 GetActualCooldownSeconds(u32 def = 0u)
        => cooldownSeconds ?? def;
}