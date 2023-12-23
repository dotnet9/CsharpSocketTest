﻿namespace SocketCore.LogHelper;

public partial class LogView : UserControl
{
    private const int MaxCount = 1000;
    private static InlineCollection? _inlines;

    private static readonly Dictionary<LogType, Brush> LogTypeBrushes = new()
    {
        { LogType.Debug, Brushes.LightSeaGreen },
        { LogType.Info, Brushes.Green },
        { LogType.Warning, Brushes.DarkOrange },
        { LogType.Error, Brushes.OrangeRed }
    };

    public LogView()
    {
        InitializeComponent();

        var graph = new Paragraph();
        _inlines = graph.Inlines;
        LogRichTextBox.Document.Blocks.Clear();
        LogRichTextBox.Document.Blocks.Add(graph);

        ReadLog();
    }

    private void ReadLog()
    {
        Task.Run(() =>
        {
            while (true)
            {
                if (Logger.Logs.Reader.TryRead(out var log))
                {
                    try
                    {
                        LogRichTextBox.Dispatcher.BeginInvoke(() =>
                        {
                            LogRichTextBox.BeginChange();

                            _inlines?.Add(new Run($"{log.Time:yyyy-MM-dd HH:mm:ss fff} {log.Content}\r\n")
                                { Foreground = LogTypeBrushes[log.Type] });
                            if (_inlines?.Count > MaxCount)
                            {
                                _inlines.Remove(_inlines.FirstInline);
                            }

                            LogRichTextBox.ScrollToEnd();
                            LogRichTextBox.EndChange();
                        });
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"日志读取失败，糟了：{ex.Message}");
                    }
                }
                else
                {
                    Thread.Sleep(TimeSpan.FromMilliseconds(30));
                }
            }
        });
    }
}