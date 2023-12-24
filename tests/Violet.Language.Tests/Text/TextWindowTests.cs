namespace Violet.Language.Text;

public class TextWindowTests
{
    [Fact]
    public void IsEmptyWhenInitialized()
    {
        const string buffer = "1234567890";
        var window = new TextWindow(SourceText.From(buffer));

        Assert.Equal(0, window.Start);
        Assert.Equal(0, window.End);
        Assert.Equal(0, window.Right.Span.Start);
        Assert.Equal(0, window.Right.Span.Length);
        Assert.Equal(0, window.Length);
        Assert.Equal("", window.Content);
        Assert.Equal("«»1234567890", window.DebuggerDisplay);
        Assert.False(window.EndOfFile);
    }

    [Fact]
    public void ExtendMovesRightEdgeOfWindow()
    {
        const string buffer = "1234567890";
        var window = new TextWindow(SourceText.From(buffer));
        window.Extend(4);

        Assert.Equal(0, window.Start);
        Assert.Equal(4, window.End);
        Assert.Equal(3, window.Right.Span.Start);
        Assert.Equal(1, window.Right.Span.Length);
        Assert.Equal(4, window.Length);
        Assert.Equal("1234", window.Content);
        Assert.Equal("«1234»567890", window.DebuggerDisplay);
        Assert.False(window.EndOfFile);
    }

    [Fact]
    public void TryExtendMovesRightEdgeOfWindowIfMatchesCandidate()
    {
        const string buffer = "1234567890";
        var window = new TextWindow(SourceText.From(buffer));

        Assert.True(window.TryExtend('1'));
        Assert.Equal(0, window.Start);
        Assert.Equal(1, window.End);
        Assert.Equal(0, window.Right.Span.Start);
        Assert.Equal(1, window.Right.Span.Length);
        Assert.Equal(1, window.Length);
        Assert.Equal("1", window.Content);
        Assert.Equal("«1»234567890", window.DebuggerDisplay);
        Assert.False(window.EndOfFile);

        Assert.False(window.TryExtend('9'));
        Assert.Equal(0, window.Start);
        Assert.Equal(1, window.End);
        Assert.Equal(0, window.Right.Span.Start);
        Assert.Equal(1, window.Right.Span.Length);
        Assert.Equal(1, window.Length);
        Assert.Equal("1", window.Content);
        Assert.Equal("«1»234567890", window.DebuggerDisplay);
        Assert.False(window.EndOfFile);
    }

    [Fact]
    public void NextMovesRightEdgeOfWindow()
    {
        const string buffer = "1234567890";
        var window = new TextWindow(SourceText.From(buffer));
        var result = window.Next(4);

        Assert.Equal("1234", result);
        Assert.Equal(0, window.Start);
        Assert.Equal(4, window.End);
        Assert.Equal(3, window.Right.Span.Start);
        Assert.Equal(1, window.Right.Span.Length);
        Assert.Equal(4, window.Length);
        Assert.Equal("1234", window.Content);
        Assert.Equal("«1234»567890", window.DebuggerDisplay);
        Assert.False(window.EndOfFile);
    }

    [Fact]
    public void BackRetractsRightEdgeOfWindow()
    {
        const string buffer = "1234567890";
        var window = new TextWindow(SourceText.From(buffer));
        var result = window.Next(4);
        Assert.Equal("1234", window.Content);
        Assert.Equal("1234", result);
        Assert.Equal("«1234»567890", window.DebuggerDisplay);

        window.Back(2);

        Assert.Equal("12", window.Content);
        Assert.Equal(0, window.Start);
        Assert.Equal(2, window.End);
        Assert.Equal(1, window.Right.Span.Start);
        Assert.Equal(1, window.Right.Span.Length);
        Assert.Equal(2, window.Length);
        Assert.Equal("12", window.Content);
        Assert.Equal("«12»34567890", window.DebuggerDisplay);
        Assert.False(window.EndOfFile);
    }

    [Fact]
    public void PeekDoesNotMoveRightEdgeOfWindow()
    {
        const string buffer = "1234567890";
        var window = new TextWindow(SourceText.From(buffer));
        var result = window.Peek();

        Assert.Equal('1', result);
        Assert.Equal(0, window.Start);
        Assert.Equal(0, window.End);
        Assert.Equal(0, window.Right.Span.Start);
        Assert.Equal(0, window.Right.Span.Length);
        Assert.Equal(0, window.Length);
        Assert.Equal("", window.Content);
        Assert.Equal("«»1234567890", window.DebuggerDisplay);
        Assert.False(window.EndOfFile);

        result = window.Peek(1);
        Assert.Equal('2', result);
    }

    [Fact]
    public void AdvanceMovesLeftEdgeOfWindowToRightEdge()
    {
        const string buffer = "1234567890";
        var window = new TextWindow(SourceText.From(buffer));
        window.Extend(4);
        window.Advance();

        Assert.Equal(4, window.Start);
        Assert.Equal(4, window.End);
        Assert.Equal(4, window.Right.Span.Start);
        Assert.Equal(0, window.Right.Span.Length);
        Assert.Equal(0, window.Length);
        Assert.Equal("", window.Content);
        Assert.Equal("1234«»567890", window.DebuggerDisplay);
        Assert.False(window.EndOfFile);
    }

    [Fact]
    public void AfterAdvancingNextMovesNewRightEdge()
    {
        const string buffer = "1234567890";
        var window = new TextWindow(SourceText.From(buffer));
        window.Extend(4);
        window.Advance();
        var result = window.Next(4);

        Assert.Equal("5678", result);
        Assert.Equal(4, window.Start);
        Assert.Equal(8, window.End);
        Assert.Equal(7, window.Right.Span.Start);
        Assert.Equal(1, window.Right.Span.Length);
        Assert.Equal(4, window.Length);
        Assert.Equal("5678", window.Content);
        Assert.Equal("1234«5678»90", window.DebuggerDisplay);
        Assert.False(window.EndOfFile);
    }

    [Fact]
    public void AfterAdvancingPeekLooksAtNextWindow()
    {
        const string buffer = "1234567890";
        var window = new TextWindow(SourceText.From(buffer));
        window.Extend(4);
        window.Advance();
        var result = window.Peek();

        Assert.Equal('5', result);
        Assert.Equal(4, window.Start);
        Assert.Equal(4, window.End);
        Assert.Equal(4, window.Right.Span.Start);
        Assert.Equal(0, window.Right.Span.Length);
        Assert.Equal(0, window.Length);
        Assert.Equal("", window.Content);
        Assert.Equal("1234«»567890", window.DebuggerDisplay);
        Assert.False(window.EndOfFile);
    }

    [Fact]
    public void ExtendingPastEndHasNoEffect()
    {
        const string buffer = "1234567890";
        var window = new TextWindow(SourceText.From(buffer));
        window.Extend(buffer.Length - 2);
        window.Advance();
        var result = window.Next(4);

        Assert.Equal("90", result);
        Assert.Equal(8, window.Start);
        Assert.Equal(10, window.End);
        Assert.Equal(9, window.Right.Span.Start);
        Assert.Equal(1, window.Right.Span.Length);
        Assert.Equal(2, window.Length);
        Assert.Equal("90", window.Content);
        Assert.Equal("12345678«90»", window.DebuggerDisplay);
        Assert.True(window.EndOfFile);
    }

    [Fact]
    public void PeekCharReturnsSingleNextCharacter()
    {
        const string buffer = "1234567890";
        var window = new TextWindow(SourceText.From(buffer));
        window.Extend(4);
        window.Advance();
        var result = window.Peek();

        Assert.Equal('5', result);
        Assert.Equal(4, window.Start);
        Assert.Equal(4, window.End);
        Assert.Equal(4, window.Right.Span.Start);
        Assert.Equal(0, window.Right.Span.Length);
        Assert.Equal(0, window.Length);
        Assert.Equal("", window.Content);
        Assert.Equal("1234«»567890", window.DebuggerDisplay);
        Assert.False(window.EndOfFile);
    }

    [Fact]
    public void PeekCharReturnsNullAtEOF()
    {
        const string buffer = "1234567890";
        var window = new TextWindow(SourceText.From(buffer));
        window.Extend(buffer.Length);
        window.Advance();
        Assert.True(window.EndOfFile);
        var result = window.Peek();

        Assert.Equal('\0', result);
        Assert.Equal(10, window.Start);
        Assert.Equal(10, window.End);
        Assert.Equal(10, window.Right.Span.Start);
        Assert.Equal(0, window.Right.Span.Length);
        Assert.Equal(0, window.Length);
        Assert.Equal("", window.Content);
        Assert.Equal("1234567890«»", window.DebuggerDisplay);
        Assert.True(window.EndOfFile);
    }

    [Fact]
    public void NextCharReturnsSingleNextCharacter()
    {
        const string buffer = "1234567890";
        var window = new TextWindow(SourceText.From(buffer));
        window.Extend(4);
        window.Advance();
        var result = window.Next();

        Assert.Equal('5', result);
        Assert.Equal(4, window.Start);
        Assert.Equal(5, window.End);
        Assert.Equal(4, window.Right.Span.Start);
        Assert.Equal(1, window.Right.Span.Length);
        Assert.Equal(1, window.Length);
        Assert.Equal("5", window.Content);
        Assert.Equal("1234«5»67890", window.DebuggerDisplay);
        Assert.False(window.EndOfFile);
    }

    [Fact]
    public void NextCharReturnsNullAtEOF()
    {
        const string buffer = "1234567890";
        var window = new TextWindow(SourceText.From(buffer));
        window.Extend(buffer.Length);
        window.Advance();
        Assert.True(window.EndOfFile);
        var result = window.Next();

        Assert.Equal('\0', result);
        Assert.Equal(10, window.Start);
        Assert.Equal(10, window.End);
        Assert.Equal(10, window.Right.Span.Start);
        Assert.Equal(0, window.Right.Span.Length);
        Assert.Equal(0, window.Length);
        Assert.Equal("", window.Content);
        Assert.Equal("1234567890«»", window.DebuggerDisplay);
        Assert.True(window.EndOfFile);
    }

    [Fact]
    public void NextWhileStopsAtEndOfFile()
    {
        const string buffer = "1212";
        var window = new TextWindow(SourceText.From(buffer));
        var result = window.NextWhile(char.IsDigit);
        Assert.Equal("1212", result);

        Assert.Equal(0, window.Start);
        Assert.Equal(4, window.End);
        Assert.Equal(3, window.Right.Span.Start);
        Assert.Equal(1, window.Right.Span.Length);
        Assert.Equal(4, window.Length);
        Assert.Equal("1212", window.Content);
        Assert.Equal("«1212»", window.DebuggerDisplay);
    }

    [Fact]
    public void NextWhileExtendsOverSequenceThatMatchesPredicate()
    {
        const string buffer = "1212ab";
        var window = new TextWindow(SourceText.From(buffer));
        var result = window.NextWhile(char.IsDigit);
        Assert.Equal("1212", result);

        Assert.Equal(0, window.Start);
        Assert.Equal(4, window.End);
        Assert.Equal(3, window.Right.Span.Start);
        Assert.Equal(1, window.Right.Span.Length);
        Assert.Equal(4, window.Length);
        Assert.Equal("1212", window.Content);
        Assert.Equal("«1212»ab", window.DebuggerDisplay);
    }

    [Fact]
    public void NextWhileExtendsOverSequenceThatMatchesAnyProvidedCharacter()
    {
        const string buffer = "121234";
        var window = new TextWindow(SourceText.From(buffer));
        var result = window.NextWhile('1', '2');
        Assert.Equal("1212", result);

        Assert.Equal(0, window.Start);
        Assert.Equal(4, window.End);
        Assert.Equal(3, window.Right.Span.Start);
        Assert.Equal(1, window.Right.Span.Length);
        Assert.Equal(4, window.Length);
        Assert.Equal("1212", window.Content);
        Assert.Equal("«1212»34", window.DebuggerDisplay);
    }

    [Fact]
    public void NextUntilStopsAtEndOfFile()
    {
        const string buffer = "abab";
        var window = new TextWindow(SourceText.From(buffer));
        var result = window.NextUntil(char.IsDigit);
        Assert.Equal("abab", result);

        Assert.Equal(0, window.Start);
        Assert.Equal(4, window.End);
        Assert.Equal(3, window.Right.Span.Start);
        Assert.Equal(1, window.Right.Span.Length);
        Assert.Equal(4, window.Length);
        Assert.Equal("abab", window.Content);
        Assert.Equal("«abab»", window.DebuggerDisplay);
    }
}
