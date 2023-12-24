using System.Collections.Immutable;
using System.Runtime.InteropServices;
using System.Text;
using Violet.Language.Syntax;
using Violet.Language.Text;

namespace Violet.Language.Syntax;

class Tokenizer
{
    readonly SyntaxTree _syntaxTree;
    readonly SourceText _text;
    readonly TextWindow _window;
    readonly ImmutableArray<SyntaxTrivia>.Builder _triviaBuilder = ImmutableArray.CreateBuilder<SyntaxTrivia>();
    SyntaxToken? _next = null;

    public DiagnosticBuilder Diagnostics { get; } = new();

    public Tokenizer(SyntaxTree syntaxTree)
    {
        _syntaxTree = syntaxTree;
        _text = syntaxTree.Text;
        _window = new TextWindow(_text);
    }

    /// <summary>
    /// Peek at the next token in the stream, but does not advance to the next token.
    /// Returns <c>null</c> if end-of-file is reached.
    /// </summary>
    public SyntaxToken Peek()
    {
        if (_next is null)
        {
            _next = GetNextToken();
        }

        return _next;
    }

    /// <summary>
    /// Reads the next token in the stream, and advances to the next token.
    /// Returns <c>null</c> if end-of-file is reached.
    /// </summary>
    public SyntaxToken Next()
    {
        var token = Peek();

        // Clear out the buffer so that we are forced to parse the next token.
        _next = null;

        return token;
    }

    void ReadTrivia(bool leading)
    {
        void EmitTrivia(SyntaxKind kind)
        {
            _triviaBuilder.Add(new SyntaxTrivia(
                _syntaxTree,
                kind,
                _window.Start,
                _window.Content));
            _window.Advance();
        }

        void ReadSingleLineComment()
        {
            _window.Assert("//");
            _window.NextUntil('\r', '\n');
            if (!_window.EndOfFile)
            {
                _window.Next(); // Consume the '\r' or '\n'
                if (_window.Last == '\r' && _window.Peek() == '\n')
                {
                    _window.Next(); // Consume the '\n' of a '\r\n' pair
                }
            }

            // End of file before the end of a single-line comment is fine
            EmitTrivia(SyntaxKind.SingleLineCommentTrivia);
        }

        void ReadMultiLineComment()
        {
            _window.Assert("/*");
            _window.NextUntil('*');
            if (_window.EndOfFile)
            {
                // End of file before the end of a multi-line comment is an error
                Diagnostics.Add(
                    DiagnosticDescriptors.UnexpectedEndOfFile,
                    _window.Right,
                    "*/");
                EmitTrivia(SyntaxKind.BlockCommentTrivia);
                return;
            }

            _window.Assert('*'); // Consume the '*'
            if (_window.Peek() == '/')
            {
                _window.Next();
                EmitTrivia(SyntaxKind.BlockCommentTrivia);
            }
        }

        void ReadWhitespace()
        {
            _window.NextWhile(char.IsWhiteSpace);
            EmitTrivia(SyntaxKind.WhitespaceTrivia);
        }

        void ReadNewline()
        {
            _window.NextWhile('\r', '\n');
            EmitTrivia(SyntaxKind.NewlineTrivia);
        }

        bool ReadOneTrivia()
        {
            switch (_window.Peek())
            {
                case '\0':
                    return false;
                case '/':
                    var la = _window.Peek(1);
                    if (la == '/')
                    {
                        ReadSingleLineComment();
                    }
                    else if (la == '*')
                    {
                        ReadMultiLineComment();
                    }
                    else
                    {
                        return false;
                    }

                    return true;
                case '\r':
                case '\n':
                    ReadNewline();
                    return true;
                case ' ':
                case '\t':
                case { } x when char.IsWhiteSpace(x):
                    ReadWhitespace();
                    return true;
                default:
                    return false;
            }
        }

        _triviaBuilder.Clear();

        while (ReadOneTrivia())
        {
            // Repeat!
        }

        // Reset the text window. Anything consumed but not used as trivia should go back to the token.
        _window.Reset();
    }

    SyntaxToken GetNextToken()
    {
        ReadTrivia(leading: true);

        var leadingTrivia = _triviaBuilder.ToImmutable();
        var tokenStart = _window.Start;

        var (kind, value) = ReadToken();
        var tokenText = _window.Content;
        _window.Advance();

        ReadTrivia(leading: false);
        var trailingTrivia = _triviaBuilder.ToImmutable();


        return new(_syntaxTree, kind, tokenStart, tokenText, value, leadingTrivia, trailingTrivia);
    }

    (SyntaxKind Kind, object? Value) ReadToken()
    {
        // The Big Switch
        // This is where we decide what kind of token we're parsing, based on the character we see.
        switch (_window.Peek())
        {
            case '\0':
                return (SyntaxKind.EndOfFileMarker, null);
            case '"':
                return ParseStringLiteral();

            case '*': return UpToTwoOperator(SyntaxKind.StarToken, '=', SyntaxKind.StarEqualsToken);
            case '/': return UpToTwoOperator(SyntaxKind.SlashToken, '=', SyntaxKind.SlashEqualsToken);
            case '%': return UpToTwoOperator(SyntaxKind.PercentToken, '=', SyntaxKind.PercentEqualsToken);
            case '(': return SingleCharacterToken(SyntaxKind.LeftParenthesisToken);
            case ')': return SingleCharacterToken(SyntaxKind.RightParenthesisToken);
            case '[': return SingleCharacterToken(SyntaxKind.LeftBracketToken);
            case ']': return SingleCharacterToken(SyntaxKind.RightBracketToken);
            case '{': return SingleCharacterToken(SyntaxKind.LeftBraceToken);
            case '}': return SingleCharacterToken(SyntaxKind.RightBraceToken);
            case '.': return SingleCharacterToken(SyntaxKind.DotToken);
            case ',': return SingleCharacterToken(SyntaxKind.CommaToken);
            case '&': return UpToTwoOperator(SyntaxKind.AmpersandToken, '&', SyntaxKind.AmpersandAmpersandToken, '=', SyntaxKind.AmpersandEqualToken);
            case '|': return UpToTwoOperator(SyntaxKind.PipeToken, '|', SyntaxKind.PipePipeToken, '=', SyntaxKind.PipeEqualToken);
            case '^': return UpToTwoOperator(SyntaxKind.HatToken, '=', SyntaxKind.HatEqualToken);
            case '=': return UpToTwoOperator(SyntaxKind.EqualsToken, '=', SyntaxKind.EqualsEqualsToken);
            case '<': return UpToTwoOperator(SyntaxKind.LessThanToken, '=', SyntaxKind.LessThanEqualToken);
            case '>': return UpToTwoOperator(SyntaxKind.GreaterThanToken, '=', SyntaxKind.GreaterThanEqualToken);
            case '!': return UpToTwoOperator(SyntaxKind.BangToken, '=', SyntaxKind.BangEqualsToken);
            case '-': return ParsePlusMinus(SyntaxKind.MinusToken, SyntaxKind.MinusEqualsToken);
            case '+': return ParsePlusMinus(SyntaxKind.PlusToken, SyntaxKind.PlusEqualsToken);

            case '_':
            case var x when char.IsLetter(x):
                _window.NextWhile(c => char.IsLetter(c) || char.IsDigit(c) || c == '_');
                return (SyntaxKind.IdentifierToken, _window.Content);

            case var x when char.IsDigit(x):
                return ParseNumericLiteral();

            default:
                _window.Extend();
                Diagnostics.Add(
                    DiagnosticDescriptors.UnexpectedCharacter,
                    _window.Location,
                    _window.Content);
                return (SyntaxKind.UnknownMarker, null);
        }
    }

    (SyntaxKind kind, object? value) SingleCharacterToken(SyntaxKind kind)
    {
        _window.Extend();
        return (kind, null);
    }

    (SyntaxKind kind, object? value) UpToTwoOperator(
        SyntaxKind oneCharType,
        char secondChar,
        SyntaxKind twoCharType)
    {
        _window.Extend();
        if (_window.TryExtend(secondChar))
        {
            return (twoCharType, null);
        }

        return (oneCharType, null);
    }

    (SyntaxKind kind, object? value) UpToTwoOperator(
        SyntaxKind oneCharType,
        char secondChar1,
        SyntaxKind twoCharType1,
        char secondChar2,
        SyntaxKind twoCharType2)
    {
        _window.Extend();
        if (_window.TryExtend(secondChar1))
        {
            return (twoCharType1, null);
        }

        if (_window.TryExtend(secondChar2))
        {
            return (twoCharType2, null);
        }

        return (oneCharType, null);
    }

    (SyntaxKind Kind, object? Value) ParsePlusMinus(SyntaxKind operatorType, SyntaxKind compoundOperatorType)
    {
        _window.Extend();
        if (_window.TryExtend('='))
        {
            return (compoundOperatorType, null);
        }
        if (char.IsDigit(_window.Peek()))
        {
            return ParseNumericLiteral();
        }

        return (operatorType, null);
    }

    (SyntaxKind Kind, object Value) ParseNumericLiteral()
    {
        if (_window.Peek() is '-' or '+')
        {
            _window.Extend(1);
        }

        _window.NextWhile(c => char.IsDigit(c) || c == '_');

        // Probably a better way to do this, but 🤷
        try
        {
            var value = long.Parse(_window.Content.Replace("_", ""));
            return (SyntaxKind.NumberToken, value);
        }
        catch (OverflowException)
        {
            Diagnostics.Add(
                DiagnosticDescriptors.IntegerLiteralTooLarge,
                _window.Location,
                _window.Content);
            return (SyntaxKind.NumberToken, long.MaxValue);
        }
    }

    (SyntaxKind Kind, object Value) ParseStringLiteral()
    {
        string ReadStringLiteral()
        {
            var value = new StringBuilder();
            while (true)
            {
                switch (_window.Next())
                {
                    case '"':
                        // End of string
                        return value.ToString();
                    case '\\':
                        switch (_window.Peek())
                        {
                            case '"':
                                _window.Extend();
                                value.Append('"');
                                break;
                            case 'n':
                                _window.Extend();
                                value.Append('\n');
                                break;
                            case '\\':
                                _window.Extend();
                                value.Append('\\');
                                break;
                            case '\0':
                                Diagnostics.Add(
                                    DiagnosticDescriptors.UnterminatedStringLiteral,
                                    _window.Location);
                                return value.ToString();
                            case { } c:
                                // Emit an error, but accept the character
                                _window.Extend();
                                Diagnostics.Add(
                                    DiagnosticDescriptors.InvalidEscapeSequence,
                                    new(_window.Source, new(_window.End - 2, 2)),
                                    $"\\{c}");
                                // Continue, ignoring the escape sequence, to recover.
                                break;
                        }

                        break;
                    case '\r':
                    case '\n':
                        _window.Back(1);
                        Diagnostics.Add(
                            DiagnosticDescriptors.UnterminatedStringLiteral,
                            _window.Location);
                        return value.ToString();
                    case '\0':
                        Diagnostics.Add(
                            DiagnosticDescriptors.UnterminatedStringLiteral,
                            _window.Location);
                        return value.ToString();
                    case var c:
                        value.Append(c);
                        break;
                }
            }
        }

        _window.Assert('"');
        var parsed = ReadStringLiteral();
        return (SyntaxKind.StringToken, parsed);
    }
}
