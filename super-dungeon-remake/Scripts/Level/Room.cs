using Godot;

namespace SuperDungeonRemake.Level;

public class Room
{
    public int Left { get; set; }
    public int Top { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    
    public int Right => Left + Width;
    public int Bottom => Top + Height;
    
    public Vector2 Center => new Vector2(
        Left + Width / 2.0f,
        Top + Height / 2.0f
    );
    
    public Room(int left, int top, int width, int height)
    {
        Left = left;
        Top = top;
        Width = width;
        Height = height;
    }
    
    public bool Contains(int x, int y)
    {
        return x >= Left && x < Right && y >= Top && y < Bottom;
    }
    
    public bool Intersects(Room other)
    {
        return Left < other.Right && Right > other.Left &&
               Top < other.Bottom && Bottom > other.Top;
    }
}