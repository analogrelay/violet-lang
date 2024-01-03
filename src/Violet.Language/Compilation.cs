using System.Collections.Immutable;
using Violet.Language.Binding;
using Violet.Language.Emit;
using Violet.Language.References;
using Violet.Language.Syntax;

namespace Violet.Language;

/// <summary>
/// Represents a single compilation session.
/// </summary>
public record Compilation
{
    readonly Lazy<BoundModule> _boundCompilation;

    BoundModule BoundModule => _boundCompilation.Value;
    public IReadOnlyList<SyntaxTree> SyntaxTrees { get; init; }
    public IReadOnlyList<MetadataReference> References { get; init; }

    /// <summary>
    /// Represents a single compilation session.
    /// </summary>
    public Compilation(IReadOnlyList<SyntaxTree> syntaxTrees, IReadOnlyList<MetadataReference> references)
    {
        SyntaxTrees = syntaxTrees;
        References = references;
        _boundCompilation = new Lazy<BoundModule>(BindGlobalScope);
    }

    public ImmutableArray<Diagnostic> Emit(string moduleName, Stream assemblyStream)
    {
        var parseDiagnostics = SyntaxTrees.SelectMany(t => t.Diagnostics);

        var preEmitDiagnostics = parseDiagnostics.Concat(BoundModule.Diagnostics).ToImmutableArray();
        if (preEmitDiagnostics.HasErrors())
        {
            return preEmitDiagnostics;
        }

        var program = Binder.BindProgram(BoundModule);

        // TODO: Temporary
        using var sw = new StringWriter();
        program.FormatTo(sw, 0);
        Console.WriteLine(sw.ToString());

        // Emit to the assembly stream
        return Emitter.Emit(moduleName, assemblyStream, program, References);
    }

    BoundModule BindGlobalScope()
    {
        return Binder.BindModule(SyntaxTrees);
    }
}
