using JetBrains.Annotations;
using Xunit.Sdk;

namespace Violet.Language.Syntax;

/// <summary>
/// Utility class to help assert the shape of a tree.
/// Walks the tree from parent to child, and asserts that the children are in the expected order.
/// When disposed, validates that the entire tree was consumed.
/// </summary>
public partial class TreeAsserter : IDisposable
{
    readonly IList<(int Depth, SyntaxNode Node)> _nodes;
    int _currentDepth;
    bool _hasErrors;

    /// <summary>
    /// Utility class to help assert the shape of a tree.
    /// Walks the tree from parent to child, and asserts that the children are in the expected order.
    /// When disposed, validates that the entire tree was consumed.
    /// </summary>
    public TreeAsserter(SyntaxNode root)
    {
        _nodes = Flatten(0, root).ToList();
    }

    public void Dispose()
    {
        // No need to throw a second error if we already had one.
        if (_hasErrors || _nodes.Count == 0)
        {
            return;
        }

        var message = "Nodes were not consumed:\n";
        foreach (var (depth, node) in _nodes)
        {
            var indent = depth > 0 ? new string(' ', depth * 2) : string.Empty;
            message += node is SyntaxToken t
                ? $"  {indent}{node.Kind} '{t.Text}'\n"
                : $"  {indent}{node.Kind}\n";
        }

        throw new XunitException(message);
    }

    /// <summary>
    /// Asserts that the next node is of the given kind.
    /// </summary>
    public void AssertNode(
        SyntaxKind kind,
        [InstantHandle] Action? childAssert = null)
    {
        var originalDepth = _currentDepth;
        try
        {
            var (actualDepth, node) = _nodes.First();
            _nodes.RemoveAt(0);

            Assert.Equal(_currentDepth, actualDepth);
            Assert.Equal(kind, node.Kind);
            Assert.IsNotType<SyntaxToken>(node);

            // Call the callback to walk the children.
            if (childAssert is not null)
            {
                _currentDepth += 1;
                childAssert();
            }
        }
        catch (Exception)
        {
            // We need to know if an assertion failed because Dispose
            // will still be called and we don't want to mask that error.
            _hasErrors = true;
            throw;
        }
        finally
        {
            _currentDepth = originalDepth;
        }
    }

    /// <summary>
    /// Asserts a node with a single token child
    /// </summary>
    public void AssertNode(SyntaxKind kind, SyntaxKind token, string text)
    {
        AssertNode(kind, () => AssertToken(token, text));
    }

    public void AssertToken(SyntaxKind kind, string text)
    {
        try
        {
            var (actualDepth, node) = _nodes.First();
            _nodes.RemoveAt(0);

            Assert.Equal(_currentDepth, actualDepth);
            Assert.Equal(kind, node.Kind);
            var token = Assert.IsType<SyntaxToken>(node);
            Assert.Equal(text, token.Text);
        }
        catch (Exception)
        {
            // We need to know if an assertion failed because Dispose
            // will still be called and we don't want to mask that error.
            _hasErrors = true;
            throw;
        }
    }

    static IEnumerable<(int Depth, SyntaxNode Node)> Flatten(int depth, SyntaxNode root)
    {
        yield return (depth, root);
        foreach (var child in root.GetChildren())
        {
            foreach (var node in Flatten(depth + 1, child))
            {
                yield return node;
            }
        }
    }
}
