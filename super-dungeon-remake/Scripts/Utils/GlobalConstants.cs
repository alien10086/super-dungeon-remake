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
    public const string GroupMonsters = "monsters";
    public const string GroupWeapons = "weapons";
    public const string GroupProjectiles = "projectiles";
    public const string GroupDecos = "decos";
}