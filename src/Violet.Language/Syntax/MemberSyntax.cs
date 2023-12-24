namespace Violet.Language.Syntax;

public abstract record MemberSyntax(SyntaxTree SyntaxTree) : SyntaxNode(SyntaxTree);

public record GlobalStatementSyntax(SyntaxTree SyntaxTree, StatementSyntax Statement) : MemberSyntax(SyntaxTree)
{
    public override SyntaxKind Kind => SyntaxKind.GlobalStatement;
    public override IEnumerable<SyntaxNode> GetChildren()
    {
        yield return Statement;
    }
}
