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
    }

    [Fact]
    public void IsTrivialReturnsTrueForAllTrivia()
    {
        var triviaByHelper = new SortedSet<SyntaxKind>(AllKinds().Where(k => k.IsTrivia()));
        var triviaByName = new SortedSet<SyntaxKind>(Trivia());
        triviaByHelper.ExceptWith(triviaByName);
        Assert.Empty(triviaByHelper);
    }

    [Fact]
    public void IsMarkerReturnsTrueForAllMarkers()
    {
        var markersByHelper = new SortedSet<SyntaxKind>(AllKinds().Where(k => k.IsMarker()));
        var markersByName = new SortedSet<SyntaxKind>(Markers());
        markersByHelper.ExceptWith(markersByName);
        Assert.Empty(markersByHelper);
    }

    [Fact]
    public void IsNodeReturnsTrueForAllOtherNodes()
    {
        var nodesByHelper = new SortedSet<SyntaxKind>(AllKinds().Where(k => k.IsNode()));
        var nodesByName = new SortedSet<SyntaxKind>(Nodes());
        nodesByHelper.ExceptWith(nodesByName);
        Assert.Empty(nodesByHelper);
    }

    [Fact]
    public void GetTextHasValueForNonDynamicTokens()
    {
        var tokens = new SortedSet<SyntaxKind>(Tokens());

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

    public static IEnumerable<SyntaxKind> AllKinds()
        => Enum.GetValues<SyntaxKind>();

    public static IEnumerable<SyntaxKind> Tokens()
        => AllKinds().Where(e => e.ToString().EndsWith("Token"));

    public static IEnumerable<SyntaxKind> Trivia()
        => AllKinds().Where(e => e.ToString().EndsWith("Trivia"));

    public static IEnumerable<SyntaxKind> Markers()
        => AllKinds().Where(e => e.ToString().EndsWith("Marker"));

    public static IEnumerable<SyntaxKind> Nodes()
        => AllKinds().Except(Tokens()).Except(Trivia()).Except(Markers());
}
