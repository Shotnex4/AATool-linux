using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
#if WINDOWS
using System.Management;
using System.Runtime.InteropServices;
#endif
using System.Text.RegularExpressions;
using AATool.Configuration;
using AATool.Data.Categories;

namespace AATool.Utilities
{
    public static class ActiveInstance
    {
#if WINDOWS
        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern IntPtr GetWindowThreadProcessId(IntPtr hWnd, out uint processId);
#endif

        private const string InstanceNumberFileName = "instanceNumber.txt";
        private const string GameDirFlag = "--gameDir ";
        private const string NativesFlag = "-Djava.library.path=";

        public static string DotMinecraftPath { get; private set; } = string.Empty;
        public static string SavesPath { get; private set; } = string.Empty;
        public static string PracticeSavesPath { get; private set; } = string.Empty;
        public static string LogFile { get; private set; } = string.Empty;
        public static int Number { get; private set; } = -1;
        public static int LastActiveId { get; private set; } = -1;

        public static bool HasNumber => Number > 0;
        public static bool Watching => Config.Tracking.WatchActiveInstance;

        private static readonly Timer RefreshCooldown = new (1);

        private static string LatestLogContents;
        private static string LatestGameVersion;
        private static DateTime LastLogWriteTimeUtc;
        private static int LogStart;

        public static void SetLogStart() => LogStart = LatestLogContents?.Length ?? 0;

        public static void Update(Time time)
        {
            RefreshCooldown.Update(time);
            if (!Watching || !RefreshCooldown.IsExpired)
                return;

            RefreshCooldown.Reset();
            if (!TryGetActive(out Process instance))
            {
                if (Config.Tracking.Source == TrackerSource.ActiveInstance)
                {
                    DotMinecraftPath = string.Empty;
                    SavesPath = Paths.Saves.DefaultAppDataSavesPath;
                    PracticeSavesPath = string.Empty;
                    LogFile = Path.Combine(HomeDefaultMinecraftPath(), "logs", "latest.log");
                }
                return;
            }

            if (instance.Id != LastActiveId)
            {
                Debug.BeginTiming("read_instance");

                string args = instance.CommandLine();
                DotMinecraftPath = TryParseDotMinecraft(args, out DirectoryInfo dotMinecraft)
                    ? dotMinecraft.FullName
                    : string.Empty;

                SavesPath = !string.IsNullOrWhiteSpace(DotMinecraftPath)
                    ? Path.Combine(DotMinecraftPath, "saves")
                    : string.Empty;

                PracticeSavesPath = !string.IsNullOrWhiteSpace(DotMinecraftPath)
                    ? Path.Combine(DotMinecraftPath, "practiceSaves")
                    : string.Empty;

                LogFile = dotMinecraft is not null
                    ? Path.Combine(dotMinecraft.FullName, "logs/latest.log")
                    : string.Empty;

                UpdateGameVersion(instance);
                UpdateInstanceNumber(dotMinecraft?.FullName);
                LastActiveId = instance.Id;

                Debug.EndTiming("read_instance");
            }

            if (Config.Tracking.AutoDetectVersion && !string.IsNullOrEmpty(LatestGameVersion) && LatestGameVersion != Tracker.Category.CurrentVersion)
                Tracker.TrySetVersion(LatestGameVersion);
        }

        private static string CommandLine(this Process process)
        {
#if WINDOWS
            string query = $"SELECT CommandLine FROM Win32_Process WHERE ProcessId = {process.Id}";
            using (var searcher = new ManagementObjectSearcher(query))
            using (ManagementObjectCollection objects = searcher.Get())
            {
                return objects.Cast<ManagementBaseObject>()
                    .SingleOrDefault()?["CommandLine"]?.ToString();
            }
#else
            try
            {
                string path = $"/proc/{process.Id}/cmdline";
                if (!File.Exists(path))
                    return string.Empty;

                string[] args = File.ReadAllText(path)
                    .Split('\0', StringSplitOptions.RemoveEmptyEntries);
                return string.Join(" ", args);
            }
            catch
            {
                return string.Empty;
            }
#endif
        }

        private static bool TryGetActive(out Process instance)
        {
            instance = null;
            try
            {
                Debug.BeginTiming("get_active_instance");
#if WINDOWS
                IntPtr hWnd = GetForegroundWindow();
                GetWindowThreadProcessId(hWnd, out uint processId);
                Process active = Process.GetProcessById((int)processId);
                Debug.EndTiming("get_active_instance");

                if (active.ProcessName.StartsWith("java") && active.MainWindowTitle.StartsWith("Minecraft"))
                    instance = active;
#else
                int processId = GetFocusedLinuxProcessId();
                if (processId > 0)
                {
                    Process active = Process.GetProcessById(processId);
                    if (LooksLikeMinecraft(active))
                        instance = active;
                }

                if (instance is null)
                    instance = FindFallbackLinuxProcess();
                Debug.EndTiming("get_active_instance");
#endif
            }
            catch
            {
            }
            return instance is not null;
        }

#if !WINDOWS
        private static DateTime LatestFallbackWriteTimeUtc;

        private static int GetFocusedLinuxProcessId()
        {
            if (TryRunProcess("xdotool", "getactivewindow getwindowpid", out string xdotoolOutput)
                && int.TryParse(xdotoolOutput.Trim(), out int xdotoolPid))
            {
                return xdotoolPid;
            }

            if (TryRunProcess("hyprctl", "activewindow -j", out string hyprOutput))
            {
                Match match = Regex.Match(hyprOutput, "\"pid\"\\s*:\\s*(\\d+)");
                if (match.Success && int.TryParse(match.Groups[1].Value, out int hyprPid))
                    return hyprPid;
            }

            return -1;
        }

        private static Process FindFallbackLinuxProcess()
        {
            try
            {
                return Process.GetProcesses()
                    .Where(LooksLikeMinecraft)
                    .OrderByDescending(process => GetLatestWorldWriteTimeUtc(process))
                    .ThenByDescending(process => process.StartTime)
                    .FirstOrDefault();
            }
            catch
            {
                return null;
            }
        }

        private static bool LooksLikeMinecraft(Process process)
        {
            try
            {
                string name = process.ProcessName?.ToLowerInvariant() ?? string.Empty;
                if (!(name.StartsWith("java") || name.Contains("minecraft")))
                    return false;

                string args = process.CommandLine();
                return !string.IsNullOrEmpty(args) && (args.Contains("--gameDir") || args.Contains("minecraft") || args.Contains("java.library.path"));
            }
            catch
            {
                return false;
            }
        }

        private static DateTime GetLatestWorldWriteTimeUtc(Process process)
        {
            try
            {
                string args = process.CommandLine();
                if (!TryParseDotMinecraft(args, out DirectoryInfo dotMinecraft) || dotMinecraft is null)
                    return DateTime.MinValue;

                string savesPath = Path.Combine(dotMinecraft.FullName, "saves");
                if (!Directory.Exists(savesPath))
                    return DateTime.MinValue;

                DateTime latest = Directory.EnumerateDirectories(savesPath)
                    .Select(path => new DirectoryInfo(path).LastWriteTimeUtc)
                    .DefaultIfEmpty(DateTime.MinValue)
                    .Max();
                LatestFallbackWriteTimeUtc = latest;
                return latest;
            }
            catch
            {
                return LatestFallbackWriteTimeUtc;
            }
        }

        private static bool TryRunProcess(string fileName, string arguments, out string output)
        {
            output = string.Empty;
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
                output = process.StandardOutput.ReadToEnd();
                process.WaitForExit(1500);
                return process.ExitCode is 0 && !string.IsNullOrWhiteSpace(output);
            }
            catch
            {
                output = string.Empty;
                return false;
            }
        }
#endif

        private static bool TryParseDotMinecraft(string args, out DirectoryInfo folder)
        {
            folder = null;
            if (string.IsNullOrEmpty(args))
                return false;

            string path;
            try
            {
                if (args.Contains(GameDirFlag))
                {
                    Match match = Regex.Match(args, @$"{GameDirFlag}(?:""(.+?)""|([^\s]+))");
                    path = args.Substring(match.Index + GameDirFlag.Length, match.Length - GameDirFlag.Length)
                        .Trim('"', '\'', ' ');
                }
                else
                {
                    Match match = Regex.Match(args, $"(?:{NativesFlag}(.+?) )|(?:\"{NativesFlag}(.+?)\")");
                    int length = match.Length;
                    int index = match.Index;
                    if (args[match.Index + NativesFlag.Length] is '=')
                    {
                        length -= 1;
                        index += 1;
                    }

                    string basePath = args.Substring(index + NativesFlag.Length, length - NativesFlag.Length - 8)
                        .Trim('"', '\'', ' ')
                        .Replace("\\", Path.DirectorySeparatorChar.ToString())
                        .Replace("/", Path.DirectorySeparatorChar.ToString());
                    path = Path.Combine(basePath, ".minecraft");

                    if (!Directory.Exists(path))
                        path = Path.Combine(basePath, "minecraft");
                }

                path = path.Replace("\\", Path.DirectorySeparatorChar.ToString())
                    .Replace("/", Path.DirectorySeparatorChar.ToString());
                folder = new DirectoryInfo(path);
            }
            catch
            {
            }
            return folder is not null;
        }

        private static string HomeDefaultMinecraftPath() =>
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".minecraft");

        public static bool TryGetLog(out string latestLog)
        {
            latestLog = null;
            if (Tracker.Category is not AllDeaths)
                return false;

            latestLog = latestLog?.Length > LogStart
                ? LatestLogContents?.Substring(LogStart)
                : LatestLogContents;

            if (string.IsNullOrEmpty(LogFile))
                return false;

            try
            {
                DateTime latestLogWriteTimeUtc = File.GetLastWriteTimeUtc(LogFile);
                if (LastLogWriteTimeUtc != latestLogWriteTimeUtc || Config.Tracking.SourceChanged)
                {
                    LastLogWriteTimeUtc = latestLogWriteTimeUtc;

                    using var stream = new FileStream(LogFile,
                        FileMode.Open,
                        FileAccess.Read,
                        FileShare.ReadWrite | FileShare.Delete);
                    using (var reader = new StreamReader(stream))
                        LatestLogContents = latestLog = reader.ReadToEnd();

                    if (latestLog.Length > LogStart)
                        latestLog = latestLog.Substring(LogStart);

                    return true;
                }
            }
            catch { }
            return false;
        }

        private static void UpdateInstanceNumber(string dotMinecraft)
        {
            Number = -1;
            if (string.IsNullOrEmpty(dotMinecraft))
                return;

            string numberPath = Path.Combine(dotMinecraft, InstanceNumberFileName);
            if (File.Exists(numberPath))
            {
                try
                {
                    Number = int.Parse(File.ReadAllText(numberPath));
                }
                catch
                {
                }
            }
        }

        private static void UpdateGameVersion(Process instance)
        {
            try
            {
                string[] title = (instance.MainWindowTitle ?? string.Empty).Split(' ');
                if (title.Length > 1)
                    LatestGameVersion = title[1];
            }
            catch
            {
            }
        }
    }
}
