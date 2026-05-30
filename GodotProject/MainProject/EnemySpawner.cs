using Godot;
using InTreeGodotSdkModule;
using InTreeModule;

namespace MultiAssemblyExample;

/// <summary>
/// Periodically spawns enemies at a random arena edge and points them at the player.
/// </summary>
[GlobalClass]
public partial class EnemySpawner : Node2D
{
    [Export] public PackedScene? EnemyScene { get; set; }
    [Export] public float SpawnInterval { get; set; } = 1.6f;
    [Export] public Vector2 ArenaSize { get; set; } = new(960, 540);

    [Signal] public delegate void EnemySpawnedEventHandler(Node2D enemy);

    private float _timer;
    private readonly RandomNumberGenerator _rng = new();

    public override void _Ready()
    {
        _rng.Randomize();
    }

    public override void _Process(double delta)
    {
        _timer -= (float)delta;
        if (_timer > 0f || EnemyScene == null) return;
        _timer = SpawnInterval;
        SpawnOne();
    }

    private void SpawnOne()
    {
        if (EnemyScene == null) return;
        var enemy = EnemyScene.Instantiate<Node2D>();
        enemy.GlobalPosition = PickEdgePoint();
        enemy.AddToGroup("enemies");

        var player = GetTree().GetFirstNodeInGroup("player") as Node2D;
        if (enemy.GetNodeOrNull<EnemyController>("EnemyController") is { } controller)
            controller.SetTarget(player);

        GetParent().AddChild(enemy);
        EmitSignal(SignalName.EnemySpawned, enemy);
    }

    private Vector2 PickEdgePoint()
    {
        int edge = _rng.RandiRange(0, 3);
        float x = _rng.RandfRange(0, ArenaSize.X);
        float y = _rng.RandfRange(0, ArenaSize.Y);
        return edge switch
        {
            0 => new Vector2(x, 0),
            1 => new Vector2(x, ArenaSize.Y),
            2 => new Vector2(0, y),
            _ => new Vector2(ArenaSize.X, y),
        };
    }
}
