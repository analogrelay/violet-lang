namespace Violet.Language.Syntax;

/// <summary>
/// Syntax trivia are nodes with no semantic relevance.
/// This includes whitespace and comments.
/// </summary>
/// <remarks>
/// A <see cref="SyntaxTrivia"/> node is not considered a primary part of the syntax tree.
/// It exists only in the <see cref="SyntaxNode.LeadingTrivia"/> or <see cref="SyntaxNode.TrailingTrivia"/>
/// of some other <see cref="SyntaxNode"/>.
/// </remarks>
public record SyntaxTrivia
{
    /// <summary>
    /// The <see cref="SyntaxTree"/> this trivia belongs to.
    /// </summary>
    public SyntaxTree Tree { get; }

    /// <summary>
    /// The <see cref="SyntaxKind"/> of this trivia.
    /// </summary>
    public SyntaxKind Kind { get; }

    /// <summary>
    /// The position of the first character of this trivia relative to the start of the file.
    /// </summary>
    public int Position { get; }

    /// <summary>
    /// The text of this trivia.
    /// </summary>
    public string Text { get; }

    /// <summary>
    /// A <see cref="TextSpan"/> that represents the span of this trivia relative to the start of the file.
    /// </summary>
    public TextSpan Span => new(Position, Text.Length);

    internal SyntaxTrivia(SyntaxTree tree, SyntaxKind kind, int position, string text)
    {
        if (!kind.IsTrivia())
        {
            throw new ArgumentOutOfRangeException("Cannot create a trivia entry from a non-trivia kind");
        }

        Tree = tree;
        Kind = kind;
        Position = position;
        Text = text;
    }
}
