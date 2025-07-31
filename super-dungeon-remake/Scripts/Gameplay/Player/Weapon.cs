using Godot;
using System;

public partial class Weapon : CharacterBody2D
{
    private float _rot = 0;
    private const float START_ANGLE = -45;
    private const float END_ANGLE = 45;
    
    private AnimationPlayer _animationPlayer;
    
    public override void _Ready()
    {
        _animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
        
        // 播放攻击动画，速度为5倍
        _animationPlayer.Play("attack", -1, 5.0f);
        
        // 根据旋转角度动态修改动画轨道的关键帧
        var animation = _animationPlayer.GetAnimation("attack");
        if (animation != null)
        {
            if (_rot > 0)
            {
                // 正向旋转
                animation.TrackSetKeyValue(0, 0, Mathf.DegToRad(START_ANGLE + _rot));
                animation.TrackSetKeyValue(0, 1, Mathf.DegToRad(END_ANGLE + _rot));
            }
            else
            {
                // 反向旋转
                animation.TrackSetKeyValue(0, 1, Mathf.DegToRad(START_ANGLE + _rot));
                animation.TrackSetKeyValue(0, 0, Mathf.DegToRad(END_ANGLE + _rot));
            }
        }
        
        // 连接动画完成信号
        _animationPlayer.AnimationFinished += OnAnimationPlayerAnimationFinished;
    }
    
    /// <summary>
    /// 设置武器的旋转角度（度数）
    /// </summary>
    /// <param name="rotationDegrees">旋转角度</param>
    public void SetRotation(float rotationDegrees)
    {
        _rot = rotationDegrees;
    }
    
    private void OnAnimationPlayerAnimationFinished(StringName animName)
    {
        // 动画完成后销毁武器
        QueueFree();
    }
}
