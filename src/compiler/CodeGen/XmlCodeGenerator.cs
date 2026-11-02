using BluePrint.Compiler.Syntax;
using System.Globalization;
using System.Xml;

namespace BluePrint.Compiler.CodeGen;

public sealed class XmlCodeGenerator(XmlWriter writer) : IDisposable
{
    private readonly XmlWriter _writer = writer;

    public void Visit(Document document)
    {
        _writer.WriteStartDocument();
        {
            _writer.WriteStartElement("interface");

            if (document.TranslationDomain is { } translationDomain)
                _writer.WriteAttributeString("domain", translationDomain);

            foreach (var @using in document.Usings)
                VisitUsing(@using);

            foreach (var element in document.Elements)
                VisitElement(element);

            _writer.WriteEndElement();
        }
        _writer.WriteEndDocument();
    }

    private void VisitUsing(Using @using)
    {
        // TODO
    }

    private void VisitElement(Element element)
    {
        switch (element)
        {
            case ObjectLiteral @object:
                VisitObject(@object);
                break;

            case Template template:
                VisitTemplate(template);
                break;

            case Menu menu:
                VisitMenu(menu);
                break;

            default:
                throw new NotImplementedException();
        }
    }

    private void VisitObject(ObjectLiteral @object)
    {
        _writer.WriteStartElement("object");
        {
            _writer.WriteAttributeString("class", Format(@object.TypeName));

            if (@object.Id is { } id)
                _writer.WriteAttributeString("id", id);

            foreach (var element in @object.Content)
                VisitObjectContentElement(element);
        }
        _writer.WriteEndElement();
    }

    private void VisitTemplate(Template template)
    {
        _writer.WriteStartElement("template");
        {
            _writer.WriteAttributeString("class", Format(template.TypeName));

            if (template.BaseName is { } parent)
                _writer.WriteAttributeString("parent", Format(parent));

            foreach (var element in template.Content)
                VisitObjectContentElement(element);
        }
        _writer.WriteEndElement();
    }

    private void VisitObjectContentElement(ObjectContentElements element)
    {
        switch (element)
        {
            case Property property:
                VisitProperty(property);
                break;

            case Signal signal:
                VisitSignal(signal);
                break;

            case Child child:
                VisitChild(child);
                break;

            default:
                throw new NotImplementedException();
        }
    }

    private void VisitProperty(Property property)
    {
        _writer.WriteStartElement("property");
        {
            _writer.WriteAttributeString("name", property.Name);

            if (property.Value is Translated)
                _writer.WriteAttributeString("translatable", "yes");

            if (property.Value is Translated { Context: { } translationContext })
                _writer.WriteAttributeString("context", translationContext);

            switch (property.Value)
            {
                case Menu menu:
                    VisitMenu(menu);
                    break;

                case ObjectLiteral @object:
                    VisitObject(@object);
                    break;

                case Translated translated:
                    _writer.WriteString(translated.Value);
                    break;

                case Flags flags:
                    _writer.WriteString(string.Join('|', flags.Values));
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

                case Bool boolean:
                    _writer.WriteString(boolean.Value ? "true" : "false");
                    break;

                case Null:
                    _writer.WriteString("null");
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

    private void VisitSignal(Signal signal)
        => throw new NotImplementedException();

    private void VisitChild(Child child)
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

            VisitObject(child.Value);
        }
        _writer.WriteEndElement();
    }

    private void VisitMenu(Menu menu)
    {
        _writer.WriteStartElement("menu");
        {
            if (menu.Id is { } id)
                _writer.WriteAttributeString("id", id);

            foreach (var child in menu.Children)
                VisitMenuChild(child);
        }
        _writer.WriteEndElement();
    }

    private void VisitMenuChild(MenuChild child)
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

    private void VisitMenuItem(MenuItem item)
    {
        _writer.WriteStartElement("item");
        {
            foreach (var attribute in item.Attributes)
                VisitMenuAttribute(attribute);
        }
        _writer.WriteEndElement();
    }

    private void VisitMenuAttribute(MenuAttribute attribute)
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

    private void VisitMenuSection(MenuSection section)
    {
        _writer.WriteStartElement("section");
        {
            if (section.Id is { } id)
                _writer.WriteAttributeString("id", id);

            foreach (var attribute in section.Attributes)
                VisitMenuAttribute(attribute);

            foreach (var child in section.Children)
                VisitMenuChild(child);
        }
        _writer.WriteEndElement();
    }

    private void VisitMenuSubMenu(MenuSubMenu subMenu)
    {
        _writer.WriteStartElement("section");
        {
            if (subMenu.Id is { } id)
                _writer.WriteAttributeString("id", id);

            foreach (var attribute in subMenu.Attributes)
                VisitMenuAttribute(attribute);

            foreach (var child in subMenu.Children)
                VisitMenuChild(child);
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
