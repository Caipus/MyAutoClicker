using System;
using System.Drawing;
using System.Windows.Forms;
using System.Diagnostics;

namespace MyAutoClicker
{
    public static class AboutDialog
    {
        public static void Show(IWin32Window owner)
        {
            using (Form about = new Form())
            {
                about.Text = "About Neo Auto Clicker";
                about.FormBorderStyle = FormBorderStyle.FixedSingle;
                about.MaximizeBox = false;
                about.MinimizeBox = false;
                about.StartPosition = FormStartPosition.CenterParent;
                about.ClientSize = new Size(380, 315);
                about.BackColor = Color.FromArgb(20, 20, 20);
                about.ForeColor = Color.White;

                // Apply same DWM dark title bar
                try
                {
                    int dark = 1;
                    Win32Helper.DwmSetWindowAttribute(about.Handle, Win32Helper.DWMWA_USE_IMMERSIVE_DARK_MODE, ref dark, sizeof(int));
                    int cap = 25 | (25 << 8) | (25 << 16);
                    Win32Helper.DwmSetWindowAttribute(about.Handle, Win32Helper.DWMWA_CAPTION_COLOR, ref cap, sizeof(int));
                    int txt = 220 | (220 << 8) | (220 << 16);
                    Win32Helper.DwmSetWindowAttribute(about.Handle, Win32Helper.DWMWA_TEXT_COLOR, ref txt, sizeof(int));
                    int brd = 0 | (180 << 8) | (216 << 16);
                    Win32Helper.DwmSetWindowAttribute(about.Handle, Win32Helper.DWMWA_BORDER_COLOR, ref brd, sizeof(int));
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"DWM styling failed: {ex.Message}");
                }

                // Accent line at top
                var topLine = new Panel
                {
                    Location = new Point(0, 0),
                    Size = new Size(380, 4),
                    BackColor = Color.FromArgb(0, 180, 216)
                };

                // App icon / title
                var lblName = new Label
                {
                    Text = "NEO AUTO CLICKER",
                    Font = new Font("Segoe UI", 16F, FontStyle.Bold),
                    ForeColor = Color.FromArgb(0, 180, 216),
                    Location = new Point(20, 22),
                    AutoSize = true
                };

                var lblVersion = new Label
                {
                    Text = "Version 1.0.0",
                    Font = new Font("Segoe UI", 9.5F, FontStyle.Regular),
                    ForeColor = Color.FromArgb(160, 160, 160),
                    Location = new Point(22, 60),
                    AutoSize = true
                };

                // Separator line
                about.Paint += (s, e) =>
                {
                    e.Graphics.DrawLine(new Pen(Color.FromArgb(45, 45, 45), 1), 20, 84, 360, 84);
                    e.Graphics.DrawLine(new Pen(Color.FromArgb(45, 45, 45), 1), 20, 210, 360, 210);
                };

                var lblDesc = new Label
                {
                    Text = "A lightweight auto-clicker with multi-monitor support,\n" +
                           "window-targeting, background clicking, and hotkey control.\n\n" +
                           "Supports Window-relative and Screen-position modes\n" +
                           "for precise click placement regardless of window position.",
                    Font = new Font("Segoe UI", 9F),
                    ForeColor = Color.FromArgb(200, 200, 200),
                    Location = new Point(20, 96),
                    Size = new Size(340, 110)
                };

                var lblCopy = new Label
                {
                    Text = "© 2026  •  Built with ♥ in C# / WinForms",
                    Font = new Font("Segoe UI", 8F),
                    ForeColor = Color.FromArgb(110, 110, 110),
                    Location = new Point(20, 228),
                    AutoSize = true
                };

                var btnClose = new Button
                {
                    Text = "Close",
                    Size = new Size(100, 34),
                    Location = new Point(140, 265),
                    FlatStyle = FlatStyle.Flat,
                    ForeColor = Color.White,
                    BackColor = Color.FromArgb(35, 35, 35),
                    Font = new Font("Segoe UI", 9F, FontStyle.Bold)
                };
                btnClose.FlatAppearance.BorderColor = Color.FromArgb(0, 180, 216);
                btnClose.FlatAppearance.BorderSize = 1;
                btnClose.FlatAppearance.MouseOverBackColor = Color.FromArgb(0, 180, 216);
                btnClose.Click += (s, e) => about.Close();
                about.AcceptButton = btnClose;

                about.Controls.AddRange(new Control[] { topLine, lblName, lblVersion, lblDesc, lblCopy, btnClose });
                about.ShowDialog(owner);
            }
        }
    }
}
