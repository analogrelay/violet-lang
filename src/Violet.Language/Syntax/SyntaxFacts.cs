namespace Violet.Language.Syntax;

public static class SyntaxFacts
{
    public static int GetUnaryOperatorPrecedence(this SyntaxKind kind) =>
        kind switch
        {
            // Unary operators have highest precedence
            SyntaxKind.PlusToken => 6,
            SyntaxKind.MinusToken => 6,
            SyntaxKind.BangToken => 6,

            // All other tokens are not unary operators
            _ => 0,
        };

    public static int GetBinaryOperatorPrecedence(this SyntaxKind kind) =>
        kind switch
        {
            // Multiplicative operators have the highest precedence of all binary operators
            SyntaxKind.StarToken => 5,
            SyntaxKind.SlashToken => 5,

            // Then addition
            SyntaxKind.PlusToken => 4,
            SyntaxKind.MinusToken => 4,

            // Comparison operators come next
            SyntaxKind.EqualsEqualsToken => 3,
            SyntaxKind.BangEqualsToken => 3,
            SyntaxKind.GreaterThanToken => 3,
            SyntaxKind.GreaterThanEqualToken => 3,
            SyntaxKind.LessThanToken => 3,
            SyntaxKind.LessThanEqualToken => 3,

            // Logical conjunction
            SyntaxKind.AmpersandToken => 2,
            SyntaxKind.AmpersandAmpersandToken => 2,

            // Logical disjunction
            SyntaxKind.PipeToken => 1,
            SyntaxKind.PipePipeToken => 1,

            // Everything else has no precedence
            _ => 0,
        };

    public static bool IsComparisonOperator(this SyntaxKind kind) =>
        kind switch
        {
            SyntaxKind.EqualsEqualsToken => true,
            SyntaxKind.BangEqualsToken => true,
            SyntaxKind.GreaterThanToken => true,
            SyntaxKind.GreaterThanEqualToken => true,
            SyntaxKind.LessThanToken => true,
            SyntaxKind.LessThanEqualToken => true,

            _ => false,
        };

    public static string? GetText(this SyntaxKind kind) =>
        kind switch
        {
            SyntaxKind.AmpersandToken => "&",
            SyntaxKind.AmpersandAmpersandToken => "&&",
            SyntaxKind.AmpersandEqualToken => "&=",
            SyntaxKind.PipeToken => "|",
            SyntaxKind.PipePipeToken => "||",
            SyntaxKind.PipeEqualToken => "|=",
            SyntaxKind.HatToken => "^",
            SyntaxKind.HatEqualToken => "^=",
            SyntaxKind.PlusToken => "+",
            SyntaxKind.PlusEqualsToken => "+=",
            SyntaxKind.MinusToken => "-",
            SyntaxKind.MinusEqualsToken => "-=",
            SyntaxKind.StarToken => "*",
            SyntaxKind.StarEqualsToken => "*=",
            SyntaxKind.SlashToken => "/",
            SyntaxKind.SlashEqualsToken => "/=",
            SyntaxKind.PercentToken => "%",
            SyntaxKind.PercentEqualsToken => "%=",
            SyntaxKind.EqualsToken => "=",
            SyntaxKind.EqualsEqualsToken => "==",
            SyntaxKind.BangToken => "!",
            SyntaxKind.BangEqualsToken => "!=",
            SyntaxKind.GreaterThanToken => ">",
            SyntaxKind.GreaterThanEqualToken => ">=",
            SyntaxKind.LessThanToken => "<",
            SyntaxKind.LessThanEqualToken => "<=",
            SyntaxKind.LeftParenthesisToken => "(",
            SyntaxKind.RightParenthesisToken => ")",
            SyntaxKind.LeftBracketToken => "[",
            SyntaxKind.RightBracketToken => "]",
            SyntaxKind.LeftBraceToken => "{",
            SyntaxKind.RightBraceToken => "}",
            SyntaxKind.DotToken => ".",
            SyntaxKind.CommaToken => ",",
            _ => null,
        };

    public static bool IsNode(this SyntaxKind kind)
        => !kind.IsToken() && !kind.IsTrivia() && !kind.IsMarker();

    public static bool IsMarker(this SyntaxKind kind) => kind switch
    {
        SyntaxKind.UnknownMarker => true,
        SyntaxKind.EndOfFileMarker => true,
        _ => false,
    };

    public static bool IsTrivia(this SyntaxKind kind) => kind switch
    {
        SyntaxKind.WhitespaceTrivia => true,
        SyntaxKind.NewlineTrivia => true,
        SyntaxKind.BlockCommentTrivia => true,
        SyntaxKind.SingleLineCommentTrivia => true,
        SyntaxKind.SkippedTextTrivia => true,
        _ => false,
    };

    public static bool IsToken(this SyntaxKind kind) => kind switch
    {
        SyntaxKind.AmpersandToken => true,
        SyntaxKind.AmpersandAmpersandToken => true,
        SyntaxKind.AmpersandEqualToken => true,
        SyntaxKind.PipeToken => true,
        SyntaxKind.PipePipeToken => true,
        SyntaxKind.PipeEqualToken => true,
        SyntaxKind.HatToken => true,
        SyntaxKind.HatEqualToken => true,
        SyntaxKind.BadToken => true,
        SyntaxKind.NumberToken => true,
        SyntaxKind.StringToken => true,
        SyntaxKind.IdentifierToken => true,
        SyntaxKind.PlusToken => true,
        SyntaxKind.PlusEqualsToken => true,
        SyntaxKind.MinusToken => true,
        SyntaxKind.MinusEqualsToken => true,
        SyntaxKind.StarToken => true,
        SyntaxKind.StarEqualsToken => true,
        SyntaxKind.SlashToken => true,
        SyntaxKind.SlashEqualsToken => true,
        SyntaxKind.PercentToken => true,
        SyntaxKind.PercentEqualsToken => true,
        SyntaxKind.EqualsToken => true,
        SyntaxKind.EqualsEqualsToken => true,
        SyntaxKind.BangToken => true,
        SyntaxKind.BangEqualsToken => true,
        SyntaxKind.GreaterThanToken => true,
        SyntaxKind.GreaterThanEqualToken => true,
        SyntaxKind.LessThanToken => true,
        SyntaxKind.LessThanEqualToken => true,
        SyntaxKind.LeftParenthesisToken => true,
        SyntaxKind.RightParenthesisToken => true,
        SyntaxKind.LeftBracketToken => true,
        SyntaxKind.RightBracketToken => true,
        SyntaxKind.LeftBraceToken => true,
        SyntaxKind.RightBraceToken => true,
        SyntaxKind.DotToken => true,
        SyntaxKind.CommaToken => true,
        _ => false,
    };

    // TODO: Keywords!
    public static bool IsKeyword(this SyntaxKind kind) => false;

    public static IEnumerable<SyntaxKind> AllKinds() => Enum.GetValues<SyntaxKind>();
    public static IEnumerable<SyntaxKind> Tokens() => AllKinds().Where(k => k.IsToken());

    public static IEnumerable<SyntaxKind> BinaryOperators()
        => AllKinds().Where(k => k.GetBinaryOperatorPrecedence() > 0);

    public static IEnumerable<SyntaxKind> UnaryOperators()
        => AllKinds().Where(k => k.GetUnaryOperatorPrecedence() > 0);
}
