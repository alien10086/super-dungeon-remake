using Godot;
using SuperDungeonRemake.Gameplay.Player;
using SuperDungeonRemake.Level;
using SuperDungeonRemake.Utils;

namespace SuperDungeonRemake.Core;

public partial class GameManager : Node
{
	public static GameManager Instance { get; private set; }
	
	[Export] public PackedScene PlayerScene { get; set; }
	[Export] public PackedScene[] EnemyScenes { get; set; }
	[Export] public PackedScene ExitScene { get; set; }
	[Export] public PackedScene MapScene { get; set; }
	
	public PlayerController CurrentPlayer { get; private set; }
	public LevelGenerator LevelGenerator { get; private set; }
	public Node2D CurrentMap { get; private set; }
	
	private CanvasLayer _hud;
	
	public override void _Ready()
	{
		if (Instance == null)
		{
			Instance = this;
			ProcessMode = ProcessModeEnum.Always;
		}
		else
		{
			QueueFree();
			return;
		}
		
		LevelGenerator = new LevelGenerator();
		AddChild(LevelGenerator);
		
		_hud = GetNode<CanvasLayer>("/root/Main/HUD");
		
		if (_hud == null)
		{
			GD.PrintErr("Failed to find HUD node at path: CanvasLayer/HUD");
		}
		
		StartNewGame();
	}
	
	public void StartNewGame()
	{
		GameData.Instance?.ResetGame();
		LoadLevel();
	}
	
	public void LoadLevel()
	{
		// Remove existing map if any
		if (CurrentMap != null)
		{
			CurrentMap.QueueFree();
		}
		
		// Remove existing monsters
		var monsters = GetTree().GetNodesInGroup(GlobalConstants.GroupMonsters);
		foreach (Node monster in monsters)
		{
			monster.QueueFree();
		}
		
		// Create new map
		CurrentMap = MapScene.Instantiate<Node2D>();
		AddChild(CurrentMap);
		
		// Create player if doesn't exist
		if (CurrentPlayer == null)
		{
			CurrentPlayer = PlayerScene.Instantiate<PlayerController>();
			AddChild(CurrentPlayer);
		}
		
		// Position player in a random room
		PositionPlayerRandomly();
		
		// Update HUD
		UpdateHUD();
	}
	
	public void NextLevel()
	{
		GameData.Instance?.SetDepth(GameData.Instance.Depth + 1);
		CallDeferred(nameof(LoadLevel));
	}
	
	private void PositionPlayerRandomly()
	{
		if (CurrentPlayer != null && LevelGenerator.AllRooms.Count > 0)
		{
			var randomRoom = LevelGenerator.AllRooms[GD.RandRange(0, LevelGenerator.AllRooms.Count - 1)];
			var playerPos = new Vector2(
				randomRoom.Left * GlobalConstants.GridSize + randomRoom.Width * GlobalConstants.GridSize / 2,
				randomRoom.Top * GlobalConstants.GridSize + randomRoom.Height * GlobalConstants.GridSize / 2
			);
			CurrentPlayer.Position = playerPos;
		}
	}
	
	private void UpdateHUD()
	{
		if (_hud != null)
		{
			var depthLabel = _hud.GetNode<Label>("TopBar/DepthLabel");
			if (depthLabel != null)
			{
				depthLabel.Text = $"{GameData.Instance?.Depth * 100 ?? 100} ft";
			}
		}
	}
}
