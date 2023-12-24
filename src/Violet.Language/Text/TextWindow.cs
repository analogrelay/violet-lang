using System.Diagnostics;

namespace Violet.Language.Text;

[DebuggerDisplay("{DebuggerDisplay,nq}")]
public class TextWindow
{
    int _start;
    int _length;

    internal string DebuggerDisplay => GetDebugDisplay();

    public SourceText Source { get; }
    public string Content => Source[Start .. End];
    public int Start => _start;
    public int Length => _length;
    public int End => _start + _length;
    public char Last => Length > 0 ? Source[End - 1] : '\0';
    public TextSpan Span => new(_start, _length);
    public TextLocation Location => new(Source, Span);
    public TextLocation Right
        => new(Source, new(Length > 0 ? End - 1 : Start, Length > 0 ? 1 : 0));
    public bool EndOfFile => _start + _length >= Source.Length;

    public TextWindow(SourceText source)
    {
        Source = source;
        _start = 0;
        _length = 0;
    }

    public string NextUntil(params char[] chars) =>
        NextUntil(chars.Contains);

    public string NextUntil(Func<char, bool> predicate)
        => NextWhile(c => !predicate(c));

    public string NextWhile(params char[] chars) =>
        NextWhile(chars.Contains);

    public string NextWhile(Func<char, bool> predicate)
    {
        var oldLength = _length;
        while (true)
        {
            var c = Peek();
            if (c == '\0' || !predicate(c))
            {
                break;
            }
            Extend(1);
        }

        return Source[(Start + oldLength) .. End];
    }

    public char Next()
    {
        var chr = Peek();
        Extend(1);
        return chr;
    }

    public bool TryExtend(char candidate)
    {
        if (Peek() == candidate)
        {
            Extend(1);
            return true;
        }

        return false;
    }

    public char Peek(int offset = 0)
    {
        var index = End + offset;
        return index < Source.Length
            ? Source[index]
            : '\0';
    }

    public string Next(int count)
    {
        var oldLength = _length;
        Extend(count);
        return Source[(_start + oldLength) .. (_start + _length)];
    }

    public void Back(int count)
    {
        if(count > _length)
        {
            throw new ArgumentOutOfRangeException(nameof(count));
        }

        _length -= count;
    }

    public void Extend(int count = 1)
    {
        var newLength = _length + count;
        _length = (_start + newLength) < Source.Length
            ? newLength
            : Source.Length - _start;
    }

    public void Advance()
    {
        _start += _length;
        _length = 0;
    }

    string GetDebugDisplay()
    {
        var displayStart = Math.Max(_start - 10, 0);
        var displayEnd = Math.Min((_start + _length) + 10, Source.Length);
        return
            $"{Source[displayStart.._start]}«{Source[_start .. (_start + _length)]}»{Source[(_start + _length) .. displayEnd]}";
    }

    public void Assert(char c)
    {
        Trace.Assert(Peek() == c);
        Extend();
    }

    public void Assert(string s)
    {
        #if TRACE
        var oldLength = _length;
        #endif

        Extend(s.Length);

        #if TRACE
        Trace.Assert(Source[(_start + oldLength) .. (_start + _length)] == s);
        #endif
    }

    public void Reset()
    {
        _length = 0;
    }
}
