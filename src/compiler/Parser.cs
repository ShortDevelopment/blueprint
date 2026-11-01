using System.Diagnostics;

namespace BluePrint.Compiler;

public ref struct Parser()
{
    public required Tokenizer Tokenizer;

    private T Expect<T>(Func<T, bool>? isValid = null) where T : Token
    {
        var next = Tokenizer.Next();
        if (next is not T token)
            throw new SyntaxError($"Expected token of type {typeof(T).Name}, got {next}");
        if (isValid?.Invoke(token) == false)
            throw new SyntaxError($"Invalid token of type {typeof(T).Name}");
        return token;
    }

    private T? TryTake<T>(Func<T, bool>? isValid = null) where T : Token
    {
        if (Tokenizer.Peek() is not T token)
            return null;
        if (isValid?.Invoke(token) == false)
            return null;

        var next = Tokenizer.Next();
        Debug.Assert(token == next);
        return token;
    }

    private IReadOnlyList<T> ParseBlock<T>(Func<T> parse)
    {
        Expect<Token.OpenBrace>();
        List<T> elements = [];
        while (TryTake<Token.CloseBrace>() is null)
            elements.Add(parse());
        return elements;
    }

    public Document Parse()
    {
        GtkDecl();

        List<Using> usings = [];
        while (Tokenizer.Peek() is Token.Identifier { Name: "using" })
            usings.Add(Using());

        string? translationDomain = null;
        if (Tokenizer.Peek() is Token.Identifier { Name: "translation-domain" })
            translationDomain = TranslationDomain();

        List<Element> elements = [];
    parseLoop:
        while (true)
        {
            switch (Tokenizer.Peek())
            {
                case Token.EOF:
                    break parseLoop;

                case Token.Identifier { Name: "template" }:
                    elements.Add(Template());
                    break;

                case Token.Identifier { Name: "menu" }:
                    elements.Add(Menu());
                    break;

                default:
                    elements.Add(Object());
                    break;
            }
        }

        Expect<Token.EOF>();

        return new Document()
        {
            Usings = usings,
            TranslationDomain = translationDomain,
            Elements = elements,
        };
    }

    private void GtkDecl()
    {
        Expect<Token.Identifier>(static x => x.Name == "using");
        Expect<Token.Identifier>(static x => x.Name == "Gtk");
        Expect<Token.Number>(static x => x.Value == 4.0);
        Expect<Token.Semicolon>();
    }

    private Using Using()
    {
        Expect<Token.Identifier>(static x => x.Name == "using");
        var @namespace = Expect<Token.Identifier>().Name;
        var version = Expect<Token.Number>().Value;
        Expect<Token.Semicolon>();
        return new(@namespace, version);
    }

    private string TranslationDomain()
    {
        Expect<Token.Identifier>(static x => x.Name == "translation-domain");
        var domain = Expect<Token.Identifier>().Name;
        Expect<Token.Semicolon>();
        return domain;
    }

    private Template Template()
    {
        Expect<Token.Identifier>(static x => x.Name == "template");

        return new Template()
        {
            TypeName = TypeName(),
            BaseName = TryTake<Token.Colon>() is { } ? TypeName() : null,
            Content = ObjectContent(),
        };
    }

    private Menu Menu()
    {
        Expect<Token.Identifier>(static x => x.Name == "menu");
        var id = TryTake<Token.Identifier>()?.Name;

        Expect<Token.OpenBrace>();

        List<MenuChild> children = [];
        while (TryTake<Token.CloseBrace>() is null)
        {
            children.Add(Tokenizer.Peek() switch
            {
                Token.Identifier { Name: "section" } => MenuSection(),
                Token.Identifier { Name: "submenu" } => MenuSubMenu(),
                Token.Identifier { Name: "item" } => MenuItem(),
                _ => throw new SyntaxError($"Invalid menu child '{Tokenizer.Peek()}'"),
            });
        }

        return new()
        {
            Id = id,
            Children = children,
        };
    }

    private MenuSubMenu MenuSubMenu()
    {
        Expect<Token.Identifier>(static x => x.Name == "submenu");
        var id = TryTake<Token.Identifier>()?.Name;

        var (children, attributes) = MenuChildrenAndAttributes();
        return new()
        {
            Id = id,
            Attributes = attributes,
            Children = children,
        };
    }

    private MenuSection MenuSection()
    {
        Expect<Token.Identifier>(static x => x.Name == "section");
        var id = TryTake<Token.Identifier>()?.Name;

        var (children, attributes) = MenuChildrenAndAttributes();
        return new()
        {
            Id = id,
            Attributes = attributes,
            Children = children,
        };
    }

    private (IReadOnlyList<MenuChild> children, IReadOnlyList<MenuAttribute> attributes) MenuChildrenAndAttributes()
    {
        Expect<Token.OpenBrace>();

        List<MenuChild> children = [];
        List<MenuAttribute> attributes = [];
        while (TryTake<Token.CloseBrace>() is null)
        {
            MenuChild? child = Tokenizer.Peek() switch
            {
                Token.Identifier { Name: "section" } => MenuSection(),
                Token.Identifier { Name: "submenu" } => MenuSubMenu(),
                Token.Identifier { Name: "item" } => MenuItem(),
                _ => null,
            };
            if (child != null)
                children.Add(child);
            else
                attributes.Add(MenuAttribute());
        }
        return (children, attributes);
    }

    private MenuItem MenuItem()
    {
        Expect<Token.Identifier>(static x => x.Name == "item");

        List<MenuAttribute> attributes = [];

        Expect<Token.OpenBrace>();
        while (TryTake<Token.CloseBrace>() is null)
            attributes.Add(MenuAttribute());

        return new()
        {
            Attributes = attributes,
        };
    }

    private MenuAttribute MenuAttribute()
    {
        var id = Expect<Token.Identifier>().Name;
        Expect<Token.Colon>();
        StringValue value = Tokenizer.Peek() switch
        {
            Token.OpenTranslatedString or Token.OpenContextTranslatedString => Translated(),
            Token.String => Expect<Token.String>().let(s => new String(s.Value)),
            _ => throw new SyntaxError($"Invalid menu attribute value '{Tokenizer.Peek()}'"),
        };
        Expect<Token.Semicolon>();
        return new MenuAttribute(id, value);
    }

    private Object Object()
    {
        return new Object()
        {
            TypeName = TypeName(),
            Id = TryTake<Token.Identifier>()?.Name,
            Content = ObjectContent(),
        };
    }

    private IReadOnlyList<ObjectContentElements> ObjectContent()
    {
        Expect<Token.OpenBrace>();

        List<ObjectContentElements> elements = [];
        while (TryTake<Token.CloseBrace>() is null)
        {
            // TODO: Extensions
            elements.Add(Tokenizer.Peek2() switch
            {
                Token.Colon => Property(),
                Token.DoubleColon or Token.LambdaArrow => Signal(),
                _ => Child(),
            });
        }

        return elements;
    }

    private Child Child()
    {
        ChildAnnotation? annotation = null;
        if (Tokenizer.Peek() is Token.OpenBracket)
            annotation = ChildAnnotation();

        return new(annotation, Object());
    }

    private ChildAnnotation ChildAnnotation()
    {
        Expect<Token.OpenBracket>();
        bool isInternal = TryTake<Token.Identifier>(static x => x.Name == "internal-child") is { };
        var type = Expect<Token.Identifier>().Name;
        // TODO: Extensions
        Expect<Token.CloseBracket>();
        return new(IsInternal: isInternal, ChildType: type);
    }

    private Property Property()
    {
        var id = Expect<Token.Identifier>().Name;
        Expect<Token.Colon>();

        PropertyValue value = Tokenizer.Peek() switch
        {
            // Binding
            Token.Identifier { Name: "bind" } => throw new NotImplementedException(),
            // ExprValue
            Token.Identifier { Name: "expr" } => throw new NotImplementedException(),
            // Menu
            Token.Identifier { Name: "menu" } => Menu(),

            // Translated
            Token.OpenTranslatedString or Token.OpenContextTranslatedString => Translated(),

            // Literals
            Token.Identifier { Name: "typeof" } => TypeLiteral(),
            Token.Identifier { Name: "true" } => Expect<Token.Identifier>().let(_ => new Bool(true)),
            Token.Identifier { Name: "false" } => Expect<Token.Identifier>().let(_ => new Bool(false)),
            Token.Identifier { Name: "null" } => Expect<Token.Identifier>().let(_ => new Null()),
            Token.String => Expect<Token.String>().let(s => new String(s.Value)),
            Token.Number or Token.Minus or Token.Plus => NumberLiteral(),

            _ => ObjectFlagsOrIdentifier(),
        };

        Expect<Token.Semicolon>();

        return new Property(Name: id, Value: value);
    }

    private Translated Translated()
    {
        string? context = null;
        if (TryTake<Token.OpenContextTranslatedString>() is { })
        {
            context = Expect<Token.String>().Value;
            Expect<Token.Comma>();
        }
        else
        {
            Expect<Token.OpenTranslatedString>();
        }

        var @string = Expect<Token.String>().Value;
        Expect<Token.CloseParentheses>();
        return new(Value: @string, Context: context);
    }

    private TypeName TypeLiteral()
    {
        Expect<Token.Identifier>(static x => x.Name == "typeof");
        Expect<Token.Less>();
        var typeName = TypeName();
        Expect<Token.Greater>();
        return typeName;
    }

    private Number NumberLiteral()
    {
        int sign = 1;
        switch (Tokenizer.Peek())
        {
            case Token.Minus:
                sign = -1;
                Expect<Token.Minus>();
                break;
            case Token.Plus:
                sign = 1;
                Expect<Token.Plus>();
                break;
        }

        return new(sign * Expect<Token.Number>().Value);
    }

    private PropertyValue ObjectFlagsOrIdentifier()
    {
        return Tokenizer.Peek2() switch
        {
            Token.Semicolon => Expect<Token.Identifier>().let(i => new Identifier(i.Name)),
            Token.Pipe => Flags(),
            _ => Object(),
        };
    }

    private Flags Flags()
    {
        List<string> values = [];
        do
        {
            values.Add(Expect<Token.Identifier>().Name);
        } while (TryTake<Token.Pipe>() is { });
        return new(values);
    }

    private Signal Signal()
    {
        var id = Expect<Token.Identifier>().Name;

        string? detail = null;
        if (TryTake<Token.DoubleColon>() is { })
            detail = Expect<Token.Identifier>().Name;

        Expect<Token.LambdaArrow>();

        var handler = Expect<Token.Identifier>().Name;

        Expect<Token.OpenParentheses>();

        string? @object = null;
        if (TryTake<Token.CloseParentheses>() is null)
        {
            @object = Expect<Token.Identifier>().Name;
        }

        Expect<Token.CloseParentheses>();

        SignalFlags flags = 0;
        while (TryTake<Token.Semicolon>() is null)
        {
            flags |= Expect<Token.Identifier>() switch
            {
                { Name: "after" } => SignalFlags.After,
                { Name: "swapped" } => SignalFlags.Swapped,
                { Name: "not-swapped" } => SignalFlags.NotSwapped,
                { Name: var flag } => throw new SyntaxError($"Invalid signal flag '{flag}'"),
            };
        }
        return new Signal(Name: id, Detail: detail, Handler: handler, Object: @object, Flags: flags);
    }

    private TypeName TypeName()
    {
        bool isExternal = TryTake<Token.Dollar>() is { };

        List<string> parts = [];
        do
        {
            parts.Add(Expect<Token.Identifier>().Name);
        } while (TryTake<Token.Dot>() is { });
        return new(IsExternal: isExternal, Parts: parts);
    }
}

public sealed class SyntaxError(string message) : Exception(message);

public sealed record TypeName(bool IsExternal, IReadOnlyList<string> Parts)
{
    public override string ToString()
        => $"{(IsExternal ? "$" : "")}{string.Join('.', Parts)}";
}

public sealed record Document
{
    public required IReadOnlyList<Using> Usings { get; init; }
    public required string? TranslationDomain { get; init; }
    public required IReadOnlyList<Element> Elements { get; init; }
}

public sealed record Using(string Namespace, double Version);

public closed record Element;

public sealed record Menu : Element
{
    public required string? Id { get; init; }
    public required IReadOnlyList<MenuChild> Children { get; init; }
}
public closed record MenuChild;
public sealed record MenuItem : MenuChild
{
    public required IReadOnlyList<MenuAttribute> Attributes { get; init; }
}
public sealed record MenuSection : MenuChild
{
    public required string? Id { get; init; }
    public required IReadOnlyList<MenuAttribute> Attributes { get; init; }
    public required IReadOnlyList<MenuChild> Children { get; init; }
}
public sealed record MenuSubMenu : MenuChild
{
    public required string? Id { get; init; }
    public required IReadOnlyList<MenuAttribute> Attributes { get; init; }
    public required IReadOnlyList<MenuChild> Children { get; init; }
}
public sealed record MenuAttribute(string Name, StringValue Value);

public sealed record Template : Element
{
    public required TypeName TypeName { get; init; }
    public required TypeName? BaseName { get; init; }
    public required IReadOnlyList<ObjectContentElements> Content { get; init; }
}
public sealed record Object : Element
{
    public required TypeName TypeName { get; init; }
    public required string? Id { get; init; }
    public required IReadOnlyList<ObjectContentElements> Content { get; init; }
}

public closed record ObjectContentElements;

public sealed record Child(ChildAnnotation? Annotation, Object Value) : ObjectContentElements;
public sealed record ChildAnnotation(bool IsInternal, string ChildType);

public sealed record Property(string Name, object Value) : ObjectContentElements;

public sealed record Signal(string Name, string? Detail, string Handler, string? Object, SignalFlags Flags) : ObjectContentElements;

[Flags]
public enum SignalFlags
{
    After = 1 << 1,
    Swapped = 1 << 2,
    NotSwapped = 1 << 3,
}

[DebuggerDisplay($"{{{nameof(Value)}}}")]
public readonly union PropertyValue(Menu, Object, Translated, Flags, TypeName, String, Number, Bool, Null, Identifier)
{
    public override string ToString() => Value?.ToString() ?? "null";
}
public sealed record Translated(string Value, string? Context);
public sealed record Flags(IReadOnlyList<string> Values);
public sealed record String(string Value);
public sealed record Number(double Value);
public sealed record Bool(bool Value);
public sealed record Identifier(string Value);
public sealed record Null;

[DebuggerDisplay($"{{{nameof(Value)}}}")]
public readonly union StringValue(String, Translated)
{
    public override string ToString() => Value?.ToString() ?? "null";
}
