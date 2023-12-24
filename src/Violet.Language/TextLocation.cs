namespace Violet.Language;

public record TextSpan(int Start, int Length)
{
    public int End => Start + Length;
    public override string ToString() => $"{Start} .. {End}";

    public static TextSpan FromBounds(int start, int end)
        => new(start, end - start);
}

public record TextLocation(SourceText Text, TextSpan Span);
