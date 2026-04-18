using System;

namespace AATool.Platform.Linux
{
    public static class LinuxScreen
    {
        public const int DefaultWidth = 1920;
        public const int DefaultHeight = 1080;

        public static bool MonitorSupportsRelaxed => DefaultWidth >= 1600 && DefaultHeight >= 900;

        public static (int width, int height) GetPrimaryBounds() => (DefaultWidth, DefaultHeight);
    }
}
