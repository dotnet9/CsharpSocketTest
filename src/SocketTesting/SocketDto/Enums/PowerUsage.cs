namespace SocketDto.Enums;

/// <summary>
///     电源使用情况
/// </summary>
public enum PowerUsage
{
    [Description("非常低")] VeryLow,
    [Description("低")] Low,
    [Description("中")] Moderate,
    [Description("高")] High,
    [Description("非常高")] VeryHigh
}