namespace src.Rules.Language.Ast;

public sealed record class CodeBlockNode
    (ValueType returnType, AstNode[] nodes) : AstNode();