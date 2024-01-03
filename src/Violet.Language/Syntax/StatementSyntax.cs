using System.Collections.Immutable;

namespace Violet.Language.Syntax;

public abstract record StatementSyntax(SyntaxTree SyntaxTree) : SyntaxNode(SyntaxTree);

public record ExpressionStatementSyntax(SyntaxTree SyntaxTree, ExpressionSyntax Expression, SyntaxToken SemicolonToken) : StatementSyntax(SyntaxTree)
{
    public override SyntaxKind Kind => SyntaxKind.ExpressionStatement;

    public override IEnumerable<SyntaxNode> GetChildren()
    {
        yield return Expression;
        yield return SemicolonToken;
    }
}

public record BlockStatementSyntax(
    SyntaxTree SyntaxTree,
    ImmutableArray<StatementSyntax> Statements) : StatementSyntax(SyntaxTree)
{
    public override SyntaxKind Kind => SyntaxKind.BlockStatement;

    public override IEnumerable<SyntaxNode> GetChildren()
    {
        foreach (var statement in Statements)
        {
            yield return statement;
        }
    }
}
