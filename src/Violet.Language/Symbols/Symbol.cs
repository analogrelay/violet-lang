namespace Violet.Language.Symbols;

public abstract record Symbol(string Name)
{
    public abstract SymbolKind Kind { get; }
}

public enum SymbolKind
{
    Function,
    Type,
}
