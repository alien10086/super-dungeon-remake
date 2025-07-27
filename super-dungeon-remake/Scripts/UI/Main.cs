using Godot;
using SuperDungeonRemake.Core;
using SuperDungeonRemake.Utils;

public partial class Main : Node
{
	// private GameManager _gameManager;
	// private GameData _gameData;
	
	public override void _Ready()
	{
		// 初始化GameData单例
		// _gameData = new GameData();
		// _gameData.Name = "GameData";
		// AddChild(_gameData);
		
		// // 初始化GameManager
		// _gameManager = new GameManager();
		// _gameManager.Name = "GameManager";
		
		// // 设置GameManager的导出属性
		// _gameManager.PlayerScene = GD.Load<PackedScene>("res://Scenes/Player.tscn");
		// _gameManager.MapScene = GD.Load<PackedScene>("res://Scenes/Level/Level.tscn");
		
		// AddChild(_gameManager);
		
		GD.Print("Main scene initialized successfully");
	}
	
	public override void _Input(InputEvent @event)
	{
		// 处理暂停功能
		if (@event.IsActionPressed("ui_cancel"))
		{
			TogglePause();
		}
	}
	
	private void TogglePause()
	{
		GD.Print("TogglePause");
		var pauseLayer = GetNode<CanvasLayer>("Pause");
		if (pauseLayer != null)
		{
            // var isPaused = GetTree().Paused;
            // GetTree().Paused = !isPaused;
            var now_pause = pauseLayer.Visible;
            // 修正逻辑：游戏暂停时显示暂停界面，游戏运行时隐藏暂停界面
            pauseLayer.Visible = !now_pause;
        }
	}
}
