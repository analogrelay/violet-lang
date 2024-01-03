namespace Violet.Language.Syntax;

public abstract record MemberSyntax(SyntaxTree SyntaxTree) : SyntaxNode(SyntaxTree);

public record FunctionDeclarationSyntax(
    SyntaxTree SyntaxTree,
    SyntaxToken FunKeyword,
    SyntaxToken Identifier,
    SyntaxToken OpenParenthesisToken,
    SyntaxToken CloseParenthesisToken,
    BlockStatementSyntax Body,
    SyntaxToken EndToken,
    SyntaxToken EndFunKeyword): MemberSyntax(SyntaxTree)
{
    public override SyntaxKind Kind => SyntaxKind.FunctionDeclaration;

    public override IEnumerable<SyntaxNode> GetChildren()
    {
        yield return FunKeyword;
        yield return Identifier;
        yield return OpenParenthesisToken;
        yield return CloseParenthesisToken;
        yield return Body;
        yield return EndToken;
        yield return EndFunKeyword;
    }
}
