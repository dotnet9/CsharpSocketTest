namespace SerializationTest.SerializeUtils;

public interface ISerializeHelper
{
    byte[] Serialize<T>(T data);

    T? Deserialize<T>(byte[] buffer);
}