using System.Collections.Immutable;

namespace Violet.Language.Syntax;

public class ParserTests
{
    [Theory]
    [MemberData(nameof(GetBinaryOperatorPairsData))]
    public void BinaryExpression_WithPrecedence(SyntaxKind op1, SyntaxKind op2)
    {
        var op1Precedence = op1.GetBinaryOperatorPrecedence();
        var op2Precedence = op2.GetBinaryOperatorPrecedence();
        var op1Text = op1.GetText().Require();
        var op2Text = op2.GetText().Require();
        var text = $"a {op1Text} b {op2Text} c";
        var (expression, _) = ParseExpression(text);

        if (op1Precedence >= op2Precedence)
        {
            // Operator 2 should be the root, because 'a op1 b' should be evaluated first.
            using var w = new TreeAsserter(expression);
            w.AssertNode(SyntaxKind.BinaryExpression, () =>
            {
                w.AssertNode(SyntaxKind.BinaryExpression, () =>
                {
                    w.AssertNode(SyntaxKind.NameExpression, SyntaxKind.IdentifierToken, "a");
                    w.AssertToken(op1, op1Text);
                    w.AssertNode(SyntaxKind.NameExpression, SyntaxKind.IdentifierToken, "b");
                });
                w.AssertToken(op2, op2Text);
                w.AssertNode(SyntaxKind.NameExpression, SyntaxKind.IdentifierToken, "c");
            });
        }
        else
        {
            // Operator 1 should be the root because 'b op2 c' should be evaluated first.
            using var w = new TreeAsserter(expression);
            w.AssertNode(SyntaxKind.BinaryExpression, () =>
            {
                w.AssertNode(SyntaxKind.NameExpression, SyntaxKind.IdentifierToken, "a");
                w.AssertToken(op1, op1Text);
                w.AssertNode(SyntaxKind.BinaryExpression, () =>
                {
                    w.AssertNode(SyntaxKind.NameExpression, SyntaxKind.IdentifierToken, "b");
                    w.AssertToken(op2, op2Text);
                    w.AssertNode(SyntaxKind.NameExpression, SyntaxKind.IdentifierToken, "c");
                });
            });
        }
    }

    [Theory]
    [MemberData(nameof(GetComparisonPairsData))]
    public void BinaryExpression_ComparisonsCannotChain(SyntaxKind op1, SyntaxKind op2)
    {
        var op1Text = op1.GetText().Require();
        var op2Text = op2.GetText().Require();
        var text = $"a {op1Text} b {op2Text} c";
        var (expression, diagnostics) = ParseExpression(text, true);

        var diag = Assert.Single(diagnostics);
        Assert.Equal(DiagnosticDescriptors.ComparisonsCannotBeChained, diag.Descriptor);
        Assert.Empty(diag.MessageArgs);
        Assert.Equal(new TextSpan(op1Text.Length + 20, op2Text.Length), diag.Location.Span);

        // Expect that the parser _did_ recover and still produced a valid expression.
        using var w = new TreeAsserter(expression);
        w.AssertNode(SyntaxKind.BinaryExpression, () =>
        {
            w.AssertNode(SyntaxKind.BinaryExpression, () =>
            {
                w.AssertNode(SyntaxKind.NameExpression, SyntaxKind.IdentifierToken, "a");
                w.AssertToken(op1, op1Text);
                w.AssertNode(SyntaxKind.NameExpression, SyntaxKind.IdentifierToken, "b");
            });
            w.AssertToken(op2, op2Text);
            w.AssertNode(SyntaxKind.NameExpression, SyntaxKind.IdentifierToken, "c");
        });
    }

    [Theory]
    [MemberData(nameof(GetUnaryBinaryOperatorPairsData))]
    public void UnaryExpression_WithPrecedence(SyntaxKind unary, SyntaxKind binary)
    {
        var unaryPrecedence = unary.GetUnaryOperatorPrecedence();
        var binaryPrecedence = binary.GetBinaryOperatorPrecedence();
        var unaryText = unary.GetText().Require();
        var binaryText = binary.GetText().Require();
        var text = $"{unaryText} a {binaryText} b";
        var (expression, _) = ParseExpression(text);

        if (unaryPrecedence >= binaryPrecedence)
        {
            // Unary operation must be conducted first, so it is lower in the tree
            using var w = new TreeAsserter(expression);
            w.AssertNode(SyntaxKind.BinaryExpression, () =>
            {
                w.AssertNode(SyntaxKind.UnaryExpression, () =>
                {
                    w.AssertToken(unary, unaryText);
                    w.AssertNode(SyntaxKind.NameExpression, SyntaxKind.IdentifierToken, "a");
                });
                w.AssertToken(binary, binaryText);
                w.AssertNode(SyntaxKind.NameExpression, SyntaxKind.IdentifierToken, "b");
            });
        }
        else
        {
            // Unary operation must be conducted last, so it is higher in the tree
            using var w = new TreeAsserter(expression);
            w.AssertNode(SyntaxKind.UnaryExpression, () =>
            {
                w.AssertToken(unary, unaryText);
                w.AssertNode(SyntaxKind.BinaryExpression, () =>
                {
                    w.AssertNode(SyntaxKind.NameExpression, SyntaxKind.IdentifierToken, "a");
                    w.AssertToken(binary, binaryText);
                    w.AssertNode(SyntaxKind.NameExpression, SyntaxKind.IdentifierToken, "b");
                });
            });
        }
    }

    (ExpressionSyntax, ImmutableArray<Diagnostic>) ParseExpression(string text, bool expectDiagnostics = false)
    {
        var actualText = $"""
                          fun Main()
                              {text};
                          end fun
                          """;

        var syntaxTree = SyntaxTree.Parse(actualText);
        if (!expectDiagnostics)
        {
            Assert.Empty(syntaxTree.Diagnostics);
        }

        var root = syntaxTree.Root;
        var member = Assert.Single(root.Members);
        var funDecl = Assert.IsType<FunctionDeclarationSyntax>(member);
        var stmt = Assert.Single(funDecl.Body.Statements);
        var exprStatement = Assert.IsType<ExpressionStatementSyntax>(stmt);
        return (exprStatement.Expression, syntaxTree.Diagnostics);
    }

    public static TheoryData<SyntaxKind, SyntaxKind> GetUnaryBinaryOperatorPairsData()
        => GetUnaryBinaryOperatorPairs().ToTheoryData();

    static IEnumerable<(SyntaxKind, SyntaxKind)> GetUnaryBinaryOperatorPairs()
    {
        foreach (var op1 in SyntaxFacts.UnaryOperators())
        {
            foreach (var op2 in SyntaxFacts.BinaryOperators())
            {
                yield return (op1, op2);
            }
        }
    }

    public static TheoryData<SyntaxKind, SyntaxKind> GetBinaryOperatorPairsData()
        => GetBinaryOperatorPairs().ToTheoryData();

    static IEnumerable<(SyntaxKind, SyntaxKind)> GetBinaryOperatorPairs()
    {
        foreach (var op1 in SyntaxFacts.BinaryOperators())
        {
            foreach (var op2 in SyntaxFacts.BinaryOperators())
            {
                // Comparison operators are special, we don't allow them to be chained.
                if (!op1.IsComparisonOperator() || !op2.IsComparisonOperator())
                {
                    yield return (op1, op2);
                }
            }
        }
    }

    public static TheoryData<SyntaxKind, SyntaxKind> GetComparisonPairsData()
        => GetComparisonPairs().ToTheoryData();

    static IEnumerable<(SyntaxKind, SyntaxKind)> GetComparisonPairs()
    {
        foreach (var op1 in SyntaxFacts.BinaryOperators().Where(o => o.IsComparisonOperator()))
        {
            foreach (var op2 in SyntaxFacts.BinaryOperators().Where(o => o.IsComparisonOperator()))
            {
                yield return (op1, op2);
            }
        }
    }
}
