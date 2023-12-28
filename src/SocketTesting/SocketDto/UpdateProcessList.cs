namespace SocketDto;

/// <summary>
/// 更新进程信息
/// </summary>
[NetHead(5, 1)]
public class UpdateProcessList : INetObject
{
    /// <summary>
    /// 进程列表
    /// </summary>
    public List<ProcessItem>? Processes { get; set; }
}