using Godot;
using System;
using SuperDungeonRemake.Utils;
using SuperDungeonRemake.Gameplay.Player;

public partial class Chest : Node2D
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
        var sfx = GetNode<AudioStreamPlayer2D>("SFX");
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
            // Add gold to player
            GameData.Instance?.AddGold(100);
            
            // Remove visual components
            GetNode<Area2D>("Area2D")?.QueueFree();
            GetNode<Sprite2D>("Sprite")?.QueueFree();
            GetNode<Node>("Particles2D-Anim")?.QueueFree();
            
            // Play sound effect
            var sfx = GetNode<AudioStreamPlayer2D>("Sfx");
            if (sfx != null)
            {
                sfx.Play();
            }
            
            // Start particle effect
            var particles = GetNode<GpuParticles2D>("Particles2D");
            if (particles != null)
            {
                particles.Emitting = true;
                particles.OneShot = true;
            }
        }
    }
    
    private void OnSfxFinished()
    {
        QueueFree();
    }
}
