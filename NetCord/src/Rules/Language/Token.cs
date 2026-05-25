namespace src.Rules.Language;

public readonly record struct Token
    (Token.Type type, string value)
{
    public enum Type : u8
    {
        OpenParen,
        CloseParen,
        OpenCurly,
        CloseCurly,
        Semi,
        Comma,
        Word
    }
}