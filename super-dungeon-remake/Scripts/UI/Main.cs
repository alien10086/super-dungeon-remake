using Godot;
using SuperDungeonRemake.Core;
using SuperDungeonRemake.Utils;

public partial class Main : Node
{
	// private GameManager _gameManager;
	// private GameData _gameData;
	private int _selectedIndex = 0; // 0: resume, 1: quit
	private AnimationPlayer _pauseAnimationPlayer;
	private Sprite2D _pointer;
	private Sprite2D _pointer2;

    private AudioStreamPlayer2D _backgroundAudioPlayer;
    private AudioStreamPlayer2D _sfcAudioPlayer;
	
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
		_pointer = GetNode<Sprite2D>("Pause/Pointer");
		_pointer2 = GetNode<Sprite2D>("Pause/Pointer2");
		_backgroundAudioPlayer =  GetNode<AudioStreamPlayer2D>("Music");
		_sfcAudioPlayer =  GetNode<AudioStreamPlayer2D>("SFxExit");
		
		
		_backgroundAudioPlayer.Play();
		GD.Print("Main scene initialized successfully");
	}
	
	public override void _Input(InputEvent @event)
	{
		// 处理暂停功能
		if (@event.IsActionPressed("ui_cancel"))
		{
			TogglePause();
		}
		
		// 处理暂停界面导航
		var pauseLayer = GetNode<CanvasLayer>("Pause");
		if (pauseLayer != null && pauseLayer.Visible)
		{
			if (@event.IsActionPressed("ui_up"))
			{
				_selectedIndex = 0; // resume
                
				// PlayPauseAnimation("select_resume");
				_pointer.Visible = true;
				_pointer2.Visible = false;
                _pauseAnimationPlayer.Stop();
				_pauseAnimationPlayer.Play("select_resume");
				
				// select_resume
			}
			else if (@event.IsActionPressed("ui_down"))
			{
				_selectedIndex = 1; // quit
				_pointer2.Visible = true;
				_pointer.Visible = false;
                _pauseAnimationPlayer.Stop();
				_pauseAnimationPlayer.Play("select_quit");
			}
		}
	}
	
	private void TogglePause()
	{
		GD.Print("TogglePause");
		var pauseLayer = GetNode<CanvasLayer>("Pause");
		if (pauseLayer != null)
		{
			var now_pause = pauseLayer.Visible;
			pauseLayer.Visible = !now_pause;
			
			// 获取动画播放器
			if (_pauseAnimationPlayer == null)
			{
				_pauseAnimationPlayer = pauseLayer.GetNode<AnimationPlayer>("AnimationPlayer");
			}
			
			//暂停界面显示时，默认选择resume
			if (pauseLayer.Visible)
			{
				_selectedIndex = 0; // resume
            
				_pointer.Visible = true;
				_pointer2.Visible = false;
                 _pauseAnimationPlayer.Stop();
				_pauseAnimationPlayer.Play("select_resume");
				// PlayPauseAnimation("select_resume");
			}
		}
	}
	
	// private void PlayPauseAnimation(string animationName)
	// {
	// 	if (_pauseAnimationPlayer != null && _pauseAnimationPlayer.HasAnimation(animationName))
	// 	{
	// 		_pauseAnimationPlayer.Play(animationName);
	// 		GD.Print($"Playing animation: {animationName}");
	// 	}
	// }
}
