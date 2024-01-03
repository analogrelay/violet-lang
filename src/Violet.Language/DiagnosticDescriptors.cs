namespace Violet.Language;

static class DiagnosticDescriptors
{
    // All errors 0000-0999 are reserved for the tokenizer
    public static readonly DiagnosticDescriptor InvalidEscapeSequence = new(
        "VI0001",
        DiagnosticSeverity.Error,
        "Invalid Escape Sequence",
        "Invalid Escape Sequence '{0}'");

    public static readonly DiagnosticDescriptor UnterminatedStringLiteral = new(
        "VI0002",
        DiagnosticSeverity.Error,
        "Unterminated String Literal",
        "Unterminated String Literal");

    public static readonly DiagnosticDescriptor IntegerLiteralTooLarge = new(
        "VI0003",
        DiagnosticSeverity.Error,
        "Integer Literal Too Large",
        "The Integer literal {0} is too large");

    public static readonly DiagnosticDescriptor UnexpectedCharacter = new(
        "VI0004",
        DiagnosticSeverity.Error,
        "Unexpected Character",
        "Unexpected '{0}', expected '{1}'");

    public static readonly DiagnosticDescriptor UnexpectedEndOfFile = new(
        "VI0005",
        DiagnosticSeverity.Error,
        "Unexpected end of file",
        "Unexpected end of file, expected '{0}'");

    // Errors 1000-1999 are reserved for the parser
    public static readonly DiagnosticDescriptor UnexpectedToken = new(
        "VI1000",
        DiagnosticSeverity.Error,
        "Unexpected Token",
        "Unexpected token <{0}>, expected <{1}>");

    public static readonly DiagnosticDescriptor ComparisonsCannotBeChained = new(
        "VI1001",
        DiagnosticSeverity.Error,
        "Comparison cannot be chained",
        "Comparison operators cannot be chained");

    public static readonly DiagnosticDescriptor ExpectedMember = new(
        "VI1002",
        DiagnosticSeverity.Error,
        "Expected a Member",
        "Expected a member declaration, but found <{0}>");

    // Errors 2000-2999 are reserved for binding.
    public static readonly DiagnosticDescriptor SymbolAlreadyDeclared = new(
        "VI2000",
        DiagnosticSeverity.Error,
        "Symbol Already Declared",
        "A symbol named '{0}' has already been declared in this scope");

    public static readonly DiagnosticDescriptor UndefinedFunction = new(
        "VI2001",
        DiagnosticSeverity.Error,
        "Undefined Function",
        "Could not resolve function '{0}'");

    public static readonly DiagnosticDescriptor NotCallable = new(
        "VI2002",
        DiagnosticSeverity.Error,
        "Not Callable",
        "The expression '{0}' is not callable");
}
