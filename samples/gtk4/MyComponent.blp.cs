namespace SimpleGtkApp;

[GObject.Subclass<Gtk.Box>(qualifiedName: nameof(MyComponent))]
[Gtk.Template<Gtk.AssemblyResource>("MyComponent.ui")]
public partial class MyComponent
{
    [Gtk.Connect("my_label")]
    private Gtk.Label _label;

    partial void Initialize()
    {
        _label.Label_ = "With support for connected members…";
    }
}
