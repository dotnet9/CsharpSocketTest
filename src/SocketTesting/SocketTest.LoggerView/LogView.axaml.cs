using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Media;
using Avalonia.Threading;
using SocketTest.Logger.Models;

namespace SocketTest.LoggerView;

public partial class LogView : UserControl
{
    private const int MaxCount = 1000;
    private static InlineCollection? _inlines;

    private static readonly Dictionary<LogType, IImmutableSolidColorBrush> LogTypeBrushes = new()
    {
        { LogType.Debug, Brushes.LightBlue },
        { LogType.Info, Brushes.Green },
        { LogType.Warning, Brushes.DarkOrange },
        { LogType.Error, Brushes.OrangeRed }
    };

    private bool _isLogging;

    public LogView()
    {
        InitializeComponent();
        LogTextView.AttachedToVisualTree += (s, e) => ReadLog();
    }

    private void ReadLog()
    {
        if (_isLogging) return;

        _isLogging = true;
        _inlines = LogTextView.Inlines;
        Task.Run(async () =>
        {
            while (true)
                if (Logger.Logger.Logs.Reader.TryRead(out var log))
                {
                    await LogAsync(log);
                    await Task.Delay(TimeSpan.FromMilliseconds(1));
                }
                else
                {
                    await Task.Delay(TimeSpan.FromMilliseconds(30));
                }
        });
    }

    private async Task LogAsync(LogInfo log)
    {
        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            try
            {
                var content = $"{log.Time:yyyy-MM-dd HH:mm:ss fff} {log.Content}\r\n";
                _inlines?.Add(new Run(content)
                    { Foreground = LogTypeBrushes[log.Type] });
                if (_inlines?.Count > MaxCount) _inlines.Remove(_inlines.First());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"日志记录失败，没法了：{ex.Message}");
            }
        });
    }
}