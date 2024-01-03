using System.Collections.Immutable;
using Violet.Language.Symbols;
using Violet.Language.Syntax;

namespace Violet.Language.Binding;

/// <summary>
/// Represents a scope in which symbols are declared.
/// </summary>
class BoundScope(BoundScope? parent)
{
    readonly Dictionary<string, Symbol> _symbols = new();
    public BoundScope? Parent => parent;

    public bool TryDeclareFunction(FunctionSymbol function)
        => TryDeclare(function);

    public ImmutableArray<FunctionSymbol> GetDeclaredFunctions()
        => GetDeclared<FunctionSymbol>();

    bool TryDeclare(Symbol symbol)
    {
        if (_symbols.ContainsKey(symbol.Name))
        {
            return false;
        }

        _symbols.Add(symbol.Name, symbol);
        return true;
    }

    ImmutableArray<T> GetDeclared<T>() where T : Symbol
        => _symbols.Values.OfType<T>().ToImmutableArray();

    public Symbol? TryResolve(string identifierText)
    {
        if (_symbols.TryGetValue(identifierText, out var symbol))
        {
            return symbol;
        }

        return Parent?.TryResolve(identifierText);
    }
}
