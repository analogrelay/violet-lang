using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Violet.Language.Syntax;

/// <summary>
/// Base class for all syntax nodes
/// </summary>
/// <param name="SyntaxTree">The <see cref="SyntaxTree"/> that this node belongs to.</param>
public abstract record SyntaxNode(SyntaxTree SyntaxTree)
{
    /// <summary>
    /// Gets the <see cref="SyntaxNode"/> that is the parent of this node in the syntax tree, if there is one.
    /// Returns <c>null</c> if this node is the root of its syntax tree.
    /// </summary>
    public SyntaxNode? Parent => SyntaxTree.GetParent(this);

    /// <summary>
    /// Gets the <see cref="SyntaxKind"/> of this node.
    /// </summary>
    public abstract SyntaxKind Kind { get; }

    /// <summary>
    /// Gets the span of this node in the source text.
    /// </summary>
    public virtual TextSpan Span
    {
        get
        {
            var (first, last) = GetBoundingChildren();
            return TextSpan.FromBounds(
                first.Span.Start,
                last.Span.End);
        }
    }

    public TextLocation Location => new(SyntaxTree.Text, Span);

    /// <summary>
    /// Enumerates the immediate child nodes of this node, in source order.
    /// </summary>
    public abstract IEnumerable<SyntaxNode> GetChildren();

    internal virtual void FormatTo(TextWriter writer, int indent = 0)
    {
        var indentString = new string(' ', indent * 4);
        writer.WriteLine($"{indentString}{FormatSelf()}");
        foreach (var child in GetChildren())
        {
            child.FormatTo(writer, indent + 1);
        }
    }

    protected virtual string FormatSelf()
    {
        return $"{Kind}[{Span}]";
    }

    [SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
    (SyntaxNode First, SyntaxNode Last) GetBoundingChildren()
    {
        var children = GetChildren();

        return (
            First: children.FirstOrDefault().Require(),
            Last: children.LastOrDefault().Require());
    }
}
