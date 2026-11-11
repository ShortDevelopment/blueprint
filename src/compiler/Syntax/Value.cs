using System.Diagnostics;

namespace BluePrint.Compiler.Syntax;

[DebuggerDisplay($"{{{nameof(Value)}}}")]
public readonly union PropertyValue(Binding, Expression, Menu, ObjectLiteral, Translated, Flags, TypeName, StringLiteral, Number, Identifier) : ISyntaxVisitable
{
    public void Visit(ISyntaxVisitor visitor)
        => ((ISyntaxVisitable)Value).Visit(visitor);

    public void VisitChildren(ISyntaxVisitor visitor)
        => ((ISyntaxVisitable)Value).VisitChildren(visitor);

    public override string ToString() => Value?.ToString() ?? "null";
}
public sealed record Translated(string Value, string? Context) : ISyntaxVisitable
{
    public void Visit(ISyntaxVisitor visitor)
        => visitor.VisitTranslated(this);

    public void VisitChildren(ISyntaxVisitor visitor) { }
}

public sealed record Flags(IReadOnlyList<Identifier> Values) : ISyntaxVisitable
{
    public void Visit(ISyntaxVisitor visitor)
        => visitor.VisitFlags(this);

    public void VisitChildren(ISyntaxVisitor visitor)
    {
        foreach (var flag in Values)
            visitor.VisitIdentifier(flag);
    }
}

public sealed record StringLiteral(string Value) : ISyntaxVisitable
{
    public void Visit(ISyntaxVisitor visitor)
        => visitor.VisitString(this);

    public void VisitChildren(ISyntaxVisitor visitor) { }
}

public sealed record Number(double Value) : ISyntaxVisitable
{
    public void Visit(ISyntaxVisitor visitor)
        => visitor.VisitNumber(this);

    public void VisitChildren(ISyntaxVisitor visitor) { }
}

public sealed record Identifier(string Value) : ISyntaxVisitable
{
    public void Visit(ISyntaxVisitor visitor)
        => visitor.VisitIdentifier(this);

    public void VisitChildren(ISyntaxVisitor visitor) { }
}

[DebuggerDisplay($"{{{nameof(Value)}}}")]
public readonly union StringValue(StringLiteral, Translated) : ISyntaxVisitable
{
    public void Visit(ISyntaxVisitor visitor)
        => ((ISyntaxVisitable)Value).Visit(visitor);

    public void VisitChildren(ISyntaxVisitor visitor)
        => ((ISyntaxVisitable)Value).VisitChildren(visitor);

    public override string ToString() => Value?.ToString() ?? "null";
}
