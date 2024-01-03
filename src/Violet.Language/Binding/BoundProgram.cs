using System.Collections.Immutable;
using Violet.Language.Symbols;

namespace Violet.Language.Binding;

record BoundProgram(
    ImmutableArray<Diagnostic> Diagnostics,
    ImmutableDictionary<FunctionSymbol, BoundStatement> FunctionBodies,
    FunctionSymbol? EntryPoint)
{
    public bool FormatTo(TextWriter writer, int indent = 0)
    {
        writer.WriteLine($"{new string(' ', indent)}Program");
        foreach (var (function, body) in FunctionBodies)
        {
            var functionIndent = indent + 2;
            var epStr = "";
            if (function == EntryPoint)
            {
                epStr = " (Entry Point)";
            }
            writer.WriteLine($"{new string(' ', functionIndent)}Function {function.Name} : {function.Type.Format()}{epStr}");
            body.FormatTo(writer, functionIndent + 2);
        }

        return true;
    }
}
