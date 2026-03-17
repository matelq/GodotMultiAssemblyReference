using Godot;

namespace TransitiveNuGetFromNuGet;

/// <summary>
/// Script in a NuGet package that is transitively referenced via ExternalNuGetModule.
/// Tests: PackageReference → PackageReference chain (NuGet → NuGet).
/// </summary>
[GlobalClass]
public partial class QuestTracker : Node
{
    [Export] public int MaxActiveQuests { get; set; } = 5;
    [Export] public bool AutoTrack { get; set; } = true;
    [Export] public string QuestLogTitle { get; set; } = "Active Quests";

    public override void _Ready()
    {
        GD.Print($"[TransitiveNuGetFromNuGet] QuestTracker ready! Max={MaxActiveQuests} AutoTrack={AutoTrack}");
    }
}
