using Godot;

namespace ExternalNuGetModule;

// Resource subclass in an out-of-tree NuGet package.
// Script path will be csharp://ExternalNuGetModule/ExternalNuGetModule.InventoryItem.cs
// Used to reproduce pcloves' Bug 1 via FileSystem -> Create New -> Resource...
[GlobalClass]
public partial class InventoryItem : Resource
{
    [Export] public string ItemName { get; set; } = "";
    [Export] public int MaxStack { get; set; } = 1;
    [Export] public float Weight { get; set; } = 0.0f;
}
