namespace SocketDto.Response;

/// <summary>
///     响应目标终端类型
/// </summary>
[NetHead(2, 1)]
public class ResponseTargetType : INetObject
{
    /// <summary>
    ///     任务Id
    /// </summary>
    public int TaskId { get; set; }


    /// <summary>
    ///     终端类型，0：Server，1：Client
    /// </summary>
    public byte Type { get; set; }
}