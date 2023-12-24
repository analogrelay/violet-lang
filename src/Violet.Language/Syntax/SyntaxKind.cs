namespace Violet.Language.Syntax;

public enum SyntaxKind
{
    UnknownMarker = 0,

    // Trivia, nodes that have no semantic meaning.
    WhitespaceTrivia,
    SingleLineCommentTrivia,
    BlockCommentTrivia,
    NewlineTrivia,
    SkippedTextTrivia,

    // Marker tokens, which represent states the tokenizer is in.
    EndOfFileMarker,

    // Tokens, terminal nodes representing actual text.
    BadToken,
    NumberToken,
    StringToken,
    IdentifierToken,
    AmpersandToken,
    AmpersandAmpersandToken,
    AmpersandEqualToken,
    PipeToken,
    PipePipeToken,
    PipeEqualToken,
    HatToken,
    HatEqualToken,
    PlusToken,
    PlusEqualsToken,
    MinusToken,
    MinusEqualsToken,
    StarToken,
    StarEqualsToken,
    SlashToken,
    SlashEqualsToken,
    PercentToken,
    PercentEqualsToken,
    EqualsToken,
    EqualsEqualsToken,
    BangToken,
    BangEqualsToken,
    GreaterThanToken,
    GreaterThanEqualToken,
    LessThanToken,
    LessThanEqualToken,
    LeftParenthesisToken,
    RightParenthesisToken,
    LeftBracketToken,
    RightBracketToken,
    LeftBraceToken,
    RightBraceToken,
    DotToken,
    CommaToken,

    // Abstract Syntax Tree nodes.
    CompilationUnit,
    BinaryExpression,
    UnaryExpression,
    LiteralExpression,
    CallExpression,
    NameExpression,
    GlobalStatement,
    ExpressionStatement
}
