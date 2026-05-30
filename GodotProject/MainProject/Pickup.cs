using ExternalNuGetModule;
using Godot;

namespace MultiAssemblyExample;

/// <summary>
/// Pickup Area2D. Holds a cross-assembly <see cref="InventoryItem"/> resource —
/// this is the exact scenario where pcloves saw <c>csharp://</c> get mangled
/// to <c>csharp:/</c> in the .tres on load. Optionally activates the player's
/// BuffSystem on pickup.
/// </summary>
[GlobalClass]
public partial class Pickup : Area2D
{
    [Export] public InventoryItem? Item { get; set; }
    [Export] public bool ActivateBuffOnPickup { get; set; }
    [Export] public float BuffDuration { get; set; } = 6f;
    [Export] public float BuffStrength { get; set; } = 1.6f;

    public override void _Ready()
    {
        BodyEntered += OnBodyEntered;
    }

    private void OnBodyEntered(Node2D body)
    {
        if (body is not PlayerController player) return;
        if (Item != null)
        {
            player.Inventory.AddItem(Item);
            GD.Print($"[MainProject] Picked up {Item.ItemName}");
        }
        if (ActivateBuffOnPickup)
            player.Speed.Activate(BuffDuration, BuffStrength);
        QueueFree();
    }
}
