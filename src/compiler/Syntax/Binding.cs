namespace BluePrint.Compiler.Syntax;

public sealed record Binding(Expression Expression, BindingFlags Flags);

[Flags]
public enum BindingFlags
{
    Inverted = 1 << 1,
    Bidirectional = 1 << 2,
    NoSyncCreate = 1 << 3,
}
