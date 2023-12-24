namespace Violet.Language.Syntax;

public abstract record StatementSyntax(SyntaxTree SyntaxTree): SyntaxNode(SyntaxTree);

public record ExpressionStatementSyntax(SyntaxTree SyntaxTree, ExpressionSyntax Expression) : StatementSyntax(SyntaxTree)
{
    public override SyntaxKind Kind => SyntaxKind.ExpressionStatement;

    public override IEnumerable<SyntaxNode> GetChildren()
    {
        yield return Expression;
    }
}
