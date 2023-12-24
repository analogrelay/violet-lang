namespace Violet.Language.Syntax;

public class TokenizerTests
{
    [Theory]
    [MemberData(nameof(GetTokenExemplars))]
    public void CanParseToken(SyntaxKind kind, string exemplar)
    {
        var tokens = SyntaxTree.ParseTokens(exemplar, out var diagnostics);
        Assert.Empty(diagnostics);
        var token = Assert.Single(tokens);
        Assert.Equal(kind, token.Kind);
        Assert.Equal(exemplar, token.Text);
    }

    [Theory]
    [MemberData(nameof(GetSeparatorExemplars))]
    public void CanParseSeparator(SyntaxKind kind, string exemplar)
    {
        var tokens = SyntaxTree.ParseTokens(exemplar, out var diagnostics, includeEndOfFile: true);
        Assert.Empty(diagnostics);
        var token = Assert.Single(tokens);
        var trivia = Assert.Single(token.LeadingTrivia);
        Assert.Equal(kind, trivia.Kind);
        Assert.Equal(exemplar, trivia.Text);
    }

    [Theory]
    [MemberData(nameof(GetTokenPairExemplars))]
    public void CanParseTokenPairs(SyntaxKind leftKind, string leftExemplar, SyntaxKind rightKind, string rightExemplar)
    {
        var exemplar = leftExemplar + rightExemplar;
        var tokens = SyntaxTree.ParseTokens(exemplar, out var diagnostics);
        Assert.Empty(diagnostics);
        Assert.Collection(tokens,
            left =>
            {
                Assert.Equal(leftKind, left.Kind);
                Assert.Equal(leftExemplar, left.Text);
            },
            right =>
            {
                Assert.Equal(rightKind, right.Kind);
                Assert.Equal(rightExemplar, right.Text);
            });
    }

    [Theory]
    [InlineData("\"Hello", "\"Hello", "Hello")]
    [InlineData("\"Hello\nWorld", "\"Hello", "Hello")]
    [InlineData("\"Hello\r\nWorld", "\"Hello", "Hello")]
    public void UnterminatedStringLiteral(string input, string text, string value)
    {
        var tokens = SyntaxTree.ParseTokens(input, out var diagnostics);
        var token = tokens.First();
        Assert.Equal(SyntaxKind.StringToken, token.Kind);
        Assert.Equal(text, token.Text);
        Assert.Equal(value, token.Value);
        var diagnostic = Assert.Single(diagnostics);
        Assert.Same(DiagnosticDescriptors.UnterminatedStringLiteral, diagnostic.Descriptor);
        Assert.Equal(0, diagnostic.Location.Span.Start);
        Assert.Equal(text.Length, diagnostic.Location.Span.End);
        Assert.Empty(diagnostic.MessageArgs);
    }

    [Theory]
    [MemberData(nameof(GetTokenPairWithSeparatorExemplars))]
    public void CanParseTokenPairsWithSeparators(
        SyntaxKind leftKind, string leftExemplar,
        SyntaxKind separatorKind, string separatorExemplar,
        SyntaxKind rightKind, string rightExemplar)
    {
        var exemplar = leftExemplar + separatorExemplar + rightExemplar;
        var tokens = SyntaxTree.ParseTokens(exemplar, out var diagnostics);
        Assert.Empty(diagnostics);
        Assert.Collection(tokens,
            left =>
            {
                Assert.Equal(leftKind, left.Kind);
                Assert.Equal(leftExemplar, left.Text);

                var trailingTrivia = Assert.Single(left.TrailingTrivia);
                Assert.Equal(separatorKind, trailingTrivia.Kind);
                Assert.Equal(separatorExemplar, trailingTrivia.Text);
            },
            right =>
            {
                Assert.Equal(rightKind, right.Kind);
                Assert.Equal(rightExemplar, right.Text);
            });
    }

    // Gets exemplars for all tokens
    public static TheoryData<SyntaxKind, string> GetTokenExemplars()
        => GetTokenExemplarData().ToTheoryData();

    // Gets exemplars for pairs of tokens
    public static TheoryData<SyntaxKind, string, SyntaxKind, string> GetTokenPairExemplars()
        => GetTokenPairExemplarData().ToTheoryData();

    // Gets exemplars for all separators
    public static TheoryData<SyntaxKind, string> GetSeparatorExemplars()
        => GetSeparatorData().ToTheoryData();

    public static TheoryData<SyntaxKind, string, SyntaxKind, string, SyntaxKind, string>
        GetTokenPairWithSeparatorExemplars() => GetTokenPairWithSeparatorData().ToTheoryData();

    static IEnumerable<(SyntaxKind, string)> GetTokenExemplarData()
    {
        var staticTokens = Enum.GetValues<SyntaxKind>()
            .Select(k => (k, text: SyntaxFacts.GetText(k)!))
            .Where(t => t.text != null!);

        var dynamicTokens = new[]
        {
            // Exemplars for dynamic tokens like 'string', 'identifier', etc.
            (SyntaxKind.StringToken, "\"Hello, World\""),
            (SyntaxKind.StringToken, "\"Hello \\\"World\\\"\""),
            (SyntaxKind.NumberToken, "1234"),
            (SyntaxKind.NumberToken, "-1234"),
            (SyntaxKind.NumberToken, "+1234"),
            (SyntaxKind.IdentifierToken, "a_cool_variable"),
            (SyntaxKind.IdentifierToken, "with_a_1_in_it"),
        };

        return staticTokens.Concat(dynamicTokens);
    }

    static IEnumerable<(SyntaxKind, string)> GetSeparatorData()
    {
        return new[]
        {
            (SyntaxKind.WhitespaceTrivia, " "),
            (SyntaxKind.WhitespaceTrivia, "  "),
            (SyntaxKind.NewlineTrivia, "\n"),
            (SyntaxKind.NewlineTrivia, "\r"),
            (SyntaxKind.NewlineTrivia, "\r\n"),
            (SyntaxKind.BlockCommentTrivia, "/**/"),
        };
    }

    static IEnumerable<(SyntaxKind, string, SyntaxKind, string)> GetTokenPairExemplarData()
    {
        var data = new TheoryData<SyntaxKind, string, SyntaxKind, string>();
        foreach (var (leftKind, leftText) in GetTokenExemplarData())
        {
            foreach (var (rightKind, rightText) in GetTokenExemplarData())
            {
                // Some tokens can't be next to each other, like '+' and '='.
                // If those two characters are next to each other, it would be a single '+=' token.
                if (!RequiresSeparator(leftKind, rightKind))
                {
                    yield return (leftKind, leftText, rightKind, rightText);
                }
            }
        }
    }

    static IEnumerable<(SyntaxKind, string, SyntaxKind, string, SyntaxKind, string)> GetTokenPairWithSeparatorData()
    {
        var data = new TheoryData<SyntaxKind, string, SyntaxKind, string>();
        foreach (var (leftKind, leftText) in GetTokenExemplarData())
        {
            foreach (var (rightKind, rightText) in GetTokenExemplarData())
            {
                // Some tokens can't be next to each other, like '+' and '='.
                // If those two characters are next to each other, it would be a single '+=' token.
                if (RequiresSeparator(leftKind, rightKind))
                {
                    foreach (var (sepKind, sepText) in GetSeparatorData())
                    {
                        // Make sure the separator doesn't require a separator itself.
                        if (!RequiresSeparator(leftKind, sepKind) && !RequiresSeparator(sepKind, rightKind))
                        {
                            yield return (leftKind, leftText, sepKind, sepText, rightKind, rightText);
                        }
                    }
                }
            }
        }
    }

    static readonly HashSet<(SyntaxKind, SyntaxKind)> CompoundTokens = new()
    {
        (SyntaxKind.AmpersandToken, SyntaxKind.AmpersandToken),
        (SyntaxKind.AmpersandToken, SyntaxKind.AmpersandAmpersandToken),
        (SyntaxKind.AmpersandToken, SyntaxKind.AmpersandEqualToken),
        (SyntaxKind.AmpersandToken, SyntaxKind.EqualsToken),
        (SyntaxKind.AmpersandToken, SyntaxKind.EqualsEqualsToken),
        (SyntaxKind.AmpersandAmpersandToken, SyntaxKind.EqualsToken),
        (SyntaxKind.PipeToken, SyntaxKind.PipeToken),
        (SyntaxKind.PipeToken, SyntaxKind.PipePipeToken),
        (SyntaxKind.PipeToken, SyntaxKind.PipeEqualToken),
        (SyntaxKind.PipeToken, SyntaxKind.EqualsToken),
        (SyntaxKind.PipeToken, SyntaxKind.EqualsEqualsToken),
        (SyntaxKind.PipePipeToken, SyntaxKind.EqualsToken),
        (SyntaxKind.HatToken, SyntaxKind.HatToken),
        (SyntaxKind.HatToken, SyntaxKind.HatEqualToken),
        (SyntaxKind.HatToken, SyntaxKind.EqualsToken),
        (SyntaxKind.HatToken, SyntaxKind.EqualsEqualsToken),
        (SyntaxKind.PlusToken, SyntaxKind.EqualsToken),
        (SyntaxKind.PlusToken, SyntaxKind.EqualsEqualsToken),
        (SyntaxKind.MinusToken, SyntaxKind.EqualsToken),
        (SyntaxKind.MinusToken, SyntaxKind.EqualsEqualsToken),
        (SyntaxKind.StarToken, SyntaxKind.EqualsToken),
        (SyntaxKind.StarToken, SyntaxKind.EqualsEqualsToken),
        (SyntaxKind.SlashToken, SyntaxKind.EqualsToken),
        (SyntaxKind.SlashToken, SyntaxKind.EqualsEqualsToken),
        (SyntaxKind.SlashToken, SyntaxKind.SlashEqualsToken),
        (SyntaxKind.SlashToken, SyntaxKind.StarEqualsToken),
        (SyntaxKind.PercentToken, SyntaxKind.EqualsToken),
        (SyntaxKind.PercentToken, SyntaxKind.EqualsEqualsToken),
        (SyntaxKind.EqualsToken, SyntaxKind.EqualsToken),
        (SyntaxKind.EqualsToken, SyntaxKind.EqualsEqualsToken),
        (SyntaxKind.BangToken, SyntaxKind.EqualsToken),
        (SyntaxKind.BangToken, SyntaxKind.EqualsEqualsToken),
        (SyntaxKind.GreaterThanToken, SyntaxKind.EqualsToken),
        (SyntaxKind.GreaterThanToken, SyntaxKind.EqualsEqualsToken),
        (SyntaxKind.LessThanToken, SyntaxKind.EqualsToken),
        (SyntaxKind.LessThanToken, SyntaxKind.EqualsEqualsToken),
        (SyntaxKind.IdentifierToken, SyntaxKind.NumberToken),
        (SyntaxKind.MinusToken, SyntaxKind.NumberToken), // -123 is just a number
        (SyntaxKind.PlusToken, SyntaxKind.NumberToken), // +123 is just a number
        (SyntaxKind.SlashToken, SyntaxKind.StarToken), // '/*' is a multi-line comment
        (SyntaxKind.SlashToken, SyntaxKind.SlashToken), // '//' is a multi-line comment
        (SyntaxKind.SlashToken, SyntaxKind.BlockCommentTrivia), // '/*' is a multi-line comment
        (SyntaxKind.SlashToken, SyntaxKind.SingleLineCommentTrivia), // '//' is a multi-line comment
    };
    static bool RequiresSeparator(SyntaxKind left, SyntaxKind right)
    {
        var leftIsKeywordOrIdentifier = left.IsKeyword() || left is SyntaxKind.IdentifierToken;
        var rightIsKeywordOrIdentifier = right.IsKeyword() || right is SyntaxKind.IdentifierToken;

        if (left == right)
        {
            return true;
        }

        if(leftIsKeywordOrIdentifier && rightIsKeywordOrIdentifier)
        {
            return true;
        }

        if (CompoundTokens.Contains((left, right)))
        {
            return true;
        }

        return false;
    }
}
