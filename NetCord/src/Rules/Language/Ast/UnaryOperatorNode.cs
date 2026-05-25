namespace src.Rules.Language.Ast;

public sealed record class UnaryOperatorNode 
    (string oper, AstNode target) : AstNode();