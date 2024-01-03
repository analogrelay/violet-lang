using Mono.Cecil;

namespace Violet.Language.Emit;

record ObjectDefinition(
    TypeDefinition TypeDefinition,
    MethodDefinition InstanceGetter);