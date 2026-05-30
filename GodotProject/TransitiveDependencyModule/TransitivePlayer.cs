using Godot;

namespace TransitiveDependencyModule;

/// <summary>
/// Player run-state holder: score, current wave, simple win/lose state.
/// Stays as a plain Node so it can be a child of any player root.
/// </summary>
[GlobalClass]
public partial class TransitivePlayer : Node
{
    [Export] public int Score { get; private set; }
    [Export] public int Wave { get; private set; } = 1;

    [Signal] public delegate void ScoreChangedEventHandler(int score);
    [Signal] public delegate void WaveChangedEventHandler(int wave);

    public override void _Ready()
    {
        GD.Print($"[TransitiveDependencyModule] TransitivePlayer ready! Score={Score} Wave={Wave}");
    }

    public void AddScore(int amount)
    {
        Score += amount;
        EmitSignal(SignalName.ScoreChanged, Score);
    }

    public void NextWave()
    {
        Wave++;
        EmitSignal(SignalName.WaveChanged, Wave);
    }
}
