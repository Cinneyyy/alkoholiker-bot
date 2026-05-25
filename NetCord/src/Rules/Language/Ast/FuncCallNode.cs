namespace src.Rules.Language.Ast;

public sealed record class FuncCallNode
    (string name, AstNode[] parameters) : AstNode()
{
    public override Value Eval()
        => Runtime.functions.Find(f => f.name == name).impl(parameters.Select(p => p.Eval()).ToArray());
}