using Godot;

namespace SuperDungeonRemake.Scripts.UI
{
	public partial class StartMenu : Control
	{
		private Label _startLabel;
		private Label _exitLabel;
		private Sprite2D _pointer;
		private Sprite2D _pointer2;
		private AnimationPlayer _animationPlayer;
		private AudioStreamPlayer2D _audioPlayer;
		private int _selectedIndex = 0;
		private readonly string[] _menuOptions = { "start", "exit" };

		public override void _Ready()
		{
			_startLabel = GetNode<Label>("start");
			_exitLabel = GetNode<Label>("exit");
			_pointer = GetNode<Sprite2D>("Pointer");
			_pointer2 = GetNode<Sprite2D>("Pointer2");
			_animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
			_audioPlayer = GetNode<AudioStreamPlayer2D>("AudioStreamPlayer2D");
			
			// 播放背景音乐
			_audioPlayer.Play();
			
			// 播放选中动画
			_animationPlayer.Play("selected");
			// GD.Print("666666");
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
			GD.Print(_selectedIndex);

			GD.Print(_pointer.Position);
			if (_selectedIndex == 0)
			{
				// start选项位置 - 对应start标签的Y位置
				_pointer2.Visible = false;
				_pointer.Visible = true;
				// _animationPlayer.Stop();
				_exitLabel.LabelSettings.FontColor = Colors.White;
				_animationPlayer.Play("selected");
			
			}
			else
			{
				_pointer.Visible = false;
				_pointer2.Visible = true;
				// _animationPlayer.Stop();
				_startLabel.LabelSettings.FontColor = Colors.White;
				_animationPlayer.Play("selected2");
			}
		}

		public override void _Input(InputEvent @event)
		{
			// GD.Print(@event.AsText());
			// GD.Print(666666);
			if (@event.IsActionPressed("move_up"))
			{
				_selectedIndex = (_selectedIndex - 1 + _menuOptions.Length) % _menuOptions.Length;
				UpdatePointerPosition();
			}
			else if (@event.IsActionPressed("move_down"))
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
