﻿namespace SerializationTest.Models;

[ProtoContract]
[MessagePackObject]
public class ResponseOrganizations
{
    /// <summary>
    ///     Id
    /// </summary>
    [ProtoMember(1)]
    [Key(0)]
    public List<Organization>? Organizations { get; set; }
}