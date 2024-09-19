using CodeWF.NetWeaver;
using SocketDto.Response;
using System.Text.Json;

namespace SocketDto.Test;

public class SocketDtoUnitTest
{
    /// <summary>
    ///     �����Ƽ������л�����
    /// </summary>
    [Fact]
    public void Test_Serialize_Success()
    {
        var obj = new ResponseProcessIDList { TaskId = 2, IDList = [1, 2] };

        var buffer = obj.Serialize(2);

        var newObj = buffer.Deserialize<ResponseProcessIDList>();

        Assert.NotNull(newObj);
        Assert.NotNull(newObj.IDList);
        Assert.Equal(obj.TaskId, newObj.TaskId);
        Assert.Equal(obj.IDList.Length, newObj.IDList.Length);
    }
}