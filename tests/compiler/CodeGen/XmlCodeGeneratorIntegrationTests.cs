using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace BluePrint.Compiler.CodeGen;

public class XmlCodeGeneratorIntegrationTests
{
    [Test]
    public Task ObjectProperties_PreserveLiteralValuesAndEscapeXml()
        => AssertGeneratedXml(
            """
            using Gtk 4.0;

            Gtk.Label status {
                label: "A & B < C";
                selectable: true;
                xalign: 0.25;
            }
            """,
            """
            <interface>
              <object class="GtkLabel" id="status">
                <property name="label">A &amp; B &lt; C</property>
                <property name="selectable">true</property>
                <property name="xalign">0.25</property>
              </object>
            </interface>
            """);

    [Test]
    public Task Template_WritesParentAndChildObjects()
        => AssertGeneratedXml(
            """
            using Gtk 4.0;

            template $StatusPanel : Gtk.Box {
                Gtk.Label title {
                    label: "Status";
                }
            }
            """,
            """
            <interface>
              <template class="StatusPanel" parent="GtkBox">
                <child>
                  <object class="GtkLabel" id="title">
                    <property name="label">Status</property>
                  </object>
                </child>
              </template>
            </interface>
            """);

    [Test]
    public Task Menu_WritesNestedItemsAndTranslationContext()
        => AssertGeneratedXml(
            """
            using Gtk 4.0;

            menu app_menu {
                section {
                    item {
                        label: C_("menu", "Open");
                        action: "app.open";
                    }
                }
            }
            """,
            """
            <interface>
              <menu id="app_menu">
                <section>
                  <item>
                    <attribute name="label" translatable="yes" context="menu">Open</attribute>
                    <attribute name="action">app.open</attribute>
                  </item>
                </section>
              </menu>
            </interface>
            """);


    [Test]
    public Task Flags_AreWrittenAsPipeSeparatedNames()
        => AssertGeneratedXml(
            """
            using Gtk 4.0;

            Gtk.EventControllerScroll {
                flags: vertical | discrete;
            }
            """,
            """
            <interface>
              <object class="GtkEventControllerScroll">
                <property name="flags">vertical|discrete</property>
              </object>
            </interface>
            """);

    private static async Task AssertGeneratedXml(string source, string expectedXml)
    {
        StringBuilder output = new();
        using (var generator = new XmlCodeGenerator(XmlWriter.Create(output)))
        {
            generator.Visit(Parser.Parser.Parse(source));
        }

        var actual = XElement.Parse(output.ToString());
        var expected = XElement.Parse(expectedXml);
        await Assert.That(actual.ToString()).IsEqualTo(expected.ToString());
    }
}
