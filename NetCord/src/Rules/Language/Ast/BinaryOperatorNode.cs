namespace src.Rules.Language.Ast;

public sealed record class BinaryOperatorNode
    (string oper, AstNode lhs, AstNode rhs) : AstNode()
{
    public override Value Eval()
    {
        Value lv = lhs.Eval();
        Value rv = rhs.Eval();

        switch(oper)
        {
            case "&": return new(ValueType.Bool, (bool)lv.value & (bool)rv.value);
            case "|": return new(ValueType.Bool, (bool)lv.value | (bool)rv.value);
            default: throw new($"Invalid operation ({oper}).");
        }
    }
}