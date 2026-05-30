using ExternalNuGetModule;
using Godot;
using InTreeGodotSdkModule;
using TransitiveDependencyModule;
using TransitiveNuGetFromProject;

namespace MultiAssemblyExample;

/// <summary>
/// CharacterBody2D-rooted player. Owns sibling component nodes from four
/// different assemblies — exercising the multi-assembly setup at runtime.
/// </summary>
[GlobalClass]
public partial class PlayerController : CharacterBody2D
{
    [Export] public float BaseSpeed { get; set; } = 220f;
    [Export] public float AttackRange { get; set; } = 140f;
    [Export] public int AttackDamage { get; set; } = 35;
    [Export] public float AttackInterval { get; set; } = 0.45f;
    [Export] public float ContactDamageCooldown { get; set; } = 0.6f;

    public HealthComponent Health { get; private set; } = null!;
    public InventorySystem Inventory { get; private set; } = null!;
    public BuffSystem Speed { get; private set; } = null!;
    public TransitivePlayer Stats { get; private set; } = null!;

    private float _attackTimer;
    private float _contactCooldown;

    public override void _Ready()
    {
        Health = GetNode<HealthComponent>("HealthComponent");
        Inventory = GetNode<InventorySystem>("InventorySystem");
        Speed = GetNode<BuffSystem>("BuffSystem");
        Stats = GetNode<TransitivePlayer>("TransitivePlayer");
        AddToGroup("player");
        GD.Print("[MainProject] PlayerController ready, components wired.");
    }

    public override void _PhysicsProcess(double delta)
    {
        Vector2 input = Input.GetVector("ui_left", "ui_right", "ui_up", "ui_down");
        Velocity = input * BaseSpeed * Speed.CurrentMultiplier;
        MoveAndSlide();

        _contactCooldown -= (float)delta;
        if (_contactCooldown <= 0f)
        {
            for (int i = 0; i < GetSlideCollisionCount(); i++)
            {
                var collider = GetSlideCollision(i).GetCollider();
                if (collider is Node node && node.IsInGroup("enemies"))
                {
                    var enemyHealth = node.GetNodeOrNull<HealthComponent>("HealthComponent");
                    Health.TakeDamage(enemyHealth?.MaxHealth / 5 ?? 10);
                    _contactCooldown = ContactDamageCooldown;
                    break;
                }
            }
        }
    }

    public override void _Process(double delta)
    {
        _attackTimer -= (float)delta;
        if (_attackTimer > 0f) return;

        HealthComponent? targetHealth = null;
        float bestDistSq = AttackRange * AttackRange;
        foreach (Node n in GetTree().GetNodesInGroup("enemies"))
        {
            if (n is not Node2D enemy) continue;
            float distSq = (enemy.GlobalPosition - GlobalPosition).LengthSquared();
            if (distSq < bestDistSq && enemy.GetNodeOrNull<HealthComponent>("HealthComponent") is { } h)
            {
                bestDistSq = distSq;
                targetHealth = h;
            }
        }

        if (targetHealth != null)
        {
            targetHealth.TakeDamage(AttackDamage);
            _attackTimer = AttackInterval;
        }
    }
}
