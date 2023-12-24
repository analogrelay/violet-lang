namespace Violet.Language;

public class SourceText
{
    readonly string _text;

    public char this[int index] => _text[index];
    public string this[Range range] => _text[range];
    public int Length => _text.Length;
    public string FileName { get; }

    SourceText(string text, string fileName)
    {
        _text = text;
        FileName = fileName;
    }

    public static SourceText From(string text, string fileName = "") => new(text, fileName);
}
