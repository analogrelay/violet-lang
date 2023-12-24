namespace Violet.Language.Syntax;

/// <summary>
/// Base class for nodes that represent an expression.
/// </summary>
/// <param name="SyntaxTree"></param>
public abstract record ExpressionSyntax(SyntaxTree SyntaxTree) : SyntaxNode(SyntaxTree);

public record UnaryExpressionSyntax(
        SyntaxTree SyntaxTree,
        SyntaxToken Operator,
        ExpressionSyntax Operand)
    : ExpressionSyntax(SyntaxTree)
{
    public override SyntaxKind Kind => SyntaxKind.UnaryExpression;

    public override IEnumerable<SyntaxNode> GetChildren()
    {
        yield return Operator;
        yield return Operand;
    }
}

public record BinaryExpressionSyntax(
        SyntaxTree SyntaxTree,
        ExpressionSyntax Left,
        SyntaxToken OperatorToken,
        ExpressionSyntax Right)
    : ExpressionSyntax(SyntaxTree)
{
    public override SyntaxKind Kind => SyntaxKind.BinaryExpression;

    public override IEnumerable<SyntaxNode> GetChildren()
    {
        yield return Left;
        yield return OperatorToken;
        yield return Right;
    }
}

public record LiteralExpressionSyntax(
        SyntaxTree SyntaxTree,
        SyntaxToken LiteralToken,
        object Value)
    : ExpressionSyntax(SyntaxTree)
{
    public override SyntaxKind Kind => SyntaxKind.LiteralExpression;

    public LiteralExpressionSyntax(SyntaxTree syntaxTree, SyntaxToken literalToken)
        : this(syntaxTree, literalToken, literalToken.Value.Require())
    {
    }

    public override IEnumerable<SyntaxNode> GetChildren()
    {
        yield return LiteralToken;
    }
}

public record CallExpressionSyntax(
    SyntaxTree SyntaxTree,
    SyntaxToken Identifier,
    SyntaxToken OpenParenthesisToken,
    SeparatedSyntaxList<ExpressionSyntax> Arguments,
    SyntaxToken RightParenthesisToken) : ExpressionSyntax(SyntaxTree)
{
    public override SyntaxKind Kind => SyntaxKind.CallExpression;

    public override IEnumerable<SyntaxNode> GetChildren()
    {
        yield return Identifier;
        yield return OpenParenthesisToken;
        foreach (var argument in Arguments.GetWithSeparators())
        {
            yield return argument;
        }
        yield return RightParenthesisToken;
    }
}

public record NameExpressionSyntax(SyntaxTree SyntaxTree, SyntaxToken IdentifierToken)
    : ExpressionSyntax(SyntaxTree)
{
    public override SyntaxKind Kind => SyntaxKind.NameExpression;

    public override IEnumerable<SyntaxNode> GetChildren()
    {
        yield return IdentifierToken;
    }
}
