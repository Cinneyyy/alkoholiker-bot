namespace src.Rules.Language.Ast;

public sealed record class InvalidNode() : AstNode()
{
    public override Value Eval()
        => new(ValueType.Void, null);
}