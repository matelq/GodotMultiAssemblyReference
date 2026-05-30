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

    public bool IsActive { get; private set; }
    public float TimeRemaining { get; private set; }

    [Signal] public delegate void BuffActivatedEventHandler(string buffName, float strength);
    [Signal] public delegate void BuffExpiredEventHandler(string buffName);

    public override void _Ready()
    {
        GD.Print($"[TransitiveNuGetFromProject] BuffSystem ready! {BuffName} duration={Duration} strength={Strength}");
    }

    public override void _Process(double delta)
    {
        if (!IsActive) return;
        TimeRemaining -= (float)delta;
        if (TimeRemaining <= 0f)
        {
            IsActive = false;
            TimeRemaining = 0f;
            EmitSignal(SignalName.BuffExpired, BuffName);
        }
    }

    public void Activate()
    {
        Activate(Duration, Strength);
    }

    public void Activate(float duration, float strength)
    {
        Duration = duration;
        Strength = strength;
        TimeRemaining = duration;
        IsActive = true;
        EmitSignal(SignalName.BuffActivated, BuffName, strength);
    }

    public float CurrentMultiplier => IsActive ? Strength : 1.0f;
}
