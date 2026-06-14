using System;
using System.IO;
using System.Text.Json;
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
        public string ClickMode { get; set; } = "Background"; // Background, Foreground
        public string CoordMode { get; set; } = "Window";     // Window = relative to window client area, Screen = fixed screen coords
        public int RelativeX { get; set; } = 0;
        public int RelativeY { get; set; } = 0;

        // Hotkeys (using virtual key codes or Keys enum)
        public Keys StartHotkey { get; set; } = Keys.F6;
        public Keys StopHotkey { get; set; } = Keys.F7;
    }

    public static class SettingsManager
    {
        private static readonly string SettingsFileName = "clicker_settings.json";
        private static readonly string SettingsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, SettingsFileName);

        public static ClickerSettings LoadSettings()
        {
            try
            {
                if (File.Exists(SettingsPath))
                {
                    string json = File.ReadAllText(SettingsPath);
                    var settings = JsonSerializer.Deserialize<ClickerSettings>(json);
                    return settings ?? new ClickerSettings();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading settings: {ex.Message}");
            }
            return new ClickerSettings();
        }

        public static void SaveSettings(ClickerSettings settings)
        {
            try
            {
                var options = new JsonSerializerOptions { WriteIndented = true };
                string json = JsonSerializer.Serialize(settings, options);
                File.WriteAllText(SettingsPath, json);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving settings: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
