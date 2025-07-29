using Godot;
using SuperDungeonRemake.Core.Interfaces;
using System;

namespace SuperDungeonRemake.Core.Abstract;

/// <summary>
/// 游戏实体抽象基类
/// 提供所有游戏对象的通用功能实现
/// </summary>
public abstract partial class GameEntity : CharacterBody2D, IHealth, IMovable
{
    #region Export Properties
    [Export] public float Speed { get; set; } = 100f;
    [Export] public int MaxHealth { get; set; } = 100;
    [Export] public float InvulnerabilityTime { get; set; } = 0.5f;
    #endregion
    
    #region IHealth Implementation
    public int CurrentHealth { get; protected set; }
    public bool IsDead { get; protected set; }
    public event Action<int, int> HealthChanged;
    #endregion
    
    #region IMovable Implementation
    public Vector2 Direction { get; set; }
    public virtual bool CanMove => !IsDead && _invulnerabilityTimer <= 0;
    #endregion
    
    #region Protected Fields
    protected AnimatedSprite2D _sprite;
    protected CollisionShape2D _collisionShape;
    protected AudioStreamPlayer2D _audioPlayer;
    protected float _invulnerabilityTimer;
    protected RandomNumberGenerator _rng;
    #endregion
    
    #region Godot Lifecycle
    public override void _Ready()
    {
        InitializeEntity();
        SetupComponents();
        ConnectSignals();
    }
    
    public override void _PhysicsProcess(double delta)
    {
        UpdateTimers(delta);
        
        if (!IsDead)
        {
            UpdateEntity(delta);
            HandleMovement(delta);
        }
    }
    #endregion
    
    #region Virtual Methods
    /// <summary>
    /// 初始化实体
    /// </summary>
    protected virtual void InitializeEntity()
    {
        CurrentHealth = MaxHealth;
        _rng = new RandomNumberGenerator();
        _rng.Randomize();
    }
    
    /// <summary>
    /// 设置组件引用
    /// </summary>
    protected virtual void SetupComponents()
    {
        _sprite = GetNodeOrNull<AnimatedSprite2D>("AnimatedSprite2D");
        _collisionShape = GetNodeOrNull<CollisionShape2D>("CollisionShape2D");
        _audioPlayer = GetNodeOrNull<AudioStreamPlayer2D>("AudioStreamPlayer2D");
    }
    
    /// <summary>
    /// 连接信号
    /// </summary>
    protected virtual void ConnectSignals()
    {
        // 子类可以重写此方法来连接特定的信号
    }
    
    /// <summary>
    /// 更新实体逻辑
    /// </summary>
    /// <param name="delta">时间增量</param>
    protected virtual void UpdateEntity(double delta)
    {
        // 子类实现具体的更新逻辑
    }
    
    /// <summary>
    /// 处理移动逻辑
    /// </summary>
    /// <param name="delta">时间增量</param>
    protected virtual void HandleMovement(double delta)
    {
        if (CanMove && Direction != Vector2.Zero)
        {
            Velocity = Direction.Normalized() * Speed;
            MoveAndSlide();
        }
        else
        {
            Velocity = Vector2.Zero;
        }
    }
    #endregion
    
    #region IHealth Implementation
    public virtual void TakeDamage(int damage, Node source = null)
    {
        if (IsDead || _invulnerabilityTimer > 0) return;
        
        CurrentHealth = Mathf.Max(0, CurrentHealth - damage);
        _invulnerabilityTimer = InvulnerabilityTime;
        
        HealthChanged?.Invoke(CurrentHealth, MaxHealth);
        
        OnDamageTaken(damage, source);
        
        if (CurrentHealth <= 0)
        {
            Die();
        }
    }
    
    public virtual void Heal(int amount)
    {
        if (IsDead) return;
        
        CurrentHealth = Mathf.Min(MaxHealth, CurrentHealth + amount);
        HealthChanged?.Invoke(CurrentHealth, MaxHealth);
        
        OnHealed(amount);
    }
    
    public virtual void Die()
    {
        if (IsDead) return;
        
        IsDead = true;
        OnDeath();
    }
    #endregion
    
    #region IMovable Implementation
    public virtual void MoveTo(Vector2 targetPosition)
    {
        Direction = (targetPosition - GlobalPosition).Normalized();
    }
    
    public virtual void MoveInDirection(Vector2 direction)
    {
        Direction = direction.Normalized();
    }
    
    public virtual void StopMovement()
    {
        Direction = Vector2.Zero;
        Velocity = Vector2.Zero;
    }
    #endregion
    
    #region Protected Event Handlers
    /// <summary>
    /// 受到伤害时调用
    /// </summary>
    /// <param name="damage">伤害值</param>
    /// <param name="source">伤害来源</param>
    protected virtual void OnDamageTaken(int damage, Node source)
    {
        // 播放受伤效果
        PlayHurtEffect();
    }
    
    /// <summary>
    /// 治疗时调用
    /// </summary>
    /// <param name="amount">治疗量</param>
    protected virtual void OnHealed(int amount)
    {
        // 播放治疗效果
    }
    
    /// <summary>
    /// 死亡时调用
    /// </summary>
    protected virtual void OnDeath()
    {
        // 播放死亡效果
        PlayDeathEffect();
    }
    #endregion
    
    #region Private Methods
    private void UpdateTimers(double delta)
    {
        if (_invulnerabilityTimer > 0)
        {
            _invulnerabilityTimer -= (float)delta;
        }
    }
    
    private void PlayHurtEffect()
    {
        if (_sprite == null) return;
        
        var tween = CreateTween();
        tween.TweenProperty(_sprite, "modulate:a", 0.5f, 0.1f);
        tween.TweenProperty(_sprite, "modulate:a", 1.0f, 0.1f);
    }
    
    private void PlayDeathEffect()
    {
        if (_sprite == null) return;
        
        var tween = CreateTween();
        tween.Parallel().TweenProperty(this, "scale", Vector2.Zero, 0.3f);
        tween.Parallel().TweenProperty(_sprite, "modulate:a", 0.0f, 0.3f);
        
        // 延迟删除
        tween.TweenCallback(Callable.From(QueueFree));
    }
    #endregion
}