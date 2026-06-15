using System;
using System.IO;
using System.Text.Json;
using System.Windows.Forms;

namespace MyAutoClicker
{
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
