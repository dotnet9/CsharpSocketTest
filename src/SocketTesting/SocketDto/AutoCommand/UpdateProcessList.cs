using SocketDto.Response;

namespace SocketDto.AutoCommand;

/// <summary>
///     更新进程信息
/// </summary>
[NetHead(11, 1)]
public class UpdateProcessList : INetObject
{
    /// <summary>
    ///     进程列表
    /// </summary>
    public List<ProcessItem>? Processes { get; set; }
}