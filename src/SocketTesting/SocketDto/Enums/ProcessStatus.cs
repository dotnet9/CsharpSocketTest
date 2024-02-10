namespace SocketDto.Enums;

/// <summary>
///     进程运行状态
/// </summary>
public enum ProcessStatus
{
    [Description("新建状态")] New,
    [Description("就绪状态")] Ready,
    [Description("运行状态")] Running,
    [Description("阻塞状态")] Blocked,
    [Description("终止状态")] Terminated
}