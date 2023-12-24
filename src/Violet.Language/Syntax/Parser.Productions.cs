using System.Collections.Immutable;
using System.Data;
using System.Net.Http.Headers;

namespace Violet.Language.Syntax;

// Contains the actual parsing logic, or "productions", for the language.

partial class Parser
{
    CompilationUnitSyntax CompilationUnit()
    {
        var members = ImmutableArray.CreateBuilder<MemberSyntax>();
        while (!At(SyntaxKind.EndOfFileMarker))
        {
            var stmt = Statement();
            members.Add(new GlobalStatementSyntax(syntaxTree, stmt));
        }

        return new CompilationUnitSyntax(syntaxTree, members.ToImmutableArray());
    }

    StatementSyntax Statement()
    {
        var expr = Expression();
        return new ExpressionStatementSyntax(syntaxTree, expr);
    }

    ExpressionSyntax Expression()
        => BinaryExpression();

    ExpressionSyntax BinaryExpression(int parentPrecedence = 0, bool inComparison = false)
    {
        // Parse the left side as a primary expression.
        ExpressionSyntax left;
        var unaryPrecedence = Current.Kind.GetUnaryOperatorPrecedence();
        if (unaryPrecedence != 0 && unaryPrecedence >= parentPrecedence)
        {
            // Parse a unary expression
            var operatorToken = NextToken();
            var operand = BinaryExpression(unaryPrecedence);
            left = new UnaryExpressionSyntax(syntaxTree, operatorToken, operand);
        }
        else
        {
            left = PrimaryExpression();
        }

        // Parse higher-precedence binary expressions.
        while (true)
        {
            // Before we continue, we check if we're chaining comparison operators.
            if (Current.Kind.IsComparisonOperator() && inComparison)
            {
                // We are. Continue to parse but emit a diagnostic because this is a syntax error
                // (we don't allow chaining comparison operators).
                _diagnostics.Add(
                    DiagnosticDescriptors.ComparisonsCannotBeChained,
                    Current.Location);
            }

            var precedence = Current.Kind.GetBinaryOperatorPrecedence();
            if (precedence == 0 || precedence <= parentPrecedence)
            {
                // Not a binary operator, or not one with precedence greater than the parent.
                break;
            }

            var operatorToken = NextToken();
            var right = BinaryExpression(precedence, operatorToken.Kind.IsComparisonOperator());
            left = new BinaryExpressionSyntax(syntaxTree, left, operatorToken, right);
        }

        return left;
    }

    /// <summary>
    /// Parses a "primary" expression, which is a low-level building block for all expressions.
    /// For example, a literal, a parenthesized expression, or a name.
    /// </summary>
    /// <returns></returns>
    ExpressionSyntax PrimaryExpression()
    {
        if (Try(SyntaxKind.NumberToken, SyntaxKind.StringToken) is {} literalToken)
        {
            return new LiteralExpressionSyntax(syntaxTree, literalToken);
        }

        if (AtSequence(SyntaxKind.IdentifierToken, SyntaxKind.LeftParenthesisToken))
        {
            var identifier = NextToken();
            var lParen = NextToken();
            var arguments = ArgumentList();
            var rParen = Expect(SyntaxKind.RightParenthesisToken);
            return new CallExpressionSyntax(syntaxTree, identifier, lParen, arguments, rParen);
        }

        return new NameExpressionSyntax(
            syntaxTree,
            Expect(SyntaxKind.IdentifierToken));
    }

    SeparatedSyntaxList<ExpressionSyntax> ArgumentList()
    {
        var nodes = ImmutableArray.CreateBuilder<SyntaxNode>();

        // Parse until we are stopped by some condition in the loop,
        // we reach the right paren,
        // or we reach the end of the file.
        var continueParsing = true;
        while (continueParsing &&
               !At(SyntaxKind.RightParenthesisToken) &&
               !At(SyntaxKind.EndOfFileMarker))
        {
            var expression = Expression();
            nodes.Add(expression);

            if (Try(SyntaxKind.CommaToken) is {} comma)
            {
                nodes.Add(comma);
            }
            else
            {
                // We are done parsing arguments, the next token should be the right paren.
                continueParsing = false;
            }
        }

        return new(nodes.ToImmutableArray());
    }
}
