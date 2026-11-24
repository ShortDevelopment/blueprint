namespace BluePrint.Compiler.Syntax;

public interface ISyntaxVisitor
{
    void VisitDefault<T>(T element) where T : ISyntaxVisitable;

    void VisitDocument(Document document)
        => VisitDefault(document);

    void VisitUsing(Using @using)
        => VisitDefault(@using);


    void VisitTemplate(Template template)
        => VisitDefault(template);

    void VisitObject(ObjectLiteral @object)
        => VisitDefault(@object);


    void VisitChild(Child child)
        => VisitDefault(child);

    void VisitProperty(Property property)
        => VisitDefault(property);

    void VisitSignal(Signal signal)
        => VisitDefault(signal);

    void VisitBinding(Binding binding)
        => VisitDefault(binding);


    void VisitMenu(Menu menu)
        => VisitDefault(menu);


    void VisitMenuItem(MenuItem item)
        => VisitDefault(item);

    void VisitMenuSection(MenuSection section)
        => VisitDefault(section);

    void VisitMenuSubMenu(MenuSubMenu subMenu)
        => VisitDefault(subMenu);

    void VisitMenuAttribute(MenuAttribute attribute)
        => VisitDefault(attribute);


    void VisitTryExpression(TryExpression expression)
        => VisitDefault(expression);

    void VisitClosureExpression(ClosureExpression expression)
        => VisitDefault(expression);

    void VisitLookupExpression(LookupExpression expression)
        => VisitDefault(expression);

    void VisitCastExpression(CastExpression expression)
        => VisitDefault(expression);


    void VisitTypeName(TypeName typeName)
        => VisitDefault(typeName);

    void VisitFlags(Flags flags)
        => VisitDefault(flags);

    void VisitTranslated(Translated translated)
        => VisitDefault(translated);

    void VisitIdentifier(Identifier identifier)
        => VisitDefault(identifier);

    void VisitNumber(Number number)
        => VisitDefault(number);

    void VisitString(StringLiteral @string)
        => VisitDefault(@string);
}

public interface ISyntaxVisitable
{
    void Visit(ISyntaxVisitor visitor);
    void VisitChildren(ISyntaxVisitor visitor);
}
