using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace AATool
{
    public static class Debug
    {
        public const string GraphicsSection = "graphics";
        public const string SystemSection = "system";
        public const string TrackingSection = "tracking";
        public const string RequestSection = "requests";
        public const string ErrorSection = "errors";

        public static readonly Dictionary<string, Stopwatch> Watches = new ();
        public static readonly Dictionary<string, StringBuilder> Logs = new ();
        public static readonly StringBuilder GlobalLog = new ();

        public static string GetGlobalLog() => GlobalLog.ToString();
        public static bool EnableTiming { get; set; } = true;

        public static string GetLog(string section) => Logs.TryGetValue(section, out StringBuilder log) ? log.ToString() : string.Empty;

        public static void BeginTiming(string name)
        {
            if (!EnableTiming)
                return;

            if (!Watches.TryGetValue(name, out Stopwatch watch))
                Watches[name] = watch = new Stopwatch();
            watch.Restart();
        }

        public static void EndTiming(string name)
        {
            if (!EnableTiming)
                return;

            if (Watches.TryGetValue(name, out Stopwatch watch))
                watch.Stop();
        }


        public static void Log(string section, string message)
        {
            if (!Logs.TryGetValue(section, out StringBuilder log))
                Logs[section] = log = new StringBuilder();

            string line = $"{DateTime.Now:hh:mm:ss} {message}";
            GlobalLog.AppendLine(line);
            log.AppendLine(line);
        }

        public static void SaveReport(Exception exception)
        {
            Directory.CreateDirectory(Paths.System.LogsFolder);
            using (StreamWriter stream = File.CreateText(Paths.System.CrashLogFile))
            {
                stream.WriteLine("OS: " + RuntimeInformation.OSDescription);
                stream.WriteLine("Architecture: " + RuntimeInformation.OSArchitecture);

                if (!Directory.Exists("assets"))
                    stream.WriteLine("\"assets\" Folder Missing!!!");

                if (exception is not null)
                {
                    stream.WriteLine("Exception: " + exception.Message);
                    stream.Write(exception.StackTrace
                        .Replace("   at ", "\n    at ")
                        .Replace(") in ", ")\n        in file: ")
                        .Replace(":line ", "\n        on line: "));
                }
                stream.Flush();
            }
        }
    }
}
