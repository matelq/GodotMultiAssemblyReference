using ExternalNuGetModule;
using Godot;
using InTreeGodotSdkModule;
using TransitiveNuGetFromNuGet;

namespace MultiAssemblyExample;

/// <summary>
/// Top-level arena bus. On spawn, hooks an enemy's Died signal to the player score
/// (TransitivePlayer) and the QuestTracker. Drives wave progression.
/// </summary>
[GlobalClass]
public partial class ArenaController : Node2D
{
    [Export] public int KillsPerWave { get; set; } = 5;
    [Export] public PackedScene? PickupScene { get; set; }
    [Export] public InventoryItem? HealthPickup { get; set; }
    [Export] public InventoryItem? SpeedPickup { get; set; }
    [Export] public float PickupSpawnInterval { get; set; } = 6f;
    [Export] public Vector2 ArenaSize { get; set; } = new(960, 540);

    private PlayerController _player = null!;
    private QuestTracker _quest = null!;
    private EnemySpawner _spawner = null!;
    private Hud _hud = null!;
    private float _pickupTimer;
    private int _waveKills;
    private readonly RandomNumberGenerator _rng = new();

    public override void _Ready()
    {
        _rng.Randomize();
        _player = GetNode<PlayerController>("Player");
        _quest = GetNode<QuestTracker>("QuestTracker");
        _spawner = GetNode<EnemySpawner>("EnemySpawner");
        _hud = GetNode<Hud>("Hud");

        _hud.Bind(_player.Health, _player.Stats, _player.Speed, _quest);
        _spawner.EnemySpawned += OnEnemySpawned;
        _player.Health.Died += () => _spawner.SetProcess(false);
    }

    public override void _Process(double delta)
    {
        if (PickupScene == null) return;
        _pickupTimer -= (float)delta;
        if (_pickupTimer <= 0f)
        {
            _pickupTimer = PickupSpawnInterval;
            SpawnPickup();
        }
    }

    private void OnEnemySpawned(Node2D enemy)
    {
        var health = enemy.GetNodeOrNull<HealthComponent>("HealthComponent");
        if (health == null) return;
        health.Died += () => OnEnemyDied(enemy);
    }

    private void OnEnemyDied(Node2D enemy)
    {
        _player.Stats.AddScore(10);
        _quest.RegisterKill();
        _waveKills++;
        if (_waveKills >= KillsPerWave)
        {
            _waveKills = 0;
            _player.Stats.NextWave();
            _spawner.SpawnInterval = Mathf.Max(0.4f, _spawner.SpawnInterval * 0.85f);
        }
        enemy.QueueFree();
    }

    private void SpawnPickup()
    {
        if (PickupScene == null) return;
        var pickup = PickupScene.Instantiate<Pickup>();
        bool wantSpeed = _rng.Randf() < 0.5f && SpeedPickup != null;
        pickup.Item = wantSpeed ? SpeedPickup : HealthPickup;
        pickup.ActivateBuffOnPickup = wantSpeed;
        pickup.GlobalPosition = new Vector2(
            _rng.RandfRange(40, ArenaSize.X - 40),
            _rng.RandfRange(40, ArenaSize.Y - 40));
        AddChild(pickup);
    }
}
