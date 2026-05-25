namespace src.Rules.Language.Ast;

public sealed record class ConstantNode
    (ValueType type, string value) : AstNode();