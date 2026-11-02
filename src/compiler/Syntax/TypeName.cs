namespace BluePrint.Compiler.Syntax;

public sealed record TypeName(bool IsExternal, IReadOnlyList<string> Parts)
{
    public override string ToString()
        => $"{(IsExternal ? "$" : "")}{string.Join('.', Parts)}";
}
