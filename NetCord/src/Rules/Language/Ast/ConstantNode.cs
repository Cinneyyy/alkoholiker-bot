namespace src.Rules.Language.Ast;

public sealed record class ConstantNode
    (ValueType type, string value) : AstNode()
{
    public override Value Eval()
        => new(type, type switch
        {
            ValueType.Bool => value.Equals("true", StringComparison.OrdinalIgnoreCase),
            ValueType.Str => value,
            ValueType.Int => i64.Parse(value),
            ValueType.Float => f64.Parse(value),
            ValueType.Void => null,
            _ => throw new($"Invalid constant type ({type}).")
        });
}