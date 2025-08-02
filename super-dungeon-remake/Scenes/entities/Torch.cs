using Godot;
using System;

public partial class Torch : Node2D
{
    public override void _Ready()
    {
        // Stop all torches moving in sync
        var animatedSprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
        if (animatedSprite != null)
        {
            animatedSprite.Frame = GD.RandRange(0, 4);
            animatedSprite.SpeedScale = (float)GD.RandRange(0.8, 3.0);
        }
    }
}
