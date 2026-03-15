using Godot;

namespace InTreeModule;

/// <summary>
/// Example script in an in-tree module assembly.
/// Source file is inside the Godot project (res://InTreeModule/EnemyController.cs),
/// so it appears in the FileSystem panel, Select Script dialog, and can be opened
/// in an external editor directly from Godot.
/// </summary>
[GlobalClass]
public partial class EnemyController : Node
{
    [Export] public int Health { get; set; } = 100;
    [Export] public float Speed { get; set; } = 5.0f;
    [Export] public string DisplayName { get; set; } = "Enemy";

    public override void _Ready()
    {
        GD.Print($"[InTreeModule] EnemyController ready! Health={Health}, Speed={Speed}, Name={DisplayName}");
    }

    public override void _Process(double delta)
    {
    }
}
