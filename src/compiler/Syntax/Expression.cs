namespace BluePrint.Compiler.Syntax;

public readonly union Expression(Translated, TryExpression, ClosureExpression, TypeName, StringLiteral, Number, Identifier, LookupExpression, CastExpression) : ISyntaxVisitable
{
    public void Visit(ISyntaxVisitor visitor)
        => ((ISyntaxVisitable)Value).Visit(visitor);

    public void VisitChildren(ISyntaxVisitor visitor)
        => ((ISyntaxVisitable)Value).VisitChildren(visitor);
}

public sealed record TryExpression(IReadOnlyList<Expression> Expressions) : ISyntaxVisitable
{
    public void Visit(ISyntaxVisitor visitor)
        => visitor.VisitTryExpression(this);

    public void VisitChildren(ISyntaxVisitor visitor)
    {
        foreach (var expression in Expressions)
            expression.Visit(visitor);
    }
}

public sealed record ClosureExpression(string Name, IReadOnlyList<Expression> Arguments) : ISyntaxVisitable
{
    public void Visit(ISyntaxVisitor visitor)
        => visitor.VisitClosureExpression(this);

    public void VisitChildren(ISyntaxVisitor visitor)
    {
        foreach (var arg in Arguments)
            arg.Visit(visitor);
    }
}

public sealed record LookupExpression(Expression Expression, string Property) : ISyntaxVisitable
{
    public void Visit(ISyntaxVisitor visitor)
        => visitor.VisitLookupExpression(this);

    public void VisitChildren(ISyntaxVisitor visitor)
    {
        Expression.Visit(visitor);
    }
}

public sealed record CastExpression(Expression Expression, TypeName TypeName) : ISyntaxVisitable
{
    public void Visit(ISyntaxVisitor visitor)
        => visitor.VisitCastExpression(this);

    public void VisitChildren(ISyntaxVisitor visitor)
    {
        Expression.Visit(visitor);
    }
}
