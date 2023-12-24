using System.Collections.Immutable;

namespace Violet.Language.Syntax;

public record CompilationUnitSyntax(SyntaxTree SyntaxTree, ImmutableArray<MemberSyntax> Members): SyntaxNode(SyntaxTree)
{
    public override SyntaxKind Kind => SyntaxKind.CompilationUnit;

    public override IEnumerable<SyntaxNode> GetChildren()
    {
        foreach (var member in Members)
        {
            yield return member;
        }
    }
}
