using System.IO;
using UnityEngine;

public static class DebugFileLogger
{
    private static string logFilePath = "debug_logs.txt";

    public static void ClearLog()
    {
        File.WriteAllText(logFilePath, ""); // Clears the file by writing an empty string
    }

    public static void Log(string message)
    {
        Debug.Log(message); // Logs to Unity Console
        File.AppendAllText(logFilePath, message + "\n"); // Writes to file
    }
}