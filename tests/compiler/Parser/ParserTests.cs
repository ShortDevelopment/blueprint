using BluePrint.Compiler.Syntax;

namespace BluePrint.Compiler.Parser;

public class ParserTests
{
    [Test]
    public async Task EmptyDocument_ShouldThrow_WhenGtkDeclarationIsMissing()
    {
        Assert.Throws<Exception>(() => Parser.Parse(""));
    }

    [Test]
    public Task EmptyDocument_ShouldSucceed_WhenGtkDeclarationIsPresent()
        => AssertSyntax("""
            using Gtk 4.0;
            """,
            new Document()
            {
                Usings = [],
                TranslationDomain = null,
                Elements = [],
            });

    [Test]
    public Task EmptyDocument()
        => AssertSyntax("""
            using Gtk 4.0;
            using Adw 1.0;
            """,
            new Document()
            {
                Usings = [new Using("Adw", 1)],
                TranslationDomain = null,
                Elements = [],
            });

    [Test]
    public Task TranslationDomain()
        => AssertSyntax("""
            using Gtk 4.0;
            
            translation-domain "test-domain";
            """,
            new Document()
            {
                Usings = [],
                TranslationDomain = "test-domain",
                Elements = [],
            });

    // TODO: Template
    // TODO: Object
    // TODO: Menu

    [Test]
    public Task PropertyValue_String()
        => AssertSyntax("""
            using Gtk 4.0;
            
            Gtk.Label {
                label: "text";
            }
            """,
            new Document()
            {
                Usings = [],
                TranslationDomain = null,
                Elements = [
                    new ObjectLiteral() {
                        TypeName = new(IsExternal: false, ["Gtk", "Label"]),
                        Id = null,
                        Content = [
                            new Property("label", new StringLiteral("text"))
                        ]
                    }
                ],
            });

    [Test]
    public Task PropertyValue_Translated()
        => AssertSyntax("""
            using Gtk 4.0;
            
            Gtk.Label {
                label: _("text");
            }
            """,
            new Document()
            {
                Usings = [],
                TranslationDomain = null,
                Elements = [
                    new ObjectLiteral() {
                        TypeName = new(IsExternal: false, ["Gtk", "Label"]),
                        Id = null,
                        Content = [
                            new Property("label", new Translated("text", Context: null))
                        ]
                    }
                ],
            });

    [Test]
    public Task PropertyValue_TranslatedWithContext()
        => AssertSyntax("""
            using Gtk 4.0;
            
            Gtk.Label {
                label: C_("app-ctx", "text");
            }
            """,
            new Document()
            {
                Usings = [],
                TranslationDomain = null,
                Elements = [
                    new ObjectLiteral() {
                        TypeName = new(IsExternal: false, ["Gtk", "Label"]),
                        Id = null,
                        Content = [
                            new Property("label", new Translated("text", Context: "app-ctx"))
                        ]
                    }
                ],
            });

    [Test]
    public Task PropertyValue_TypeOf()
        => AssertSyntax("""
            using Gtk 4.0;
            
            Gtk.Label {
                type: typeof<Box>;
                type1: typeof<$Box>;
                type2: typeof<Gtk.Box>;
                type3: typeof<$MyApp.Box>;
                type4: typeof<$MyApp.Controls.Box>; // Extension of official Blueprint syntax
            }
            """,
            new Document()
            {
                Usings = [],
                TranslationDomain = null,
                Elements = [
                    new ObjectLiteral() {
                        TypeName = new(IsExternal: false, ["Gtk", "Label"]),
                        Id = null,
                        Content = [
                            new Property("type", new TypeName(IsExternal: false, ["Box"])),
                            new Property("type1", new TypeName(IsExternal: true, ["Box"])),
                            new Property("type2", new TypeName(IsExternal: false, ["Gtk", "Box"])),
                            new Property("type3", new TypeName(IsExternal: true, ["MyApp", "Box"])),
                            new Property("type4", new TypeName(IsExternal: true, ["MyApp", "Controls", "Box"])),
                        ]
                    }
                ],
            });

    [Test]
    public Task PropertyValue_Number()
        => AssertSyntax("""
            using Gtk 4.0;
            
            Gtk.Label {
                number: 42.123;
                number1: +42.123;
                number2: -42.123;
            }
            """,
            new Document()
            {
                Usings = [],
                TranslationDomain = null,
                Elements = [
                    new ObjectLiteral() {
                        TypeName = new(IsExternal: false, ["Gtk", "Label"]),
                        Id = null,
                        Content = [
                            new Property("number", new Number(42.123)),
                            new Property("number1", new Number(+42.123)),
                            new Property("number2", new Number(-42.123)),
                        ]
                    }
                ],
            });

    [Test]
    public Task PropertyValue_Identifier()
        => AssertSyntax("""
            using Gtk 4.0;
            
            Gtk.Label {
                align: vertical;
                align1: true;
                align2: false;
                align3: null;
            }
            """,
            new Document()
            {
                Usings = [],
                TranslationDomain = null,
                Elements = [
                    new ObjectLiteral() {
                        TypeName = new(IsExternal: false, ["Gtk", "Label"]),
                        Id = null,
                        Content = [
                            new Property("align", new Identifier("vertical")),
                            new Property("align1", new Identifier("true")),
                            new Property("align2", new Identifier("false")),
                            new Property("align3", new Identifier("null")),
                        ]
                    }
                ],
            });

    [Test]
    public Task PropertyValue_Flags()
        => AssertSyntax("""
            using Gtk 4.0;
            
            Gtk.Label {
                align: vertical | top;
                align1: vertical | top | abc;
            }
            """,
            new Document()
            {
                Usings = [],
                TranslationDomain = null,
                Elements = [
                    new ObjectLiteral() {
                        TypeName = new(IsExternal: false, ["Gtk", "Label"]),
                        Id = null,
                        Content = [
                            new Property("align", new Flags([new Identifier("vertical"), new Identifier("top")])),
                            new Property("align1", new Flags([new Identifier("vertical"), new Identifier("top"), new Identifier("abc")])),
                        ]
                    }
                ],
            });

    private static async Task AssertSyntax(string input, Document document)
    {
        await Assert.That(Parser.Parse(input)).IsEquivalentTo(document);
    }
}
