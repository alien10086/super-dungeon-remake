using Godot;
using SuperDungeonRemake.Core.Interfaces;

namespace SuperDungeonRemake.Core.Abstract;

/// <summary>
/// 道具基类
/// 实现IItem接口的基本功能
/// </summary>
public abstract partial class BaseItem : Node2D, IItem
{
    #region Export Properties
    [Export] public string ItemId { get; protected set; } = "";
    [Export] public string ItemName { get; protected set; } = "";
    [Export] public string Description { get; protected set; } = "";
    [Export] public Texture2D Icon { get; protected set; }
    [Export] public ItemType Type { get; protected set; } = ItemType.Misc;
    [Export] public ItemRarity Rarity { get; protected set; } = ItemRarity.Common;
    [Export] public int Value { get; protected set; } = 1;
    [Export] public bool IsStackable { get; protected set; } = false;
    [Export] public int MaxStackSize { get; protected set; } = 1;
    [Export] public int StackCount { get; set; } = 1;
    [Export] public AudioStream PickupSound { get; set; }
    [Export] public AudioStream UseSound { get; set; }
    #endregion
    
    #region Protected Fields
    protected Sprite2D _sprite;
    protected Area2D _pickupArea;
    protected CollisionShape2D _pickupCollision;
    protected AudioStreamPlayer2D _audioPlayer;
    protected AnimationPlayer _animationPlayer;
    protected bool _isPickedUp = false;
    #endregion
    
    #region Events
    /// <summary>
    /// 道具被拾取时触发
    /// </summary>
    [Signal] public delegate void ItemPickedUpEventHandler(BaseItem item, Node2D picker);
    
    /// <summary>
    /// 道具被使用时触发
    /// </summary>
    [Signal] public delegate void ItemUsedEventHandler(BaseItem item, Node2D user);
    #endregion
    
    #region Godot Lifecycle
    public override void _Ready()
    {
        InitializeItem();
        SetupComponents();
        ConnectSignals();
        SetupVisuals();
    }
    
    /// <summary>
    /// 初始化道具
    /// </summary>
    protected virtual void InitializeItem()
    {
        // 确保堆叠数量在有效范围内
        if (IsStackable)
        {
            StackCount = Mathf.Clamp(StackCount, 1, MaxStackSize);
        }
        else
        {
            StackCount = 1;
            MaxStackSize = 1;
        }
        
        // 添加到道具组
        AddToGroup("items");
    }
    
    /// <summary>
    /// 设置组件
    /// </summary>
    protected virtual void SetupComponents()
    {
        _sprite = GetNodeOrNull<Sprite2D>("Sprite2D");
        _pickupArea = GetNodeOrNull<Area2D>("PickupArea");
        _pickupCollision = GetNodeOrNull<CollisionShape2D>("PickupArea/CollisionShape2D");
        _audioPlayer = GetNodeOrNull<AudioStreamPlayer2D>("AudioStreamPlayer2D");
        _animationPlayer = GetNodeOrNull<AnimationPlayer>("AnimationPlayer");
        
        // 设置拾取区域
        if (_pickupArea != null)
        {
            _pickupArea.CollisionLayer = 0;
            _pickupArea.CollisionMask = 1; // 只检测玩家层
        }
    }
    
    /// <summary>
    /// 连接信号
    /// </summary>
    protected virtual void ConnectSignals()
    {
        if (_pickupArea != null)
        {
            _pickupArea.BodyEntered += OnPickupAreaBodyEntered;
        }
    }
    
    /// <summary>
    /// 设置视觉效果
    /// </summary>
    protected virtual void SetupVisuals()
    {
        if (_sprite != null && Icon != null)
        {
            _sprite.Texture = Icon;
        }
        
        // 根据稀有度设置颜色
        SetRarityColor();
        
        // 播放出现动画
        PlaySpawnAnimation();
    }
    #endregion
    
    #region IItem Implementation
    /// <summary>
    /// 使用道具
    /// </summary>
    /// <param name="user">使用者</param>
    /// <returns>是否成功使用</returns>
    public virtual bool Use(Node2D user)
    {
        if (user == null) return false;
        
        // 播放使用音效
        PlaySound(UseSound);
        
        // 触发使用事件
        EmitSignal(SignalName.ItemUsed, this, user);
        
        return true;
    }
    
    /// <summary>
    /// 获取道具信息
    /// </summary>
    /// <returns>道具信息字符串</returns>
    public virtual string GetItemInfo()
    {
        var info = $"[color={GetRarityColorHex()}]{ItemName}[/color]\n";
        info += $"{Description}\n";
        info += $"类型: {GetTypeDisplayName()}\n";
        info += $"稀有度: {GetRarityDisplayName()}\n";
        info += $"价值: {Value} 金币";
        
        if (IsStackable && StackCount > 1)
        {
            info += $"\n数量: {StackCount}/{MaxStackSize}";
        }
        
        return info;
    }
    
    /// <summary>
    /// 克隆道具
    /// </summary>
    /// <returns>道具副本</returns>
    public virtual IItem Clone()
    {
        var scene = GD.Load<PackedScene>(SceneFilePath);
        if (scene == null) return null;
        
        var clone = scene.Instantiate() as BaseItem;
        if (clone != null)
        {
            clone.StackCount = StackCount;
        }
        
        return clone;
    }
    #endregion
    
    #region Pickup System
    /// <summary>
    /// 拾取道具
    /// </summary>
    /// <param name="picker">拾取者</param>
    /// <returns>是否成功拾取</returns>
    public virtual bool Pickup(Node2D picker)
    {
        if (_isPickedUp || picker == null) return false;
        
        _isPickedUp = true;
        
        // 播放拾取音效
        PlaySound(PickupSound);
        
        // 播放拾取动画
        PlayPickupAnimation();
        
        // 触发拾取事件
        EmitSignal(SignalName.ItemPickedUp, this, picker);
        
        return true;
    }
    
    /// <summary>
    /// 拾取区域进入事件
    /// </summary>
    /// <param name="body">进入的物体</param>
    protected virtual void OnPickupAreaBodyEntered(Node2D body)
    {
        if (body.IsInGroup("player"))
        {
            Pickup(body);
        }
    }
    #endregion
    
    #region Stack Management
    /// <summary>
    /// 添加到堆叠
    /// </summary>
    /// <param name="amount">添加数量</param>
    /// <returns>实际添加的数量</returns>
    public virtual int AddToStack(int amount)
    {
        if (!IsStackable) return 0;
        
        var canAdd = MaxStackSize - StackCount;
        var actualAdd = Mathf.Min(amount, canAdd);
        StackCount += actualAdd;
        
        return actualAdd;
    }
    
    /// <summary>
    /// 从堆叠中移除
    /// </summary>
    /// <param name="amount">移除数量</param>
    /// <returns>实际移除的数量</returns>
    public virtual int RemoveFromStack(int amount)
    {
        var actualRemove = Mathf.Min(amount, StackCount);
        StackCount -= actualRemove;
        
        return actualRemove;
    }
    
    /// <summary>
    /// 是否可以与其他道具合并
    /// </summary>
    /// <param name="other">其他道具</param>
    /// <returns>是否可以合并</returns>
    public virtual bool CanStackWith(IItem other)
    {
        return IsStackable && 
               other.IsStackable && 
               ItemId == other.ItemId && 
               StackCount < MaxStackSize;
    }
    #endregion
    
    #region Visual Effects
    /// <summary>
    /// 设置稀有度颜色
    /// </summary>
    protected virtual void SetRarityColor()
    {
        if (_sprite == null) return;
        
        var color = GetRarityColor();
        _sprite.Modulate = color;
    }
    
    /// <summary>
    /// 获取稀有度颜色
    /// </summary>
    /// <returns>稀有度对应的颜色</returns>
    protected virtual Color GetRarityColor()
    {
        return Rarity switch
        {
            ItemRarity.Common => Colors.White,
            ItemRarity.Uncommon => Colors.LightGreen,
            ItemRarity.Rare => Colors.LightBlue,
            ItemRarity.Epic => Colors.Purple,
            ItemRarity.Legendary => Colors.Orange,
            _ => Colors.White
        };
    }
    
    /// <summary>
    /// 获取稀有度颜色十六进制字符串
    /// </summary>
    /// <returns>颜色十六进制字符串</returns>
    protected virtual string GetRarityColorHex()
    {
        return Rarity switch
        {
            ItemRarity.Common => "#FFFFFF",
            ItemRarity.Uncommon => "#90EE90",
            ItemRarity.Rare => "#ADD8E6",
            ItemRarity.Epic => "#800080",
            ItemRarity.Legendary => "#FFA500",
            _ => "#FFFFFF"
        };
    }
    
    /// <summary>
    /// 播放出现动画
    /// </summary>
    protected virtual void PlaySpawnAnimation()
    {
        if (_animationPlayer != null && _animationPlayer.HasAnimation("spawn"))
        {
            _animationPlayer.Play("spawn");
        }
        else
        {
            // 默认出现效果
            var tween = CreateTween();
            Scale = Vector2.Zero;
            tween.TweenProperty(this, "scale", Vector2.One, 0.3f)
                 .SetEase(Tween.EaseType.Out);
        }
    }
    
    /// <summary>
    /// 播放拾取动画
    /// </summary>
    protected virtual void PlayPickupAnimation()
    {
        if (_animationPlayer != null && _animationPlayer.HasAnimation("pickup"))
        {
            _animationPlayer.Play("pickup");
            _animationPlayer.AnimationFinished += OnPickupAnimationFinished;
        }
        else
        {
            // 默认拾取效果
            var tween = CreateTween();
            tween.Parallel().TweenProperty(this, "scale", Vector2.Zero, 0.2f);
            tween.Parallel().TweenProperty(this, "modulate:a", 0f, 0.2f);
            tween.TweenCallback(Callable.From(QueueFree));
        }
    }
    
    /// <summary>
    /// 拾取动画完成事件
    /// </summary>
    /// <param name="animName">动画名称</param>
    protected virtual void OnPickupAnimationFinished(StringName animName)
    {
        if (animName == "pickup")
        {
            QueueFree();
        }
    }
    #endregion
    
    #region Audio
    /// <summary>
    /// 播放音效
    /// </summary>
    /// <param name="sound">音效</param>
    protected virtual void PlaySound(AudioStream sound)
    {
        if (_audioPlayer != null && sound != null)
        {
            _audioPlayer.Stream = sound;
            _audioPlayer.Play();
        }
    }
    #endregion
    
    #region Helper Methods
    /// <summary>
    /// 获取类型显示名称
    /// </summary>
    /// <returns>类型显示名称</returns>
    protected virtual string GetTypeDisplayName()
    {
        return Type switch
        {
            ItemType.Weapon => "武器",
            ItemType.Armor => "护甲",
            ItemType.Accessory => "饰品",
            ItemType.Consumable => "消耗品",
            ItemType.Material => "材料",
            ItemType.Quest => "任务物品",
            ItemType.Misc => "杂项",
            _ => "未知"
        };
    }
    
    /// <summary>
    /// 获取稀有度显示名称
    /// </summary>
    /// <returns>稀有度显示名称</returns>
    protected virtual string GetRarityDisplayName()
    {
        return Rarity switch
        {
            ItemRarity.Common => "普通",
            ItemRarity.Uncommon => "不常见",
            ItemRarity.Rare => "稀有",
            ItemRarity.Epic => "史诗",
            ItemRarity.Legendary => "传说",
            _ => "未知"
        };
    }
    #endregion
}