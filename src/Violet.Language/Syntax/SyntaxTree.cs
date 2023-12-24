using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Violet.Language.Syntax;

/// <summary>
/// Stores an entire Violet syntax tree, usually representing a single file or compilation unit.
/// </summary>
public class SyntaxTree
{
    Dictionary<SyntaxNode, SyntaxNode?>? _parents;

    /// <summary>
    /// Gets the <see cref="SourceText"/> representing the text of the document represented by this syntax tree.
    /// </summary>
    public SourceText Text { get; }

    /// <summary>
    /// Gets the root <see cref="SyntaxNode"/> of the syntax tree.
    /// </summary>
    // We assert that this will be initialized in Parse().
    // The only places that use an under-initialized SyntaxTree is the Tokenizer and the Parser.
    public CompilationUnitSyntax Root { get; private set; } = null!;

    /// <summary>
    /// Gets a list of <see cref="Diagnostic"/>s that were found during parsing.
    /// </summary>
    public ImmutableArray<Diagnostic> Diagnostics { get; private set; }

    /// <summary>
    /// Creates a new <see cref="SyntaxTree"/> instance.
    /// An instance created this way has no root node and an empty diagnostics list.
    /// </summary>
    /// <param name="text">The <see cref="SourceText"/> containing the text represented by this syntax tree.</param>
    SyntaxTree(SourceText text)
    {
        Text = text;
        Diagnostics = ImmutableArray<Diagnostic>.Empty;
    }

    /// <summary>
    /// Gets the parent of the specified <paramref name="syntaxNode"/> in the syntax tree, if there is one.
    /// </summary>
    /// <param name="syntaxNode">The <see cref="SyntaxNode"/> to find the parent of.</param>
    internal SyntaxNode? GetParent(SyntaxNode syntaxNode)
    {
        if (Root is null)
        {
            return null;
        }

        if (_parents is null)
        {
            var parents = new Dictionary<SyntaxNode, SyntaxNode?>();
            FindParents(parents, Root);
            Interlocked.CompareExchange(ref _parents, parents, null);
        }

        return _parents.TryGetValue(syntaxNode, out var parent)
            ? parent
            : throw new UnreachableException("This node is not a member of this tree");
    }

    void Initialize(CompilationUnitSyntax root, ImmutableArray<Diagnostic> diagnostics)
    {
        Root = root;
        Diagnostics = diagnostics;
    }

    void FindParents(Dictionary<SyntaxNode, SyntaxNode?> parents, SyntaxNode node)
    {
        foreach (var child in node.GetChildren())
        {
            parents.Add(child, node);
            FindParents(parents, child);
        }
    }

    /// <summary>
    /// Parses the specified <paramref name="text"/> into a <see cref="SyntaxTree"/>.
    /// </summary>
    /// <param name="text">The text to be parsed</param>
    /// <returns>A <see cref="SyntaxTree"/> representing the parsed text.</returns>
    public static SyntaxTree Parse(string text) => Parse(SourceText.From(text));

    /// <summary>
    /// Parses the specified <paramref name="text"/> into a <see cref="SyntaxTree"/>.
    /// </summary>
    /// <param name="text">The text to be parsed</param>
    /// <returns>A <see cref="SyntaxTree"/> representing the parsed text.</returns>
    public static SyntaxTree Parse(SourceText text)
    {
        var tree = new SyntaxTree(text);

        // Tokenize the entire text.
        var tokens = ImmutableArray.CreateBuilder<SyntaxToken>();
        var badTokens = new List<SyntaxToken>();

        var tokenizer = new Tokenizer(tree);
        SyntaxToken token;
        do
        {
            token = tokenizer.Next();

            // If the token is "bad" (i.e. an unexpected character), collect it.
            if (token.Kind is SyntaxKind.BadToken)
            {
                // Collect this until either EOF or a good token.
                badTokens.Add(token);
            }
            else
            {
                // If we have collected bad tokens, convert them to leading trivia in the next token.
                if (badTokens.Count > 0)
                {
                    var leadingTrivia = token.LeadingTrivia.ToBuilder();

                    foreach (var badToken in badTokens)
                    {
                        // Copy the leading trivia from the bad token to the next token.
                        foreach (var leading in badToken.LeadingTrivia)
                        {
                            leadingTrivia.Add(leading);
                        }

                        // Convert the bad token to trivia
                        var trivia = new SyntaxTrivia(tree, SyntaxKind.SkippedTextTrivia, badToken.Position,
                            badToken.Text);
                        leadingTrivia.Add(trivia);

                        // Convert the trailing trivia from the bad token to the next token.
                        foreach (var trailing in badToken.TrailingTrivia)
                        {
                            leadingTrivia.Add(trailing);
                        }
                    }

                    badTokens.Clear();
                    token = token.WithLeadingTrivia(leadingTrivia.ToImmutableArray());
                }

                tokens.Add(token);
            }
        } while (token.Kind != SyntaxKind.EndOfFileMarker);

        var parser = new Parser(tree, tokens.ToImmutableArray());
        var root = parser.Parse();

        var diagnostics = ImmutableArray.CreateBuilder<Diagnostic>();
        diagnostics.AddRange(tokenizer.Diagnostics.ToImmutableArray());
        diagnostics.AddRange(parser.Diagnostics.ToImmutableArray());

        tree.Initialize(
            root,
            diagnostics.ToImmutableArray());
        return tree;
    }

    /// <summary>
    /// Loads the content of the specified <paramref name="path"/> into a <see cref="SyntaxTree"/>.
    /// </summary>
    /// <param name="path">The path to the file to load.</param>
    /// <returns>A <see cref="SyntaxTree"/> representing the result of parsing.</returns>
    public static SyntaxTree Load(string path)
    {
        var text = File.ReadAllText(path);
        var sourceText = SourceText.From(text, path);
        return Parse(sourceText);
    }

    /// <summary>
    /// Parses the specified <paramref name="text"/> into raw tokens.
    /// </summary>
    /// <param name="text">The input test to parse</param>
    /// <param name="diagnostics">Returns any diagnostics raised during tokenization.</param>
    /// <param name="includeEndOfFile">If true, includes a token for "end-of-file" (useful for capturing end-of-file trivia)</param>
    /// <returns>The <see cref="SyntaxToken"/>s parsed.</returns>
    public static ImmutableArray<SyntaxToken> ParseTokens(string text, out ImmutableArray<Diagnostic> diagnostics, bool includeEndOfFile = false)
    {
        var sourceText = SourceText.From(text);
        var tree = new SyntaxTree(sourceText);
        var tokenizer = new Tokenizer(tree);
        var tokens = ImmutableArray.CreateBuilder<SyntaxToken>();
        while (true)
        {
            var token = tokenizer.Next();
            if (includeEndOfFile || token.Kind is not SyntaxKind.EndOfFileMarker)
            {
                tokens.Add(token);
            }

            if (token.Kind is SyntaxKind.EndOfFileMarker)
            {
                break;
            }
        }

        diagnostics = tokenizer.Diagnostics.ToImmutableArray();
        return tokens.ToImmutableArray();
    }

    public string Format()
    {
        var sb = new StringBuilder();
        var sw = new StringWriter(sb);
        Root.FormatTo(sw, indent: 0);
        return sb.ToString();
    }
}
