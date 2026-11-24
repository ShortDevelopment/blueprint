using BluePrint.Compiler.Syntax;
using System.Text;
using System.Xml;

namespace BluePrint.Compiler.CodeGen;

public class XmlCodeGeneratorTests
{
    private static readonly ObjectLiteral Label = new()
    {
        TypeName = new TypeName(false, ["Gtk", "Label"]),
        Id = null,
        Content = [],
    };

    [Test]
    public Task VisitDocument_WritesInterface()
    => AssertXml(
        () => new Document
        {
            Usings = [],
            TranslationDomain = null,
            Elements = [],
        },
        """<interface />""",
        ConformanceLevel.Document);

    [Test]
    public Task VisitDocument_WritesInterfaceWithTranslationDomain()
        => AssertXml(
            () => new Document
            {
                Usings = [],
                TranslationDomain = "my-app",
                Elements = [],
            },
            """<interface domain="my-app" />""",
            ConformanceLevel.Document);

    [Test]
    public Task VisitUsing_CurrentlyWritesNothing()
        => AssertXml(() => new Using("Adw", 1), "");


    #region Object

    [Test]
    public Task VisitObject_WritesClass()
        => AssertXml(
            () => Label with
            {
                Id = null,
                Content = [],
            },
            """<object class="GtkLabel" />""");

    [Test]
    public Task VisitObject_WritesClassAndId()
        => AssertXml(
            () => Label with
            {
                Id = "status",
                Content = [],
            },
            """<object class="GtkLabel" id="status" />""");

    [Test]
    public Task VisitObject_WritesClassAndIdAndContent()
        => AssertXml(
            () => Label with
            {
                Id = "status",
                Content = [new Property("label", new StringLiteral("A & B"))],
            },
            """<object class="GtkLabel" id="status"><property name="label">A &amp; B</property></object>""");

    #endregion Object


    #region Template

    [Test]
    public Task VisitTemplate_WritesClass()
        => AssertXml(
            () => new Template
            {
                TypeName = new TypeName(true, ["MyApp", "Controls", "MyControl"]),
                BaseName = null,
                Content = [],
            },
            """<template class="MyAppControlsMyControl" />""");

    [Test]
    public Task VisitTemplate_WritesClassAndParent()
        => AssertXml(
            () => new Template
            {
                TypeName = new TypeName(true, ["MyApp", "Controls", "MyControl"]),
                BaseName = new TypeName(false, ["Gtk", "Box"]),
                Content = [],
            },
            """<template class="MyAppControlsMyControl" parent="GtkBox" />""");

    [Test]
    public Task VisitTemplate_WritesClassAndParentAndContent()
        => AssertXml(
            () => new Template
            {
                TypeName = new TypeName(true, ["MyApp", "Controls", "MyControl"]),
                BaseName = new TypeName(false, ["Gtk", "Box"]),
                Content = [new Property("label", new StringLiteral("A & B"))],
            },
            """<template class="MyAppControlsMyControl" parent="GtkBox"><property name="label">A &amp; B</property></template>""");

    #endregion Template


    #region VisitProperty

    [Test]
    public Task VisitProperty_WritesTranslated()
        => AssertXml(
            () => new Property("label", new Translated("abc", Context: null)),
            """<property name="label" translatable="yes">abc</property>""");

    [Test]
    public Task VisitProperty_WritesTranslatedWithContext()
        => AssertXml(
            () => new Property("label", new Translated("abc", Context: "app-ctx")),
            """<property name="label" translatable="yes" context="app-ctx">abc</property>""");

    [Test]
    public Task VisitProperty_WritesSingleFlagWithoutSeparator()
        => AssertXml(
            () => new Property("flags", new Flags([new Identifier("vertical")])),
            """<property name="flags">vertical</property>""");

    [Test]
    public Task VisitProperty_WritesPipeSeparatedFlagNames()
        => AssertXml(
            () => new Property("flags", new Flags([new Identifier("vertical"), new Identifier("discrete")])),
            """<property name="flags">vertical|discrete</property>""");

    [Test]
    public Task VisitProperty_PreservesOrderForThreeFlags()
        => AssertXml(
            () => new Property("flags", new Flags([new Identifier("vertical"), new Identifier("discrete"), new Identifier("kinetic")])),
            """<property name="flags">vertical|discrete|kinetic</property>""");

    [Test]
    public Task VisitProperty_WritesTypeName()
        => AssertXml(
            () => new Property("type", new TypeName(IsExternal: false, ["Gtk", "Box"])),
            """<property name="type">GtkBox</property>""");

    [Test]
    public Task VisitProperty_WritesTypeName_IgnoreExternal()
        => AssertXml(
            () => new Property("type", new TypeName(IsExternal: true, ["MyApp", "Controls", "MyControl"])),
            """<property name="type">MyAppControlsMyControl</property>""");

    [Test]
    public Task VisitProperty_WriteString()
        => AssertXml(
            () => new Property("label", new StringLiteral("hey")),
            """<property name="label">hey</property>""");

    [Test]
    public Task VisitProperty_WriteNumber_Integer()
        => AssertXml(
            () => new Property("number", new Number(42)),
            """<property name="number">42</property>""");

    [Test]
    public Task VisitProperty_WriteNumber_Decimal()
        => AssertXml(
            () => new Property("number", new Number(42.123)),
            """<property name="number">42.123</property>""");

    [Test]
    public Task VisitProperty_WritesIdentifier()
        => AssertXml(
            () => new Property("flags", new Identifier("vertical")),
            """<property name="flags">vertical</property>""");

    #endregion VisitProperty


    [Test]
    public void VisitExpression_CurrentlyThrows()
    {
        using var generator = new XmlCodeGenerator(XmlWriter.Create(new StringBuilder()));
        Assert.Throws<NotImplementedException>(
            () => ((Expression)new Identifier("source")).Visit(generator)
        );
    }

    [Test]
    public void VisitSignal_CurrentlyThrows()
    {
        using var generator = new XmlCodeGenerator(XmlWriter.Create(new StringBuilder()));
        Assert.Throws<NotImplementedException>(
            () => new Signal("clicked", null, "handle_click", null, default).Visit(generator)
        );
    }


    #region Child

    [Test]
    public Task VisitChild_WritesChild()
        => AssertXml(
            () => new Child(Annotation: null, Label),
            """<child><object class="GtkLabel" /></child>""");

    [Test]
    public Task VisitChild_WritesChildWithType()
        => AssertXml(
            () => new Child(new ChildAnnotation(IsInternal: false, "title"), Label),
            """<child type="title"><object class="GtkLabel" /></child>""");

    [Test]
    public Task VisitChild_WritesWithInteralType()
        => AssertXml(
            () => new Child(new ChildAnnotation(IsInternal: true, "label"), Label),
            """<child internal-child="label"><object class="GtkLabel" /></child>""");

    #endregion Child


    #region Menu

    [Test]
    public Task VisitMenu_WritesEmpty_WhenEmpty()
        => AssertXml(
            () => new Menu { Id = null, Children = [] },
            """<menu />""");

    [Test]
    public Task VisitMenu_WritesId()
        => AssertXml(
            () => new Menu { Id = "app_menu", Children = [] },
            """<menu id="app_menu" />""");

    [Test]
    public Task VisitMenu_WritesIdAndChildren()
        => AssertXml(
            () => new Menu { Id = "app_menu", Children = [new MenuItem { Attributes = [] }] },
            """<menu id="app_menu"><item /></menu>""");

    [Test]
    public Task VisitMenuItem_WritesEmpty_WhenEmpty()
        => AssertXml(
            () => new MenuItem { Attributes = [] },
            """<item />""");

    [Test]
    public Task VisitMenuAttribute_WritesString()
        => AssertXml(
            () => new MenuAttribute("label", new StringLiteral("Open")),
            """<attribute name="label">Open</attribute>""");

    [Test]
    public Task VisitMenuAttribute_WritesTranslated()
        => AssertXml(
            () => new MenuAttribute("label", new Translated("Open", Context: null)),
            """<attribute name="label" translatable="yes">Open</attribute>""");

    [Test]
    public Task VisitMenuAttribute_WritesTranslatedWithContext()
        => AssertXml(
            () => new MenuAttribute("label", new Translated("Open", Context: "menu")),
            """<attribute name="label" translatable="yes" context="menu">Open</attribute>""");

    [Test]
    public Task VisitMenuSection_WritesEmpty_WhenEmpty()
        => AssertXml(
            () => new MenuSection { Id = null, Attributes = [], Children = [] },
            """<section />""");

    [Test]
    public Task VisitMenuSection_WritesId()
        => AssertXml(
            () => new MenuSection { Id = "app_menu", Attributes = [], Children = [] },
            """<section id="app_menu" />""");

    [Test]
    public Task VisitMenuSection_WritesIdAndAttribute()
        => AssertXml(
            () => new MenuSection { Id = "app_menu", Attributes = [new MenuAttribute(Name: "label", new StringLiteral("hey"))], Children = [] },
            """<section id="app_menu"><attribute name="label">hey</attribute></section>""");

    [Test]
    public Task VisitMenuSection_WritesIdAndChildren()
        => AssertXml(
            () => new MenuSection { Id = "app_menu", Attributes = [], Children = [new MenuItem { Attributes = [] }] },
            """<section id="app_menu"><item /></section>""");

    [Test]
    public Task VisitMenuSubMenu_WritesEmpty_WhenEmpty()
        => AssertXml(
            () => new MenuSubMenu { Id = null, Attributes = [], Children = [] },
            """<submenu />""");

    [Test]
    public Task VisitMenuSubMenu_WritesId()
        => AssertXml(
            () => new MenuSubMenu { Id = "app_menu", Attributes = [], Children = [] },
            """<submenu id="app_menu" />""");

    [Test]
    public Task VisitMenuSubMenu_WritesIdAndAttribute()
        => AssertXml(
            () => new MenuSubMenu { Id = "app_menu", Attributes = [new MenuAttribute(Name: "label", new StringLiteral("hey"))], Children = [] },
            """<submenu id="app_menu"><attribute name="label">hey</attribute></submenu>""");

    [Test]
    public Task VisitMenuSubMenu_WritesIdAndChildren()
        => AssertXml(
            () => new MenuSubMenu { Id = "app_menu", Attributes = [], Children = [new MenuItem { Attributes = [] }] },
            """<submenu id="app_menu"><item /></submenu>""");

    #endregion Menu

    private static async Task AssertXml(
        Func<ISyntaxVisitable> factory,
        string expected,
        ConformanceLevel conformanceLevel = ConformanceLevel.Fragment)
    {
        var output = new StringBuilder();
        var settings = new XmlWriterSettings
        {
            ConformanceLevel = conformanceLevel,
            OmitXmlDeclaration = true,
        };

        using (var generator = new XmlCodeGenerator(XmlWriter.Create(output, settings)))
        {
            factory().Visit(generator);
        }

        await Assert.That(output.ToString()).IsEqualTo(expected);
    }
}
