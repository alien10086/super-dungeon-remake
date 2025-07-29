using Godot;

namespace SuperDungeonRemake.Core.Interfaces;

/// <summary>
/// 移动能力接口
/// 定义了所有可移动游戏对象的基本行为
/// </summary>
public interface IMovable
{
    /// <summary>
    /// 移动速度
    /// </summary>
    float Speed { get; set; }
    
    /// <summary>
    /// 当前速度向量
    /// </summary>
    Vector2 Velocity { get; set; }
    
    /// <summary>
    /// 移动方向
    /// </summary>
    Vector2 Direction { get; set; }
    
    /// <summary>
    /// 是否可以移动
    /// </summary>
    bool CanMove { get; }
    
    /// <summary>
    /// 移动到指定位置
    /// </summary>
    /// <param name="targetPosition">目标位置</param>
    void MoveTo(Vector2 targetPosition);
    
    /// <summary>
    /// 按方向移动
    /// </summary>
    /// <param name="direction">移动方向</param>
    void MoveInDirection(Vector2 direction);
    
    /// <summary>
    /// 停止移动
    /// </summary>
    void StopMovement();
}