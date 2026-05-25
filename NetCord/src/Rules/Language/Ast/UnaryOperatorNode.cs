namespace src.Rules.Language.Ast;

public sealed record class UnaryOperatorNode 
    (string oper, AstNode target) : AstNode()
{
    public override Value Eval()
        => oper switch
        {
            "!" => new(ValueType.Bool, !(bool)target.Eval().value),
            ":" => new(ValueType.Emoji, Runtime.GetEmoji(target.Eval().value)),
            "@" => new(ValueType.User, Runtime.GetUser(target.Eval())),
            "#" => new(ValueType.Channel, Runtime.GetChannel(target.Eval())),
            _ => throw new($"Invalid unary operator ({oper}).")
        };
}