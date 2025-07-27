using Godot;

public static class GlobalConstants
{
    // Game constants
    public const int TILE_SIZE = 16;
    public const int ROOM_MIN_SIZE = 6;
    public const int ROOM_MAX_SIZE = 12;
    public const int MAX_ROOMS = 30;
    public const int MapSize = 64;
    public const float SplitPercentage = 0.5f;
    public const int MaxDepth = 8;
    public const int GridSize = 16;
    
    // Layer constants
    public const int FLOOR_LAYER = 0;
    public const int WALL_LAYER = 1;
    public const int DETAIL_LAYER = 2;
    
    // Tile IDs
    public const int FLOOR_TILE_ID = 0;
    public const int WALL_TILE_ID = 1;
    public const int DOOR_TILE_ID = 2;
    public const int TileIdxFloor = 0;
    public const int TileIdxWall = 1;
    public const int TileIdxDoor = 2;
    
    // Group names for collision detection
    public static class GroupNames
    {
        public const string PLAYER = "player";
        public const string ENEMY = "enemy";
        public const string ENEMIES = "enemies";
        public const string WALL = "wall";
        public const string ITEM = "item";
        public const string EXIT = "exit";
        public const string PROJECTILE = "projectile";
    }
    
    // Legacy group names for compatibility
    public const string PLAYER = "player";
    
    // Legacy group names for compatibility
    public const string GroupMonsters = "enemies";
    
    // Animation names
    public static class Animations
    {
        public const string IDLE = "idle";
        public const string WALK = "walk";
        public const string ATTACK = "attack";
        public const string DEATH = "death";
        public const string HURT = "hurt";
    }
    
    // Audio names
    public static class Audio
    {
        public const string PLAYER_ATTACK = "player_attack";
        public const string PLAYER_HURT = "player_hurt";
        public const string ENEMY_HURT = "enemy_hurt";
        public const string ENEMY_DEATH = "enemy_death";
        public const string PICKUP = "pickup";
        public const string DOOR_OPEN = "door_open";
    }
}