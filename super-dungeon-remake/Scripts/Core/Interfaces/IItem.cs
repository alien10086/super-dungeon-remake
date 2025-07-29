using Godot;

namespace SuperDungeonRemake.Core.Interfaces;

/// <summary>
/// 道具接口
/// 定义所有道具的基本属性和行为
/// </summary>
public interface IItem
{
    #region Properties
    /// <summary>
    /// 道具ID
    /// </summary>
    string ItemId { get; }
    
    /// <summary>
    /// 道具名称
    /// </summary>
    string ItemName { get; }
    
    /// <summary>
    /// 道具描述
    /// </summary>
    string Description { get; }
    
    /// <summary>
    /// 道具图标
    /// </summary>
    Texture2D Icon { get; }
    
    /// <summary>
    /// 道具类型
    /// </summary>
    ItemType Type { get; }
    
    /// <summary>
    /// 道具稀有度
    /// </summary>
    ItemRarity Rarity { get; }
    
    /// <summary>
    /// 道具价值
    /// </summary>
    int Value { get; }
    
    /// <summary>
    /// 是否可堆叠
    /// </summary>
    bool IsStackable { get; }
    
    /// <summary>
    /// 最大堆叠数量
    /// </summary>
    int MaxStackSize { get; }
    
    /// <summary>
    /// 当前堆叠数量
    /// </summary>
    int StackCount { get; set; }
    #endregion
    
    #region Methods
    /// <summary>
    /// 使用道具
    /// </summary>
    /// <param name="user">使用者</param>
    /// <returns>是否成功使用</returns>
    bool Use(Node2D user);
    
    /// <summary>
    /// 获取道具信息
    /// </summary>
    /// <returns>道具信息字符串</returns>
    string GetItemInfo();
    
    /// <summary>
    /// 克隆道具
    /// </summary>
    /// <returns>道具副本</returns>
    IItem Clone();
    
    /// <summary>
    /// 检查是否可以与另一个道具堆叠
    /// </summary>
    /// <param name="other">另一个道具</param>
    /// <returns>是否可以堆叠</returns>
    bool CanStackWith(IItem other);
    #endregion
}

/// <summary>
/// 可装备道具接口
/// </summary>
public interface IEquippable : IItem
{
    #region Properties
    /// <summary>
    /// 装备槽位类型
    /// </summary>
    EquipmentSlot SlotType { get; }
    
    /// <summary>
    /// 是否已装备
    /// </summary>
    bool IsEquipped { get; set; }
    
    /// <summary>
    /// 装备等级要求
    /// </summary>
    int LevelRequirement { get; }
    #endregion
    
    #region Methods
    /// <summary>
    /// 装备道具
    /// </summary>
    /// <param name="equipper">装备者</param>
    /// <returns>是否成功装备</returns>
    bool Equip(Node2D equipper);
    
    /// <summary>
    /// 卸下道具
    /// </summary>
    /// <param name="equipper">装备者</param>
    /// <returns>是否成功卸下</returns>
    bool Unequip(Node2D equipper);
    
    /// <summary>
    /// 获取装备属性加成
    /// </summary>
    /// <returns>属性加成字典</returns>
    Godot.Collections.Dictionary<string, float> GetStatBonuses();
    #endregion
}

/// <summary>
/// 可消耗道具接口
/// </summary>
public interface IConsumable : IItem
{
    #region Properties
    /// <summary>
    /// 冷却时间
    /// </summary>
    float CooldownTime { get; }
    
    /// <summary>
    /// 是否在冷却中
    /// </summary>
    bool IsOnCooldown { get; }
    #endregion
    
    #region Methods
    /// <summary>
    /// 消耗道具
    /// </summary>
    /// <param name="consumer">消耗者</param>
    /// <returns>是否成功消耗</returns>
    bool Consume(Node2D consumer);
    
    /// <summary>
    /// 获取消耗效果描述
    /// </summary>
    /// <returns>效果描述</returns>
    string GetEffectDescription();
    #endregion
}

/// <summary>
/// 道具类型枚举
/// </summary>
public enum ItemType
{
    Weapon,     // 武器
    Armor,      // 护甲
    Accessory,  // 饰品
    Consumable, // 消耗品
    Material,   // 材料
    Quest,      // 任务物品
    Misc        // 杂项
}

/// <summary>
/// 道具稀有度枚举
/// </summary>
public enum ItemRarity
{
    Common,     // 普通 - 白色
    Uncommon,   // 不常见 - 绿色
    Rare,       // 稀有 - 蓝色
    Epic,       // 史诗 - 紫色
    Legendary   // 传说 - 橙色
}

/// <summary>
/// 装备槽位枚举
/// </summary>
public enum EquipmentSlot
{
    MainHand,   // 主手
    OffHand,    // 副手
    Head,       // 头部
    Chest,      // 胸部
    Legs,       // 腿部
    Feet,       // 脚部
    Ring,       // 戒指
    Necklace    // 项链
}