using System.Collections.Immutable;
using System.Diagnostics;
using Violet.Language.Symbols;
using Violet.Language.Syntax;

namespace Violet.Language.Binding;

abstract record BoundExpression(SyntaxNode Syntax): BoundNode
{
    public abstract TypeSymbol Type { get; }
    public virtual BoundConstant? ConstantValue => null;
    protected override string FormatSelf() => GetType().Name;
}

record BoundErrorExpression(SyntaxNode Syntax): BoundExpression(Syntax)
{
    public override TypeSymbol Type => TypeSymbol.Error;
}

record BoundLiteralExpression(SyntaxNode Syntax, object Value):
    BoundExpression(Syntax)
{
    public override TypeSymbol Type { get; } = GetTypeForValue(Value);
    public override BoundConstant? ConstantValue { get; } = new(Value);

    static TypeSymbol GetTypeForValue(object value)
    {
        if (value is string)
        {
            return TypeSymbol.String;
        }

        throw new UnreachableException($"Unknown literal type for value '{value}'");
    }
    protected override string FormatSelf() => $"{base.FormatSelf()} {FormatValue(Value)} : {Type.Format()}";

    static string FormatValue(object? value)
    {
        if (value is null)
        {
            return "<null>";
        }

        if (value is string s)
        {
            return $"\"{s.Unescape()}\"";
        }

        return value.ToString() ?? "<unknown>";
    }
}

record BoundCallExpression(SyntaxNode Syntax, FunctionSymbol Function, ImmutableArray<BoundExpression> Arguments):
    BoundExpression(Syntax)
{
    public override TypeSymbol Type => Function.Type;

    protected override string FormatSelf() => $"{base.FormatSelf()} {Function.Name} : {Function.Type.Format()}";

    protected override IEnumerable<BoundNode> GetChildren()
    {
        foreach(var argument in Arguments)
        {
            yield return argument;
        }
    }
}
