using System.Text;

namespace src.Rules;

public readonly record struct Poll()
{
    public TextEmojiPair question { get; init; }
    public TextEmojiPair[] answers { get; init; }
    public bool multiselect { get; init; }
    public u32 hours { get; init; } = 1;


    public override string ToString()
        => ToString("");
    public string ToString(string lnPrefix)
    {
        StringBuilder sb = new();

        sb.AppendLine($"{lnPrefix}question: {question}");
        sb.AppendLine($"{lnPrefix}answers: {string.Join(", ", answers)}");
        sb.AppendLine($"{lnPrefix}multiselect: {multiselect}");
        sb.AppendLine($"{lnPrefix}duration: {hours}h");

        return sb.ToString();
    }
}