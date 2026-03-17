using Godot;

namespace TransitiveNuGetFromProject;

/// <summary>
/// Script in a NuGet package that is transitively referenced via InTreeModule.
/// Tests: ProjectReference → PackageReference chain.
/// </summary>
[GlobalClass]
public partial class BuffSystem : Node
{
    [Export] public float Duration { get; set; } = 10.0f;
    [Export] public float Strength { get; set; } = 1.5f;
    [Export] public string BuffName { get; set; } = "Speed Boost";

    public override void _Ready()
    {
        GD.Print($"[TransitiveNuGetFromProject] BuffSystem ready! {BuffName} duration={Duration} strength={Strength}");
    }
}