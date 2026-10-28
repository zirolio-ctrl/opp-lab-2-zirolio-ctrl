using System;
using System.IO;

namespace opp_lab_2_zirolio_ctrl.Services;

public sealed class EventLogger
{
    private static EventLogger? _instance;
    private static readonly object _lock = new();
    private readonly string _logFilePath;

    private EventLogger()
    {
        _logFilePath = Path.Combine(AppContext.BaseDirectory, "app_events.log");
    }

    public static EventLogger Instance
    {
        get
        {
            lock (_lock)
            {
                _instance ??= new EventLogger();
                return _instance;
            }
        }
    }

    public void Log(string importanceLevel, string message)
    {
        string timestamp = DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss");
        string logEntry = $"{timestamp} {importanceLevel}. {message}";

        try
        {
            File.AppendAllText(_logFilePath, logEntry + Environment.NewLine);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Помилка запису в лог: {ex.Message}");
        }
    }
}