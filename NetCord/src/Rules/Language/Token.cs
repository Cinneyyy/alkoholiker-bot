namespace src.Rules.Language;

public readonly record struct Token
    (Token.Type type, string value)
{
    public enum Type : u8
    {
        Invalid,
        OpenParen,
        CloseParen,
        OpenCurly,
        CloseCurly,
        OpenSquare,
        CloseSquare,
        BinaryOperator,
        UnaryOperator,
        Number,
        Word,
        Comma,
        Semi
    }
}