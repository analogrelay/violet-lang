using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Violet.Language;

static class RequireExtensions
{
    /// <summary>
    /// Asserts that the given value is not null and throws an <see cref="UnreachableException"/> if it is.
    /// </summary>
    /// <param name="value">The value to be asserted.</param>
    /// <param name="expression">The expression 'value' represents</param>
    [return: NotNull]
    public static T Require<T>([NotNull] this T? value, [CallerArgumentExpression(nameof(value))] string? expression = null)
    {
        return value is not null
            ? value
            : throw new UnreachableException(expression is null
                ? "Value is null, but was expected to be non-null"
                : $"'{expression}' is null, but was expected to be non-null");
    }

    /// <summary>
    /// Asserts that the given value is a <typeparamref name="T"/> and throws an <see cref="UnreachableException"/> if it is.
    /// </summary>
    /// <param name="value">The value to be asserted.</param>
    /// <param name="expression">The expression 'value' represents</param>
    [return: NotNull]
    public static T Require<T>([NotNull] this object? value, [CallerArgumentExpression(nameof(value))] string? expression = null)
        where T: notnull
    {
        return value is null
            ? throw new UnreachableException(expression is null
                ? "Value is null, but was expected to be non-null"
                : $"'{expression}' is null, but was expected to be non-null")
            : value is T t
                ? t
                : throw new UnreachableException(expression is null
                    ? $"Value is of type '{value.GetType().Name}', but was expected to be of type '{typeof(T).Name}'"
                    : $"'{expression}' is of type '{value.GetType().Name}', but was expected to be of type '{typeof(T).Name}'");
    }
}
