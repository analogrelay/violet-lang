using System.Collections.Immutable;
using Violet.Language.Syntax;

namespace Violet.Language.Binding;

abstract record BoundStatement : BoundNode;

record BoundExpressionStatement(
    ExpressionStatementSyntax ExpressionStatementSyntax,
    BoundExpression Expression) : BoundStatement
{
    protected override IEnumerable<BoundNode> GetChildren()
    {
        yield return Expression;
    }
}

record BoundBlockStatement(
    BlockStatementSyntax BlockStatementSyntax,
    ImmutableArray<BoundStatement> Statements) : BoundStatement
{
    public override void FormatTo(TextWriter writer, int indent)
    {
        foreach(var statement in Statements)
        {
            statement.FormatTo(writer, indent);
        }
    }
}
