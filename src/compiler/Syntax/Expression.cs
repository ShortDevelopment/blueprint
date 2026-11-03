namespace BluePrint.Compiler.Syntax;

public readonly union Expression(Translated, TryExpression, ClosureExpression, TypeName, StringLiteral, Number, Identifier, LookupExpression, CastExpression);

public sealed record TryExpression(IReadOnlyList<Expression> Expressions);
public sealed record ClosureExpression(string Name, IReadOnlyList<Expression> Arguments);

public sealed record LookupExpression(Expression Expression, string Property);
public sealed record CastExpression(Expression Expression, TypeName TypeName);
