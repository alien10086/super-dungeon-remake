using Godot;
using System.Collections.Generic;
using System.Linq;
using SuperDungeonRemake.Utils;

namespace SuperDungeonRemake.Level;

/// <summary>
/// 地牢生成器
/// 使用BSP（二叉空间分割）算法生成地牢布局
/// 支持房间生成、走廊连接、装饰添加等功能
/// </summary>
public partial class LevelGenerator : Node
{
    #region Properties
    /// <summary>
    /// 所有生成的房间列表
    /// </summary>
    public List<Room> AllRooms { get; private set; } = new();
    
    /// <summary>
    /// 随机数生成器
    /// </summary>
    private RandomNumberGenerator _rng;

    [Export] public TileMap WorkerTileMap { get; set; }
    
    /// <summary>
    /// 生成统计信息
    /// </summary>
    public GenerationStats Stats { get; private set; } = new();
    #endregion
    
    #region Constants
    private const int MIN_ROOM_SIZE = 4;
    private const int MAX_ROOM_SIZE = 12;
    private const float DECORATION_PROBABILITY = 0.3f;
    private const int PILLAR_MIN_ROOM_AREA = 15;
    #endregion
    
    #region Godot Lifecycle
    public override void _Ready()
    {
        InitializeGenerator();
    }
    
    /// <summary>
    /// 初始化生成器
    /// </summary>
    private void InitializeGenerator()
    {
        _rng = new RandomNumberGenerator();
        _rng.Randomize();
        Stats = new GenerationStats();

        GenerateLevel(WorkerTileMap);
    }
    #endregion
    
    #region Level Generation
    /// <summary>
    /// 生成完整的地牢关卡
    /// </summary>
    /// <param name="tileMap">瓦片地图</param>
    public void GenerateLevel(TileMap tileMap)
    {
        var startTime = Time.GetUnixTimeFromSystem();
        
        // 重置数据
        ResetGenerationData();
        
        // 创建根BSP节点
        var rootNode = new BSPNode(0, 0, GlobalConstants.MapSize, GlobalConstants.MapSize, 0);
        
        // 使用BSP算法生成房间
        GenerateRooms(rootNode);
        
        // 生成走廊连接房间
        GenerateCorridors(rootNode, tileMap);
        
        // 渲染地图
        RenderMap(tileMap);
        
        // 添加装饰元素
        AddDecorations(tileMap);
        
        // 优化地图布局
        OptimizeLayout(tileMap);
        
        // 更新统计信息
        UpdateGenerationStats(startTime);
        
        GD.Print($"地牢生成完成：{AllRooms.Count}个房间，耗时{Stats.GenerationTime:F2}秒");
        
        // 调试信息：输出房间详情
        if (AllRooms.Count == 0)
        {
            GD.PrintErr("警告：没有生成任何房间！");
        }
        else
        {
            GD.Print($"房间列表：");
            for (int i = 0; i < AllRooms.Count; i++)
            {
                var room = AllRooms[i];
                GD.Print($"  房间{i}: ({room.Left}, {room.Top}) 尺寸: {room.Width}x{room.Height}");
            }
        }
    }
    
    /// <summary>
    /// 重置生成数据
    /// </summary>
    private void ResetGenerationData()
    {
        AllRooms.Clear();
        Stats = new GenerationStats();
    }
    
    /// <summary>
    /// 更新生成统计信息
    /// </summary>
    /// <param name="startTime">开始时间</param>
    private void UpdateGenerationStats(double startTime)
    {
        Stats.GenerationTime = Time.GetUnixTimeFromSystem() - startTime;
        Stats.RoomCount = AllRooms.Count;
        Stats.TotalArea = CalculateTotalArea();
        Stats.AverageRoomSize = Stats.TotalArea / (float)Stats.RoomCount;
    }
    
    /// <summary>
    /// 计算总面积
    /// </summary>
    /// <returns>总面积</returns>
    private int CalculateTotalArea()
    {
        int totalArea = 0;
        foreach (var room in AllRooms)
        {
            totalArea += room.Width * room.Height;
        }
        return totalArea;
    }
    #endregion
    
    private void GenerateRooms(BSPNode node)
    {
        if (node.Split())
        {
            GenerateRooms(node.LeftChild);
            GenerateRooms(node.RightChild);
        }
        else
        {
            node.CreateRoom();
            if (node.Room != null)
            {
                AllRooms.Add(node.Room);
            }
        }
    }
    
    private void GenerateCorridors(BSPNode node, TileMap tileMap)
    {
        if (node.IsLeaf)
            return;
            
        var leftRoom = node.LeftChild?.GetRoom();
        var rightRoom = node.RightChild?.GetRoom();
        
        if (leftRoom != null && rightRoom != null)
        {
            CreateCorridor(leftRoom, rightRoom, tileMap);
        }
        
        GenerateCorridors(node.LeftChild, tileMap);
        GenerateCorridors(node.RightChild, tileMap);
    }
    
    private void CreateCorridor(Room roomA, Room roomB, TileMap tileMap)
    {
        var pointA = roomA.Center;
        var pointB = roomB.Center;
        
        // Create L-shaped corridor
        if (_rng.Randf() > 0.5f)
        {
            // Horizontal first, then vertical
            FillCells(tileMap, (int)pointA.X, (int)pointA.Y, (int)(pointB.X - pointA.X), 1, GlobalConstants.TileIdxFloor);
            FillCells(tileMap, (int)pointB.X, (int)pointA.Y, 1, (int)(pointB.Y - pointA.Y), GlobalConstants.TileIdxFloor);
        }
        else
        {
            // Vertical first, then horizontal
            FillCells(tileMap, (int)pointA.X, (int)pointA.Y, 1, (int)(pointB.Y - pointA.Y), GlobalConstants.TileIdxFloor);
            FillCells(tileMap, (int)pointA.X, (int)pointB.Y, (int)(pointB.X - pointA.X), 1, GlobalConstants.TileIdxFloor);
        }
    }
    
    private void RenderMap(TileMap tileMap)
    {
        // Clear the tilemap
        tileMap.Clear();
        
        // Fill rooms with floor tiles
        foreach (var room in AllRooms)
        {
            FillCellsFloor(tileMap, room.Left, room.Top, room.Width, room.Height);
        }
        
        // Add walls around floors
        AddWalls(tileMap);
    }
    
    private void FillCells(TileMap tileMap, int left, int top, int width, int height, int tileIdx)
    {
        for (int y = top; y < top + height; y++)
        {
            for (int x = left; x < left + width; x++)
            {
                tileMap.SetCell(0, new Vector2I(x, y), 0, new Vector2I(tileIdx, 0));
            }
        }
    }
    
    private void FillCellsFloor(TileMap tileMap, int left, int top, int width, int height)
    {
        for (int y = top; y < top + height; y++)
        {
            for (int x = left; x < left + width; x++)
            {
                var atlasCoords = new Vector2I(_rng.RandiRange(0, 3), _rng.RandiRange(0, 2));
                tileMap.SetCell(0, new Vector2I(x, y), 0, atlasCoords);
            }
        }
    }
    
    private void AddWalls(TileMap tileMap)
    {
        for (int y = -2; y < GlobalConstants.MapSize + 2; y++)
        {
            for (int x = -2; x < GlobalConstants.MapSize + 2; x++)
            {
                var currentCell = tileMap.GetCellSourceId(0, new Vector2I(x, y));
                if (currentCell == -1) // Empty cell
                {
                    // Check if adjacent to floor
                    if (IsAdjacentToFloor(tileMap, x, y))
                    {
                        var atlasCoords = GetWallTileCoords(tileMap, x, y);
                        tileMap.SetCell(0, new Vector2I(x, y), 0, atlasCoords);
                    }
                }
            }
        }
    }
    
    private bool IsAdjacentToFloor(TileMap tileMap, int x, int y)
    {
        var directions = new Vector2I[]
        {
            new(0, 1), new(0, -1), new(1, 0), new(-1, 0),
            new(1, 1), new(1, -1), new(-1, 1), new(-1, -1)
        };
        
        foreach (var dir in directions)
        {
            if (tileMap.GetCellSourceId(0, new Vector2I(x + dir.X, y + dir.Y)) != -1)
            {
                return true;
            }
        }
        
        return false;
    }
    
    private Vector2I GetWallTileCoords(TileMap tileMap, int x, int y)
    {
        // Simplified wall tile selection - can be enhanced later
        if (tileMap.GetCellSourceId(0, new Vector2I(x, y + 1)) != -1)
            return new Vector2I(_rng.RandiRange(1, 4), 0); // North wall
        if (tileMap.GetCellSourceId(0, new Vector2I(x, y - 1)) != -1)
            return new Vector2I(_rng.RandiRange(1, 4), 4); // South wall
        if (tileMap.GetCellSourceId(0, new Vector2I(x + 1, y)) != -1)
            return new Vector2I(0, _rng.RandiRange(0, 3)); // West wall
        if (tileMap.GetCellSourceId(0, new Vector2I(x - 1, y)) != -1)
            return new Vector2I(5, _rng.RandiRange(0, 3)); // East wall
            
        return new Vector2I(1, 1); // Default wall
    }
    
    private void AddDecorations(TileMap tileMap)
    {
        // Add pillars to larger rooms
        foreach (var room in AllRooms)
        {
            if (room.Width * room.Height > 15 && room.Width > 4 && room.Height > 4)
            {
                var pillarCount = _rng.RandiRange(3, 8);
                for (int p = 0; p < pillarCount; p++)
                {
                    var pillarLeft = _rng.RandiRange(1, room.Width - 3);
                    var pillarTop = _rng.RandiRange(1, room.Height - 3);
                    
                    // Remove floor tiles to create pillars
                    for (int py = 0; py < 2; py++)
                    {
                        for (int px = 0; px < 2; px++)
                        {
                            tileMap.EraseCell(0, new Vector2I(room.Left + pillarLeft + px, room.Top + pillarTop + py));
                        }
                    }
                }
            }
        }
    }
    
    public Vector2I GetRandomFloorCell(Room room)
    {
        var cells = new List<Vector2I>();
        
        for (int y = room.Top; y < room.Top + room.Height; y++)
        {
            for (int x = room.Left; x < room.Left + room.Width; x++)
            {
                cells.Add(new Vector2I(x, y));
            }
        }
        
        if (cells.Count > 0)
        {
            return cells[_rng.RandiRange(0, cells.Count - 1)];
        }
        
        return new Vector2I(room.Left, room.Top);
    }
    
    /// <summary>
    /// 优化地图布局
    /// </summary>
    /// <param name="tileMap">瓦片地图</param>
    private void OptimizeLayout(TileMap tileMap)
    {
        // 移除孤立的墙壁
        RemoveIsolatedWalls(tileMap);
        
        // 平滑走廊连接
        SmoothCorridors(tileMap);
        
        // 添加门
        AddDoors(tileMap);
    }
    
    /// <summary>
    /// 移除孤立的墙壁
    /// </summary>
    /// <param name="tileMap">瓦片地图</param>
    private void RemoveIsolatedWalls(TileMap tileMap)
    {
        var wallsToRemove = new List<Vector2I>();
        
        for (int y = 0; y < GlobalConstants.MapSize; y++)
        {
            for (int x = 0; x < GlobalConstants.MapSize; x++)
            {
                var pos = new Vector2I(x, y);
                var cellId = tileMap.GetCellSourceId(0, pos);
                
                // 如果是墙壁且周围都是空的，则标记为移除
                if (cellId != -1 && IsIsolatedWall(tileMap, x, y))
                {
                    wallsToRemove.Add(pos);
                }
            }
        }
        
        foreach (var pos in wallsToRemove)
        {
            tileMap.EraseCell(0, pos);
        }
    }
    
    /// <summary>
    /// 检查是否为孤立墙壁
    /// </summary>
    /// <param name="tileMap">瓦片地图</param>
    /// <param name="x">X坐标</param>
    /// <param name="y">Y坐标</param>
    /// <returns>是否为孤立墙壁</returns>
    private bool IsIsolatedWall(TileMap tileMap, int x, int y)
    {
        var directions = new Vector2I[]
        {
            new(0, 1), new(0, -1), new(1, 0), new(-1, 0)
        };
        
        int adjacentWalls = 0;
        foreach (var dir in directions)
        {
            var checkPos = new Vector2I(x + dir.X, y + dir.Y);
            if (tileMap.GetCellSourceId(0, checkPos) != -1)
            {
                adjacentWalls++;
            }
        }
        
        return adjacentWalls <= 1;
    }
    
    /// <summary>
    /// 平滑走廊连接
    /// </summary>
    /// <param name="tileMap">瓦片地图</param>
    private void SmoothCorridors(TileMap tileMap)
    {
        // 找到走廊的拐角并进行平滑处理
        for (int y = 1; y < GlobalConstants.MapSize - 1; y++)
        {
            for (int x = 1; x < GlobalConstants.MapSize - 1; x++)
            {
                var pos = new Vector2I(x, y);
                if (tileMap.GetCellSourceId(0, pos) != -1) // 是地板
                {
                    SmoothCornerAt(tileMap, x, y);
                }
            }
        }
    }
    
    /// <summary>
    /// 在指定位置平滑拐角
    /// </summary>
    /// <param name="tileMap">瓦片地图</param>
    /// <param name="x">X坐标</param>
    /// <param name="y">Y坐标</param>
    private void SmoothCornerAt(TileMap tileMap, int x, int y)
    {
        // 检查是否为L型拐角
        var left = tileMap.GetCellSourceId(0, new Vector2I(x - 1, y)) != -1;
        var right = tileMap.GetCellSourceId(0, new Vector2I(x + 1, y)) != -1;
        var up = tileMap.GetCellSourceId(0, new Vector2I(x, y - 1)) != -1;
        var down = tileMap.GetCellSourceId(0, new Vector2I(x, y + 1)) != -1;
        
        // 如果是拐角，添加对角线地板
        if ((left && up) && !right && !down)
        {
            var atlasCoords = new Vector2I(_rng.RandiRange(0, 3), _rng.RandiRange(0, 2));
            tileMap.SetCell(0, new Vector2I(x - 1, y - 1), 0, atlasCoords);
        }
        else if ((right && up) && !left && !down)
        {
            var atlasCoords = new Vector2I(_rng.RandiRange(0, 3), _rng.RandiRange(0, 2));
            tileMap.SetCell(0, new Vector2I(x + 1, y - 1), 0, atlasCoords);
        }
        else if ((left && down) && !right && !up)
        {
            var atlasCoords = new Vector2I(_rng.RandiRange(0, 3), _rng.RandiRange(0, 2));
            tileMap.SetCell(0, new Vector2I(x - 1, y + 1), 0, atlasCoords);
        }
        else if ((right && down) && !left && !up)
        {
            var atlasCoords = new Vector2I(_rng.RandiRange(0, 3), _rng.RandiRange(0, 2));
            tileMap.SetCell(0, new Vector2I(x + 1, y + 1), 0, atlasCoords);
        }
    }
    
    /// <summary>
    /// 添加门
    /// </summary>
    /// <param name="tileMap">瓦片地图</param>
    private void AddDoors(TileMap tileMap)
    {
        foreach (var room in AllRooms)
        {
            // 为每个房间找到合适的门位置
            var doorPositions = FindDoorPositions(tileMap, room);
            
            foreach (var doorPos in doorPositions)
            {
                // 使用特殊的门瓦片ID
                var doorAtlasCoords = new Vector2I(6, 0); // 假设门在图集的(6,0)位置
                tileMap.SetCell(0, doorPos, 0, doorAtlasCoords);
            }
        }
    }
    
    /// <summary>
    /// 找到房间的门位置
    /// </summary>
    /// <param name="tileMap">瓦片地图</param>
    /// <param name="room">房间</param>
    /// <returns>门位置列表</returns>
    private List<Vector2I> FindDoorPositions(TileMap tileMap, Room room)
    {
        var doorPositions = new List<Vector2I>();
        
        // 检查房间边界，找到连接走廊的位置
        for (int x = room.Left; x < room.Left + room.Width; x++)
        {
            // 检查上边界
            var topPos = new Vector2I(x, room.Top - 1);
            if (tileMap.GetCellSourceId(0, topPos) != -1)
            {
                doorPositions.Add(new Vector2I(x, room.Top));
            }
            
            // 检查下边界
            var bottomPos = new Vector2I(x, room.Top + room.Height);
            if (tileMap.GetCellSourceId(0, bottomPos) != -1)
            {
                doorPositions.Add(new Vector2I(x, room.Top + room.Height - 1));
            }
        }
        
        for (int y = room.Top; y < room.Top + room.Height; y++)
        {
            // 检查左边界
            var leftPos = new Vector2I(room.Left - 1, y);
            if (tileMap.GetCellSourceId(0, leftPos) != -1)
            {
                doorPositions.Add(new Vector2I(room.Left, y));
            }
            
            // 检查右边界
            var rightPos = new Vector2I(room.Left + room.Width, y);
            if (tileMap.GetCellSourceId(0, rightPos) != -1)
            {
                doorPositions.Add(new Vector2I(room.Left + room.Width - 1, y));
            }
        }
        
        // 限制门的数量，避免过多
        if (doorPositions.Count > 3)
        {
            doorPositions = doorPositions.Take(3).ToList();
        }
        
        return doorPositions;
    }
}