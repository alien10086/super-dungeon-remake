using Godot;
using System;
using System.Collections.Generic;
using SuperDungeonRemake.Utils;
using SuperDungeonRemake.Level;

namespace SuperDungeonRemake.Level;

/// <summary>
/// 游戏主地图类
/// 负责地图生成、房间创建、装饰添加等功能
/// 从 GDScript 版本转换而来
/// </summary>
public partial class Map : TileMap
{
    #region Constants
    /// <summary>
    /// 地图大小，总是正方形
    /// </summary>
    public const int MAP_SIZE = 48;
    
    /// <summary>
    /// 生成时的最大递归深度
    /// </summary>
    public const int MAX_DEPTH = 4;
    
    /// <summary>
    /// 瓦片索引定义
    /// </summary>
    public const int TILE_IDX_UNSET = -1;
    public const int TILE_IDX_WALL = 0;
    public const int TILE_IDX_FLOOR = 1;
    #endregion
    
    #region Scene Resources
    /// <summary>
    /// 预加载的场景资源
    /// </summary>
    [Export] public PackedScene TorchScene { get; set; }
    [Export] public PackedScene ChestScene { get; set; }
    [Export] public PackedScene PotionScene { get; set; }
    #endregion
    
    #region Properties
    /// <summary>
    /// 随机数生成器
    /// </summary>
    private RandomNumberGenerator _rng;
    
    /// <summary>
    /// 所有生成的房间列表
    /// </summary>
    public List<Room> AllRooms { get; private set; } = new();
    #endregion
    
    #region Godot Lifecycle
    public override void _Ready()
    {
        _rng = new RandomNumberGenerator();
        _rng.Randomize();
        AllRooms.Clear();
        
        // 加载默认场景资源（如果没有在编辑器中设置）
        // LoadDefaultScenes();
        
        // 生成整个关卡
        GenerateLevel();
    }
    #endregion
    
    #region Level Generation
    /// <summary>
    /// 生成完整关卡
    /// </summary>
    private void GenerateLevel()
    {
        // 使用 LevelGenerator 来生成基础地图结构
        var levelGenerator = new LevelGenerator();
        levelGenerator.GenerateLevel(this);
        
        // 获取生成的房间列表
        AllRooms = levelGenerator.AllRooms;
        
        // 检查是否有房间生成
        if (AllRooms == null || AllRooms.Count == 0)
        {
            GD.PrintErr("错误：没有生成任何房间，无法继续地图生成！");
            return;
        }
        
        GD.Print($"Map.cs: 获得 {AllRooms.Count} 个房间");
        
        // 为每个房间添加细节、柱子和宝藏
        foreach (var room in AllRooms)
        {
            AddPillars(room);
            
            // 普通宝藏概率
            var treasureChance = 0.6f;
            
            // 小概率的超级宝藏房间！
            var gameData = GameData.Instance;
            var depth = gameData?.Depth ?? 1;
            if (_rng.Randf() * 100 < (0.0f + depth))
            {
                treasureChance = 25.0f;
            }
            
            // 基于房间大小的生成循环
            for (int mi = 0; mi < room.Width * room.Height; mi++)
            {
                // 宝藏 = 药水和宝箱
                if (_rng.Randf() * 100 <= treasureChance)
                {
                    Node2D treasure;
                    // 50/50 概率生成药水或宝箱
                    if (_rng.Randf() * 100 < 50)
                    {
                        treasure = ChestScene?.Instantiate<Node2D>();
                    }
                    else
                    {
                        treasure = PotionScene?.Instantiate<Node2D>();
                    }
                    
                    // 超级宝藏房间，只有宝箱！
                    if (treasureChance >= 25.0f)
                    {
                        treasure = ChestScene?.Instantiate<Node2D>();
                    }
                    
                    if (treasure != null)
                    {
                        var tCell = GetRandomFloorCell(room.Left, room.Top, room.Width, room.Height);
                        treasure.Position = new Vector2(
                            tCell.X * GlobalConstants.GridSize,
                            tCell.Y * GlobalConstants.GridSize
                        );
                        AddChild(treasure);
                    }
                    continue;
                }
                
                // 随机地板装饰，让地图更有趣
                // 这些没有游戏效果
                if (_rng.Randf() * 100 <= 5.0f)
                {
                    var deco = new Sprite2D();
                    var r = _rng.RandiRange(0, 3);
                    switch (r)
                    {
                        case 0:
                             deco.Texture = GD.Load<Texture2D>("res://assets/misc/deco/blood.png");
                             break;
                         case 1:
                             deco.Texture = GD.Load<Texture2D>("res://assets/misc/deco/crack.png");
                             break;
                         case 2:
                             deco.Texture = GD.Load<Texture2D>("res://assets/misc/deco/skull.png");
                             break;
                         case 3:
                             deco.Texture = GD.Load<Texture2D>("res://assets/misc/deco/bones.png");
                             break;
                    }
                    
                    if (_rng.Randf() > 0.5f)
                    {
                        deco.FlipH = true;
                    }
                    
                    var mCell = GetRandomFloorCell(room.Left, room.Top, room.Width, room.Height);
                    deco.Position = new Vector2(
                        mCell.X * GlobalConstants.GridSize + 8,
                        mCell.Y * GlobalConstants.GridSize + 8
                    );
                    deco.AddToGroup("decos");
                    deco.SelfModulate = SelfModulate;
                    AddChild(deco);
                }
            }
        }
        
        // 最后用墙壁瓦片"绘制"边缘
        AddWalls();
    }
    
    /// <summary>
    /// 加载默认场景资源
    /// </summary>
    private void LoadDefaultScenes()
    {
        // TorchScene is not available yet
        // if (TorchScene == null)
        // {
        //     TorchScene = GD.Load<PackedScene>("res://entities/torch.tscn");
        // }
        // ChestScene and PotionScene are not available yet
        // if (ChestScene == null)
        // {
        //     ChestScene = GD.Load<PackedScene>("res://entities/chest.tscn");
        // }
        // if (PotionScene == null)
        // {
        //     PotionScene = GD.Load<PackedScene>("res://entities/potion.tscn");
        // }
    }
    #endregion
    
    #region Helper Methods
    /// <summary>
    /// 用给定的瓦片填充矩形区域
    /// </summary>
    /// <param name="left">左边界</param>
    /// <param name="top">上边界</param>
    /// <param name="width">宽度</param>
    /// <param name="height">高度</param>
    /// <param name="tileIdx">瓦片索引</param>
    /// <param name="tileCoords">瓦片坐标</param>
    public void FillCells(int left, int top, int width, int height, int tileIdx, Vector2I tileCoords)
    {
        for (int y = top; y < top + height; y++)
        {
            for (int x = left; x < left + width; x++)
            {
                SetCell(0, new Vector2I(x, y), tileIdx, tileCoords);
            }
        }
    }
    
    /// <summary>
    /// 用随机选择的地板瓦片填充矩形区域
    /// </summary>
    /// <param name="left">左边界</param>
    /// <param name="top">上边界</param>
    /// <param name="width">宽度</param>
    /// <param name="height">高度</param>
    public void FillCellsFloor(int left, int top, int width, int height)
    {
        for (int y = top; y < top + height; y++)
        {
            for (int x = left; x < left + width; x++)
            {
                var tileCoords = new Vector2I(
                    _rng.RandiRange(0, 3),
                    _rng.RandiRange(0, 2)
                );
                SetCell(0, new Vector2I(x, y), TILE_IDX_FLOOR, tileCoords);
            }
        }
    }
    
    /// <summary>
    /// 房间是大的空矩形，这个方法随机在房间中放置 2x2 的"洞"
    /// </summary>
    /// <param name="room">房间</param>
    private void AddPillars(Room room)
    {
        if (room.Width * room.Height > 15 && room.Width > 4 && room.Height > 4)
        {
            var decoCount = _rng.RandiRange(3, 8);
            for (int p = 0; p < decoCount; p++)
            {
                var pillarLeft = _rng.RandiRange(1, room.Width - 3);
                var pillarTop = _rng.RandiRange(1, room.Height - 3);
                FillCells(
                    room.Left + pillarLeft,
                    room.Top + pillarTop,
                    2, 2,
                    TILE_IDX_UNSET,
                    Vector2I.Zero
                );
            }
        }
    }
    
    /// <summary>
    /// 这个方法设置房间边缘周围的瓦片为正确的瓦片
    /// 基于瓦片的绘制方式和它们在集合中的位置，有很多硬编码逻辑
    /// </summary>
    private void AddWalls()
    {
        for (int y = -2; y < MAP_SIZE + 2; y++)
        {
            for (int x = -2; x < MAP_SIZE + 2; x++)
            {
                if (GetCellSourceId(0, new Vector2I(x, y)) == TILE_IDX_UNSET)
                {
                    // 特殊的 1 厚度墙壁 - 看起来不太好
                    if (GetCellSourceId(0, new Vector2I(x - 1, y)) == TILE_IDX_FLOOR &&
                        GetCellSourceId(0, new Vector2I(x + 1, y)) == TILE_IDX_FLOOR)
                    {
                        FillCellsFloor(x, y, 1, 1);
                        continue;
                    }
                    if (GetCellSourceId(0, new Vector2I(x, y - 1)) == TILE_IDX_FLOOR &&
                        GetCellSourceId(0, new Vector2I(x, y + 1)) == TILE_IDX_FLOOR)
                    {
                        FillCellsFloor(x, y, 1, 1);
                        continue;
                    }
                    
                    // 基本方向
                    if (GetCellSourceId(0, new Vector2I(x, y + 1)) == TILE_IDX_FLOOR)
                    {
                        var tileCoords = new Vector2I(_rng.RandiRange(1, 4), 0);
                        SetCell(0, new Vector2I(x, y), TILE_IDX_WALL, tileCoords);
                        if (_rng.Randf() <= 0.2f)
                        {
                            AddTorch(x, y);
                        }
                        continue;
                    }
                    if (GetCellSourceId(0, new Vector2I(x, y - 1)) == TILE_IDX_FLOOR)
                    {
                        // "北"墙是特殊情况，由于假透视
                        Vector2I tileCoords;
                        if (GetCellSourceId(0, new Vector2I(x - 1, y)) == TILE_IDX_FLOOR)
                        {
                            tileCoords = new Vector2I(0, 5);
                        }
                        else if (GetCellSourceId(0, new Vector2I(x + 1, y)) == TILE_IDX_FLOOR)
                        {
                            tileCoords = new Vector2I(5, 5);
                        }
                        else
                        {
                            tileCoords = new Vector2I(_rng.RandiRange(1, 4), 4);
                        }
                        SetCell(0, new Vector2I(x, y), TILE_IDX_WALL, tileCoords);
                        continue;
                    }
                    if (GetCellSourceId(0, new Vector2I(x + 1, y)) == TILE_IDX_FLOOR)
                    {
                        var tileCoords = new Vector2I(0, _rng.RandiRange(0, 3));
                        SetCell(0, new Vector2I(x, y), TILE_IDX_WALL, tileCoords);
                        continue;
                    }
                    if (GetCellSourceId(0, new Vector2I(x - 1, y)) == TILE_IDX_FLOOR)
                    {
                        var tileCoords = new Vector2I(5, _rng.RandiRange(0, 3));
                        SetCell(0, new Vector2I(x, y), TILE_IDX_WALL, tileCoords);
                        continue;
                    }
                    
                    // 对角线
                    if (GetCellSourceId(0, new Vector2I(x + 1, y - 1)) == TILE_IDX_FLOOR)
                    {
                        SetCell(0, new Vector2I(x, y), TILE_IDX_WALL, new Vector2I(0, 4));
                        continue;
                    }
                    if (GetCellSourceId(0, new Vector2I(x - 1, y - 1)) == TILE_IDX_FLOOR)
                    {
                        SetCell(0, new Vector2I(x, y), TILE_IDX_WALL, new Vector2I(5, 4));
                        continue;
                    }
                    if (GetCellSourceId(0, new Vector2I(x + 1, y + 1)) == TILE_IDX_FLOOR)
                    {
                        SetCell(0, new Vector2I(x, y), TILE_IDX_WALL, new Vector2I(0, 0));
                        continue;
                    }
                    if (GetCellSourceId(0, new Vector2I(x - 1, y + 1)) == TILE_IDX_FLOOR)
                    {
                        SetCell(0, new Vector2I(x, y), TILE_IDX_WALL, new Vector2I(5, 0));
                        continue;
                    }
                }
            }
        }
    }
    
    /// <summary>
    /// 在朝南的墙上放置火把
    /// </summary>
    /// <param name="x">X坐标</param>
    /// <param name="y">Y坐标</param>
    private void AddTorch(int x, int y)
    {
        if (TorchScene != null)
        {
            var torchNode = TorchScene.Instantiate<Node2D>();
            torchNode.Position = new Vector2(
                x * GlobalConstants.GridSize,
                y * GlobalConstants.GridSize
            );
            AddChild(torchNode);
        }
    }
    
    /// <summary>
    /// 在给定矩形内选择一个随机的开放地板瓦片
    /// </summary>
    /// <param name="left">左边界</param>
    /// <param name="top">上边界</param>
    /// <param name="width">宽度</param>
    /// <param name="height">高度</param>
    /// <returns>随机地板单元格坐标</returns>
    public Vector2I GetRandomFloorCell(int left, int top, int width, int height)
    {
        // 构建给定矩形内所有地板瓦片的数组
        var cells = new List<Vector2I>();
        for (int y = top; y < top + height; y++)
        {
            for (int x = left; x < left + width; x++)
            {
                if (GetCellSourceId(0, new Vector2I(x, y)) == TILE_IDX_FLOOR)
                {
                    cells.Add(new Vector2I(x, y));
                }
            }
        }
        
        // 随机选择一个
        if (cells.Count > 0)
        {
            return cells[_rng.RandiRange(0, cells.Count - 1)];
        }
        
        // 如果没有找到地板瓦片，返回矩形中心
        return new Vector2I(left + width / 2, top + height / 2);
    }
    #endregion
}
