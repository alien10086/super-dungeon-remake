using Godot;
using SuperDungeonRemake.Utils;

namespace SuperDungeonRemake.Gameplay.Player;

public partial class PlayerController : CharacterBody2D
{
    [Export] public float Speed { get; set; } = 100f;
    [Export] public int MaxHealth { get; set; } = 100;
    [Export] public float AttackCooldown { get; set; } = 0.5f;
    [Export] public float WeaponDamage { get; set; } = 10f;
    [Export] public PackedScene WeaponScene { get; set; }
    
    [Signal]
    public delegate void HealthChangedEventHandler(int newHealth);
    
    public int CurrentHealth { get; private set; }
    
    private Vector2 _lastDirection = Vector2.Right;
    private Vector2 _attackDirection = Vector2.Right;
    private float _attackTimer = 0f;
    private float _recoilTime = 0f;
    private Vector2 _recoilDirection = Vector2.Zero;
    
    private AnimatedSprite2D _sprite;
    private Camera2D _camera;
    private Light2D _light;
    private Area2D _hitbox;
    private AudioStreamPlayer2D _footstepSfx;
    private AudioStreamPlayer2D _painSfx;
    private AudioStreamPlayer2D _swipeSfx;
    
    private RandomNumberGenerator _rng;
    
    public override void _Ready()
    {
        CurrentHealth = MaxHealth;
        _rng = new RandomNumberGenerator();
        _rng.Randomize();
        
        // Get node references
        _sprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
        _camera = GetNode<Camera2D>("Camera2D");
        _light = GetNode<Light2D>("Light2D");
        _hitbox = GetNode<Area2D>("Hitbox");
        _footstepSfx = GetNode<AudioStreamPlayer2D>("SfxFootstep");
        _painSfx = GetNode<AudioStreamPlayer2D>("SfxPain");
        _swipeSfx = GetNode<AudioStreamPlayer2D>("SfxSwipe");
        
        // Connect signals
        _hitbox.BodyEntered += OnHitboxBodyEntered;
        _sprite.FrameChanged += OnSpriteFrameChanged;
        
        // Set up camera zoom based on screen DPI
        var zoomFactor = DisplayServer.ScreenGetDpi() / 480.0f;
        _camera.Zoom = new Vector2(zoomFactor, zoomFactor);
        
        EmitSignal(SignalName.HealthChanged, CurrentHealth);
    }
    
    public override void _PhysicsProcess(double delta)
    {
        _attackTimer -= (float)delta;
        _recoilTime -= (float)delta;
        
        HandleMovement();
        HandleAttack();
        HandleAnimation();
        UpdateLight(delta);
    }
    
    private void HandleMovement()
    {
        var direction = Input.GetVector("move_left", "move_right", "move_up", "move_down");
        
        if (direction != Vector2.Zero)
        {
            _attackDirection = direction;
        }
        
        // Apply recoil if active
        if (_recoilTime > 0)
        {
            direction = _recoilDirection;
        }
        
        if (direction != Vector2.Zero)
        {
            _lastDirection = direction;
            Velocity = direction * Speed;
        }
        else
        {
            Velocity = Vector2.Zero;
        }
        
        MoveAndSlide();
    }
    
    private void HandleAttack()
    {
        if (Input.IsActionPressed("attack") && _attackTimer <= 0f)
        {
            Attack();
        }
    }
    
    private void HandleAnimation()
    {
        _sprite.FlipH = _lastDirection.X < 0;
        
        if (_recoilTime > 0)
        {
            _sprite.Play("hit");
        }
        else if (Velocity.Length() > 0)
        {
            _sprite.Play("walk");
        }
        else
        {
            _sprite.Play("idle");
        }
    }
    
    private void UpdateLight(double delta)
    {
        // Light flicker effect
        var time = Time.GetUnixTimeFromSystem();
        // Note: TextureScale is not available in Godot 4 Light2D
        _light.Energy = 2.4f + (Mathf.Cos((float)time * 2) * 0.2f);
    }
    
    private void Attack()
    {
        if (WeaponScene == null) return;
        
        var weapon = WeaponScene.Instantiate() as Node2D;
        if (weapon == null) return;
        
        weapon.AddToGroup(GlobalConstants.GroupNames.PROJECTILE);
        
        // Position weapon based on attack direction
        if (_attackDirection.X > 0)
        {
            weapon.Rotation = Mathf.Pi / 2; // 90 degrees
            weapon.Position = new Vector2(8, 16);
        }
        else if (_attackDirection.X < 0)
        {
            weapon.Rotation = -Mathf.Pi / 2; // -90 degrees
            weapon.Position = new Vector2(8, 16);
        }
        else if (_attackDirection.Y > 0)
        {
            weapon.Rotation = Mathf.Pi; // 180 degrees
            weapon.Position = new Vector2(8, 16);
            weapon.ZIndex = 11;
        }
        else if (_attackDirection.Y < 0)
        {
            weapon.Rotation = 0; // 0 degrees
            weapon.Position = new Vector2(8, 16);
        }
        
        AddChild(weapon);
        
        // Play attack sound
        _swipeSfx.PitchScale = _rng.RandfRange(0.9f, 1.8f);
        _swipeSfx.Play();
        
        _attackTimer = AttackCooldown;
    }
    
    public void TakeDamage(Vector2 collisionDirection, float damage, float factor = 1f)
    {
        if (_recoilTime > 0) return;
        
        // Play pain sound
        _painSfx.VolumeDb = _rng.RandfRange(-10f, 1f);
        _painSfx.PitchScale = _rng.RandfRange(0.7f, 1.3f);
        _painSfx.Play();
        
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
            _footstepSfx.VolumeDb = _rng.RandfRange(-20f, -10f);
            _footstepSfx.PitchScale = _rng.RandfRange(0.7f, 1.3f);
            _footstepSfx.Play();
        }
    }
}