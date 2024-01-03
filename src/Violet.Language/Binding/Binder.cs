using System.Collections.Immutable;
using System.Diagnostics;
using Violet.Language.Symbols;
using Violet.Language.Syntax;

namespace Violet.Language.Binding;

class Binder(BoundScope scope)
{
    BoundScope Scope { get; set; } = scope;

    DiagnosticBuilder Diagnostics { get; } = new();

    /// <summary>
    /// Creates a <see cref="BoundModule"/> representing the module formed by binding the specified <see cref="SyntaxTree"/>s.
    /// </summary>
    public static BoundModule BindModule(IReadOnlyList<SyntaxTree> syntaxTrees)
    {
        var rootScope = CreateRootScope();
        var scope = new BoundScope(rootScope);

        var binder = new Binder(scope);
        binder.Diagnostics.AddRange(syntaxTrees.SelectMany(t => t.Diagnostics));
        if (binder.Diagnostics.HasErrors())
        {
            // Don't continue binding if the parsing has errors.
            return new BoundModule(
                binder.Diagnostics.ToImmutableArray(),
                new BoundScope(null));
        }

        // Bind all the functions
        var functions = syntaxTrees.SelectMany(t => t.Root.Members)
            .OfType<FunctionDeclarationSyntax>();
        foreach (var function in functions)
        {
            // Binding will automatically add the function to the scope.
            binder.BindFunctionDeclaration(function);
        }

        return new BoundModule(
            binder.Diagnostics.ToImmutableArray(),
            binder.Scope);
    }

    /// <summary>
    /// Creates a <see cref="BoundProgram"/> that represents a program created from the specified <see cref="BoundModule"/>.
    /// </summary>
    public static BoundProgram BindProgram(BoundModule module)
    {
        if (module.Diagnostics.HasErrors())
        {
            return new BoundProgram(
                module.Diagnostics,
                ImmutableDictionary<FunctionSymbol, BoundStatement>.Empty,
                null);
        }

        var bodies = ImmutableDictionary.CreateBuilder<FunctionSymbol, BoundStatement>();
        var diagnostics = new DiagnosticBuilder();

        foreach (var function in module.GlobalScope.GetDeclaredFunctions())
        {
            var binder = new Binder(module.GlobalScope);
            var declaration = function.Declaration.Require();
            var body = binder.BindBlockStatement(declaration.Body);

            // TODO: Lowering!
            // TODO: Validate that all paths return!

            bodies.Add(function, body);
            diagnostics.AddRange(binder.Diagnostics.ToImmutableArray());
        }

        var entryPoint = bodies.Keys.FirstOrDefault(f => f.Name == "Main");

        return new BoundProgram(
            diagnostics.ToImmutableArray(),
            bodies.ToImmutableDictionary(),
            entryPoint);
    }

    BoundStatement BindStatement(StatementSyntax statementSyntax)
    {
        switch (statementSyntax)
        {
            case BlockStatementSyntax blockStatementSyntax:
                return BindBlockStatement(blockStatementSyntax);
            case ExpressionStatementSyntax expressionStatementSyntax:
                return BindExpressionStatement(expressionStatementSyntax);
            default:
                throw new UnreachableException($"Unexpected statement type: {statementSyntax.Kind}");
        }
    }

    BoundStatement BindExpressionStatement(ExpressionStatementSyntax expressionStatementSyntax)
    {
        var expr = BindExpression(expressionStatementSyntax.Expression);
        return new BoundExpressionStatement(expressionStatementSyntax, expr);
    }

    BoundExpression BindExpression(ExpressionSyntax expressionSyntax)
    {
        switch (expressionSyntax)
        {
            case LiteralExpressionSyntax literalExpressionSyntax:
                return new BoundLiteralExpression(literalExpressionSyntax, literalExpressionSyntax.Value);
            case CallExpressionSyntax callExpressionSyntax:
                return BindCallExpression(callExpressionSyntax);
            case NameExpressionSyntax nameExpressionSyntax:
                throw new NotImplementedException();
            case UnaryExpressionSyntax unaryExpressionSyntax:
                throw new NotImplementedException();
            case BinaryExpressionSyntax binaryExpressionSyntax:
                throw new NotImplementedException();
            default:
                throw new UnreachableException($"Unexpected expression type: {expressionSyntax.Kind}");
        }
    }

    BoundExpression BindCallExpression(CallExpressionSyntax callExpressionSyntax)
    {
        var symbol = Scope.TryResolve(callExpressionSyntax.Identifier.Text);
        if (symbol == null)
        {
            Diagnostics.Add(
                DiagnosticDescriptors.UndefinedFunction,
                callExpressionSyntax.Identifier.Location,
                callExpressionSyntax.Identifier.Text);
            return new BoundErrorExpression(callExpressionSyntax);
        }

        if (symbol is not FunctionSymbol functionSymbol)
        {
            Diagnostics.Add(
                DiagnosticDescriptors.NotCallable,
                callExpressionSyntax.Identifier.Location,
                callExpressionSyntax.Identifier.Text);
            return new BoundErrorExpression(callExpressionSyntax);
        }

        var arguments = ImmutableArray.CreateBuilder<BoundExpression>();
        foreach (var argumentSyntax in callExpressionSyntax.Arguments)
        {
            arguments.Add(BindExpression(argumentSyntax));
        }

        return new BoundCallExpression(
            callExpressionSyntax,
            functionSymbol,
            arguments.ToImmutableArray());
    }

    BoundBlockStatement BindBlockStatement(BlockStatementSyntax blockStatementSyntax)
    {
        var statements = ImmutableArray.CreateBuilder<BoundStatement>();

        // Create a child scope.
        Scope = new BoundScope(Scope);

        foreach (var statementSyntax in blockStatementSyntax.Statements)
        {
            statements.Add(BindStatement(statementSyntax));
        }

        // Pop the child scope.
        Scope = Scope.Parent.Require();

        return new BoundBlockStatement(blockStatementSyntax, statements.ToImmutable());
    }

    void BindFunctionDeclaration(FunctionDeclarationSyntax functionDeclaration)
    {
        var function = new FunctionSymbol(
            functionDeclaration.Identifier.Text,
            TypeSymbol.Void,
            functionDeclaration);
        if (!Scope.TryDeclareFunction(function))
        {
            Diagnostics.Add(
                DiagnosticDescriptors.SymbolAlreadyDeclared,
                functionDeclaration.Identifier.Location,
                function.Name);
        }
    }

    static BoundScope CreateRootScope()
    {
        var rootScope = new BoundScope(null);
        foreach (var function in BuiltinFunctions.GetAll())
        {
            if (!rootScope.TryDeclareFunction(function))
            {
                throw new UnreachableException($"Unable to declare builtin function: {function.Name}");
            }
        }

        return rootScope;
    }
}
