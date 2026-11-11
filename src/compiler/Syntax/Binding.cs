namespace BluePrint.Compiler.Syntax;

public sealed record Binding(Expression Expression, BindingFlags Flags) : ISyntaxVisitable
{
    public void Visit(ISyntaxVisitor visitor)
        => visitor.VisitBinding(this);

    public void VisitChildren(ISyntaxVisitor visitor)
    {
        Expression.Visit(visitor);
    }
}

[Flags]
public enum BindingFlags
{
    Inverted = 1 << 1,
    Bidirectional = 1 << 2,
    NoSyncCreate = 1 << 3,
}
