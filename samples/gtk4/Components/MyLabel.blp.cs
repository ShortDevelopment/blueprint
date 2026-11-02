namespace SimpleGtkApp.Components;

[GObject.Subclass<Gtk.Label>(qualifiedName: nameof(MyLabel))]
[Gtk.Template<Gtk.AssemblyResource>("Components.MyLabel.ui")]
public partial class MyLabel
{
    partial void Initialize()
    {
        Label_ = "…and loading templates from GResources!";
    }
}
