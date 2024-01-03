using Mono.Cecil;

namespace Violet.Language.References;

public abstract record MetadataReference
{
    public static MetadataReference FromAssemblyPath(string path) => new PathMetadataReference(path);

    internal abstract AssemblyDefinition GetAssemblyDefinition();
}

record PathMetadataReference(string AssemblyPath) : MetadataReference
{
    internal override AssemblyDefinition GetAssemblyDefinition()
    {
        return AssemblyDefinition.ReadAssembly(AssemblyPath);
    }
}
