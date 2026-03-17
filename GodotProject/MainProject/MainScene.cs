using Godot;

namespace MultiAssemblyExample;

/// <summary>
/// Main scene script demonstrating usage of types from external assemblies.
/// </summary>
public partial class MainScene : Node
{
    [Export] public string DisplayName { get; set; } = "Enemy";
    
    public override void _Ready()
    {
        GD.Print("=== Multi-Assembly Example ===");
    }
}
