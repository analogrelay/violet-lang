namespace Violet.Language.Syntax;

public class SyntaxFactsTests
{
    [Fact]
    public void IsTokenReturnsTrueForAllTokens()
    {
        var tokensByHelper = new SortedSet<SyntaxKind>(AllKinds().Where(k => k.IsToken()));
        var tokensByName = new SortedSet<SyntaxKind>(Tokens());
        tokensByHelper.ExceptWith(tokensByName);
        Assert.Empty(tokensByHelper);

        // Validate that the other helpers return false for tokens
        Assert.DoesNotContain(tokensByName, k => k.IsKeyword());
        Assert.DoesNotContain(tokensByName, k => k.IsNode());
        Assert.DoesNotContain(tokensByName, k => k.IsTrivia());
        Assert.DoesNotContain(tokensByName, k => k.IsMarker());
    }

    [Fact]
    public void IsTriviaReturnsTrueForAllTrivia()
    {
        var triviaByHelper = new SortedSet<SyntaxKind>(AllKinds().Where(k => k.IsTrivia()));
        var triviaByName = new SortedSet<SyntaxKind>(Trivia());
        triviaByHelper.ExceptWith(triviaByName);
        Assert.Empty(triviaByHelper);

        // Validate that the other helpers return false for trivia
        Assert.DoesNotContain(triviaByName, k => k.IsKeyword());
        Assert.DoesNotContain(triviaByName, k => k.IsNode());
        Assert.DoesNotContain(triviaByName, k => k.IsToken());
        Assert.DoesNotContain(triviaByName, k => k.IsMarker());
    }

    [Fact]
    public void IsMarkerReturnsTrueForAllMarkers()
    {
        var markersByHelper = new SortedSet<SyntaxKind>(AllKinds().Where(k => k.IsMarker()));
        var markersByName = new SortedSet<SyntaxKind>(Markers());
        markersByHelper.ExceptWith(markersByName);
        Assert.Empty(markersByHelper);

        // Validate that the other helpers return false for markers
        Assert.DoesNotContain(markersByName, k => k.IsKeyword());
        Assert.DoesNotContain(markersByName, k => k.IsNode());
        Assert.DoesNotContain(markersByName, k => k.IsToken());
        Assert.DoesNotContain(markersByName, k => k.IsTrivia());
    }

    [Fact]
    public void IsKeywordReturnsTrueForAllKeywords()
    {
        var keywordsByHelper = new SortedSet<SyntaxKind>(AllKinds().Where(k => k.IsKeyword()));
        var keywordsByName = new SortedSet<SyntaxKind>(Keywords());
        keywordsByHelper.ExceptWith(keywordsByName);
        Assert.Empty(keywordsByHelper);

        // Validate that the other helpers return false for keywords
        Assert.DoesNotContain(keywordsByName, k => k.IsMarker());
        Assert.DoesNotContain(keywordsByName, k => k.IsNode());
        Assert.DoesNotContain(keywordsByName, k => k.IsToken());
        Assert.DoesNotContain(keywordsByName, k => k.IsTrivia());
    }

    [Fact]
    public void IsNodeReturnsTrueForAllOtherNodes()
    {
        var nodesByHelper = new SortedSet<SyntaxKind>(AllKinds().Where(k => k.IsNode()));
        var nodesByName = new SortedSet<SyntaxKind>(Nodes());
        nodesByHelper.ExceptWith(nodesByName);
        Assert.Empty(nodesByHelper);

        // Validate that the other helpers return false for nodes
        Assert.DoesNotContain(nodesByName, k => k.IsMarker());
        Assert.DoesNotContain(nodesByName, k => k.IsKeyword());
        Assert.DoesNotContain(nodesByName, k => k.IsToken());
        Assert.DoesNotContain(nodesByName, k => k.IsTrivia());
    }

    [Fact]
    public void GetTextHasValueForNonDynamicTokensAndKeywords()
    {
        var tokens = new SortedSet<SyntaxKind>(Tokens().Concat(Keywords()));

        // Remove dynamic tokens
        tokens.Remove(SyntaxKind.NumberToken);
        tokens.Remove(SyntaxKind.StringToken);
        tokens.Remove(SyntaxKind.IdentifierToken);

        // "Bad" token is just a placeholder for errors
        tokens.Remove(SyntaxKind.BadToken);

        var tokensWithText = new SortedSet<SyntaxKind>(AllKinds().Where(k => k.GetText() is not null));
        tokens.ExceptWith(tokensWithText);
        Assert.Empty(tokens);
    }

    static IEnumerable<SyntaxKind> AllKinds()
        => Enum.GetValues<SyntaxKind>();

    static IEnumerable<SyntaxKind> Tokens()
        => AllKinds().Where(e => e.ToString().EndsWith("Token"));

    static IEnumerable<SyntaxKind> Trivia()
        => AllKinds().Where(e => e.ToString().EndsWith("Trivia"));

    static IEnumerable<SyntaxKind> Markers()
        => AllKinds().Where(e => e.ToString().EndsWith("Marker"));

    static IEnumerable<SyntaxKind> Keywords()
        => AllKinds().Where(e => e.ToString().EndsWith("Keyword"));

    static IEnumerable<SyntaxKind> Nodes()
        => AllKinds().Except(Tokens()).Except(Trivia()).Except(Markers()).Except(Keywords());
}
