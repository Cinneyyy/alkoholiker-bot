using System.Collections.Generic;

namespace src.Rules;

public sealed class RuleCollectionJsonParsable
{
    public Dictionary<string, string> emoji { get; init; } = [];
    public Dictionary<string, u64> channels { get; init; } = [];
    public Dictionary<string, u64> users { get; init; } = [];
    public i32 defaultCooldownSeconds { get; init; } = 0;
    public Rule[] rules { get; init; } = [];
}