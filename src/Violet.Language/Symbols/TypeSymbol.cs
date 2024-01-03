namespace Violet.Language.Symbols;

public record TypeSymbol(string Name): Symbol(Name)
{
    public static readonly TypeSymbol Error = new TypeSymbol("<error>");
    public static readonly TypeSymbol String = new TypeSymbol("string");
    public static readonly TypeSymbol Void = new TypeSymbol("void");

    public override SymbolKind Kind => SymbolKind.Type;

    public string Format() => Name;
}
