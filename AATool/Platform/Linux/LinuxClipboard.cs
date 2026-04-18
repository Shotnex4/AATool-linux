using System;
using System.Diagnostics;
using System.IO;

namespace AATool.Platform.Linux
{
    public static class LinuxClipboard
    {
        public static bool TrySetText(string text)
        {
            return TryRun("wl-copy", text)
                || TryRun("xclip", text, "-selection clipboard")
                || TryWriteFallback(text);
        }

        public static bool TryGetText(out string text)
        {
            if (TryRead("wl-paste", out text))
                return true;

            if (TryRead("xclip", out text, "-selection clipboard -o"))
                return true;

            if (TryReadFallback(out text))
                return true;

            text = string.Empty;
            return false;
        }

        private static string FallbackPath => Path.Combine(Environment.CurrentDirectory, "clipboard.txt");

        private static bool TryRun(string fileName, string text, string arguments = "")
        {
            try
            {
                using var process = new Process();
                process.StartInfo = new ProcessStartInfo {
                    FileName = fileName,
                    Arguments = arguments,
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                };
                process.Start();
                process.StandardInput.Write(text ?? string.Empty);
                process.StandardInput.Close();
                process.WaitForExit(2000);
                return process.ExitCode is 0;
            }
            catch
            {
                return false;
            }
        }

        private static bool TryRead(string fileName, out string text, string arguments = "")
        {
            text = string.Empty;
            try
            {
                using var process = new Process();
                process.StartInfo = new ProcessStartInfo {
                    FileName = fileName,
                    Arguments = arguments,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                };
                process.Start();
                text = process.StandardOutput.ReadToEnd();
                process.WaitForExit(2000);
                return process.ExitCode is 0;
            }
            catch
            {
                text = string.Empty;
                return false;
            }
        }

        private static bool TryWriteFallback(string text)
        {
            try
            {
                File.WriteAllText(FallbackPath, text ?? string.Empty);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static bool TryReadFallback(out string text)
        {
            text = string.Empty;
            try
            {
                if (!File.Exists(FallbackPath))
                    return false;

                text = File.ReadAllText(FallbackPath);
                return true;
            }
            catch
            {
                text = string.Empty;
                return false;
            }
        }
    }
}
