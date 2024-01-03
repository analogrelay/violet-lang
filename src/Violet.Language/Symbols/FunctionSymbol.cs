using Violet.Language.Syntax;

namespace Violet.Language.Symbols;

public record FunctionSymbol(
    string Name,
    TypeSymbol Type,
    FunctionDeclarationSyntax? Declaration = null): Symbol(Name)
{
    public override SymbolKind Kind => SymbolKind.Function;
}
