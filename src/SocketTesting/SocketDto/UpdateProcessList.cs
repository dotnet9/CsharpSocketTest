namespace SocketDto;

/// <summary>
/// 更新进程信息
/// </summary>
[NetHead(5, 1)]
[MessagePackObject]
public class UpdateProcessList : INetObject
{
    /// <summary>
    /// 进程列表
    /// </summary>
    [Key(0)]
    public List<ProcessItem>? Processes { get; set; }
}