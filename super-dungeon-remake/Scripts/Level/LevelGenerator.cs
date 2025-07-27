using Godot;
using System.Collections.Generic;
using SuperDungeonRemake.Utils;

namespace SuperDungeonRemake.Level;

public partial class LevelGenerator : Node
{
    public List<Room> AllRooms { get; private set; } = new();
    
    private RandomNumberGenerator _rng;
    
    public override void _Ready()
    {
        _rng = new RandomNumberGenerator();
        _rng.Randomize();
    }
    
    public void GenerateLevel(TileMap tileMap)
    {
        AllRooms.Clear();
        
        // Create root BSP node
        var rootNode = new BSPNode(0, 0, GlobalConstants.MapSize, GlobalConstants.MapSize, 0);
        
        // Generate rooms using BSP
        GenerateRooms(rootNode);
        
        // Generate corridors
        GenerateCorridors(rootNode, tileMap);
        
        // Render the map
        RenderMap(tileMap);
        
        // Add decorations
        AddDecorations(tileMap);
    }
    
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
}