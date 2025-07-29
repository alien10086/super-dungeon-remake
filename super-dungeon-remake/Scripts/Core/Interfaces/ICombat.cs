using Godot;

namespace SuperDungeonRemake.Core.Interfaces;

/// <summary>
/// 战斗能力接口
/// 定义了所有具有战斗能力的游戏对象的基本行为
/// </summary>
public interface ICombat
{
    /// <summary>
    /// 攻击力
    /// </summary>
    int AttackPower { get; set; }
    
    /// <summary>
    /// 攻击冷却时间
    /// </summary>
    float AttackCooldown { get; set; }
    
    /// <summary>
    /// 攻击范围
    /// </summary>
    float AttackRange { get; set; }
    
    /// <summary>
    /// 是否可以攻击
    /// </summary>
    bool CanAttack { get; }
    
    /// <summary>
    /// 攻击目标
    /// </summary>
    /// <param name="target">攻击目标</param>
    void Attack(Node2D target);
    
    /// <summary>
    /// 在指定方向攻击
    /// </summary>
    /// <param name="direction">攻击方向</param>
    void AttackInDirection(Vector2 direction);
    
    /// <summary>
    /// 检查是否在攻击范围内
    /// </summary>
    /// <param name="target">目标</param>
    /// <returns>是否在范围内</returns>
    bool IsInAttackRange(Node2D target);
}

/// <summary>
/// AI行为接口
/// 定义了AI控制的游戏对象的基本行为
/// </summary>
public interface IAIBehavior
{
    /// <summary>
    /// AI状态
    /// </summary>
    AIState CurrentState { get; }
    
    /// <summary>
    /// 目标对象
    /// </summary>
    Node2D Target { get; set; }
    
    /// <summary>
    /// 视野范围
    /// </summary>
    float VisionRange { get; set; }
    
    /// <summary>
    /// 更新AI行为
    /// </summary>
    /// <param name="delta">时间增量</param>
    void UpdateAI(double delta);
    
    /// <summary>
    /// 设置AI状态
    /// </summary>
    /// <param name="newState">新状态</param>
    void SetState(AIState newState);
    
    /// <summary>
    /// 寻找目标
    /// </summary>
    /// <returns>找到的目标</returns>
    Node2D FindTarget();
}

/// <summary>
/// AI状态枚举
/// </summary>
public enum AIState
{
    Idle,       // 空闲
    Patrol,     // 巡逻
    Chase,      // 追击
    Attack,     // 攻击
    Flee,       // 逃跑
    Dead        // 死亡
}