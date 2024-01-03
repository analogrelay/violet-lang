using System.Collections.Immutable;
using Violet.Language.Symbols;

namespace Violet.Language.Binding;

record BoundModule(
    ImmutableArray<Diagnostic> Diagnostics,
    BoundScope GlobalScope);
