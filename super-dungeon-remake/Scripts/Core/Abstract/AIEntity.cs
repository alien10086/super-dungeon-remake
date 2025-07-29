using Godot;
using SuperDungeonRemake.Core.Interfaces;
using SuperDungeonRemake.Utils;
using System;

namespace SuperDungeonRemake.Core.Abstract;

/// <summary>
/// AI实体抽象基类
/// 继承自CombatEntity，添加AI行为功能
/// </summary>
public abstract partial class AIEntity : CombatEntity, IAIBehavior
{
    #region Export Properties
    [Export] public float VisionRange { get; set; } = 100f;
    [Export] public float PatrolRadius { get; set; } = 50f;
    [Export] public float StateChangeInterval { get; set; } = 2f;
    [Export] public int GoldValue { get; set; } = 5;
    #endregion
    
    #region IAIBehavior Implementation
    public AIState CurrentState { get; private set; } = AIState.Idle;
    public Node2D Target { get; set; }
    #endregion
    
    #region Protected Fields
    protected Vector2 _spawnPosition;
    protected Vector2 _patrolTarget;
    protected float _stateTimer;
    protected float _lastTargetDistance = float.MaxValue;
    protected ProgressBar _healthBar;
    #endregion
    
    #region State Machine Events
    public event Action<AIState, AIState> StateChanged;
    public event Action<Node2D> TargetAcquired;
    public event Action TargetLost;
    #endregion
    
    #region Godot Lifecycle
    protected override void InitializeEntity()
    {
        base.InitializeEntity();
        
        _spawnPosition = GlobalPosition;
        _patrolTarget = _spawnPosition;
        
        AddToGroup(GlobalConstants.GroupNames.ENEMIES);
    }
    
    protected override void SetupComponents()
    {
        base.SetupComponents();
        
        _healthBar = GetNodeOrNull<ProgressBar>("HealthBar");
        
        if (_healthBar != null)
        {
            _healthBar.MaxValue = MaxHealth;
            _healthBar.Value = CurrentHealth;
            _healthBar.Visible = false;
        }
    }
    
    protected override void ConnectSignals()
    {
        base.ConnectSignals();
        
        // 连接生命值变化事件
        HealthChanged += OnHealthChanged;
    }
    
    protected override void UpdateEntity(double delta)
    {
        base.UpdateEntity(delta);
        
        if (!IsDead)
        {
            UpdateAI(delta);
            UpdateHealthBar();
        }
    }
    #endregion
    
    #region IAIBehavior Implementation
    public virtual void UpdateAI(double delta)
    {
        _stateTimer += (float)delta;
        
        // 根据当前状态执行相应的AI逻辑
        switch (CurrentState)
        {
            case AIState.Idle:
                UpdateIdleState(delta);
                break;
            case AIState.Patrol:
                UpdatePatrolState(delta);
                break;
            case AIState.Chase:
                UpdateChaseState(delta);
                break;
            case AIState.Attack:
                UpdateAttackState(delta);
                break;
            case AIState.Flee:
                UpdateFleeState(delta);
                break;
        }
        
        // 检查状态转换
        CheckStateTransitions();
    }
    
    public virtual void SetState(AIState newState)
    {
        if (CurrentState == newState) return;
        
        var oldState = CurrentState;
        CurrentState = newState;
        _stateTimer = 0f;
        
        OnStateChanged(oldState, newState);
        StateChanged?.Invoke(oldState, newState);
    }
    
    public virtual Node2D FindTarget()
    {
        var player = GetTree().GetFirstNodeInGroup(GlobalConstants.GroupNames.PLAYER);
        if (player == null) return null;
        
        var playerNode = player as Node2D;
        if (playerNode == null) return null;
        
        var distance = GlobalPosition.DistanceTo(playerNode.GlobalPosition);
        if (distance <= VisionRange)
        {
            // 检查视线是否被阻挡
            if (HasLineOfSight(playerNode))
            {
                return playerNode;
            }
        }
        
        return null;
    }
    #endregion
    
    #region Virtual State Methods
    /// <summary>
    /// 更新空闲状态
    /// </summary>
    protected virtual void UpdateIdleState(double delta)
    {
        StopMovement();
        
        // 定期切换到巡逻状态
        if (_stateTimer >= StateChangeInterval)
        {
            SetState(AIState.Patrol);
        }
    }
    
    /// <summary>
    /// 更新巡逻状态
    /// </summary>
    protected virtual void UpdatePatrolState(double delta)
    {
        // 移动到巡逻目标点
        var distanceToPatrol = GlobalPosition.DistanceTo(_patrolTarget);
        
        if (distanceToPatrol < 10f)
        {
            // 到达巡逻点，选择新的巡逻目标
            GenerateNewPatrolTarget();
        }
        else
        {
            MoveTo(_patrolTarget);
        }
    }
    
    /// <summary>
    /// 更新追击状态
    /// </summary>
    protected virtual void UpdateChaseState(double delta)
    {
        if (Target == null)
        {
            SetState(AIState.Patrol);
            return;
        }
        
        var distance = GlobalPosition.DistanceTo(Target.GlobalPosition);
        
        if (distance <= AttackRange)
        {
            SetState(AIState.Attack);
        }
        else
        {
            MoveTo(Target.GlobalPosition);
        }
        
        _lastTargetDistance = distance;
    }
    
    /// <summary>
    /// 更新攻击状态
    /// </summary>
    protected virtual void UpdateAttackState(double delta)
    {
        if (Target == null)
        {
            SetState(AIState.Patrol);
            return;
        }
        
        var distance = GlobalPosition.DistanceTo(Target.GlobalPosition);
        
        if (distance > AttackRange * 1.2f) // 添加一些滞后，避免状态频繁切换
        {
            SetState(AIState.Chase);
        }
        else if (CanAttack)
        {
            Attack(Target);
        }
        
        // 面向目标
        var direction = (Target.GlobalPosition - GlobalPosition).Normalized();
        _lastAttackDirection = direction;
    }
    
    /// <summary>
    /// 更新逃跑状态
    /// </summary>
    protected virtual void UpdateFleeState(double delta)
    {
        if (Target == null)
        {
            SetState(AIState.Patrol);
            return;
        }
        
        // 远离目标
        var fleeDirection = (GlobalPosition - Target.GlobalPosition).Normalized();
        MoveInDirection(fleeDirection);
        
        // 如果距离足够远，回到巡逻状态
        var distance = GlobalPosition.DistanceTo(Target.GlobalPosition);
        if (distance > VisionRange * 1.5f)
        {
            Target = null;
            SetState(AIState.Patrol);
        }
    }
    #endregion
    
    #region Protected Methods
    /// <summary>
    /// 检查状态转换条件
    /// </summary>
    protected virtual void CheckStateTransitions()
    {
        // 寻找目标
        var newTarget = FindTarget();
        
        if (newTarget != null && Target != newTarget)
        {
            Target = newTarget;
            TargetAcquired?.Invoke(Target);
            
            if (CurrentState != AIState.Chase && CurrentState != AIState.Attack)
            {
                SetState(AIState.Chase);
            }
        }
        else if (Target != null && newTarget == null)
        {
            // 失去目标
            Target = null;
            TargetLost?.Invoke();
            
            if (CurrentState == AIState.Chase || CurrentState == AIState.Attack)
            {
                SetState(AIState.Patrol);
            }
        }
        
        // 检查是否需要逃跑（生命值过低）
        if (CurrentHealth < MaxHealth * 0.2f && CurrentState != AIState.Flee)
        {
            SetState(AIState.Flee);
        }
    }
    
    /// <summary>
    /// 生成新的巡逻目标点
    /// </summary>
    protected virtual void GenerateNewPatrolTarget()
    {
        var angle = _rng.Randf() * Mathf.Tau;
        var distance = _rng.RandfRange(PatrolRadius * 0.3f, PatrolRadius);
        
        _patrolTarget = _spawnPosition + new Vector2(
            Mathf.Cos(angle) * distance,
            Mathf.Sin(angle) * distance
        );
    }
    
    /// <summary>
    /// 检查是否有视线
    /// </summary>
    /// <param name="target">目标</param>
    /// <returns>是否有视线</returns>
    protected virtual bool HasLineOfSight(Node2D target)
    {
        var spaceState = GetWorld2D().DirectSpaceState;
        var query = PhysicsRayQueryParameters2D.Create(
            GlobalPosition,
            target.GlobalPosition
        );
        
        // 排除自己和目标（如果它们是CollisionObject2D）
        var excludeList = new Godot.Collections.Array<Rid>();
        if (this is CollisionObject2D selfCollision)
            excludeList.Add(selfCollision.GetRid());
        if (target is CollisionObject2D targetCollision)
            excludeList.Add(targetCollision.GetRid());
        query.Exclude = excludeList;
        
        var result = spaceState.IntersectRay(query);
        return result.Count == 0; // 没有碰撞说明有视线
    }
    
    /// <summary>
    /// 更新生命值条
    /// </summary>
    protected virtual void UpdateHealthBar()
    {
        if (_healthBar == null) return;
        
        _healthBar.Value = CurrentHealth;
        _healthBar.Visible = CurrentHealth < MaxHealth;
    }
    #endregion
    
    #region Event Handlers
    /// <summary>
    /// 状态改变时调用
    /// </summary>
    /// <param name="oldState">旧状态</param>
    /// <param name="newState">新状态</param>
    protected virtual void OnStateChanged(AIState oldState, AIState newState)
    {
        // 子类可以重写此方法来处理状态变化
        GD.Print($"{Name}: State changed from {oldState} to {newState}");
    }
    
    /// <summary>
    /// 生命值变化时调用
    /// </summary>
    /// <param name="currentHealth">当前生命值</param>
    /// <param name="maxHealth">最大生命值</param>
    protected virtual void OnHealthChanged(int currentHealth, int maxHealth)
    {
        UpdateHealthBar();
    }
    
    /// <summary>
    /// 死亡时调用
    /// </summary>
    protected override void OnDeath()
    {
        base.OnDeath();
        
        SetState(AIState.Dead);
        
        // 给玩家金币奖励
        var gameData = GetNodeOrNull<GameData>("/root/GameData");
        if (gameData != null)
        {
            gameData.AddGold(GoldValue);
            gameData.AddKill();
        }
    }
    #endregion
}