namespace src.Rules;

public readonly record struct UserAlias()
{
    public string alias { get; init; }
    public u64 id { get; init; }

    public string mention => $"<@{id}>";
}