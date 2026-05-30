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
    [Export] public int TargetKills { get; set; } = 10;

    public int Kills { get; private set; }
    public bool Completed { get; private set; }

    [Signal] public delegate void KillRegisteredEventHandler(int kills, int target);
    [Signal] public delegate void QuestCompletedEventHandler();

    public override void _Ready()
    {
        GD.Print($"[TransitiveNuGetFromNuGet] QuestTracker ready! Max={MaxActiveQuests} AutoTrack={AutoTrack} Target={TargetKills}");
    }

    public void RegisterKill()
    {
        if (Completed) return;
        Kills++;
        EmitSignal(SignalName.KillRegistered, Kills, TargetKills);
        if (Kills >= TargetKills)
        {
            Completed = true;
            EmitSignal(SignalName.QuestCompleted);
        }
    }
}
