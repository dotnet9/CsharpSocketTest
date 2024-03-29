﻿using System.Threading.Channels;
using SocketTest.Logger.Models;

namespace SocketTest.Logger;

public static class Logger
{
    public static Channel<LogInfo> Logs = Channel.CreateBounded<LogInfo>(new BoundedChannelOptions(1000)
        { FullMode = BoundedChannelFullMode.DropOldest });

    public static void Debug(string content)
    {
        Logs.Writer.TryWrite(new LogInfo(LogType.Debug, content, DateTime.Now));
    }

    public static void Info(string content)
    {
        Logs.Writer.TryWrite(new LogInfo(LogType.Info, content, DateTime.Now));
    }

    public static void Warning(string content)
    {
        Logs.Writer.TryWrite(new LogInfo(LogType.Warning, content, DateTime.Now));
    }

    public static void Error(string content)
    {
        Logs.Writer.TryWrite(new LogInfo(LogType.Error, content, DateTime.Now));
    }
}