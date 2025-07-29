using Godot;
using SuperDungeonRemake.Gameplay.Enemies;
using SuperDungeonRemake.Utils;

namespace SuperDungeonRemake.Gameplay.Enemies
{
    /// <summary>
    /// 具体的敌人实现类
    /// 继承自BaseEnemy，可以进一步自定义特定敌人的行为
    /// </summary>
    public partial class Enemy : BaseEnemy
    {
        #region Godot Lifecycle
        public override void _Ready()
        {
            // 调用基类的初始化
            base._Ready();
            
            // 可以在这里添加特定敌人的初始化逻辑
            SetupSpecificBehavior();
        }
        #endregion
        
        #region Specific Behavior
        /// <summary>
        /// 设置特定敌人的行为
        /// </summary>
        private void SetupSpecificBehavior()
        {
            // 根据敌人类型设置特殊行为
            switch (Type)
            {
                case EnemyType.Goblin:
                    SetupGoblinBehavior();
                    break;
                case EnemyType.Skeleton:
                    SetupSkeletonBehavior();
                    break;
                case EnemyType.Slime:
                    SetupSlimeBehavior();
                    break;
                case EnemyType.Orc:
                    SetupOrcBehavior();
                    break;
            }
        }
        
        private void SetupGoblinBehavior()
        {
            // 哥布林更加敏捷，巡逻范围更小
            PatrolRadius = 40f;
            StateChangeInterval = 1.5f;
        }
        
        private void SetupSkeletonBehavior()
        {
            // 骷髅更有纪律，巡逻时间更长
            PatrolRadius = 60f;
            StateChangeInterval = 3f;
        }
        
        private void SetupSlimeBehavior()
        {
            // 史莱姆移动更随机
            PatrolRadius = 30f;
            StateChangeInterval = 1f;
        }
        
        private void SetupOrcBehavior()
        {
            // 兽人更加谨慎，巡逻范围大
            PatrolRadius = 80f;
            StateChangeInterval = 4f;
            FleeHealthThreshold = 0.2f; // 更低的逃跑阈值
        }
        #endregion
    }
}