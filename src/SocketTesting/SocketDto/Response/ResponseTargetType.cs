namespace SocketDto.Response;

/// <summary>
///     响应目标终端类型
/// </summary>
[NetHead(2, 1)]
[MessagePackObject]
public class ResponseTargetType : INetObject
{
    /// <summary>
    ///     任务Id
    /// </summary>
    [Key(0)]
    public int TaskId { get; set; }


    /// <summary>
    ///     终端类型，0：Server，1：Client
    /// </summary>
    [Key(1)]
    public byte Type { get; set; }
}