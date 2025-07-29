using Godot;
using System.Collections.Generic;
using System.Linq;
using SuperDungeonRemake.Core.Interfaces;
using SuperDungeonRemake.Core.Abstract;

namespace SuperDungeonRemake.Gameplay.Systems;

/// <summary>
/// 库存管理系统
/// 处理道具的存储、管理和使用
/// </summary>
public partial class InventorySystem : Node
{
    #region Signals
    /// <summary>
    /// 道具添加到库存时触发
    /// </summary>
    [Signal] public delegate void ItemAddedEventHandler(string itemId, int amount);
    
    /// <summary>
    /// 道具从库存移除时触发
    /// </summary>
    [Signal] public delegate void ItemRemovedEventHandler(string itemId, int amount);
    
    /// <summary>
    /// 道具使用时触发
    /// </summary>
    [Signal] public delegate void ItemUsedEventHandler(string itemId, string userId);
    
    /// <summary>
    /// 库存满时触发
    /// </summary>
    [Signal] public delegate void InventoryFullEventHandler();
    #endregion
    
    #region Export Properties
    [Export] public int MaxSlots { get; set; } = 30;
    [Export] public bool AutoSort { get; set; } = true;
    [Export] public bool AutoStack { get; set; } = true;
    #endregion
    
    #region Properties
    /// <summary>
    /// 库存槽位列表
    /// </summary>
    public List<InventorySlot> Slots { get; private set; }
    
    /// <summary>
    /// 当前使用的槽位数量
    /// </summary>
    public int UsedSlots => Slots.Count(slot => slot.Item != null);
    
    /// <summary>
    /// 剩余空槽位数量
    /// </summary>
    public int FreeSlots => MaxSlots - UsedSlots;
    
    /// <summary>
    /// 库存是否已满
    /// </summary>
    public bool IsFull => UsedSlots >= MaxSlots;
    #endregion
    
    #region Godot Lifecycle
    public override void _Ready()
    {
        InitializeInventory();
    }
    
    /// <summary>
    /// 初始化库存系统
    /// </summary>
    private void InitializeInventory()
    {
        Slots = new List<InventorySlot>();
        
        // 初始化槽位
        for (int i = 0; i < MaxSlots; i++)
        {
            Slots.Add(new InventorySlot(i));
        }
    }
    #endregion
    
    #region Item Management
    /// <summary>
    /// 添加道具到库存
    /// </summary>
    /// <param name="item">要添加的道具</param>
    /// <param name="amount">数量</param>
    /// <returns>实际添加的数量</returns>
    public int AddItem(IItem item, int amount = 1)
    {
        if (item == null || amount <= 0) return 0;
        
        int remainingAmount = amount;
        
        // 如果启用自动堆叠，先尝试堆叠到现有道具
        if (AutoStack && item.IsStackable)
        {
            remainingAmount = TryStackItem(item, remainingAmount);
        }
        
        // 如果还有剩余，尝试放入空槽位
        if (remainingAmount > 0)
        {
            remainingAmount = TryAddToEmptySlots(item, remainingAmount);
        }
        
        int actualAdded = amount - remainingAmount;
        
        if (actualAdded > 0)
        {
            EmitSignal(SignalName.ItemAdded, item.ItemId, actualAdded);
            
            if (AutoSort)
            {
                SortInventory();
            }
        }
        
        if (remainingAmount > 0)
        {
            EmitSignal(SignalName.InventoryFull);
        }
        
        return actualAdded;
    }
    
    /// <summary>
    /// 从库存移除道具
    /// </summary>
    /// <param name="item">要移除的道具</param>
    /// <param name="amount">数量</param>
    /// <returns>实际移除的数量</returns>
    public int RemoveItem(IItem item, int amount = 1)
    {
        if (item == null || amount <= 0) return 0;
        
        int remainingAmount = amount;
        var slotsToCheck = Slots.Where(slot => slot.Item?.ItemId == item.ItemId).ToList();
        
        foreach (var slot in slotsToCheck)
        {
            if (remainingAmount <= 0) break;
            
            int removeFromSlot = Mathf.Min(remainingAmount, slot.Item.StackCount);
            slot.Item.StackCount -= removeFromSlot;
            remainingAmount -= removeFromSlot;
            
            if (slot.Item.StackCount <= 0)
            {
                slot.Item = null;
            }
        }
        
        int actualRemoved = amount - remainingAmount;
        
        if (actualRemoved > 0)
        {
            EmitSignal(SignalName.ItemRemoved, item.ItemId, actualRemoved);
        }
        
        return actualRemoved;
    }
    
    /// <summary>
    /// 使用道具
    /// </summary>
    /// <param name="slotIndex">槽位索引</param>
    /// <param name="user">使用者</param>
    /// <returns>是否成功使用</returns>
    public bool UseItem(int slotIndex, Node2D user)
    {
        if (slotIndex < 0 || slotIndex >= Slots.Count) return false;
        
        var slot = Slots[slotIndex];
        if (slot.Item == null) return false;
        
        bool success = slot.Item.Use(user);
        
        if (success)
        {
            EmitSignal(SignalName.ItemUsed, slot.Item.ItemId, user.Name);
            
            // 如果是消耗品，减少数量
            if (slot.Item is IConsumable)
            {
                slot.Item.StackCount--;
                if (slot.Item.StackCount <= 0)
                {
                    slot.Item = null;
                }
            }
        }
        
        return success;
    }
    
    /// <summary>
    /// 获取道具数量
    /// </summary>
    /// <param name="itemId">道具ID</param>
    /// <returns>道具总数量</returns>
    public int GetItemCount(string itemId)
    {
        return Slots.Where(slot => slot.Item?.ItemId == itemId)
                   .Sum(slot => slot.Item.StackCount);
    }
    
    /// <summary>
    /// 检查是否有足够的道具
    /// </summary>
    /// <param name="itemId">道具ID</param>
    /// <param name="amount">需要的数量</param>
    /// <returns>是否有足够的道具</returns>
    public bool HasItem(string itemId, int amount = 1)
    {
        return GetItemCount(itemId) >= amount;
    }
    
    /// <summary>
    /// 获取指定类型的所有道具
    /// </summary>
    /// <param name="itemType">道具类型</param>
    /// <returns>道具列表</returns>
    public List<IItem> GetItemsByType(ItemType itemType)
    {
        return Slots.Where(slot => slot.Item?.Type == itemType)
                   .Select(slot => slot.Item)
                   .ToList();
    }
    #endregion
    
    #region Helper Methods
    /// <summary>
    /// 尝试堆叠道具到现有槽位
    /// </summary>
    /// <param name="item">道具</param>
    /// <param name="amount">数量</param>
    /// <returns>剩余数量</returns>
    private int TryStackItem(IItem item, int amount)
    {
        int remainingAmount = amount;
        
        var stackableSlots = Slots.Where(slot => 
            slot.Item != null && 
            slot.Item.CanStackWith(item))
            .ToList();
        
        foreach (var slot in stackableSlots)
        {
            if (remainingAmount <= 0) break;
            
            int canAdd = slot.Item.MaxStackSize - slot.Item.StackCount;
            int addAmount = Mathf.Min(remainingAmount, canAdd);
            
            slot.Item.StackCount += addAmount;
            remainingAmount -= addAmount;
        }
        
        return remainingAmount;
    }
    
    /// <summary>
    /// 尝试添加到空槽位
    /// </summary>
    /// <param name="item">道具</param>
    /// <param name="amount">数量</param>
    /// <returns>剩余数量</returns>
    private int TryAddToEmptySlots(IItem item, int amount)
    {
        int remainingAmount = amount;
        
        var emptySlots = Slots.Where(slot => slot.Item == null).ToList();
        
        foreach (var slot in emptySlots)
        {
            if (remainingAmount <= 0) break;
            
            var newItem = item.Clone();
            if (newItem != null)
            {
                int addAmount = Mathf.Min(remainingAmount, newItem.MaxStackSize);
                newItem.StackCount = addAmount;
                slot.Item = newItem;
                remainingAmount -= addAmount;
            }
        }
        
        return remainingAmount;
    }
    
    /// <summary>
    /// 排序库存
    /// </summary>
    public void SortInventory()
    {
        var items = Slots.Where(slot => slot.Item != null)
                         .Select(slot => slot.Item)
                         .OrderBy(item => item.Type)
                         .ThenBy(item => item.Rarity)
                         .ThenBy(item => item.ItemName)
                         .ToList();
        
        // 清空所有槽位
        foreach (var slot in Slots)
        {
            slot.Item = null;
        }
        
        // 重新放置道具
        for (int i = 0; i < items.Count && i < Slots.Count; i++)
        {
            Slots[i].Item = items[i];
        }
    }
    
    /// <summary>
    /// 清空库存
    /// </summary>
    public void ClearInventory()
    {
        foreach (var slot in Slots)
        {
            slot.Item = null;
        }
    }
    
    /// <summary>
    /// 获取库存信息字符串
    /// </summary>
    /// <returns>库存信息</returns>
    public string GetInventoryInfo()
    {
        var info = $"库存信息:\n";
        info += $"使用槽位: {UsedSlots}/{MaxSlots}\n";
        info += $"道具种类: {Slots.Where(slot => slot.Item != null).Select(slot => slot.Item.ItemId).Distinct().Count()}\n";
        
        var itemsByType = Slots.Where(slot => slot.Item != null)
                              .GroupBy(slot => slot.Item.Type)
                              .ToDictionary(g => g.Key, g => g.Sum(slot => slot.Item.StackCount));
        
        foreach (var kvp in itemsByType)
        {
            info += $"{kvp.Key}: {kvp.Value}个\n";
        }
        
        return info;
    }
    #endregion
}

/// <summary>
/// 库存槽位类
/// </summary>
public partial class InventorySlot : RefCounted
{
    #region Properties
    /// <summary>
    /// 槽位索引
    /// </summary>
    public int Index { get; private set; }
    
    /// <summary>
    /// 槽位中的道具
    /// </summary>
    public IItem Item { get; set; }
    
    /// <summary>
    /// 槽位是否为空
    /// </summary>
    public bool IsEmpty => Item == null;
    
    /// <summary>
    /// 槽位是否已满
    /// </summary>
    public bool IsFull => Item != null && Item.StackCount >= Item.MaxStackSize;
    #endregion
    
    #region Constructor
    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="index">槽位索引</param>
    public InventorySlot(int index)
    {
        Index = index;
        Item = null;
    }
    #endregion
    
    #region Methods
    /// <summary>
    /// 获取槽位信息
    /// </summary>
    /// <returns>槽位信息字符串</returns>
    public string GetSlotInfo()
    {
        if (IsEmpty)
        {
            return $"槽位 {Index}: 空";
        }
        
        return $"槽位 {Index}: {Item.ItemName} x{Item.StackCount}";
    }
    #endregion
}