using System;
using System.Linq;
using System.Text;

namespace src.Rules;

public readonly record struct Rule()
{
    public string name { get; init; }
    public Predicate predicate { get; init; }
    public bool @break { get; init; } = true;
    public Reply[] replies { get; init; }
    public Reply reply
    {
        get => replies.First();
        init => replies = [value];
    }
    public Reply randomReply
    {
        get
        {
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


    public override string ToString()
        => ToString("");
    public string ToString(string lnPrefix)
    {
        StringBuilder sb = new($"{lnPrefix}name: \"{name}\"\n");
        sb.AppendLine($"{lnPrefix}predicate:");
        sb.Append(predicate.ToString("  " + lnPrefix));

        if(replies.Length == 1)
        {
            sb.AppendLine($"{lnPrefix}reply:");
            sb.Append(reply.ToString("  " + lnPrefix));
        }
        else
        {
            sb.AppendLine($"{lnPrefix}replies:");

            for(i32 i = 0; i < replies.Length; i++)
            {
                sb.AppendLine($"  {lnPrefix}{i+1}.");
                sb.Append(replies[i].ToString("    " + lnPrefix));
            }
        }

        if(@break) sb.Append($"{lnPrefix}break\n");

        return sb.ToString();
    }
}
