using Godot;

namespace InTreeModule;

/// <summary>
/// Example script in an in-tree module assembly.
/// Source file is inside the Godot project (res://InTreeModule/EnemyController.cs),
/// so it appears in the FileSystem panel, Select Script dialog, and can be opened
/// in an external editor directly from Godot.
/// Attached as a child of a CharacterBody2D — chases <see cref="TargetPath"/> each physics step.
/// </summary>
[GlobalClass, Icon("res://icon.svg")]
public partial class EnemyController : Node
{
    [Export] public int Health { get; set; } = 100;
    [Export] public float Speed { get; set; } = 80.0f;
    [Export] public string DisplayName { get; set; } = "Enemy";
    [Export] public int DamageOnHit { get; set; } = 10;
    [Export] public NodePath TargetPath { get; set; } = new();

    private CharacterBody2D? _body;
    private Node2D? _target;

    public override void _Ready()
    {
        GD.Print($"[InTreeModule] EnemyController ready! Health={Health}, Speed={Speed}, Name={DisplayName}");
        _body = GetParent() as CharacterBody2D;
        if (!TargetPath.IsEmpty)
            _target = GetNodeOrNull<Node2D>(TargetPath);
    }

    public void SetTarget(Node2D? target)
    {
        _target = target;
    }

    public override void _PhysicsProcess(double delta)
    {
        if (_body == null || _target == null) return;
        Vector2 toTarget = _target.GlobalPosition - _body.GlobalPosition;
        _body.Velocity = toTarget.LengthSquared() > 1.0f
            ? toTarget.Normalized() * Speed
            : Vector2.Zero;
        _body.MoveAndSlide();
    }
}
