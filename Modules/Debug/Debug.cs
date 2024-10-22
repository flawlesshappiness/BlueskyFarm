using Godot;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;

public static class Debug
{
    public const bool PRINT_ENABLED = true;

    public static int Indent = 0;

    public static bool IsEditor => Engine.IsEditorHint();

    public static List<DebugAction> RegisteredActions = new();

    public static event Action<DebugAction> OnActionAdded;

    private static List<LogMessage> _logs = new();

    public static void Log(object o)
    {
        var message = o == null ? "null" : o.ToString();
        Log(message);
    }

    public static void Log(string message)
    {
        var log = new LogMessage
        {
            Message = message,
            Type = LogType.Log,
            Indent = Indent,
        };

        _logs.Add(log);

        if (PRINT_ENABLED)
        {
            GD.Print(log.GetDebugMessage());
        }
    }

    public static void LogMethod(object obj = null, [CallerFilePath] string file = "", [CallerMemberName] string caller = "")
    {
        if (string.IsNullOrEmpty(caller)) return;
        if (string.IsNullOrEmpty(file)) return;
        var filename = Path.GetFileNameWithoutExtension(file);
        var message = obj?.ToString() ?? "";
        Log(string.IsNullOrEmpty(message) ? $"{filename}.{caller}" : $"{filename}.{caller}: {message}");
    }

    public static void LogError(string message)
    {
        var log = new LogMessage
        {
            Message = message,
            Type = LogType.Error,
            Indent = Indent,
        };

        _logs.Add(log);

        if (PRINT_ENABLED)
        {
            GD.PrintErr(log.GetDebugMessage());
        }
    }

    public static void Trace(object o)
    {
        var message = o == null ? "null" : o.ToString();
        Trace(message);
    }

    public static void Trace(string message)
    {
        var log = new LogMessage
        {
            Message = message,
            Type = LogType.Trace,
            Indent = Indent,
        };

        _logs.Add(log);
    }

    public static void TraceMethod(object obj = null, [CallerFilePath] string file = "", [CallerMemberName] string caller = "")
    {
        if (string.IsNullOrEmpty(caller)) return;
        if (string.IsNullOrEmpty(file)) return;
        var filename = Path.GetFileNameWithoutExtension(file);
        var message = obj?.ToString() ?? "";
        Trace(string.IsNullOrEmpty(message) ? $"{filename}.{caller}" : $"{filename}.{caller}: {message}");
    }

    public static void WriteLogsToPersistentData()
    {
        LogMethod();

        var path = $"user://log.txt";
        var file = Godot.FileAccess.Open(path, Godot.FileAccess.ModeFlags.Write);

        foreach (var log in _logs)
        {
            file.StoreLine(log.GetLogMessage());
        }
    }

    public static void RegisterAction(DebugAction action)
    {
        RegisteredActions.Add(action);
        OnActionAdded?.Invoke(action);
    }

    public static void RegisterDebugActions()
    {
        var category = "DEBUG";

        Debug.RegisterAction(new DebugAction
        {
            Text = "Quit",
            Category = category,
            Action = _ =>
            {
                Data.Game.Save();
                Scene.Tree.Quit();
            }
        });

        Debug.RegisterAction(new DebugAction
        {
            Text = "Hide content",
            Category = category,
            Action = v => v.HideContent()
        });

        Debug.RegisterAction(new DebugAction
        {
            Text = "Log",
            Category = category,
            Action = DebugShowLog
        });
    }

    private static void DebugShowLog(DebugView v)
    {
        v.HideContent();
        v.Content.Show();
        v.ContentList.Show();
        v.ContentList.Clear();

        foreach (var log in _logs)
        {
            v.ContentList.AddText(log.GetLogMessage());
        }
    }
}

public class DebugAction
{
    public string Text { get; set; }

    public string Category { get; set; }

    public System.Action<DebugView> Action { get; set; }
}

public enum LogType { Log, Trace, Exception, Error }

public class LogMessage
{
    public string Message { get; set; }
    public LogType Type { get; set; } = LogType.Log;
    public int Indent { get; set; } = 0;
    public string UtcTime { get; private set; }
    public string GameTime { get; private set; }

    public LogMessage()
    {
        UtcTime = DateTime.UtcNow.ToString("hh:mm:ss:f", CultureInfo.InvariantCulture);
        GameTime = Time.GetTicksMsec().ToString();
    }

    public string GetDebugMessage()
    {
        return GetIndentString() + Message;
    }

    public string GetLogMessage()
    {
        return $"[{UtcTime}] {Type}: {Message}";
    }

    private string GetIndentString()
    {
        string s = "";

        for (int i = 0; i < Indent; i++)
        {
            s += "  ";
        }

        return s;
    }
}