namespace src.Rules.Language.Ast;

public abstract record class AstNode()
{
    public abstract Value Eval();
}