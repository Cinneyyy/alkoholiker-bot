namespace src.Rules.Language.Ast;

public sealed record class BinaryOperatorNode
    (string oper, AstNode lhs, AstNode rhs) : AstNode()
{
    public override Value Eval()
    {
        Value lv = lhs.Eval();
        Value rv = rhs.Eval();

        return oper switch
        {
            "&" => new(ValueType.Bool, (bool)lv.value & (bool)rv.value),
            "|" => new(ValueType.Bool, (bool)lv.value | (bool)rv.value),
            _ => throw new($"Invalid operation ({oper})."),
        };
    }
}