namespace src.Rules.Language.Ast;

public sealed record class LogicBlockNode
    (AstNode tree) : AstNode()
{
    public override Value Eval()
        => tree.Eval();
}