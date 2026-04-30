using System;
using System.Text;
using System.Text.Json.Serialization;

namespace src.Rules;

public readonly record struct Predicate()
{
    [JsonIgnore] public i32 predicateId { get; } = Random.Shared.Next();
    public f32 chance { get; init; } = 1f;
    public string regex { get; init; } = null;
    public bool regexIgnoreCase { get; init; }
    public string channel { get; init; } = null;
    public string user { get; init; } = null;
    public i32 cooldownSeconds { get; init; } = -1;
    public bool? refMessage { get; init; } = null;
    public bool? hasImage { get; init; } = null;


    public override string ToString()
        => ToString("");
    public string ToString(string lnPrefix)
    {
        StringBuilder sb = new();

        if(chance != 1f) sb.AppendLine($"{lnPrefix}chance: {chance}");
        if(!string.IsNullOrEmpty(regex)) sb.AppendLine($"{lnPrefix}regex: /{regex}/ (ignore case: {regexIgnoreCase})");
        if(!string.IsNullOrEmpty(user)) sb.AppendLine($"{lnPrefix}user: {user}");
        if(!string.IsNullOrEmpty(channel)) sb.AppendLine($"{lnPrefix}channel: {channel}");
        sb.AppendLine($"{lnPrefix}cooldown: {GetActualCooldown(RuleMgr.rules)}s{(GetActualCooldown(RuleMgr.rules) == RuleMgr.rules.defaultCooldownSeconds ? " (default)" : "")}");
        if(refMessage is not null) sb.AppendLine($"{lnPrefix}is reply: {refMessage.Value}");
        if(hasImage is not null) sb.AppendLine($"{lnPrefix}has image: {hasImage.Value}");

        return sb.ToString();
    }

    public u32 GetActualCooldown(RuleCollection ruleColl)
        => cooldownSeconds < 0 ? ruleColl.defaultCooldownSeconds : (u32)cooldownSeconds;
}