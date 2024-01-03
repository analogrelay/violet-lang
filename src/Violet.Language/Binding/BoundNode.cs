namespace Violet.Language.Binding;

abstract record BoundNode
{
    public virtual void FormatTo(TextWriter writer, int indent)
    {
        var self = FormatSelf();
        writer.WriteLine($"{new string(' ', indent)}{self}");
        foreach (var child in GetChildren())
        {
            child.FormatTo(writer, indent + 2);
        }
    }

    protected virtual string FormatSelf() => $"{GetType().Name}";

    protected virtual IEnumerable<BoundNode> GetChildren() => Array.Empty<BoundNode>();
}
