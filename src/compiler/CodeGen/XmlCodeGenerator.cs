using BluePrint.Compiler.Syntax;
using System.Globalization;
using System.Xml;

namespace BluePrint.Compiler.CodeGen;

public sealed class XmlCodeGenerator(XmlWriter writer) : ISyntaxVisitor, IDisposable
{
    private readonly XmlWriter _writer = writer;

    void ISyntaxVisitor.VisitDefault<T>(T element)
        => throw new NotImplementedException();

    public void Visit(Document document)
        => document.Visit(this);

    void ISyntaxVisitor.VisitDocument(Document document)
    {
        _writer.WriteStartDocument();
        {
            _writer.WriteStartElement("interface");

            if (document.TranslationDomain is { } translationDomain)
                _writer.WriteAttributeString("domain", translationDomain);

            foreach (var @using in document.Usings)
                @using.Visit(this);

            foreach (var element in document.Elements)
                element.Visit(this);

            _writer.WriteEndElement();
        }
        _writer.WriteEndDocument();
    }

    void ISyntaxVisitor.VisitUsing(Using @using)
    {
        // TODO
    }

    void ISyntaxVisitor.VisitObject(ObjectLiteral @object)
    {
        _writer.WriteStartElement("object");
        {
            _writer.WriteAttributeString("class", Format(@object.TypeName));

            if (@object.Id is { } id)
                _writer.WriteAttributeString("id", id);

            foreach (var element in @object.Content)
                element.Visit(this);
        }
        _writer.WriteEndElement();
    }

    void ISyntaxVisitor.VisitTemplate(Template template)
    {
        _writer.WriteStartElement("template");
        {
            _writer.WriteAttributeString("class", Format(template.TypeName));

            if (template.BaseName is { } parent)
                _writer.WriteAttributeString("parent", Format(parent));

            foreach (var element in template.Content)
                element.Visit(this);
        }
        _writer.WriteEndElement();
    }

    void ISyntaxVisitor.VisitProperty(Property property)
    {
        _writer.WriteStartElement(property.Value is Binding ? "binding" : "property");
        {
            _writer.WriteAttributeString("name", property.Name);

            if (property.Value is Translated)
                _writer.WriteAttributeString("translatable", "yes");

            if (property.Value is Translated { Context: { } translationContext })
                _writer.WriteAttributeString("context", translationContext);

            switch (property.Value)
            {
                case Binding binding:
                    if (binding.Flags != 0)
                        throw new NotImplementedException();
                    binding.Expression.Visit(this);
                    break;

                case Expression expression:
                    expression.Visit(this);
                    break;

                case Menu menu:
                    menu.Visit(this);
                    break;

                case ObjectLiteral @object:
                    @object.Visit(this);
                    break;

                case Translated translated:
                    _writer.WriteString(translated.Value);
                    break;

                case Flags flags:
                    _writer.WriteString(string.Join('|', flags.Values.Select(x => x.Value)));
                    break;

                case TypeName typeName:
                    _writer.WriteString(Format(typeName));
                    break;

                case StringLiteral stringLiteral:
                    _writer.WriteString(stringLiteral.Value);
                    break;

                case Number number:
                    _writer.WriteString(number.Value.ToString(CultureInfo.InvariantCulture));
                    break;

                case Identifier id:
                    _writer.WriteString(id.Value);
                    break;

                default:
                    throw new NotImplementedException();
            }
        }
        _writer.WriteEndElement();
    }

    void ISyntaxVisitor.VisitSignal(Signal signal)
       => throw new NotImplementedException();

    void ISyntaxVisitor.VisitChild(Child child)
    {
        _writer.WriteStartElement("child");
        {
            if (child.Annotation is { } annotation)
            {
                if (annotation.IsInternal)
                    _writer.WriteAttributeString("internal-child", annotation.ChildType);
                else
                    _writer.WriteAttributeString("type", annotation.ChildType);
            }

            child.Value.Visit(this);
        }
        _writer.WriteEndElement();
    }

    void ISyntaxVisitor.VisitMenu(Menu menu)
    {
        _writer.WriteStartElement("menu");
        {
            if (menu.Id is { } id)
                _writer.WriteAttributeString("id", id);

            foreach (var child in menu.Children)
                child.Visit(this);
        }
        _writer.WriteEndElement();
    }

    void ISyntaxVisitor.VisitMenuItem(MenuItem item)
    {
        _writer.WriteStartElement("item");
        {
            foreach (var attribute in item.Attributes)
                attribute.Visit(this);
        }
        _writer.WriteEndElement();
    }

    void ISyntaxVisitor.VisitMenuAttribute(MenuAttribute attribute)
    {
        _writer.WriteStartElement("attribute");
        {
            _writer.WriteAttributeString("name", attribute.Name);

            if (attribute.Value is Translated)
                _writer.WriteAttributeString("translatable", "yes");

            if (attribute.Value is Translated { Context: { } translationContext })
                _writer.WriteAttributeString("context", translationContext);

            _writer.WriteString(attribute.Value switch
            {
                Translated translated1 => translated1.Value,
                StringLiteral stringLiteral => stringLiteral.Value,
            });
        }
        _writer.WriteEndElement();
    }

    void ISyntaxVisitor.VisitMenuSection(MenuSection section)
    {
        _writer.WriteStartElement("section");
        {
            if (section.Id is { } id)
                _writer.WriteAttributeString("id", id);

            foreach (var attribute in section.Attributes)
                attribute.Visit(this);

            foreach (var child in section.Children)
                child.Visit(this);
        }
        _writer.WriteEndElement();
    }

    void ISyntaxVisitor.VisitMenuSubMenu(MenuSubMenu subMenu)
    {
        _writer.WriteStartElement("submenu");
        {
            if (subMenu.Id is { } id)
                _writer.WriteAttributeString("id", id);

            foreach (var attribute in subMenu.Attributes)
                attribute.Visit(this);

            foreach (var child in subMenu.Children)
                child.Visit(this);
        }
        _writer.WriteEndElement();
    }

    private static string Format(TypeName typeName)
        => string.Join("", typeName.Parts);

    public void Dispose()
    {
        _writer.Dispose();
    }
}
