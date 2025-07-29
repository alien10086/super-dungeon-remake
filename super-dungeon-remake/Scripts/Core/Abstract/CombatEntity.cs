using Godot;
using SuperDungeonRemake.Core.Interfaces;
using System;

namespace SuperDungeonRemake.Core.Abstract;

/// <summary>
/// 战斗实体抽象基类
/// 继承自GameEntity，添加战斗功能
/// </summary>
public abstract partial class CombatEntity : GameEntity, ICombat
{
    #region Export Properties
    [Export] public int AttackPower { get; set; } = 10;
    [Export] public float AttackCooldown { get; set; } = 1.0f;
    [Export] public float AttackRange { get; set; } = 32f;
    [Export] public PackedScene WeaponScene { get; set; }
    #endregion
    
    #region ICombat Implementation
    public bool CanAttack => !IsDead && _attackTimer <= 0;
    #endregion
    
    #region Protected Fields
    protected float _attackTimer;
    protected Vector2 _lastAttackDirection = Vector2.Right;
    protected Area2D _attackArea;
    protected AudioStreamPlayer2D _attackSfx;
    #endregion
    
    #region Events
    public event Action<Node2D> AttackPerformed;
    public event Action<int> DamageDealt;
    #endregion
    
    #region Godot Lifecycle
    protected override void SetupComponents()
    {
        base.SetupComponents();
        
        _attackArea = GetNodeOrNull<Area2D>("AttackArea");
        _attackSfx = GetNodeOrNull<AudioStreamPlayer2D>("AttackSfx");
    }
    
    protected override void ConnectSignals()
    {
        base.ConnectSignals();
        
        if (_attackArea != null)
        {
            _attackArea.BodyEntered += OnAttackAreaBodyEntered;
        }
    }
    
    protected override void UpdateEntity(double delta)
    {
        base.UpdateEntity(delta);
        UpdateCombat(delta);
    }
    #endregion
    
    #region Virtual Methods
    /// <summary>
    /// 更新战斗逻辑
    /// </summary>
    /// <param name="delta">时间增量</param>
    protected virtual void UpdateCombat(double delta)
    {
        if (_attackTimer > 0)
        {
            _attackTimer -= (float)delta;
        }
    }
    
    /// <summary>
    /// 创建武器实例
    /// </summary>
    /// <param name="direction">攻击方向</param>
    /// <returns>武器节点</returns>
    protected virtual Node2D CreateWeapon(Vector2 direction)
    {
        if (WeaponScene == null) return null;
        
        var weapon = WeaponScene.Instantiate<Node2D>();
        if (weapon == null) return null;
        
        // 设置武器位置和旋转
        SetupWeaponTransform(weapon, direction);
        
        return weapon;
    }
    
    /// <summary>
    /// 设置武器的变换
    /// </summary>
    /// <param name="weapon">武器节点</param>
    /// <param name="direction">攻击方向</param>
    protected virtual void SetupWeaponTransform(Node2D weapon, Vector2 direction)
    {
        var angle = direction.Angle();
        weapon.Rotation = angle;
        weapon.Position = direction.Normalized() * 16f; // 武器偏移距离
    }
    
    /// <summary>
    /// 播放攻击音效
    /// </summary>
    protected virtual void PlayAttackSound()
    {
        if (_attackSfx == null) return;
        
        _attackSfx.PitchScale = _rng.RandfRange(0.9f, 1.1f);
        _attackSfx.Play();
    }
    
    /// <summary>
    /// 计算实际伤害值
    /// </summary>
    /// <param name="baseDamage">基础伤害</param>
    /// <returns>实际伤害</returns>
    protected virtual int CalculateDamage(int baseDamage)
    {
        // 添加随机伤害波动
        var variance = baseDamage * 0.2f; // 20%的伤害波动
        var randomOffset = _rng.RandfRange(-variance, variance);
        return Mathf.Max(1, Mathf.RoundToInt(baseDamage + randomOffset));
    }
    #endregion
    
    #region ICombat Implementation
    public virtual void Attack(Node2D target)
    {
        if (!CanAttack || target == null) return;
        
        var direction = (target.GlobalPosition - GlobalPosition).Normalized();
        AttackInDirection(direction);
    }
    
    public virtual void AttackInDirection(Vector2 direction)
    {
        if (!CanAttack) return;
        
        _lastAttackDirection = direction.Normalized();
        _attackTimer = AttackCooldown;
        
        // 创建武器
        var weapon = CreateWeapon(_lastAttackDirection);
        if (weapon != null)
        {
            AddChild(weapon);
        }
        
        // 播放攻击音效
        PlayAttackSound();
        
        // 触发攻击事件
        OnAttackPerformed();
    }
    
    public virtual bool IsInAttackRange(Node2D target)
    {
        if (target == null) return false;
        
        var distance = GlobalPosition.DistanceTo(target.GlobalPosition);
        return distance <= AttackRange;
    }
    #endregion
    
    #region Event Handlers
    /// <summary>
    /// 攻击区域检测到目标时调用
    /// </summary>
    /// <param name="body">进入的物体</param>
    protected virtual void OnAttackAreaBodyEntered(Node2D body)
    {
        // 子类可以重写此方法来处理攻击碰撞
    }
    
    /// <summary>
    /// 攻击执行时调用
    /// </summary>
    protected virtual void OnAttackPerformed()
    {
        AttackPerformed?.Invoke(null);
    }
    
    /// <summary>
    /// 造成伤害时调用
    /// </summary>
    /// <param name="damage">伤害值</param>
    protected virtual void OnDamageDealt(int damage)
    {
        DamageDealt?.Invoke(damage);
    }
    #endregion
    
    #region Public Methods
    /// <summary>
    /// 对目标造成伤害
    /// </summary>
    /// <param name="target">目标</param>
    /// <param name="damageMultiplier">伤害倍数</param>
    public virtual void DealDamageTo(Node2D target, float damageMultiplier = 1.0f)
    {
        if (target == null || !target.HasMethod("TakeDamage")) return;
        
        var damage = CalculateDamage(Mathf.RoundToInt(AttackPower * damageMultiplier));
        target.Call("TakeDamage", damage, this);
        
        OnDamageDealt(damage);
    }
    #endregion
}