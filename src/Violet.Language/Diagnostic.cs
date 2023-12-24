namespace Violet.Language;

public enum DiagnosticSeverity
{
    /// <summary>
    /// An error is an problem that prevents the program from being compiled.
    /// </summary>
    Error,

    /// <summary>
    /// A warning is a problem that will, by default, prevent the program from being compiled.
    /// Unlike an <see cref="Error"/> though, it can be suppressed so that the compilation can continue.
    /// </summary>
    Warning,

    /// <summary>
    /// A lint is a problem that is purely stylistic and doesn't affect the compilation.
    /// Lints prevent compilation by default, but can be suppressed.
    /// </summary>
    Lint,
}

public record DiagnosticDescriptor(string Id, DiagnosticSeverity Severity, string Title, string MessageFormat)
{
    public string Format(IReadOnlyList<object> messageArgs)
    {
        var message = string.Format(MessageFormat, messageArgs);
        return $"{Severity} {Id}: {message}";
    }
}

public class Diagnostic
{
    object[] _messageArgs;

    public DiagnosticDescriptor Descriptor { get; }
    public TextLocation Location { get; }
    public IReadOnlyList<object> MessageArgs => _messageArgs;
    public string Message => string.Format(Descriptor.MessageFormat, _messageArgs);
    public string Id => Descriptor.Id;
    public string Title => Descriptor.Title;

    public Diagnostic(DiagnosticDescriptor descriptor, TextLocation location, object[] messageArgs)
    {
        Descriptor = descriptor;
        Location = location;
        _messageArgs = messageArgs;
    }

    public string Format()
    {
        return
            $"{Location.Text.FileName}[{Location.Span.Start} .. {Location.Span.End}] {Descriptor.Format(MessageArgs)}";
    }
}
