using Godot;
using SuperDungeonRemake.Gameplay.Player;
using SuperDungeonRemake.Level;
using SuperDungeonRemake.Utils;
using SuperDungeonRemake.Gameplay.Enemies;

namespace SuperDungeonRemake.Core;

public partial class GameManager : Node
{
	public static GameManager Instance { get; private set; }
	
	[Export] public PackedScene PlayerScene { get; set; }
	[Export] public PackedScene[] EnemyScenes { get; set; }
	// [Export] public PackedScene ExitScene { get; set; }
	[Export] public PackedScene MapScene { get; set; }
	
	public PlayerController CurrentPlayer { get; private set; }
	public LevelGenerator LevelGenerator { get; private set; }
	public Node2D CurrentMap { get; private set; }
	public EnemySpawner EnemySpawner { get; private set; }
	
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
		
		LevelGenerator = GetNode<LevelGenerator>("../LevelGen");
		
		EnemySpawner = GetNode<EnemySpawner>("../EnemySpawner");
		EnemySpawner.Initialize(LevelGenerator);
		
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
		
		// Spawn enemies for the level
		SpawnEnemiesForLevel();
		
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
			var depthLabel = _hud.GetNode<Label>("DepthLabel");
			if (depthLabel != null)
			{
				depthLabel.Text = $"{GameData.Instance?.Depth * 100 ?? 100} ft";
			}
		}
	}
	
	/// <summary>
	/// 为当前关卡生成敌人
	/// </summary>
	private void SpawnEnemiesForLevel()
	{
		if (EnemySpawner == null)
		{
			GD.PrintErr("EnemySpawner is null, cannot spawn enemies");
			return;
		}
		
		// 获取当前关卡深度
		var currentDepth = GameData.Instance?.Depth ?? 1;
		
		// 根据关卡深度生成敌人
		var spawnedEnemies = EnemySpawner.SpawnRandomEnemiesForLevel(currentDepth);
		
		GD.Print($"Spawned {spawnedEnemies.Count} enemies for level {currentDepth}");
	}
	
	/// <summary>
	/// 手动生成指定类型的敌人
	/// </summary>
	/// <param name="enemyType">敌人类型</param>
	/// <param name="position">生成位置（可选）</param>
	/// <returns>生成的敌人实例</returns>
	public Enemy SpawnEnemy(EnemyType enemyType, Vector2? position = null)
	{
		if (EnemySpawner == null)
		{
			GD.PrintErr("EnemySpawner is null, cannot spawn enemy");
			return null;
		}
		
		if (position.HasValue)
		{
			return EnemySpawner.SpawnEnemy(position.Value, enemyType);
		}
		else
		{
			return EnemySpawner.SpawnEnemyInRandomRoom(enemyType);
		}
	}
}
