using Godot;
using SuperDungeonRemake.Utils;

namespace SuperDungeonRemake.Level;

public class BSPNode
{
    public int Left { get; set; }
    public int Top { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public int Depth { get; set; }
    
    public Vector2 Center { get; private set; }
    public Room Room { get; set; }
    
    public BSPNode LeftChild { get; set; }
    public BSPNode RightChild { get; set; }
    
    private RandomNumberGenerator _rng;
    
    public BSPNode(int left, int top, int width, int height, int depth)
    {
        Left = left;
        Top = top;
        Width = width;
        Height = height;
        Depth = depth;
        Center = new Vector2(left + width / 2.0f, top + height / 2.0f);
        
        _rng = new RandomNumberGenerator();
        _rng.Randomize();
    }
    
    public bool IsLeaf => LeftChild == null && RightChild == null;
    
    public bool Split()
    {
        if (!IsLeaf)
            return false;
            
        // 进一步降低最小分割尺寸要求，在更大地图上生成更多房间
        if (Depth >= GlobalConstants.MaxDepth || Width < 12 || Height < 12)
            return false;
            
        bool splitHorizontally;
        
        if (Width / (float)Height >= 1.25f)
        {
            splitHorizontally = false; // Split vertically
        }
        else if (Height / (float)Width >= 1.25f)
        {
            splitHorizontally = true; // Split horizontally
        }
        else
        {
            splitHorizontally = _rng.Randf() > 0.5f;
        }
        
        if (splitHorizontally)
        {
            var halfHeight = Mathf.CeilToInt(Height / 2.0f);
            var offset = (int)(Height * GlobalConstants.SplitPercentage);
            var splitHeight = _rng.RandiRange(halfHeight - offset, halfHeight + offset);
            
            LeftChild = new BSPNode(Left, Top, Width, splitHeight, Depth + 1);
            RightChild = new BSPNode(Left, Top + splitHeight, Width, Height - splitHeight, Depth + 1);
        }
        else
        {
            var halfWidth = Mathf.CeilToInt(Width / 2.0f);
            var offset = (int)(Width * GlobalConstants.SplitPercentage);
            var splitWidth = _rng.RandiRange(halfWidth - offset, halfWidth + offset);
            
            LeftChild = new BSPNode(Left, Top, splitWidth, Height, Depth + 1);
            RightChild = new BSPNode(Left + splitWidth, Top, Width - splitWidth, Height, Depth + 1);
        }
        
        return true;
    }
    
    public void CreateRoom()
    {
        if (!IsLeaf)
            return;
            
        // 确保有足够的空间创建房间
        if (Width < 12 || Height < 12)
            return;
            
        var minRoomSize = 10; // 进一步增加最小房间尺寸
        var maxRoomWidth = Mathf.Max(minRoomSize, Width - 4); // 留更多边距
        var maxRoomHeight = Mathf.Max(minRoomSize, Height - 4);
        
        // 确保房间不会太小
        if (maxRoomWidth < minRoomSize || maxRoomHeight < minRoomSize)
            return;
            
        var roomWidth = _rng.RandiRange(minRoomSize, maxRoomWidth);
        var roomHeight = _rng.RandiRange(minRoomSize, maxRoomHeight);
        
        // 计算房间位置，确保有边距
        var maxLeft = Mathf.Max(0, Width - roomWidth - 2);
        var maxTop = Mathf.Max(0, Height - roomHeight - 2);
        
        var roomLeft = Left + _rng.RandiRange(2, Mathf.Max(2, maxLeft + 2));
        var roomTop = Top + _rng.RandiRange(2, Mathf.Max(2, maxTop + 2));
        
        Room = new Room(roomLeft, roomTop, roomWidth, roomHeight);
    }
    
    public Room GetRoom()
    {
        if (IsLeaf)
        {
            return Room;
        }
        
        var leftRoom = LeftChild?.GetRoom();
        var rightRoom = RightChild?.GetRoom();
        
        if (leftRoom == null)
            return rightRoom;
        if (rightRoom == null)
            return leftRoom;
            
        return _rng.Randf() > 0.5f ? leftRoom : rightRoom;
    }
}