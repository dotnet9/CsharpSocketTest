namespace SocketDto.Enums;

/// <summary>
///     进程告警状态（没有意义，只用于测试枚举位域使用）
/// </summary>
[Flags]
public enum AlarmStatus
{
    [Description("正常")] Normal = 0,
    [Description("超时")] Overtime = 1,
    [Description("超限")] OverLimit = 2,
    [Description("切换用户")] UserChanged = 4
}