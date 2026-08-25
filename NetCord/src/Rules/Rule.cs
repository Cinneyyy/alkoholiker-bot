using System.Text.Json.Serialization;

namespace src.Rules;

public readonly record struct Rule()
{
    public required string name { get; init; }
    public required Predicate predicate { get; init; }
    public required Reply[] replies { get; init; }
    public required bool @break { get; init; }
    public i32 order { get; init; } = 0;
    public bool useRandomReply { get; init; } = true;
    [JsonIgnore] public Reply randomReply
    {
        get
        {
            if(replies.Length == 0)
                return default;

            if(replies.Length == 1)
                return replies.First();

            f32 weightedRange = replies.Sum(r => r.weight);
            f32 randVal = Random.Shared.NextSingle() * weightedRange;
            f32 cum = 0f;

            for(i32 i = 0; i < replies.Length; i++)
            {
                cum += replies[i].weight;

                if(randVal < cum)
                    return replies[i];
            }

            return replies[^1];
        }
    }
}
