﻿using MessagePack.Resolvers;

namespace SerializationTest.SerializeUtils.Helpers;

/// <summary>
///     标准带压缩：这种方式需要在类和字段上添加特性
/// </summary>
public class MessagePackStandardWithCompressionSerializeHelper : ISerializeHelper
{
    private readonly MessagePackSerializerOptions _options =
        MessagePackSerializerOptions.Standard.WithCompression(MessagePackCompression.Lz4BlockArray);

    public byte[] Serialize<T>(T data)
    {
        return MessagePackSerializer.Serialize(data, _options);
    }

    public T Deserialize<T>(byte[] buffer)
    {
        return MessagePackSerializer.Deserialize<T>(buffer, _options);
    }
}

/// <summary>
///     标准不带压缩：这种方式需要在类和字段上添加特性
/// </summary>
public class MessagePackStandardWithOutCompressionSerializeHelper : ISerializeHelper
{
    private readonly MessagePackSerializerOptions _options = MessagePackSerializerOptions.Standard;

    public byte[] Serialize<T>(T data)
    {
        return MessagePackSerializer.Serialize(data, _options);
    }

    public T Deserialize<T>(byte[] buffer)
    {
        return MessagePackSerializer.Deserialize<T>(buffer, _options);
    }
}

/// <summary>
///     新推功能带压缩：这种方式不需要给传输对象添加特性
/// </summary>
public class MessagePackContractlessStandardResolverWithCompressionSerializeHelper : ISerializeHelper
{
    private readonly MessagePackSerializerOptions _options =
        ContractlessStandardResolver.Options.WithCompression(MessagePackCompression
            .Lz4BlockArray);

    public byte[] Serialize<T>(T data)
    {
        return MessagePackSerializer.Serialize(data, _options);
    }

    public T Deserialize<T>(byte[] buffer)
    {
        return MessagePackSerializer.Deserialize<T>(buffer, _options);
    }
}

/// <summary>
///     新推功能不带压缩：这种方式不需要给传输对象添加特性
/// </summary>
public class MessagePackContractlessStandardResolverWithOutCompressionSerializeHelper : ISerializeHelper
{
    private readonly MessagePackSerializerOptions _options =
        ContractlessStandardResolver.Options;

    public byte[] Serialize<T>(T data)
    {
        return MessagePackSerializer.Serialize(data, _options);
    }

    public T Deserialize<T>(byte[] buffer)
    {
        return MessagePackSerializer.Deserialize<T>(buffer, _options);
    }
}