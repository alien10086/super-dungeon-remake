using Godot;
using SuperDungeonRemake.Utils;

namespace SuperDungeonRemake.Gameplay.Enemies
{
    public partial class Enemy : CharacterBody2D
    {
        [Export] public float Speed { get; set; } = 50.0f;
        [Export] public int MaxHealth { get; set; } = 50;
        [Export] public int Damage { get; set; } = 10;
        [Export] public int GoldValue { get; set; } = 5;
        
        protected int _currentHealth;
        protected Sprite2D _sprite;
        protected AnimationPlayer _animationPlayer;
        protected AudioStreamPlayer2D _audioPlayer;
        protected ProgressBar _healthBar;
        protected Vector2 _targetPosition;
        protected bool _isDead = false;
        
        public override void _Ready()
        {
            _sprite = GetNode<Sprite2D>("Sprite2D");
            _animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
            _audioPlayer = GetNode<AudioStreamPlayer2D>("AudioStreamPlayer2D");
            _healthBar = GetNode<ProgressBar>("HealthBar");
            
            _currentHealth = MaxHealth;
            _healthBar.MaxValue = MaxHealth;
            _healthBar.Value = _currentHealth;
            _targetPosition = GlobalPosition;
            
            AddToGroup(GlobalConstants.GroupNames.Enemies);
        }
        
        public override void _PhysicsProcess(double delta)
        {
            if (_isDead) return;
            
            UpdateMovement(delta);
            UpdateHealthBar();
        }
        
        protected virtual void UpdateMovement(double delta)
        {
            // 基础AI：朝向玩家移动
            var player = GetTree().GetFirstNodeInGroup(GlobalConstants.GroupNames.Player);
            if (player != null)
            {
                var playerPos = ((Node2D)player).GlobalPosition;
                var direction = (playerPos - GlobalPosition).Normalized();
                Velocity = direction * Speed;
                MoveAndSlide();
            }
        }
        
        protected void UpdateHealthBar()
        {
            _healthBar.Value = _currentHealth;
            _healthBar.Visible = _currentHealth < MaxHealth;
        }
        
        public virtual void TakeDamage(int damage)
        {
            if (_isDead) return;
            
            _currentHealth -= damage;
            
            if (_currentHealth <= 0)
            {
                Die();
            }
            else
            {
                // 播放受伤动画
                PlayHurtAnimation();
            }
        }
        
        protected virtual void PlayHurtAnimation()
        {
            // 简单的受伤效果：闪烁
            var tween = CreateTween();
            tween.TweenProperty(_sprite, "modulate:a", 0.5f, 0.1f);
            tween.TweenProperty(_sprite, "modulate:a", 1.0f, 0.1f);
        }
        
        protected virtual void Die()
        {
            _isDead = true;
            
            // 更新游戏数据
            var gameData = GetNode<GameData>("/root/GameData");
            gameData.AddGold(GoldValue);
            gameData.AddKill();
            
            // 播放死亡动画和音效
            PlayDeathAnimation();
            
            // 延迟删除节点
            var timer = GetTree().CreateTimer(0.5f);
            timer.Timeout += QueueFree;
        }
        
        protected virtual void PlayDeathAnimation()
        {
            // 简单的死亡效果：缩放消失
            var tween = CreateTween();
            tween.Parallel().TweenProperty(this, "scale", Vector2.Zero, 0.3f);
            tween.Parallel().TweenProperty(_sprite, "modulate:a", 0.0f, 0.3f);
        }
        
        public virtual void OnAreaEntered(Area2D area)
        {
            // 处理与玩家攻击区域的碰撞
            if (area.GetParent().IsInGroup(GlobalConstants.GroupNames.Player))
            {
                TakeDamage(20); // 默认玩家攻击伤害
            }
        }
    }
}