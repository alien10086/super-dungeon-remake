using Godot;
using SuperDungeonRemake.Gameplay.Enemies;
using SuperDungeonRemake.Level;
using SuperDungeonRemake.Utils;
using System.Collections.Generic;

namespace SuperDungeonRemake.Core;

/// <summary>
/// 敌人生成器
/// 负责使用 PackedScene 加载和初始化不同类型的敌人
/// </summary>
public partial class EnemySpawner : Node
{
    #region Properties
    /// <summary>
    /// 敌人场景资源
    /// </summary>
    [Export] public PackedScene EnemyScene { get; set; }
    
    /// <summary>
    /// 随机数生成器
    /// </summary>
    private RandomNumberGenerator _rng;
    
    /// <summary>
    /// 关卡生成器引用
    /// </summary>
    private LevelGenerator _levelGenerator;
    #endregion
    
    #region Godot Lifecycle
    public override void _Ready()
    {
        _rng = new RandomNumberGenerator();
        _rng.Randomize();
        
        // 获取关卡生成器引用
        var gameManager = GetNode<GameManager>("/root/Main/GameContainer/GameManage");
        _levelGenerator = gameManager.LevelGenerator;
        
        // 如果没有设置敌人场景，尝试加载默认场景
        if (EnemyScene == null)
        {
            EnemyScene = GD.Load<PackedScene>("res://Scenes/Enemies/Enemy.tscn");
        }
    }
    #endregion
    
    #region Enemy Spawning
    /// <summary>
    /// 在指定位置生成敌人
    /// </summary>
    /// <param name="position">生成位置</param>
    /// <param name="enemyType">敌人类型</param>
    /// <returns>生成的敌人实例</returns>
    public Enemy SpawnEnemy(Vector2 position, EnemyType enemyType)
    {
        if (EnemyScene == null)
        {
            GD.PrintErr("EnemyScene is null, cannot spawn enemy");
            return null;
        }
        
        // 实例化敌人场景
        var enemy = EnemyScene.Instantiate<Enemy>();
        if (enemy == null)
        {
            GD.PrintErr("Failed to instantiate enemy from PackedScene");
            return null;
        }
        
        // 设置敌人类型（在添加到场景树之前设置）
        enemy.Type = enemyType;
        
        // 设置位置
        enemy.Position = position;
        
        // 添加到场景树
        GetTree().CurrentScene.AddChild(enemy);
        
        // 添加到敌人组
        enemy.AddToGroup(GlobalConstants.GroupNames.ENEMIES);
        
        GD.Print($"Spawned {enemyType} enemy at position {position}");
        
        return enemy;
    }
    
    /// <summary>
    /// 在随机房间中生成敌人
    /// </summary>
    /// <param name="enemyType">敌人类型</param>
    /// <returns>生成的敌人实例</returns>
    public Enemy SpawnEnemyInRandomRoom(EnemyType enemyType)
    {
        if (_levelGenerator?.AllRooms == null || _levelGenerator.AllRooms.Count == 0)
        {
            GD.PrintErr("No rooms available for enemy spawning");
            return null;
        }
        
        // 选择随机房间
        var randomRoom = _levelGenerator.AllRooms[_rng.RandiRange(0, _levelGenerator.AllRooms.Count - 1)];
        
        // 在房间中心生成敌人
        var spawnPosition = new Vector2(
            randomRoom.Left * GlobalConstants.GridSize + randomRoom.Width * GlobalConstants.GridSize / 2,
            randomRoom.Top * GlobalConstants.GridSize + randomRoom.Height * GlobalConstants.GridSize / 2
        );
        
        return SpawnEnemy(spawnPosition, enemyType);
    }
    
    /// <summary>
    /// 批量生成敌人
    /// </summary>
    /// <param name="enemyConfigs">敌人配置列表</param>
    /// <returns>生成的敌人列表</returns>
    public List<Enemy> SpawnEnemies(List<EnemySpawnConfig> enemyConfigs)
    {
        var spawnedEnemies = new List<Enemy>();
        
        foreach (var config in enemyConfigs)
        {
            Enemy enemy;
            
            if (config.UseSpecificPosition)
            {
                enemy = SpawnEnemy(config.Position, config.EnemyType);
            }
            else
            {
                enemy = SpawnEnemyInRandomRoom(config.EnemyType);
            }
            
            if (enemy != null)
            {
                spawnedEnemies.Add(enemy);
            }
        }
        
        return spawnedEnemies;
    }
    
    /// <summary>
    /// 根据关卡深度生成随机敌人
    /// </summary>
    /// <param name="levelDepth">关卡深度</param>
    /// <param name="enemyCount">敌人数量</param>
    /// <returns>生成的敌人列表</returns>
    public List<Enemy> SpawnRandomEnemiesForLevel(int levelDepth, int enemyCount = -1)
    {
        // 如果没有指定敌人数量，根据关卡深度计算
        if (enemyCount == -1)
        {
            enemyCount = Mathf.Min(2 + levelDepth, 8); // 最少2个，最多8个
        }
        
        var enemyConfigs = new List<EnemySpawnConfig>();
        
        for (int i = 0; i < enemyCount; i++)
        {
            var enemyType = GetRandomEnemyTypeForLevel(levelDepth);
            enemyConfigs.Add(new EnemySpawnConfig
            {
                EnemyType = enemyType,
                UseSpecificPosition = false
            });
        }
        
        return SpawnEnemies(enemyConfigs);
    }
    
    /// <summary>
    /// 根据关卡深度获取随机敌人类型
    /// </summary>
    /// <param name="levelDepth">关卡深度</param>
    /// <returns>敌人类型</returns>
    private EnemyType GetRandomEnemyTypeForLevel(int levelDepth)
    {
        // 根据关卡深度调整敌人类型概率
        var random = _rng.Randf();
        
        if (levelDepth <= 2)
        {
            // 前期主要是哥布林和史莱姆
            return random < 0.6f ? EnemyType.Goblin : EnemyType.Slime;
        }
        else if (levelDepth <= 5)
        {
            // 中期加入骷髅
            if (random < 0.4f) return EnemyType.Goblin;
            if (random < 0.7f) return EnemyType.Skeleton;
            return EnemyType.Slime;
        }
        else
        {
            // 后期加入兽人
            if (random < 0.3f) return EnemyType.Goblin;
            if (random < 0.5f) return EnemyType.Skeleton;
            if (random < 0.7f) return EnemyType.Slime;
            return EnemyType.Orc;
        }
    }
    
    /// <summary>
    /// 清除所有敌人
    /// </summary>
    public void ClearAllEnemies()
    {
        var enemies = GetTree().GetNodesInGroup(GlobalConstants.GroupNames.ENEMIES);
        foreach (Node enemy in enemies)
        {
            enemy.QueueFree();
        }
        
        GD.Print($"Cleared {enemies.Count} enemies");
    }
    #endregion
}

/// <summary>
/// 敌人生成配置
/// </summary>
public class EnemySpawnConfig
{
    /// <summary>
    /// 敌人类型
    /// </summary>
    public EnemyType EnemyType { get; set; }
    
    /// <summary>
    /// 是否使用指定位置
    /// </summary>
    public bool UseSpecificPosition { get; set; } = false;
    
    /// <summary>
    /// 指定位置（当 UseSpecificPosition 为 true 时使用）
    /// </summary>
    public Vector2 Position { get; set; }
}