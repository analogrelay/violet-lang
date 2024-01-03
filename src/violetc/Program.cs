using System.Reflection;
using McMaster.Extensions.CommandLineUtils;
using Violet.Language;
using Violet.Language.References;
using Violet.Language.Syntax;

var app = new CommandLineApplication();
app.Name = "violetc";
app.Description = "The Violet Compiler";

app.HelpOption();
app.VersionOption("--version", typeof(Program).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? "0.0.0");

var projectPath = app.Argument("INPUT", "The input file to compile.")
    .IsRequired(true, "You must provide an input file to compile");
var outputPath = app.Option("-o|--output <output>", "Write output to <output>", CommandOptionType.SingleValue);
var referencePath = app.Option("-r|--reference <path>", "Reference the assembly at '<path>'", CommandOptionType.MultipleValue);

app.OnExecute(() =>
{
    static string GetDefaultOutputPath(string inputPath)
    {
        var directoryName = Path.GetDirectoryName(inputPath)!;
        var baseName = Path.GetFileNameWithoutExtension(inputPath);
        return Path.Combine(directoryName, $"{baseName}.dll");
    }

    var inputPath = projectPath.Value!;
    var outputAssembly = outputPath.Value() is { Length: > 0 } o
        ? o
        : GetDefaultOutputPath(inputPath);

    var references = new List<MetadataReference>();
    foreach(var reference in referencePath.Values)
    {
        if (!File.Exists(reference))
        {
            Console.Error.WriteLine($"Could not find reference '{reference}'");
            return;
        }
        references.Add(MetadataReference.FromAssemblyPath(reference));
    }

    // Parse the file
    var syntaxTree = SyntaxTree.Load(projectPath.Value!);

    Console.WriteLine(syntaxTree.Format());

    // Create a compilation from this syntax tree
    var compilation = new Compilation(new []
    {
        syntaxTree,
    }, references);

    using var assemblyStream = File.OpenWrite(outputAssembly);
    compilation.Emit("MyProgram", assemblyStream);
});

return app.Execute(args);
