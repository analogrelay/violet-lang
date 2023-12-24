using System.Collections.Immutable;
using System.Runtime.CompilerServices;

namespace Violet.Language.Syntax;

partial class Parser(SyntaxTree syntaxTree, ImmutableArray<SyntaxToken> tokens)
{
    readonly SourceText _text = syntaxTree.Text;
    readonly DiagnosticBuilder _diagnostics = new();
    int _position;

    public DiagnosticBuilder Diagnostics => _diagnostics;
    public SyntaxToken Current => Peek();

    public CompilationUnitSyntax Parse() => CompilationUnit();

    SyntaxToken Peek(int offset = 0)
    {
        var index = _position + offset;
        if (index >= tokens.Length)
        {
            return tokens[^1];
        }

        return tokens[index];
    }

    SyntaxToken NextToken()
    {
        var current = Current;
        _position += 1;
        return current;
    }

    SyntaxToken Expect(SyntaxKind kind)
    {
        if (Current.Kind == kind)
        {
            return NextToken();
        }

        _diagnostics.Add(DiagnosticDescriptors.UnexpectedToken, Current.Location, Current.Kind, kind);
        return new SyntaxToken(
            syntaxTree,
            kind,
            Current.Position,
            null,
            null,
            ImmutableArray<SyntaxTrivia>.Empty,
            ImmutableArray<SyntaxTrivia>.Empty);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    bool At(SyntaxKind kind)
        => Current.Kind == kind;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    bool At(SyntaxKind kind1, SyntaxKind kind2)
        => Current.Kind == kind1 || Current.Kind == kind2;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    bool AtSequence(SyntaxKind kind1, SyntaxKind kind2)
        => Current.Kind == kind1 && Peek(1).Kind == kind2;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    bool Ahead(int offset, SyntaxKind kind)
        => Peek(offset).Kind == kind;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    SyntaxToken? Try(SyntaxKind kind)
        => At(kind) ? NextToken() : null;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    SyntaxToken? Try(SyntaxKind kind1, SyntaxKind kind2)
        => At(kind1, kind2) ? NextToken() : null;
}
