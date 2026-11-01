namespace BluePrint.Compiler;

internal static class Extensions
{
    public static TResult let<TSource, TResult>(this TSource source, Func<TSource, TResult> func) => func(source);
}
