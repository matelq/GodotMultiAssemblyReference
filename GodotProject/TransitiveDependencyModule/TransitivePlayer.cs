using Godot;

namespace TransitiveDependencyModule;

[GlobalClass]
public partial class TransitivePlayer : Node
{
    [Export] public int MaxHealth { get; set; } = 100;
    [Export] public int CurrentHealth { get; set; } = 100;
    [Export] public bool Invincible { get; set; }

    [Signal]
    public delegate void HealthChangedEventHandler(int oldHealth, int newHealth);

    [Signal]
    public delegate void DiedEventHandler();

    public override void _Ready()
    {
        GD.Print($"[InTreeGodotSdkModule] HealthComponent ready! MaxHealth={MaxHealth}");
    }

    public void TakeDamage(int amount)
    {
        if (Invincible) return;
        int oldHealth = CurrentHealth;
        CurrentHealth = Mathf.Max(0, CurrentHealth - amount);
        EmitSignal(SignalName.HealthChanged, oldHealth, CurrentHealth);
        if (CurrentHealth <= 0)
            EmitSignal(SignalName.Died);
    }
}
