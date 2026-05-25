using src.Rules.Language.Ast;

namespace src.Rules.Language;

public sealed class Parser(Token[] tokens)
{
    private readonly Token[] tokens = tokens;


    public AstNode Parse()
        => ParseRegion(0, out _);


    private AstNode ParseRegion(i32 first, out i32 last)
    {
        if(Get(first).type == Token.Type.Word)
        {
            if(Get(first+1).type == Token.Type.OpenParen)
            {
                last = first+1;
                i32 parenBalance = 1;

                while(parenBalance > 0)
                {
                    last++;

                    if(Get(last).type == Token.Type.OpenParen) parenBalance++;
                    else if(Get(last).type == Token.Type.CloseParen) parenBalance--;
                }

                // first+1: index of opening paren; end: index of closing paren.

                i32 curlyBalance = 0;
                i32 squareBalance = 0;
                parenBalance = 0;

                List<(i32 first, i32 last)> parameters = [];
                i32 paramStart = first+2;
                for(i32 i = first+2; i <= last; i++)
                {
                    if(i == last)
                    {
                        if(paramStart < i)
                        {
                            parameters.Add((paramStart, i-1));
                            break;
                        }

                        break;
                    }

                    switch(Get(i).type)
                    {
                        case Token.Type.OpenParen: parenBalance++; break;
                        case Token.Type.CloseParen: parenBalance--; goto case Token.Type.Semi;
                        case Token.Type.OpenCurly: curlyBalance++; break;
                        case Token.Type.CloseCurly: curlyBalance--; break;                       
                        case Token.Type.OpenSquare: squareBalance++; break;
                        case Token.Type.CloseSquare: squareBalance--; break;
                        case Token.Type.Semi:
                        {
                            if(parenBalance == 0 && curlyBalance == 0 && squareBalance == 0)
                            {
                                parameters.Add((paramStart, i-1));
                                paramStart = i+1;
                            }

                            break;
                        }
                    }
                }

                return new FuncCallNode(
                    Get(first).value, 
                    parameters
                        .Select(p => ParseRegion(p.first, out _))
                        .Where(n => n is not InvalidNode)
                        .ToArray()
                );
            }
            else
            {
                last = first+1;
                return new ConstantNode(ValueType.Str, Get(first).value);
            }
        }
        else if(Get(first).type == Token.Type.OpenCurly)
        {
            i32 balance = 1;
            last = first+1;

            while(balance > 0)
            {
                Token token = Get(last++);

                if(token.type == Token.Type.OpenCurly)
                    balance++;
                else if(token.type == Token.Type.CloseCurly)
                    balance--;
            }

            List<AstNode> nodes = [];
            for(i32 i = first+1; i < last; i++)
            {
                nodes.Add(ParseRegion(i, out i32 endOfNode));
                i = endOfNode;
            }

            return new CodeBlockNode(ValueType.CodeBlock, nodes.Where(n => n is not InvalidNode).ToArray());
        }
        else if(Get(first).type == Token.Type.OpenSquare)
        {
            i32 balance = 1;
            last = first+1;

            while(balance > 0)
            {
                Token token = Get(last++);

                if(token.type == Token.Type.OpenSquare)
                    balance++;
                else if(token.type == Token.Type.CloseSquare)
                    balance--;
            }

            List<AstNode> exprNodes = [];
            for(i32 i = first+1; i < last-1; i++)
            {
                if(Get(i).type == Token.Type.BinaryOperator)
                    exprNodes.Add(new BinaryOperatorNode(Get(i).value, default, default));
                else
                {
                    exprNodes.Add(ParseRegion(i, out i32 exprEnd));
                    i = exprEnd;
                }
            }

            if(exprNodes.Count == 1)
                return exprNodes.First();

            AstNode eval(i32 index)
            {
                if(index >= exprNodes.Count)
                    return exprNodes[index-1];
                
                return new BinaryOperatorNode((exprNodes[index] as BinaryOperatorNode).oper, exprNodes[index-1], eval(index+2));
            }

            return eval(1) as BinaryOperatorNode;
        }
        else if(Get(first).type == Token.Type.UnaryOperator)
        {
            AstNode target = ParseRegion(first+1, out last);
            return new UnaryOperatorNode(Get(first).value, target);
        }
        else
        {
            last = first+1;    
            return new InvalidNode();
        }
    }

    private Token Get(i32 index)
        => index >= tokens.Length
            ? new(Token.Type.Invalid, null)
            : tokens[index];
}