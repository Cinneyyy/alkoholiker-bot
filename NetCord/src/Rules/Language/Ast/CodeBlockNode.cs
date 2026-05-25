namespace src.Rules.Language.Ast;

public sealed record class CodeBlockNode
    (AstNode[] nodes) : AstNode()
{
    public override Value Eval()
        => new(ValueType.CodeBlock, this);

    public void Execute()
    {
        foreach(AstNode node in nodes)
            node.Eval();
    }
}