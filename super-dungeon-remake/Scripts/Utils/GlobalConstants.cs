using Godot;

namespace SuperDungeonRemake.Utils;

public static class GlobalConstants
{
    public const int GridSize = 16;
    public const int MapSize = 48;
    public const int MaxDepth = 4;
    public const float SplitPercentage = 0.3f;
    
    // Tile indices
    public const int TileIdxUnset = -1;
    public const int TileIdxWall = 0;
    public const int TileIdxFloor = 1;
    
    // Layer names
    public const string LayerWorld = "world";
    public const string LayerPlayer = "player";
    public const string LayerEnemies = "enemies";
    
    // Group names
    public static class GroupNames
    {
        public const string Monsters = "monsters";
        public const string Weapons = "weapons";
        public const string Projectiles = "projectiles";
        public const string Decos = "decos";
        public const string ENEMIES = "enemies";
        public const string PLAYER = "player";
        public const string ENEMY = "enemy";
        public const string PROJECTILE = "projectile";
    }
    
    // 保持向后兼容性的旧常量
    public const string GroupMonsters = "monsters";
    public const string GroupWeapons = "weapons";
    public const string GroupProjectiles = "projectiles";
    public const string GroupDecos = "decos";
}