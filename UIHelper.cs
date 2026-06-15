using System;
using System.Drawing;
using System.Windows.Forms;

namespace MyAutoClicker
{
    public static class UIHelper
    {
        public static void ApplyModernStyle(Button btn)
        {
            btn.FlatStyle = FlatStyle.Flat;
            btn.ForeColor = Color.White;
            btn.BackColor = Color.FromArgb(35, 35, 35);
            btn.FlatAppearance.BorderColor = Color.FromArgb(60, 60, 60);
            btn.FlatAppearance.BorderSize = 1;
            btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(0, 180, 216);
            btn.FlatAppearance.MouseDownBackColor = Color.FromArgb(0, 150, 180);
            btn.Font = new Font("Segoe UI", 9F, FontStyle.Bold);

            btn.Paint += (s, e) =>
            {
                if (btn.Enabled)
                {
                    return;
                }

                var g = e.Graphics;
                g.Clear(Color.FromArgb(28, 28, 28));  // dark disabled background

                // Border
                using var borderPen = new Pen(Color.FromArgb(48, 48, 48));
                g.DrawRectangle(borderPen, 0, 0, btn.Width - 1, btn.Height - 1);

                // Text with explicit disabled gray so it's always readable
                TextRenderer.DrawText(g, btn.Text, btn.Font, btn.ClientRectangle,
                    Color.FromArgb(110, 110, 110),
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine);
            };
        }
    }
}
