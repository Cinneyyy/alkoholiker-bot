namespace src.Rules;

public readonly record struct ChannelAlias()
{
    public string alias { get; init; }
    public u64 id { get; init; }
}