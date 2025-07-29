using Godot;

namespace SuperDungeonRemake.Level;

/// <summary>
/// 地牢生成统计信息
/// 记录生成过程中的各种数据
/// </summary>
public partial class GenerationStats : RefCounted
{
    #region Properties
    /// <summary>
    /// 生成耗时（秒）
    /// </summary>
    public double GenerationTime { get; set; }
    
    /// <summary>
    /// 房间数量
    /// </summary>
    public int RoomCount { get; set; }
    
    /// <summary>
    /// 总面积
    /// </summary>
    public int TotalArea { get; set; }
    
    /// <summary>
    /// 平均房间大小
    /// </summary>
    public float AverageRoomSize { get; set; }
    
    /// <summary>
    /// 走廊段数
    /// </summary>
    public int CorridorCount { get; set; }
    
    /// <summary>
    /// 装饰物数量
    /// </summary>
    public int DecorationCount { get; set; }
    
    /// <summary>
    /// BSP分割深度
    /// </summary>
    public int MaxDepth { get; set; }
    
    /// <summary>
    /// 最大房间面积
    /// </summary>
    public int MaxRoomArea { get; set; }
    
    /// <summary>
    /// 最小房间面积
    /// </summary>
    public int MinRoomArea { get; set; }
    
    /// <summary>
    /// 地图利用率（房间面积/总面积）
    /// </summary>
    public float MapUtilization { get; set; }
    #endregion
    
    #region Constructor
    /// <summary>
    /// 构造函数
    /// </summary>
    public GenerationStats()
    {
        Reset();
    }
    #endregion
    
    #region Methods
    /// <summary>
    /// 重置统计信息
    /// </summary>
    public void Reset()
    {
        GenerationTime = 0;
        RoomCount = 0;
        TotalArea = 0;
        AverageRoomSize = 0;
        CorridorCount = 0;
        DecorationCount = 0;
        MaxDepth = 0;
        MaxRoomArea = 0;
        MinRoomArea = int.MaxValue;
        MapUtilization = 0;
    }
    
    /// <summary>
    /// 获取统计信息字符串
    /// </summary>
    /// <returns>格式化的统计信息</returns>
    public string GetStatsString()
    {
        return $"地牢生成统计:\n" +
               $"- 生成时间: {GenerationTime:F3}秒\n" +
               $"- 房间数量: {RoomCount}\n" +
               $"- 总面积: {TotalArea}\n" +
               $"- 平均房间大小: {AverageRoomSize:F1}\n" +
               $"- 走廊段数: {CorridorCount}\n" +
               $"- 装饰物数量: {DecorationCount}\n" +
               $"- 最大分割深度: {MaxDepth}\n" +
               $"- 最大房间面积: {MaxRoomArea}\n" +
               $"- 最小房间面积: {MinRoomArea}\n" +
               $"- 地图利用率: {MapUtilization:P1}";
    }
    
    /// <summary>
    /// 打印统计信息到控制台
    /// </summary>
    public void PrintStats()
    {
        GD.Print(GetStatsString());
    }
    #endregion
}