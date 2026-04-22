using Godot;

namespace ExternalNuGetModule;

/// <summary>
/// Example script in an external NuGet package assembly.
/// Source file is OUTSIDE the Godot project directory, so this script
/// gets a csharp:// path. It appears in the Add Node dialog and works
/// with [Export] properties, but cannot be opened in an external editor
/// from Godot (since the source is not available under res://).
/// </summary>
[GlobalClass, Icon("res://icon.svg")]
public partial class InventorySystem : Node
{
    [Export] public int MaxSlots { get; set; } = 20;
    [Export] public bool AllowStacking { get; set; } = true;

    public override void _Ready()
    {
        GD.Print($"[ExternalNuGetModule] InventorySystem ready! MaxSlots={MaxSlots}, Stacking={AllowStacking}");
    }

    public override void _Process(double delta)
    {
    }
}
