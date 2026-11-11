namespace BluePrint.Compiler.Syntax;

public interface ISyntaxVisitor
{
    void VisitDocument(Document document)
        => document.VisitChildren(this);

    void VisitUsing(Using @using)
        => @using.VisitChildren(this);


    void VisitTemplate(Template template)
        => template.VisitChildren(this);

    void VisitObject(ObjectLiteral @object)
        => @object.VisitChildren(this);


    void VisitChild(Child child)
        => child.VisitChildren(this);

    void VisitProperty(Property property)
        => property.VisitChildren(this);

    void VisitSignal(Signal signal)
        => signal.VisitChildren(this);

    void VisitBinding(Binding binding)
        => binding.VisitChildren(this);


    void VisitMenu(Menu menu)
        => menu.VisitChildren(this);

    void VisitMenuChild(MenuChild child)
    {
        switch (child)
        {
            case MenuItem item:
                VisitMenuItem(item);
                break;

            case MenuSection section:
                VisitMenuSection(section);
                break;

            case MenuSubMenu subMenu:
                VisitMenuSubMenu(subMenu);
                break;
        }
    }

    void VisitMenuItem(MenuItem item)
        => item.VisitChildren(this);

    void VisitMenuSection(MenuSection section)
        => section.VisitChildren(this);

    void VisitMenuSubMenu(MenuSubMenu subMenu)
        => subMenu.VisitChildren(this);

    void VisitMenuAttribute(MenuAttribute attribute)
        => attribute.VisitChildren(this);


    void VisitTryExpression(TryExpression expression)
        => expression.VisitChildren(this);

    void VisitClosureExpression(ClosureExpression expression)
        => expression.VisitChildren(this);

    void VisitLookupExpression(LookupExpression expression)
        => expression.VisitChildren(this);

    void VisitCastExpression(CastExpression expression)
        => expression.VisitChildren(this);


    void VisitTypeName(TypeName typeName)
        => typeName.VisitChildren(this);

    void VisitFlags(Flags flags)
        => flags.VisitChildren(this);

    void VisitTranslated(Translated translated)
        => translated.VisitChildren(this);

    void VisitIdentifier(Identifier identifier)
        => identifier.VisitChildren(this);

    void VisitNumber(Number number)
        => number.VisitChildren(this);

    void VisitString(StringLiteral @string)
        => @string.VisitChildren(this);
}

public interface ISyntaxVisitable
{
    void Visit(ISyntaxVisitor visitor);
    void VisitChildren(ISyntaxVisitor visitor);
}
