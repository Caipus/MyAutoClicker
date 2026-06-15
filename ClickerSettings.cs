using System;
using System.Windows.Forms;

namespace MyAutoClicker
{
    public class ClickerSettings
    {
        // Interval
        public int Hours { get; set; } = 0;
        public int Minutes { get; set; } = 0;
        public int Seconds { get; set; } = 0;
        public int Milliseconds { get; set; } = 100;
        public bool UseRandomTolerance { get; set; } = false;
        public int RandomTolerancePercent { get; set; } = 10;

        // Repeat
        public bool InfiniteRepeat { get; set; } = true;
        public int RepeatCount { get; set; } = 10;

        // Mouse Options
        public string MouseButton { get; set; } = "Left"; // Left, Right, Middle
        public string ClickType { get; set; } = "Single";   // Single, Double

        // Target Window Settings
        public string ProcessName { get; set; } = string.Empty;
        public string WindowTitle { get; set; } = string.Empty;
        public string WindowClassName { get; set; } = string.Empty;
        public string ClickMode { get; set; } = "Foreground"; // Background, Foreground
        public string CoordMode { get; set; } = "Window";     // Window = relative to window client area, Screen = fixed screen coords
        public int RelativeX { get; set; } = 0;
        public int RelativeY { get; set; } = 0;

        // Hotkeys (using virtual key codes or Keys enum)
        public Keys StartHotkey { get; set; } = Keys.F6;
        public Keys StopHotkey { get; set; } = Keys.F7;
    }
}
