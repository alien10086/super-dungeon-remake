using Godot;
using SuperDungeonRemake.Core.Abstract;
using SuperDungeonRemake.Core.Interfaces;
using SuperDungeonRemake.Utils;

namespace SuperDungeonRemake.Gameplay.Enemies;

/// <summary>
/// 基础敌人类
/// 继承自AIEntity，实现敌人的通用行为
/// </summary>
public partial class BaseEnemy : AIEntity
{
    #region Export Properties
    [Export] public EnemyType Type { get; set; } = EnemyType.Goblin;
    [Export] public float AggroRange { get; set; } = 80f;
    [Export] public float FleeHealthThreshold { get; set; } = 0.3f;
    [Export] public AudioStream[] HurtSounds { get; set; }
    [Export] public AudioStream[] DeathSounds { get; set; }
    [Export] public AudioStream[] AttackSounds { get; set; }
    #endregion
    
    #region Protected Fields
    protected AudioStreamPlayer2D _hurtSfx;
    protected AudioStreamPlayer2D _deathSfx;
    protected Timer _behaviorTimer;
    #endregion
    
    #region Godot Lifecycle
    protected override void InitializeEntity()
    {
        base.InitializeEntity();
        
        // 根据敌人类型设置属性
        SetupEnemyStats();
    }
    
    protected override void SetupComponents()
    {
        base.SetupComponents();
        
        _hurtSfx = GetNodeOrNull<AudioStreamPlayer2D>("HurtSfx");
        _deathSfx = GetNodeOrNull<AudioStreamPlayer2D>("DeathSfx");
        _behaviorTimer = GetNodeOrNull<Timer>("BehaviorTimer");
        
        if (_behaviorTimer != null)
        {
            _behaviorTimer.WaitTime = StateChangeInterval;
            _behaviorTimer.Timeout += OnBehaviorTimerTimeout;
        }
    }
    
    protected override void ConnectSignals()
    {
        base.ConnectSignals();
        
        // 连接攻击区域信号
        if (_attackArea != null)
        {
            _attackArea.BodyEntered += OnAttackAreaBodyEntered;
        }
    }
    #endregion
    
    #region Enemy Type Setup
    /// <summary>
    /// 根据敌人类型设置属性
    /// </summary>
    private void SetupEnemyStats()
    {
        switch (Type)
        {
            case EnemyType.Goblin:
                SetupGoblinStats();
                break;
            case EnemyType.Skeleton:
                SetupSkeletonStats();
                break;
            case EnemyType.Slime:
                SetupSlimeStats();
                break;
            case EnemyType.Orc:
                SetupOrcStats();
                break;
        }
    }
    
    private void SetupGoblinStats()
    {
        MaxHealth = 30;
        CurrentHealth = MaxHealth;
        Speed = 60f;
        AttackPower = 8;
        AttackCooldown = 1.2f;
        AttackRange = 25f;
        VisionRange = 80f;
        GoldValue = 3;
    }
    
    private void SetupSkeletonStats()
    {
        MaxHealth = 40;
        CurrentHealth = MaxHealth;
        Speed = 45f;
        AttackPower = 12;
        AttackCooldown = 1.5f;
        AttackRange = 30f;
        VisionRange = 90f;
        GoldValue = 5;
    }
    
    private void SetupSlimeStats()
    {
        MaxHealth = 20;
        CurrentHealth = MaxHealth;
        Speed = 80f;
        AttackPower = 6;
        AttackCooldown = 0.8f;
        AttackRange = 20f;
        VisionRange = 60f;
        GoldValue = 2;
    }
    
    private void SetupOrcStats()
    {
        MaxHealth = 60;
        CurrentHealth = MaxHealth;
        Speed = 40f;
        AttackPower = 18;
        AttackCooldown = 2.0f;
        AttackRange = 35f;
        VisionRange = 100f;
        GoldValue = 8;
    }
    #endregion
    
    #region AI Behavior Overrides
    protected override void UpdateChaseState(double delta)
    {
        base.UpdateChaseState(delta);
        
        // 哥布林在追击时会发出叫声
        if (Type == EnemyType.Goblin && _stateTimer > 2f)
        {
            PlayRandomSound(AttackSounds);
            _stateTimer = 0f;
        }
    }
    
    protected override void UpdateAttackState(double delta)
    {
        base.UpdateAttackState(delta);
        
        // 史莱姆攻击时会跳跃
        if (Type == EnemyType.Slime && CanAttack)
        {
            PerformSlimeJumpAttack();
        }
    }
    
    protected override void CheckStateTransitions()
    {
        base.CheckStateTransitions();
        
        // 检查是否需要逃跑
        var healthPercentage = (float)CurrentHealth / MaxHealth;
        if (healthPercentage <= FleeHealthThreshold && CurrentState != AIState.Flee)
        {
            SetState(AIState.Flee);
        }
    }
    #endregion
    
    #region Combat Overrides
    protected override void OnAttackAreaBodyEntered(Node2D body)
    {
        if (body.IsInGroup(GlobalConstants.GroupNames.PLAYER))
        {
            DealDamageTo(body);
            PlayRandomSound(AttackSounds);
        }
    }
    
    public override void AttackInDirection(Vector2 direction)
    {
        base.AttackInDirection(direction);
        
        // 播放攻击动画
        if (_sprite != null)
        {
            _sprite.Play("attack");
        }
    }
    #endregion
    
    #region Damage and Death Handling
    protected override void OnDamageTaken(int damage, Node source)
    {
        base.OnDamageTaken(damage, source);
        
        PlayRandomSound(HurtSounds);
        
        // 受到伤害时进入追击状态
        if (source != null && source.IsInGroup(GlobalConstants.GroupNames.PLAYER))
        {
            Target = source as Node2D;
            if (CurrentState != AIState.Attack)
            {
                SetState(AIState.Chase);
            }
        }
    }
    
    protected override void OnDeath()
    {
        base.OnDeath();
        
        PlayRandomSound(DeathSounds);
        
        // 播放死亡动画
        if (_sprite != null)
        {
            _sprite.Play("death");
        }
        
        // 禁用碰撞
        if (_collisionShape != null)
        {
            _collisionShape.SetDeferred("disabled", true);
        }
    }
    #endregion
    
    #region Special Abilities
    /// <summary>
    /// 史莱姆跳跃攻击
    /// </summary>
    private void PerformSlimeJumpAttack()
    {
        if (Target == null) return;
        
        var jumpDirection = (Target.GlobalPosition - GlobalPosition).Normalized();
        var jumpForce = jumpDirection * Speed * 2f;
        
        // 创建跳跃效果
        var tween = CreateTween();
        tween.TweenProperty(this, "position", GlobalPosition + jumpForce, 0.3f);
        tween.TweenCallback(Callable.From(() => DealDamageTo(Target)));
    }
    #endregion
    
    #region Audio
    /// <summary>
    /// 播放随机音效
    /// </summary>
    /// <param name="sounds">音效数组</param>
    private void PlayRandomSound(AudioStream[] sounds)
    {
        if (sounds == null || sounds.Length == 0 || _audioPlayer == null) return;
        
        var randomIndex = _rng.RandiRange(0, sounds.Length - 1);
        _audioPlayer.Stream = sounds[randomIndex];
        _audioPlayer.PitchScale = _rng.RandfRange(0.8f, 1.2f);
        _audioPlayer.Play();
    }
    #endregion
    
    #region Event Handlers
    private void OnBehaviorTimerTimeout()
    {
        // 定期改变行为，增加随机性
        if (CurrentState == AIState.Idle)
        {
            SetState(AIState.Patrol);
        }
        else if (CurrentState == AIState.Patrol && Target == null)
        {
            if (_rng.Randf() > 0.7f)
            {
                SetState(AIState.Idle);
            }
        }
    }
    #endregion
}

/// <summary>
/// 敌人类型枚举
/// </summary>
public enum EnemyType
{
    Goblin,     // 哥布林：快速、弱小
    Skeleton,   // 骷髅：中等、平衡
    Slime,      // 史莱姆：快速、跳跃攻击
    Orc         // 兽人：强壮、慢速
}