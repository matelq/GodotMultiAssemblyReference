using Godot;

namespace MainProject;

/// <summary>
/// Main scene script demonstrating usage of types from external assemblies.
/// </summary>
public partial class MainScene : Node
{
    public override void _Ready()
    {
        GD.Print("=== Multi-Assembly Example ===");
        GD.Print($"  InTreeModule (Microsoft.NET.Sdk):  {typeof(InTreeModule.EnemyController)}");
        GD.Print($"  InTreeGodotSdkModule (Godot.NET.Sdk): {typeof(InTreeGodotSdkModule.HealthComponent)}");
        GD.Print($"  ExternalNuGetModule (NuGet package):   {typeof(ExternalNuGetModule.InventorySystem)}");
    }
}
