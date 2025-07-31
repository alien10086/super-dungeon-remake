using Godot;
using SuperDungeonRemake.Utils;
using SuperDungeonRemake.Core.Abstract;
using SuperDungeonRemake.Core.Interfaces;

namespace SuperDungeonRemake.Gameplay.Player;

/// <summary>
/// 玩家控制器类
/// 继承自CombatEntity，实现玩家特有的功能
/// </summary>
public partial class PlayerController : CombatEntity
{
    [Export] public float RecoilDuration { get; set; } = 0.2f;
    [Export] public float CameraZoomFactor { get; set; } = 1.0f;
    
    [Signal]
    public delegate void HealthChangedEventHandler(int currentHealth, int maxHealth);
    [Signal]
    public delegate void GoldChangedEventHandler(int newGold);
    
    private Vector2 _inputDirection;
    private float _recoilTime;
    private Vector2 _recoilDirection;
    private Vector2 _attackDirection;
    
    private Camera2D _camera;
    private PointLight2D _light;
    private Area2D _hitbox;
    private AudioStreamPlayer2D _footstepSfx;
    private AudioStreamPlayer2D _painSfx;
    private AudioStreamPlayer2D _swipeSfx;
    
    private int _currentGold;
    
    #region Godot Lifecycle
    protected override void InitializeEntity()
    {
        base.InitializeEntity();
        
        AddToGroup(GlobalConstants.GroupNames.PLAYER);
        _currentGold = 0;
    }
    
    protected override void SetupComponents()
    {
        base.SetupComponents();
        
        _camera = GetNodeOrNull<Camera2D>("Camera2D");
        _light = GetNodeOrNull<PointLight2D>("PointLight2D");
        _hitbox = GetNodeOrNull<Area2D>("Hitbox");
        _footstepSfx = GetNodeOrNull<AudioStreamPlayer2D>("SFXFootstep");
        _painSfx = GetNodeOrNull<AudioStreamPlayer2D>("SFXPain");
        _swipeSfx = GetNodeOrNull<AudioStreamPlayer2D>("SFXSwipe");
        
        SetupCamera();
        SetupLight();
    }
    
    /// <summary>
    /// 设置摄像机
    /// </summary>
    private void SetupCamera()
    {
        if (_camera != null)
        {
            _camera.Zoom = Vector2.One * CameraZoomFactor;
            _camera.Enabled = true;
        }
    }
    
    /// <summary>
    /// 设置灯光
    /// </summary>
    private void SetupLight()
    {
        if (_light != null)
        {
            _light.Visible = true;
            _light.Energy = 1.5f;
            _light.TextureScale = 4.0f; // 设置纹理缩放来控制照射范围
        }
    }
    
    protected override void ConnectSignals()
    {
        base.ConnectSignals();
        
        if (_hitbox != null)
        {
            _hitbox.BodyEntered += OnHitboxBodyEntered;
        }
        
        if (_sprite != null)
        {
            _sprite.FrameChanged += OnSpriteFrameChanged;
        }
        
        // 连接生命值变化事件
        HealthChanged += (current, max) => EmitSignal(SignalName.HealthChanged, current, max);
    }
    
    public override void _Input(InputEvent @event)
    {
        HandleInput(@event);
    }
    #endregion
    
    protected override void UpdateEntity(double delta)
    {
        base.UpdateEntity(delta);
        
        UpdateRecoil(delta);
        UpdateLight(delta);
        HandlePlayerInput();
        HandleAnimation();
    }
    
    protected override void HandleMovement(double delta)
    {
        // 获取输入方向
        _inputDirection = Input.GetVector("move_left", "move_right", "move_up", "move_down");
        
        // 如果在后坐力状态中，使用后坐力方向
        if (_recoilTime > 0)
        {
            Direction = _recoilDirection;
        }
        else
        {
            Direction = _inputDirection;
            
            // 更新攻击方向
            if (_inputDirection != Vector2.Zero)
            {
                _lastAttackDirection = _inputDirection;
            }
        }
        
        base.HandleMovement(delta);
    }
    
    #region Player Input Handling
    /// <summary>
    /// 处理输入事件
    /// </summary>
    /// <param name="event">输入事件</param>
    private void HandleInput(InputEvent @event)
    {
        // 处理特殊输入（如暂停、截图等）
        if (@event.IsActionPressed("pause"))
        {
            // 暂停游戏逻辑
        }
        
        if (@event.IsActionPressed("screenshot"))
        {
            // 截图逻辑
        }
    }
    
    /// <summary>
    /// 处理玩家输入
    /// </summary>
    private void HandlePlayerInput()
    {
        // 处理攻击输入
        if (Input.IsActionPressed("attack") && CanAttack)
        {
            AttackInDirection(_lastAttackDirection);
        }
    }
    #endregion
    
    #region Animation and Visual Effects
    /// <summary>
    /// 处理动画逻辑
    /// </summary>
    private void HandleAnimation()
    {
        if (_sprite == null) return;
        
        // 根据移动方向翻转精灵
        if (_lastAttackDirection.X != 0)
        {
            _sprite.FlipH = _lastAttackDirection.X < 0;
        }
        
        // 根据状态播放动画
        if (_recoilTime > 0)
        {
            if (_sprite.Animation != "hit")
                _sprite.Play("hit");
        }
        else if (Velocity.Length() > 0)
        {
            if (_sprite.Animation != "walk")
            {
                _sprite.Play("walk");
                // 开始播放脚步音效
                PlayFootstepSound();
            }
        }
        else
        {
            if (_sprite.Animation != "idle")
            {
                _sprite.Play("idle");
                // 停止脚步音效
                StopFootstepSound();
            }
        }
    }
    
    private void UpdateLight(double delta)
    {
        if (_light == null) return;
        
        // Light flicker effect
        var time = Time.GetUnixTimeFromSystem();
        _light.Energy = 2.4f + (Mathf.Cos((float)time * 2) * 0.2f);
    }
    
    /// <summary>
    /// 播放脚步音效
    /// </summary>
    private void PlayFootstepSound()
    {
        if (_footstepSfx != null && !_footstepSfx.Playing)
        {
            _footstepSfx.Play();
        }
    }
    
    /// <summary>
    /// 停止脚步音效
    /// </summary>
    private void StopFootstepSound()
    {
        if (_footstepSfx != null && _footstepSfx.Playing)
        {
            _footstepSfx.Stop();
        }
    }
    
    /// <summary>
    /// 播放攻击挥舞音效
    /// </summary>
    private void PlaySwipeSound()
    {
        if (_swipeSfx != null)
        {
            _swipeSfx.Play();
        }
    }
    
    /// <summary>
    /// 重写攻击方法，添加动画和武器自动销毁
    /// </summary>
    public override void AttackInDirection(Vector2 direction)
    {
        if (!CanAttack) return;
        
        // 播放攻击音效
        PlaySwipeSound();
        
        // 调用基类方法
        base.AttackInDirection(direction);
        
        // 播放攻击动画（使用hit动画代替attack）
        if (_sprite != null && _sprite.Animation != "hit")
        {
            _sprite.Play("hit");
        }
        
        // 武器现在会在自己的_Ready方法中自动播放动画
    }
    
    /// <summary>
    /// 重写武器变换设置，使用新的Weapon类的SetRotation方法
    /// </summary>
    protected override void SetupWeaponTransform(Node2D weapon, Vector2 direction)
    {
        // 使用玩家的输入方向而不是攻击方向
        var attackDir = _inputDirection != Vector2.Zero ? _inputDirection.Normalized() : _lastAttackDirection.Normalized();
        
        // 设置武器位置
        weapon.Position = new Vector2(8, 16);
        
        // 设置武器显示在玩家后面
        weapon.ZIndex = -1;
        
        // 如果是Weapon类型，设置旋转角度
        if (weapon is Weapon weaponScript)
        {
            float rotationDegrees = 0;
            
            if (attackDir.X > 0)
            {
                // 向右：旋转90度
                rotationDegrees = 90;
                weapon.Position = new Vector2(20, 24);
            }
            else if (attackDir.X < 0)
            {
                // 向左：旋转-90度
                rotationDegrees = -90;
                weapon.Position = new Vector2(-20, 24);
            }
            else if (attackDir.Y > 0)
            {
                // 向下：旋转180度
                rotationDegrees = 180;
                weapon.Position = new Vector2(20, 24);
            }
            else if (attackDir.Y < 0)
            {
                // 向上：旋转0度
                rotationDegrees = 0;
                weapon.Position = new Vector2(20, 20);
            }
            
            // 设置武器旋转角度
            weaponScript.SetRotation(rotationDegrees);
        }
    }
    
    public void TakeDamage(Vector2 collisionDirection, float damage, float factor = 1f)
    {
        if (_recoilTime > 0) return;
        
        // Play pain sound
        if (_painSfx != null)
        {
            _painSfx.VolumeDb = _rng.RandfRange(-10f, 1f);
            _painSfx.PitchScale = _rng.RandfRange(0.7f, 1.3f);
            _painSfx.Play();
        }
        
        var actualDamage = Mathf.FloorToInt((damage + _rng.RandiRange(0, 5)) * factor);
        CurrentHealth -= actualDamage;
        
        if (CurrentHealth <= 0)
        {
            CurrentHealth = 0;
            GetTree().ChangeSceneToFile("res://Scenes/UI/GameOver.tscn");
        }
        
        _recoilDirection = collisionDirection;
        _recoilTime = 0.2f;
        
        EmitSignal(SignalName.HealthChanged, CurrentHealth);
    }
    
    public void Heal(int amount)
    {
        CurrentHealth = Mathf.Min(CurrentHealth + amount, MaxHealth);
        EmitSignal(SignalName.HealthChanged, CurrentHealth);
    }
    
    public void AddGold(int amount)
    {
        GameData.Instance?.AddGold(amount);
    }
    
    private void OnHitboxBodyEntered(Node2D body)
    {
        var collisionDirection = (Position - body.Position).Normalized();
        
        if (body.IsInGroup(GlobalConstants.GroupNames.ENEMY))
        {
            // Set monster hit time if it has this property
            if (body.HasMethod("SetHitPlayerTime"))
            {
                body.Call("SetHitPlayerTime", 0.0);
            }
            
            // Bounce monster away
            if (body.HasMethod("SetDirection"))
            {
                var direction = (body.Position - Position).Normalized();
                body.Call("SetDirection", direction);
            }
            
            // Take damage
            var damage = body.Get("damage").AsSingle();
            var factor = body.Get("factor").AsSingle();
            TakeDamage(collisionDirection, damage, factor);
        }
        
        if (body.IsInGroup(GlobalConstants.GroupNames.PROJECTILE))
        {
            var damage = body.Get("damage").AsSingle();
            var factor = body.Get("factor").AsSingle();
            TakeDamage(collisionDirection, damage, factor);
            body.QueueFree();
        }
    }
    
    private void OnSpriteFrameChanged()
    {
        // Play footstep sound on certain frames during walk animation
        if (_sprite.Animation == "walk" && (_sprite.Frame == 1 || _sprite.Frame == 3))
        {
            if (_footstepSfx != null)
            {
                _footstepSfx.VolumeDb = _rng.RandfRange(-20f, -10f);
                _footstepSfx.PitchScale = _rng.RandfRange(0.7f, 1.3f);
                _footstepSfx.Play();
            }
        }
    }
    
    /// <summary>
    /// 更新后坐力效果
    /// </summary>
    /// <param name="delta">帧时间</param>
    private void UpdateRecoil(double delta)
    {
        if (_recoilTime > 0)
        {
            _recoilTime -= (float)delta;
            if (_recoilTime <= 0)
            {
                _recoilDirection = Vector2.Zero;
            }
        }
    }
    #endregion
}