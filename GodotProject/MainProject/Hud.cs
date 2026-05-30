using Godot;
using InTreeGodotSdkModule;
using TransitiveDependencyModule;
using TransitiveNuGetFromNuGet;
using TransitiveNuGetFromProject;

namespace MultiAssemblyExample;

/// <summary>
/// Minimal HUD: HP, score, quest progress, active buff timer.
/// Wires itself to the four cross-assembly nodes provided in Setup.
/// </summary>
[GlobalClass]
public partial class Hud : CanvasLayer
{
    private Label _hp = null!;
    private Label _score = null!;
    private Label _quest = null!;
    private Label _buff = null!;
    private Label _wave = null!;
    private Label _result = null!;

    private BuffSystem? _buffSystem;

    public override void _Ready()
    {
        _hp = GetNode<Label>("Root/HP");
        _score = GetNode<Label>("Root/Score");
        _quest = GetNode<Label>("Root/Quest");
        _buff = GetNode<Label>("Root/Buff");
        _wave = GetNode<Label>("Root/Wave");
        _result = GetNode<Label>("Result");
        _result.Visible = false;
    }

    public override void _Process(double delta)
    {
        if (_buffSystem is { IsActive: true })
            _buff.Text = $"{_buffSystem.BuffName}: {_buffSystem.TimeRemaining:F1}s (x{_buffSystem.Strength:F1})";
        else
            _buff.Text = "Buff: —";
    }

    public void Bind(HealthComponent health, TransitivePlayer stats, BuffSystem buff, QuestTracker quest)
    {
        _buffSystem = buff;
        health.HealthChanged += (oldHp, newHp) => _hp.Text = $"HP: {newHp} / {health.MaxHealth}";
        health.Died += () => ShowResult("You died.", Colors.IndianRed);
        stats.ScoreChanged += score => _score.Text = $"Score: {score}";
        stats.WaveChanged += wave => _wave.Text = $"Wave: {wave}";
        quest.KillRegistered += (kills, target) => _quest.Text = $"Quest: {kills} / {target} kills";
        quest.QuestCompleted += () => ShowResult("Quest complete!", Colors.LimeGreen);

        _hp.Text = $"HP: {health.CurrentHealth} / {health.MaxHealth}";
        _score.Text = "Score: 0";
        _wave.Text = $"Wave: {stats.Wave}";
        _quest.Text = $"Quest: 0 / {quest.TargetKills} kills";
        _buff.Text = "Buff: —";
    }

    private void ShowResult(string text, Color color)
    {
        _result.Text = text;
        _result.Modulate = color;
        _result.Visible = true;
    }
}
