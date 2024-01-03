using System.Reflection;

namespace Violet.Language.Symbols;

static class BuiltinFunctions
{
    public static readonly FunctionSymbol Print = new("Print", TypeSymbol.Void);

    public static IEnumerable<FunctionSymbol> GetAll()
        => typeof(BuiltinFunctions).GetFields(BindingFlags.Public | BindingFlags.Static)
            .Where(f => f.FieldType == typeof(FunctionSymbol))
            .Select(f => (FunctionSymbol) f.GetValue(null)!);
}
