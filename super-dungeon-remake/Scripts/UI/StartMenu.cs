using Godot;
using SuperDungeonRemake.Core;

namespace SuperDungeonRemake.UI
{
    public partial class StartMenu : Control
    {
        private Button _startButton;
        private Button _quitButton;

        public override void _Ready()
        {
            _startButton = GetNode<Button>("VBoxContainer/StartButton");
            _quitButton = GetNode<Button>("VBoxContainer/QuitButton");

            _startButton.Pressed += OnStartButtonPressed;
            _quitButton.Pressed += OnQuitButtonPressed;
        }

        private void OnStartButtonPressed()
        {
            // 切换到主游戏场景
            GetTree().ChangeSceneToFile("res://Scenes/Main.tscn");
        }

        private void OnQuitButtonPressed()
        {
            GetTree().Quit();
        }

        public override void _Input(InputEvent @event)
        {
            if (@event.IsActionPressed("ui_select"))
            {
                OnStartButtonPressed();
            }
        }
    }
}