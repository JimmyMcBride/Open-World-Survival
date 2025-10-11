using System;
using Godot;

namespace OpenWorldSurvival.engine.core;

public partial class Log : Node
{
    // Enum to define log levels
    public enum LogLevel
    {
        Debug,
        Info,
        Warning,
        Error
    }

    private static Log _instance;
    private string _logFilePath;

    // Singleton pattern to ensure only one instance of logger
    public static Log Instance
    {
        get
        {
            if (_instance != null) return _instance;
            _instance = new Log();
            _instance._logFilePath = "user://game.log"; // Define your log file path here

            return _instance;
        }
    }

    // Method to log a message with different log levels
    public void Logger(string message, LogLevel level = LogLevel.Info)
    {
        var formattedMessage = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [{level}] {message}";

        // Print to Godot's output console
        GD.Print(formattedMessage);

        // Write to the log file
        WriteToFile(formattedMessage);
    }

    // Method to write logs into a file
    private void WriteToFile(string message)
    {
        // var file = File.Open(_logFilePath, FileMode.OpenOrCreate);
        // var buffer = Encoding.UTF8.GetBytes(message + Environment.NewLine);
        // file.Write(buffer);
        // file.Close();
    }

    // Utility methods for easy access
    public static void Debug(string message)
    {
        Instance.Logger(message, LogLevel.Debug);
    }

    public static void Info(string message)
    {
        Instance.Logger(message);
    }

    public static void Warning(string message)
    {
        Instance.Logger(message, LogLevel.Warning);
    }

    public static void Error(string message)
    {
        Instance.Logger(message, LogLevel.Error);
    }
}