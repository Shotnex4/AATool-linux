using System;

namespace AATool.Platform.Linux
{
    public static class LinuxRuntime
    {
        public static void Report(string title, string message)
        {
            Console.Error.WriteLine($"{title}: {message}");
            Debug.Log(Debug.ErrorSection, $"{title}: {message}");
        }

        public static void ReportUpdate(string message)
        {
            Console.WriteLine(message);
            Debug.Log(Debug.SystemSection, message);
        }

        public static bool Confirm(string title, string message, bool defaultResult = true)
        {
            Report(title, message + $" Defaulting to {(defaultResult ? "yes" : "no")} on Linux.");
            return defaultResult;
        }
    }
}
