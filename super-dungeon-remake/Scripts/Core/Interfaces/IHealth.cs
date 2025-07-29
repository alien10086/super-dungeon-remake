using Godot;

namespace SuperDungeonRemake.Core.Interfaces;

/// <summary>
/// 生命值管理接口
/// 定义了所有具有生命值的游戏对象的基本行为
/// </summary>
public interface IHealth
{
    /// <summary>
    /// 当前生命值
    /// </summary>
    int CurrentHealth { get; }
    
    /// <summary>
    /// 最大生命值
    /// </summary>
    int MaxHealth { get; }
    
    /// <summary>
    /// 是否已死亡
    /// </summary>
    bool IsDead { get; }
    
    /// <summary>
    /// 生命值变化事件
    /// </summary>
    event System.Action<int, int> HealthChanged; // (currentHealth, maxHealth)
    
    /// <summary>
    /// 受到伤害
    /// </summary>
    /// <param name="damage">伤害值</param>
    /// <param name="source">伤害来源</param>
    void TakeDamage(int damage, Node source = null);
    
    /// <summary>
    /// 恢复生命值
    /// </summary>
    /// <param name="amount">恢复量</param>
    void Heal(int amount);
    
    /// <summary>
    /// 死亡处理
    /// </summary>
    void Die();
}