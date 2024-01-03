using System.Collections.Immutable;
using System.Diagnostics;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Violet.Language.Binding;
using Violet.Language.References;
using Violet.Language.Symbols;
using Violet.Language.Syntax;

namespace Violet.Language.Emit;

class Emitter
{
    readonly string _moduleName;
    readonly AssemblyDefinition _assemblyDefinition;
    readonly DiagnosticBuilder _diagnostics = new();
    readonly Dictionary<FunctionSymbol, MethodDefinition> _globalFunctions = new();

    List<AssemblyDefinition>? _referenceAssemblies;
    ObjectDefinition? _globalObject;
    Dictionary<FunctionSymbol, MethodReference>? _builtins;

    public ImmutableArray<MetadataReference> References { get; }

    public Emitter(string moduleName, ImmutableArray<MetadataReference> references)
    {
        _moduleName = moduleName;
        References = references;

        // TODO: Read assembly name info from somewhere
        var assemblyName = new AssemblyNameDefinition(moduleName, new Version(1, 0));
        _assemblyDefinition = AssemblyDefinition.CreateAssembly(
            assemblyName,
            moduleName,
            // TODO: Read module kind from somewhere
            ModuleKind.Console);
    }

    public static ImmutableArray<Diagnostic> Emit(string moduleName, Stream outputAssemblyStream, BoundProgram program, IReadOnlyList<MetadataReference> references)
    {
        if (program.Diagnostics.HasErrors())
        {
            // Don't emit on errors!
            return program.Diagnostics;
        }

        var emitter = new Emitter(moduleName, references.ToImmutableArray());
        return emitter.EmitProgram(program, outputAssemblyStream);
    }

    ImmutableArray<Diagnostic> EmitProgram(BoundProgram program, Stream outputAssemblyStream)
    {
        // Declare all the functions first so that we get symbols we can reference in the bodies.
        foreach (var (symbol, _) in program.FunctionBodies)
        {
            DeclareGlobalFunction(symbol);
        }

        // Then emit the bodies.
        foreach(var (symbol, body) in program.FunctionBodies)
        {
            EmitGlobalFunction(symbol, body);
        }

        if (program.EntryPoint is not null)
        {
            var stub = EmitEntryPointStub(program.EntryPoint);
            _assemblyDefinition.EntryPoint = stub;
        }

        _assemblyDefinition.Write(outputAssemblyStream);

        return _diagnostics.ToImmutableArray();
    }

    MethodDefinition EmitEntryPointStub(FunctionSymbol programEntryPoint)
    {
        var function = ResolveFunction(programEntryPoint);
        var globalObject = EnsureGlobalObject();

        var stub = new MethodDefinition(
            "<>EntryPoint",
            MethodAttributes.Private |
            MethodAttributes.HideBySig |
            MethodAttributes.Static |
            MethodAttributes.SpecialName,
            _assemblyDefinition.MainModule.TypeSystem.Void);
        var body = stub.Body.GetILProcessor();
        body.Emit(OpCodes.Call, globalObject.InstanceGetter);
        body.Emit(OpCodes.Call, function);
        body.Emit(OpCodes.Ret);
        globalObject.TypeDefinition.Methods.Add(stub);

        return stub;
    }

    ObjectDefinition EnsureGlobalObject()
    {
        if (_globalObject is not null)
        {
            return _globalObject;
        }

        _globalObject = DeclareObject(_moduleName, "Globals", TypeAttributes.SpecialName);
        return _globalObject;
    }

    void DeclareGlobalFunction(FunctionSymbol symbol)
    {
        // Global functions are instance methods on the module's global object.
        var globalObject = EnsureGlobalObject();

        // Declare the method
        var method = new MethodDefinition(
            symbol.Name,
            MethodAttributes.Public |
            MethodAttributes.HideBySig,
            _assemblyDefinition.MainModule.TypeSystem.Void);
        globalObject.TypeDefinition.Methods.Add(method);
        _globalFunctions.Add(symbol, method);
    }

    void EmitGlobalFunction(FunctionSymbol symbol, BoundStatement body)
    {
        var methodDeclaration = _globalFunctions.TryGetValue(symbol, out var m)
            ? m : throw new UnreachableException($"Attempted to emit function {symbol} that was not declared.");
        EmitFunctionBody(methodDeclaration, body);
    }

    void EmitFunctionBody(MethodDefinition methodDeclaration, BoundStatement body)
    {
        var il = methodDeclaration.Body.GetILProcessor();
        EmitStatement(il, body);
        il.Emit(OpCodes.Ret);
    }

    void EmitStatement(ILProcessor il, BoundStatement stmt)
    {
        switch (stmt)
        {
            case BoundBlockStatement boundBlockStatement:
                foreach (var statement in boundBlockStatement.Statements)
                {
                    EmitStatement(il, statement);
                }
                break;

            case BoundExpressionStatement boundExpressionStatement:
                EmitExpression(il, boundExpressionStatement.Expression);
                break;

            default:
                throw new UnreachableException($"Attempted to emit unknown statement: {stmt}");
        }
    }

    void EmitExpression(ILProcessor il, BoundExpression expr)
    {
        switch (expr)
        {
            case BoundLiteralExpression literalExpression:
                EmitLiteral(il, literalExpression);
                break;
            case BoundCallExpression callExpression:
                EmitCall(il, callExpression);
                break;
            default:
                throw new UnreachableException($"Attempted to emit unknown expression: {expr}");
        }
    }

    void EmitCall(ILProcessor il, BoundCallExpression callExpression)
    {
        var function = ResolveFunction(callExpression.Function);

        foreach(var argument in callExpression.Arguments)
        {
            EmitExpression(il, argument);
        }

        il.Emit(OpCodes.Call, function);
    }

    void EmitLiteral(ILProcessor il, BoundLiteralExpression literalExpression)
    {
        switch (literalExpression.Value)
        {
            case string s:
                il.Emit(OpCodes.Ldstr, s);
                break;
            default:
                throw new UnreachableException($"Attempted to emit unknown literal: {literalExpression}");
        }
    }

    MethodReference ResolveFunction(FunctionSymbol function)
    {
        if (function == BuiltinFunctions.Print)
        {
            var builtins = EnsureBuiltins();
            return builtins[function];
        }

        if (_globalFunctions.TryGetValue(function, out var m))
        {
            return m;
        }

        throw new UnreachableException($"Unable to resolve function: {function.Name}");
    }

    Dictionary<FunctionSymbol, MethodReference> EnsureBuiltins()
    {
        if (_builtins is not null)
        {
            return _builtins;
        }

        _builtins = new Dictionary<FunctionSymbol, MethodReference>
        {
            [BuiltinFunctions.Print] = ResolveFunction("System.Console", "WriteLine", new[] {"System.Object" })
        };
        return _builtins;
    }

    TypeReference ResolveType(string typeName)
    {
        var types = EnsureReferenceAssemblies()
            .SelectMany(a => a.Modules)
            .SelectMany(m => m.Types)
            .Where(t => t.FullName == typeName)
            .ToArray();
        if (types is [var typ])
        {
            return _assemblyDefinition.MainModule.ImportReference(typ);
        }

        throw new UnreachableException($"Failed to find required type: {typeName}. TODO: Diagnostic this.");
    }


    MethodReference ResolveFunction(string typeName, string methodName, string[] parameterTypeNames)
    {
        var types = EnsureReferenceAssemblies()
            .SelectMany(a => a.Modules)
            .SelectMany(m => m.Types)
            .Where(t => t.FullName == typeName)
            .ToArray();
        foreach(var type in types)
        {
            foreach (var method in type.Methods)
            {
                if (method.Name != methodName || method.Parameters.Count != parameterTypeNames.Length)
                {
                    continue;
                }

                var allParametersMatch = true;
                for (var i = 0; i < parameterTypeNames.Length; i++)
                {
                    if (method.Parameters[i].ParameterType.FullName != parameterTypeNames[i])
                    {
                        allParametersMatch = false;
                        break;
                    }
                }

                if (allParametersMatch)
                {
                    return _assemblyDefinition.MainModule.ImportReference(method);
                }
            }
        }

        throw new UnreachableException($"Failed to find required method: {typeName}.{methodName}. TODO: Diagnostic this.");
    }

    List<AssemblyDefinition> EnsureReferenceAssemblies()
    {
        if (_referenceAssemblies is not null)
        {
            return _referenceAssemblies;
        }

        _referenceAssemblies = new List<AssemblyDefinition>();
        foreach (var reference in References)
        {
            var assembly = reference.GetAssemblyDefinition();
            _referenceAssemblies.Add(assembly);
        }

        return _referenceAssemblies;
    }

    /// <summary>
    /// Declares an "Object", which is a singleton class with a private constructor and a public static "Instance" property.
    /// </summary>
    /// <param name="ns">The namespace for the object.</param>
    /// <param name="name">The name for the object.</param>
    /// <param name="additionalAttributes">Additional <see cref="TypeAttributes"/> to apply, on top of the default 'sealed' and 'public' attributes.</param>
    /// <returns>An <see cref="ObjectDefinition"/> representing the declared object.</returns>
    ObjectDefinition DeclareObject(string ns, string name, TypeAttributes additionalAttributes = 0)
    {
        var obj = ResolveType("System.Object");
        var objCtor = ResolveFunction("System.Object", ".ctor", Array.Empty<string>());
        var typeDefinition = new TypeDefinition(
            ns,
            name,
            TypeAttributes.Class |
            TypeAttributes.Public |
            TypeAttributes.Sealed |
            TypeAttributes.BeforeFieldInit |
            additionalAttributes);
        typeDefinition.BaseType = obj;
        _assemblyDefinition.MainModule.Types.Add(typeDefinition);

        // Create the private constructor
        var ctor = new MethodDefinition(
            ".ctor",
            MethodAttributes.Private |
            MethodAttributes.HideBySig |
            MethodAttributes.SpecialName |
            MethodAttributes.RTSpecialName,
            _assemblyDefinition.MainModule.TypeSystem.Void);
        var ctorBody = ctor.Body.GetILProcessor();
        // TODO: Initialize fields
        ctorBody.Emit(OpCodes.Ldarg_0);
        ctorBody.Emit(OpCodes.Call, objCtor);
        ctorBody.Emit(OpCodes.Ret);
        typeDefinition.Methods.Add(ctor);

        // Create the private "_instance" field
        var instanceField = new FieldDefinition(
            "_instance",
            FieldAttributes.Private |
            FieldAttributes.Static |
            FieldAttributes.InitOnly,
            typeDefinition);
        typeDefinition.Fields.Add(instanceField);

        // Create the static constructor to initialize the "_instance" field
        var staticCtor = new MethodDefinition(
            ".cctor",
            MethodAttributes.Private |
            MethodAttributes.HideBySig |
            MethodAttributes.SpecialName |
            MethodAttributes.RTSpecialName |
            MethodAttributes.Static,
            _assemblyDefinition.MainModule.TypeSystem.Void);
        var cctorBody = staticCtor.Body.GetILProcessor();
        cctorBody.Emit(OpCodes.Newobj, ctor);
        cctorBody.Emit(OpCodes.Stsfld, instanceField);
        cctorBody.Emit(OpCodes.Ret);
        typeDefinition.Methods.Add(staticCtor);

        // Create the "Instance" property
        var instanceProperty = new PropertyDefinition(
            "Instance",
            PropertyAttributes.SpecialName,
            typeDefinition)
        {
            GetMethod = new MethodDefinition(
                "get_Instance",
                MethodAttributes.Public |
                MethodAttributes.HideBySig |
                MethodAttributes.SpecialName |
                MethodAttributes.Static,
                typeDefinition)
        };
        var getterBody = instanceProperty.GetMethod.Body.GetILProcessor();
        getterBody.Emit(OpCodes.Ldsfld, instanceField);
        getterBody.Emit(OpCodes.Ret);
        typeDefinition.Properties.Add(instanceProperty);
        typeDefinition.Methods.Add(instanceProperty.GetMethod);

        return new(typeDefinition, instanceProperty.GetMethod);
    }
}
