using SimpleGtkApp;

Gtk.Module.Initialize();
GirCore.Integration.Initialize();

var application = Gtk.Application.New("de.shortdev.blueprint.gtk4", Gio.ApplicationFlags.FlagsNone);
application.OnActivate += (sender, args) =>
{
    var window = Gtk.ApplicationWindow.New((Gtk.Application)sender);
    window.Title = "Gtk4 Window";
    window.SetDefaultSize(300, 300);
    window.Child = MyComponent.NewWithProperties([]);
    window.Show();
};

return application.RunWithSynchronizationContext(args);
