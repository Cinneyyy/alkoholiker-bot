using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace src.Rules;

public static class ScriptLoader
{
    public static Assembly Load(string[] filePaths)
    {
        IEnumerable<SyntaxTree> syntaxTrees = filePaths
            .Select(File.ReadAllText)
            .Select(CSharpSyntaxTree.ParseText);
    }
}