using System.Collections.Immutable;

namespace Violet.Language;

public class DiagnosticBuilder
{
    readonly ImmutableArray<Diagnostic>.Builder _builder = ImmutableArray.CreateBuilder<Diagnostic>();

    public void Add(DiagnosticDescriptor descriptor, TextLocation location, params object[] args)
        => Add(new(descriptor, location, args));

    public void Add(Diagnostic diagnostic) => _builder.Add(diagnostic);

    public void AddRange(IEnumerable<Diagnostic> diagnostics) => _builder.AddRange(diagnostics);

    public ImmutableArray<Diagnostic> ToImmutableArray() => _builder.ToImmutableArray();

    public bool HasErrors() => _builder.Any(d => d.Descriptor.Severity == DiagnosticSeverity.Error);
}

public static class DiagnosticExtensions
{
    public static bool HasErrors(this ImmutableArray<Diagnostic> diagnostics)
        => diagnostics.Any(d => d.Descriptor.Severity == DiagnosticSeverity.Error);
}
