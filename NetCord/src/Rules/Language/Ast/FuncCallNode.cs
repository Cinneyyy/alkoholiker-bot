namespace src.Rules.Language.Ast;

public sealed record class FuncCallNode
    (string name, AstNode[] parameters) : AstNode();