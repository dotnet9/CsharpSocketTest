using CodeWF.Tools.Extensions;
using MessagePack.Resolvers;

namespace SerializationTest.Test;

[MemoryDiagnoser]
[RankColumn]
public class BenchmarkTest
{
    /// <summary>
    ///     测试数据量
    /// </summary>
    private const int MockCount = 100;

    private static readonly Random RandomShared = new(DateTime.Now.Millisecond);

    static BenchmarkTest()
    {
        MockData = new ResponseOrganizations
        {
            Organizations = Enumerable.Range(0, MockCount).Select(orgIndex => new Organization
            {
                Id = orgIndex,
                Name = $"Name{orgIndex}",
                Tags = Enumerable.Range(RandomShared.Next(0, 5), RandomShared.Next(10, 15))
                    .Select(tagIndex => $"标签{tagIndex}")
                    .ToList(),
                Address = $"地址{orgIndex}",
                EmployeeCount = RandomShared.Next(10, 1000),
                Departments = Enumerable.Range(2, 10).Select(i => new Department
                {
                    Id = i,
                    Code = $"D{Lorem.Words(1, 3)}",
                    Name = $"部门{Lorem.Words(1, 3)}",
                    Description = $"描述{Lorem.Words(1, 3)}",
                    Location = $"位置{Lorem.Words(1, 3)}",
                    EmployeeCount = RandomShared.Next(5, 100),
                    Employees = Enumerable.Range(10, 100).Select(empIndex => new Employee
                    {
                        Id = empIndex,
                        Code = $"E{Lorem.Words(1, 3)}",
                        FirstName = $"名{Lorem.Words(1, 3)}",
                        LastName = $"姓{Lorem.Words(1, 3)}",
                        NickName = $"昵称{Lorem.Words(1, 3)}",
                        BirthDate = DateTime.Now.AddMilliseconds(-1 * RandomShared.Next(500000, 500000000))
                            .GetUnixTimeMilliseconds(),
                        Description = $"描述{Lorem.Words(1, 3)}",
                        Address = $"地址{Lorem.Words(1, 3)}",
                        Email = $"邮件{Lorem.Words(1, 3)}@dotnet9.com",
                        PhoneNumber = RandomShared.Next(1000000, 999999999).ToString(),
                        Salary = RandomShared.Next(2000, 100000),
                        DepartmentId = i,
                        EntryTime = DateTime.Now.AddMilliseconds(-1 * RandomShared.Next(500000, 500000000))
                            .GetUnixTimeMilliseconds()
                    }).ToList(),
                    Budget = RandomShared.Next(2000, 100000) + (decimal)RandomShared.NextDouble(),
                    Value = RandomShared.NextDouble(),
                    CreateTime = DateTime.Now.AddMilliseconds(-1 * RandomShared.Next(500000, 500000000))
                        .GetUnixTimeMilliseconds()
                }).ToList(),
                AnnualBudget = RandomShared.Next(20000, 1000000) + (decimal)RandomShared.NextDouble(),
                FoundationDate = DateTime.Now.AddMilliseconds(-1 * RandomShared.Next(500000, 500000000))
                    .GetUnixTimeMilliseconds()
            }).ToList()
        };

        MockDpartment = new Department
        {
            Id = DateTime.Now.Millisecond,
            Code = $"D{Lorem.Words(1, 3)}",
            Name = $"部门{Lorem.Words(1, 3)}",
            Description = $"描述{Lorem.Words(1, 3)}",
            Location = $"位置{Lorem.Words(1, 3)}",
            EmployeeCount = RandomShared.Next(5, 100),
            Employees = Enumerable.Range(0, 10000).Select(empIndex => new Employee
            {
                Id = empIndex,
                Code = $"E{Lorem.Words(1, 3)}",
                FirstName = $"名{Lorem.Words(1, 3)}",
                LastName = $"姓{Lorem.Words(1, 3)}",
                NickName = $"昵称{Lorem.Words(1, 3)}",
                BirthDate = DateTime.Now.AddMilliseconds(-1 * RandomShared.Next(500000, 500000000)).GetUnixTimeMilliseconds(),
                Description = $"描述{Lorem.Words(1, 3)}",
                Address = $"地址{Lorem.Words(1, 3)}",
                Email = $"邮件{Lorem.Words(1, 3)}@dotnet9.com",
                PhoneNumber = RandomShared.Next(1000000, 999999999).ToString(),
                Salary = RandomShared.Next(2000, 100000),
                DepartmentId = 3,
                EntryTime = DateTime.Now.AddMilliseconds(-1 * RandomShared.Next(500000, 500000000)).GetUnixTimeMilliseconds()
            }).ToList(),
            Budget = RandomShared.Next(2000, 100000) + (decimal)RandomShared.NextDouble(),
            Value = RandomShared.NextDouble(),
            CreateTime = DateTime.Now.AddMilliseconds(-1 * RandomShared.Next(500000, 500000000)).GetUnixTimeMilliseconds()
        };
    }

    /// <summary>
    ///     测试数据
    /// </summary>
    private static ResponseOrganizations MockData { get; }

    /// <summary>
    ///     测试数据
    /// </summary>
    private static Department MockDpartment { get; }


    //[Benchmark]
    //public void JsonByteSerialize()
    //{
    //    RunSerialize(new JsonSerializeHelper());
    //}

    [Benchmark]
    public void CustomSerialize()
    {
        RunSerialize(new CustomSerializeHelper());
    }

    [Benchmark]
    public void ProtoBufSerialize()
    {
        RunSerialize(new ProtoBufSerializeHelper());
    }

    [Benchmark]
    public void MessagePackStandardWithCompressionSerializeHelper()
    {
        RunSerialize(new MessagePackStandardWithCompressionSerializeHelper());
    }

    [Benchmark]
    public void MessagePackStandardWithOutCompressionSerializeHelper()
    {
        RunSerialize(new MessagePackStandardWithOutCompressionSerializeHelper());
    }

    [Benchmark]
    public void MessagePackContractlessStandardResolverWithCompressionSerializeHelper()
    {
        RunSerialize(new MessagePackContractlessStandardResolverWithCompressionSerializeHelper());
    }

    [Benchmark]
    public void MessagePackContractlessStandardResolverWithOutCompressionSerializeHelper()
    {
        RunSerialize(new MessagePackContractlessStandardResolverWithOutCompressionSerializeHelper());
    }


    /// <summary>
    ///     简单测试
    /// </summary>
    public static void Test(List<ISerializeHelper>? moreHelpers = null)
    {
        MessagePackSerializer.DefaultOptions = ContractlessStandardResolver.Options;
        var serializeHelpers = new List<ISerializeHelper>
        {
            new CustomSerializeHelper(),
            new ProtoBufSerializeHelper(),
            new MessagePackStandardWithCompressionSerializeHelper(),
            new MessagePackStandardWithOutCompressionSerializeHelper(),
            new MessagePackContractlessStandardResolverWithCompressionSerializeHelper(),
            new MessagePackContractlessStandardResolverWithOutCompressionSerializeHelper()
        };
        if (moreHelpers?.Count() > 0) serializeHelpers.AddRange(moreHelpers);

        serializeHelpers.ForEach(RunSerialize);
    }


    private static void RunSerialize(ISerializeHelper helper)
    {
        var sw = Stopwatch.StartNew();

        var buffer = helper.Serialize(MockDpartment);

        sw.Stop();
        Log($"{helper.GetType().Name} Serialize {sw.ElapsedMilliseconds}ms {buffer.Length}byte");

        sw.Restart();

        var data = helper.Deserialize<Department>(buffer);

        sw.Stop();

        Log($"{helper.GetType().Name} Deserialize {sw.ElapsedMilliseconds}ms {data?.Employees?.Count}项");
    }

    private static void Log(string log)
    {
        Console.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss fff}: {log}");
    }

    public static void TestBit()
    {
        var processData = new ProcessData
        {
            Cpu = 253, Memory = 510, Disk = 32, Network = 323, Gpu = 32, GpuEngine = 1, PowerUsage = 3,
            PowerUsageTrend = 2, Type = 1, Status = 1
        };
        var serializedData = processData.Serialize(); // Serialize to byte array  
        var deserializedData = ProcessData.Deserialize(serializedData);
        if (deserializedData.Cpu == processData.Cpu && deserializedData.Memory == processData.Memory)
            Console.WriteLine("success");
    }
}