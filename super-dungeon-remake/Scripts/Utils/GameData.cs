using Godot;

namespace SuperDungeonRemake.Utils;

public partial class GameData : Node
{
    public static GameData Instance { get; private set; }
    
    [Signal]
    public delegate void GoldChangedEventHandler(int newGold);
    
    [Signal]
    public delegate void HealthChangedEventHandler(int newHealth);
    
    [Signal]
    public delegate void DepthChangedEventHandler(int newDepth);
    
    public int Gold { get; private set; } = 0;
    public int Depth { get; private set; } = 1;
    public int Kills { get; private set; } = 0;
    
    public override void _Ready()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            QueueFree();
        }
    }
    
    public void AddGold(int amount)
    {
        Gold += amount;
        EmitSignal(SignalName.GoldChanged, Gold);
    }
    
    public void SetDepth(int depth)
    {
        Depth = depth;
        EmitSignal(SignalName.DepthChanged, Depth);
    }
    
    public void AddKill()
    {
        Kills++;
    }
    
    public void ResetGame()
    {
        Gold = 0;
        Depth = 1;
        Kills = 0;
        EmitSignal(SignalName.GoldChanged, Gold);
        EmitSignal(SignalName.DepthChanged, Depth);
    }
}