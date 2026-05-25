namespace src.Rules.Language;

public sealed class Lexer(string code)
{
    public const string BinaryOperators = "&|";
    public const string UnaryOperators = ":@#!";

    private readonly string code = code;
    private i32 index = 0;
    private List<Token> tokens = [];
    private List<char> current = [];


    private bool isFinished => index >= code.Length;


    public Token[] Analyze()
    {
        index = 0;
        tokens.Clear();
        current.Clear();

        while(ReadChar(out char chr))
        {
            const string InterruptingChars = "{}()[];," + BinaryOperators + UnaryOperators;

            switch(chr)
            {
                case var _ when InterruptingChars.Contains(chr):
                {
                    FinishToken();
                    tokens.Add(chr switch
                    {
                        '{' => new(Token.Type.OpenCurly, null),
                        '}' => new(Token.Type.CloseCurly, null),
                        '(' => new(Token.Type.OpenParen, null),
                        ')' => new(Token.Type.CloseParen, null),
                        '[' => new(Token.Type.OpenSquare, null),
                        ']' => new(Token.Type.CloseSquare, null),
                        ';' => new(Token.Type.Semi, null),
                        ',' => new(Token.Type.Comma, null),
                        _ when BinaryOperators.Contains(chr) => new(Token.Type.BinaryOperator, chr.ToString()),
                        _ when UnaryOperators.Contains(chr) => new(Token.Type.UnaryOperator, chr.ToString()),
                        _ => throw new($"Invalid operator (\"{chr}\").")
                    });
                    break;
                }
                case var _ when char.IsWhiteSpace(chr):
                {
                    FinishToken();
                    break;
                }
                default:
                {
                    PushTokenChar(chr);
                    break;
                }
            }
        }

        FinishToken();
        return tokens.ToArray();
    }
    

    private void FinishToken()
    {
        if(current.Count == 0)
            return;

        string value = string.Join(null, current);
        current.Clear();

        tokens.Add(value switch
        {
            _ when value.All(IsNumberChar) => new(Token.Type.Number, value),
            _ when value.All(IsWordChar) => new(Token.Type.Word, value),
            _ => throw new($"Invalid token (\"{value}\")")
        });
    }

    private void PushTokenChar(char chr)
        => current.Add(chr);

    private bool ReadChar(out char chr)
    {
        if(isFinished)
        {
            chr = default;
            return false;
        }

        chr = code[index++];
        return true;
    }


    private static bool IsNumberChar(char chr)
        => char.IsDigit(chr) || "+-.".Contains(chr);

    private static bool IsWordChar(char chr)
        => char.IsLetterOrDigit(chr) || chr == '_';
}