using Godot;
using SuperDungeonRemake.Gameplay.Enemies;
using SuperDungeonRemake.Utils;

namespace SuperDungeonRemake.Gameplay.Enemies
{
    /// <summary>
    /// 具体的敌人实现类
    /// 继承自BaseEnemy，可以进一步自定义特定敌人的行为
    /// 整合了Monster类的物理移动和碰撞检测功能
    /// </summary>
    public partial class Enemy : BaseEnemy
    {
        #region Monster Properties
        [Export] public float Factor { get; set; } = 1.0f;
        [Export] public string DeathSfx { get; set; } = "1";
        
        private Vector2 _velocity = Vector2.Zero;
        private Vector2 _direction = Vector2.Zero;
        private float _recoilCountdown = 0.0f;
        private float _timeSinceHitPlayer = 2000.0f;
        private bool _dead = false;
        
        private const float RECOIL_SPEED = 200f;
        private const float RECOIL_TIME = 0.15f;
        
        // 节点引用
        private GpuParticles2D _particles;
        private AnimationPlayer _animationPlayer;
        private AnimatedSprite2D _animatedSprite;
        private AudioStreamPlayer2D _sfxHit;
        private AudioStreamPlayer2D _sfxDeath;
        private Area2D _hitbox;
        private CollisionShape2D _hitboxCollision;
        #endregion
        
        #region Godot Lifecycle
        public override void _Ready()
        {
            // 调用基类的初始化
            base._Ready();
            
            // 获取节点引用
            SetupMonsterNodes();
            
            // 可以在这里添加特定敌人的初始化逻辑
            SetupSpecificBehavior();
        }
        
        public override void _PhysicsProcess(double delta)
        {
            base._PhysicsProcess(delta);
            
            // 如果已死亡，不处理物理
            if (_dead) return;
            
            // 更新计时器
            _timeSinceHitPlayer += (float)delta;
            var currentSpeed = Speed;
            
            // 如果正在后退
            if (_recoilCountdown >= 0)
            {
                currentSpeed = RECOIL_SPEED;
                _recoilCountdown -= (float)delta;
            }
            
            // 应用移动
            _velocity = _direction * currentSpeed * Factor;
            Velocity = _velocity;
            MoveAndSlide();
            
            // 处理碰撞反弹
            for (int i = 0; i < GetSlideCollisionCount(); i++)
            {
                var collision = GetSlideCollision(i);
                if (collision != null)
                {
                    _direction = _direction.Bounce(collision.GetNormal());
                }
            }
            
            // 检查死亡
            if (CurrentHealth <= 0 && !_dead)
            {
                HandleDeath();
            }
        }
        #endregion
        
        #region Monster Setup
        /// <summary>
        /// 设置Monster相关节点引用
        /// </summary>
        private void SetupMonsterNodes()
        {
            _particles = GetNodeOrNull<GpuParticles2D>("GPUParticles2D");
            _animationPlayer = GetNodeOrNull<AnimationPlayer>("AnimationPlayer");
            _animatedSprite = GetNodeOrNull<AnimatedSprite2D>("AnimatedSprite2D");
            _sfxHit = GetNodeOrNull<AudioStreamPlayer2D>("SFXHit");
            _sfxDeath = GetNodeOrNull<AudioStreamPlayer2D>("SFXDeath");
            _hitbox = GetNodeOrNull<Area2D>("Hitbox");
            
            if (_hitbox != null)
            {
                _hitboxCollision = _hitbox.GetNodeOrNull<CollisionShape2D>("CollisionShape2D");
                _hitbox.BodyEntered += OnHitboxBodyEntered;
            }
            
            if (_sfxDeath != null)
            {
                _sfxDeath.Finished += OnSfxDeathFinished;
            }
        }
        
        /// <summary>
        /// 处理敌人死亡
        /// </summary>
        private void HandleDeath()
        {
            // 标记为死亡
            _dead = true;
            
            // 禁用碰撞和hitbox
            SetCollisionMaskValue(1, false);
            SetCollisionLayerValue(2, false);
            if (_hitboxCollision != null)
            {
                _hitboxCollision.SetDeferred("disabled", true);
            }
            
            // 停止移动
            Speed = 0;
            
            // 显示粒子效果
            if (_particles != null)
            {
                _particles.OneShot = true;
                _particles.Emitting = true;
                _particles.Scale = new Vector2(1.2f, 1.2f);
            }
            
            // 播放死亡动画
            if (_animationPlayer != null)
            {
                _animationPlayer.Play("death");
            }
            
            // 播放死亡音效
            if (_sfxDeath != null)
            {
                _sfxDeath.PitchScale = (float)GD.RandRange(0.5, 1.5);
                _sfxDeath.Play();
            }
            
            // 增加玩家金币和击杀数
            // TODO: 替换为游戏管理器的相关方法
            // globals.player.add_gold(GoldValue * Factor);
            // globals.kills += 1;
        }
        #endregion
        
        #region Event Handlers
        /// <summary>
        /// 当死亡音效播放完成时
        /// </summary>
        private void OnSfxDeathFinished()
        {
            QueueFree();
        }
        
        /// <summary>
        /// 当hitbox检测到碰撞时
        /// </summary>
        private void OnHitboxBodyEntered(Node2D body)
        {
            // 当被武器击中
            if (body.Name == "Weapon")
            {
                // 如果不在后退状态，受到伤害
                if (_recoilCountdown < 0)
                {
                    if (_particles != null)
                    {
                        _particles.OneShot = true;
                        _particles.Emitting = true;
                        _particles.Scale = new Vector2(0.5f, 0.5f);
                    }
                    
                    // TODO: 替换为从武器获取伤害值
                    // CurrentHealth -= globals.player.weapon_damage / Factor;
                    TakeDamage(10); // 临时固定伤害值
                    
                    // 计算后退方向
                    // TODO: 替换为从玩家获取位置
                    // _direction = Position - globals.player.Position;
                    _direction = _direction.Normalized();
                    _recoilCountdown = RECOIL_TIME;
                    
                    // 播放受击音效
                    if (_sfxHit != null)
                    {
                        _sfxHit.PitchScale = (float)GD.RandRange(0.8, 1.8);
                        _sfxHit.Play();
                    }
                }
            }
        }
        #endregion
        
        #region Specific Behavior
        /// <summary>
        /// 设置特定敌人的行为
        /// </summary>
        private void SetupSpecificBehavior()
        {
            // 根据敌人类型设置特殊行为
            switch (Type)
            {
                case EnemyType.Goblin:
                    SetupGoblinBehavior();
                    break;
                case EnemyType.Skeleton:
                    SetupSkeletonBehavior();
                    break;
                case EnemyType.Slime:
                    SetupSlimeBehavior();
                    break;
                case EnemyType.Orc:
                    SetupOrcBehavior();
                    break;
            }
        }
        
        private void SetupGoblinBehavior()
        {
            // 哥布林更加敏捷，巡逻范围更小
            PatrolRadius = 40f;
            StateChangeInterval = 1.5f;
            
            // 设置哥布林皮肤
            if (_animatedSprite != null)
            {
                _animatedSprite.Animation = "goblin";
                _animatedSprite.Play();
            }
        }
        
        private void SetupSkeletonBehavior()
        {
            // 骷髅更有纪律，巡逻时间更长
            PatrolRadius = 60f;
            StateChangeInterval = 3f;
            
            // 设置骷髅皮肤
            if (_animatedSprite != null)
            {
                _animatedSprite.Animation = "skel";
                _animatedSprite.Play();
            }
        }
        
        private void SetupSlimeBehavior()
        {
            // 史莱姆移动更随机
            PatrolRadius = 30f;
            StateChangeInterval = 1f;
            
            // 设置史莱姆皮肤
            if (_animatedSprite != null)
            {
                _animatedSprite.Animation = "slime";
                _animatedSprite.Play();
            }
        }
        
        private void SetupOrcBehavior()
        {
            // 兽人更加谨慎，巡逻范围大
            PatrolRadius = 80f;
            StateChangeInterval = 4f;
            FleeHealthThreshold = 0.2f; // 更低的逃跑阈值
            
            // 兽人使用哥布林皮肤（因为场景文件中没有兽人动画）
            if (_animatedSprite != null)
            {
                _animatedSprite.Animation = "goblin";
                _animatedSprite.Play();
            }
        }
        #endregion
    }
}