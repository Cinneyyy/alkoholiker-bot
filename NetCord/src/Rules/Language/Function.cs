namespace src.Rules.Language;

public readonly record struct Function
    (string name, Func<Value[], Value> impl);