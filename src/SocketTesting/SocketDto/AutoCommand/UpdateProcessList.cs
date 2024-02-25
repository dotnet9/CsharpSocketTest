using SocketDto.Response;

namespace SocketDto.AutoCommand;

/// <summary>
///     更新进程信息
/// </summary>
[NetHead(9, 1)]
[MessagePackObject]
public class UpdateProcessList : INetObject
{
    /// <summary>
    ///     进程列表
    /// </summary>
    [Key(0)]
    public List<ProcessItem>? Processes { get; set; }
}