using Violet.Language.Syntax;

namespace Violet.Language;

/// <summary>
/// Represents a single compilation session.
/// </summary>
public record Compilation(IEnumerable<SyntaxTree> SyntaxTrees)
{
    public Compilation(params SyntaxTree[] syntaxTrees)
        : this((IEnumerable<SyntaxTree>) syntaxTrees)
    {
    }
}
