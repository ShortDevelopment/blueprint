namespace BluePrint.Compiler;

internal static class Extensions
{
    public static TResult let<TSource, TResult>(this TSource source, Func<TSource, TResult> func) => func(source);
    public static string Join<T>(this IEnumerable<T> source, string separator) => string.Join(separator, source);
}
