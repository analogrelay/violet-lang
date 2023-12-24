using Violet.Language;
using Violet.Language.Syntax;

if (args.Length < 1)
{
    Console.WriteLine("Usage: violetc <path-to-source>");
    return;
}

var inputFile = args[0];

// Parse the file
var syntaxTree = SyntaxTree.Load(inputFile);

// Create a compilation from this syntax tree
var compilation = new Compilation(syntaxTree);

// TODO: Emit the compilation to an assembly
