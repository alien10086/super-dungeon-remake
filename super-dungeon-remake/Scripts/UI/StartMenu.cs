using Godot;
using SuperDungeonRemake.Core;

namespace SuperDungeonRemake.UI
{
	public partial class StartMenu : Control
	{
		private Label _startLabel;
		private Label _exitLabel;
		private Sprite2D _pointer;
		private AnimationPlayer _animationPlayer;
		private AudioStreamPlayer2D _audioPlayer;
		private int _selectedIndex = 0;
		private readonly string[] _menuOptions = { "start", "exit" };

		public override void _Ready()
		{
			_startLabel = GetNode<Label>("start");
			_exitLabel = GetNode<Label>("exit");
			_pointer = GetNode<Sprite2D>("Pointer");
			_animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
			_audioPlayer = GetNode<AudioStreamPlayer2D>("AudioStreamPlayer2D");
			
			// 播放背景音乐
			_audioPlayer.Play();
			
			// 播放选中动画
			_animationPlayer.Play("selected");
		}

		private void OnStartSelected()
		{
			// 切换到主游戏场景
			GetTree().ChangeSceneToFile("res://Scenes/Main.tscn");
		}

		private void OnExitSelected()
		{
			GetTree().Quit();
		}

		private void UpdatePointerPosition()
		{
			// 根据选中的选项更新指针位置
			if (_selectedIndex == 0)
			{
				_pointer.Position = new Vector2(500, 709); // start选项位置
				_animationPlayer.Play("selected");
			}
			else
			{
				_pointer.Position = new Vector2(500, 813); // exit选项位置
				_animationPlayer.Stop();
			}
		}

		public override void _Input(InputEvent @event)
		{
			if (@event.IsActionPressed("ui_up"))
			{
				_selectedIndex = (_selectedIndex - 1 + _menuOptions.Length) % _menuOptions.Length;
				UpdatePointerPosition();
			}
			else if (@event.IsActionPressed("ui_down"))
			{
				_selectedIndex = (_selectedIndex + 1) % _menuOptions.Length;
				UpdatePointerPosition();
			}
			else if (@event.IsActionPressed("ui_select"))
			{
				if (_selectedIndex == 0)
				{
					OnStartSelected();
				}
				else
				{
					OnExitSelected();
				}
			}
		}
	}
}
