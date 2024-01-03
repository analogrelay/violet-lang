using System.Collections.Immutable;
using Violet.Language.Syntax;

namespace Violet.Language.Syntax;

/// <summary>
/// A <see cref="SyntaxNode"/> representing a token.
/// </summary>
/// <remarks>
/// Tokens are one of two kinds of leaf nodes in the syntax tree.
/// They represent actual text in the source code with semantic meaning.
/// The other kind of leaf node in the syntax tree are <see cref="SyntaxTrivia"/>,
/// which represent text that has no semantic meaning (such as whitespace or comments)
/// </remarks>
public record SyntaxToken : SyntaxNode
{
    SyntaxKind _kind;

    public override SyntaxKind Kind => _kind;

    public int Position { get; }
    public string Text { get; }
    public object? Value { get; }
    public ImmutableArray<SyntaxTrivia> LeadingTrivia { get; }
    public ImmutableArray<SyntaxTrivia> TrailingTrivia { get; }

    public override TextSpan Span => new(Position, Text.Length);

    internal SyntaxToken(
        SyntaxTree tree,
        SyntaxKind kind,
        int position,
        string? text,
        object? value,
        ImmutableArray<SyntaxTrivia> leadingTrivia,
        ImmutableArray<SyntaxTrivia> trailingTrivia) : base(tree)
    {
        _kind = kind;
        Position = position;
        Text = text ?? string.Empty;
        Value = value;
        LeadingTrivia = leadingTrivia;
        TrailingTrivia = trailingTrivia;
    }

    public override IEnumerable<SyntaxNode> GetChildren()
        => Array.Empty<SyntaxNode>();

    public override string ToString()
    {
        return Value is not null
            ? $"{Kind}[{Span}]({Value.GetType()}:{Value})"
            : $"{Kind}[{Span}]";
    }

    public SyntaxToken WithLeadingTrivia(ImmutableArray<SyntaxTrivia> leadingTrivia)
        => new(SyntaxTree, Kind, Position, Text, Value, leadingTrivia, TrailingTrivia);

    internal override void FormatTo(TextWriter writer, int indent = 0)
    {
        var indentString = new string(' ', indent * 4);

        foreach (var lt in LeadingTrivia)
        {
            writer.WriteLine($"{indentString}/{lt.Kind}[{lt.Span}] \"{lt.Text.Unescape()}\"");
        }

        writer.WriteLine($"{indentString}{FormatSelf()}");

        foreach (var tt in TrailingTrivia)
        {
            writer.WriteLine($"{indentString}\\{tt.Kind}[{tt.Span}] \"{tt.Text.Unescape()}\"");
        }
    }

    protected override string FormatSelf()
    {
        return $"{base.FormatSelf()} \"{Text.Unescape()}\"";
    }
}
