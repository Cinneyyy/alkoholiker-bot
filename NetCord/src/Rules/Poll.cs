namespace src.Rules;

public readonly record struct Poll()
{
    public TextEmojiPair question { get; init; }
    public TextEmojiPair[] answers { get; init; }
    public bool multiselect { get; init; }
    public u32 hours { get; init; } = 1u;
}
