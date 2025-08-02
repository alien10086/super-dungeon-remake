using Godot;
using System;
using SuperDungeonRemake.Utils;
using SuperDungeonRemake.Gameplay.Player;

public partial class Potion : Node2D
{
    public override void _Ready()
    {
        // Connect Area2D signals
        var area2D = GetNode<Area2D>("Area2D");
        if (area2D != null)
        {
            area2D.BodyEntered += OnArea2DBodyEntered;
        }
        
        // Connect audio finished signal
        var sfx = GetNode<AudioStreamPlayer2D>("Sfx");
        if (sfx != null)
        {
            sfx.Finished += OnSfxFinished;
        }
    }
    
    private void OnArea2DBodyEntered(Node2D body)
    {
        // Check if the body is the player
        if (body is PlayerController player)
        {
            // Heal player with random amount (10-19)
            var healAmount = 10 + GD.RandRange(0, 9);
            player.Heal(healAmount);
            
            // Remove visual components
            var area2D = GetNode<Area2D>("Area2D");
            area2D?.QueueFree();
            
            var light2D = GetNode<Light2D>("Light2D");
            light2D?.QueueFree();
            
            var particles2D = GetNode<GpuParticles2D>("Particles2D");
            particles2D?.QueueFree();
            
            var sprite = GetNode<Sprite2D>("Sprite");
            sprite?.QueueFree();
            
            // Play sound effect
            var sfx = GetNode<AudioStreamPlayer2D>("Sfx");
            sfx?.Play(0.0f);
        }
    }
    
    private void OnSfxFinished()
    {
        // Remove the potion object when sound finishes
        QueueFree();
    }
}
