namespace src.Rules.Language.Ast;

public sealed record class BinaryOperatorNode
    (string oper, AstNode lhs, AstNode rhs) : AstNode();