using System;
using System.Drawing;
using System.Windows.Forms;

namespace MyAutoClicker
{
    public static class HelpDialog
    {
        public static void Show(IWin32Window owner)
        {
            using (Form dialog = new Form())
            {
                dialog.Text = "Neo Auto Clicker - Help";
                dialog.Size = new Size(500, 470);
                dialog.FormBorderStyle = FormBorderStyle.FixedDialog;
                dialog.MaximizeBox = false;
                dialog.MinimizeBox = false;
                dialog.StartPosition = FormStartPosition.CenterParent;
                dialog.BackColor = Color.FromArgb(20, 20, 20);
                dialog.ForeColor = Color.White;

                try
                {
                    int dark = 1;
                    Win32Helper.DwmSetWindowAttribute(dialog.Handle, Win32Helper.DWMWA_USE_IMMERSIVE_DARK_MODE, ref dark, sizeof(int));
                    int cap = 25 | (25 << 8) | (25 << 16);
                    Win32Helper.DwmSetWindowAttribute(dialog.Handle, Win32Helper.DWMWA_CAPTION_COLOR, ref cap, sizeof(int));
                    int txt = 220 | (220 << 8) | (220 << 16);
                    Win32Helper.DwmSetWindowAttribute(dialog.Handle, Win32Helper.DWMWA_TEXT_COLOR, ref txt, sizeof(int));
                    int brd = 0 | (180 << 8) | (216 << 16);
                    Win32Helper.DwmSetWindowAttribute(dialog.Handle, Win32Helper.DWMWA_BORDER_COLOR, ref brd, sizeof(int));
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"DWM styling failed: {ex.Message}");
                }

                var topLine = new Panel
                {
                    Location = new Point(0, 0),
                    Size = new Size(500, 4),
                    BackColor = Color.FromArgb(0, 180, 216)
                };

                Button btnEN = new Button { Text = "English", Location = new Point(20, 15), Size = new Size(70, 28) };
                Button btnDE = new Button { Text = "Deutsch", Location = new Point(95, 15), Size = new Size(70, 28) };
                
                UIHelper.ApplyModernStyle(btnEN);
                UIHelper.ApplyModernStyle(btnDE);

                TextBox txtHelpContent = new TextBox
                {
                    Location = new Point(20, 55),
                    Size = new Size(445, 300),
                    Multiline = true,
                    ReadOnly = true,
                    ScrollBars = ScrollBars.Vertical,
                    BackColor = Color.FromArgb(28, 28, 28),
                    ForeColor = Color.FromArgb(220, 220, 220),
                    BorderStyle = BorderStyle.FixedSingle,
                    Font = new Font("Segoe UI", 9.5F)
                };

                Button btnClose = new Button { Text = "Close", Location = new Point(190, 375), Size = new Size(100, 34), DialogResult = DialogResult.OK };
                UIHelper.ApplyModernStyle(btnClose);

                string helpEN = 
                    "=== NEO AUTO CLICKER USER HELP ===\r\n\r\n" +
                    "1. CLICK INTERVAL\r\n" +
                    "   Set the delay between clicks. Milliseconds must be greater than 0 if hours/minutes/seconds are 0.\r\n\r\n" +
                    "2. CLICK REPEAT\r\n" +
                    "   - Infinite: Clicks continuously until you press the Stop Hotkey.\r\n" +
                    "   - Repeat count: Performs the specified number of clicks and then stops automatically.\r\n\r\n" +
                    "3. HUMAN TOLERANCE (New)\r\n" +
                    "   - Enable this to add human-like variation to the click speed.\r\n" +
                    "   - Enter a percentage (e.g., 10%). The click interval will be randomly shifted earlier or later by up to that percentage (e.g. 1000ms +/- 100ms).\r\n" +
                    "   - A live preview of the maximum millisecond offset is displayed on the screen.\r\n\r\n" +
                    "4. CLICK OPTIONS\r\n" +
                    "   - Mouse Button: Select Left, Right, or Middle click.\r\n" +
                    "   - Click Type: Select Single or Double click.\r\n\r\n" +
                    "5. CLICK POSITION\r\n" +
                    "   - Current Cursor Position: Clicks wherever the mouse is currently hovering.\r\n" +
                    "   - Target Specific Application: Targets a specific window/process.\r\n\r\n" +
                    "6. TARGET APPLICATION DETAILS\r\n" +
                    "   - Click Mode:\r\n" +
                    "     * Foreground: Focuses the target window and sends clicks (highly reliable).\r\n" +
                    "     * Background: Sends click events directly to the window in the background without focusing it.\r\n" +
                    "   - Coord Mode:\r\n" +
                    "     * Window-relative: Coordinates follow the window if it moves.\r\n" +
                    "     * Screen position: Coordinates stay at a fixed physical screen position.\r\n" +
                    "   - Pick Location: Click this, then hover and click on your target application window to link it automatically.\r\n\r\n" +
                    "7. HOTKEYS\r\n" +
                    "   - Default Start Hotkey: F6\r\n" +
                    "   - Default Stop Hotkey: F7\r\n" +
                    "   - You can change these hotkeys in the 'Hotkeys' dialog.";

                string helpDE = 
                    "=== NEO AUTO CLICKER BENUTZER-HILFE ===\r\n\r\n" +
                    "1. KLICK-INTERVALL\r\n" +
                    "   Stellt die Verzögerung zwischen den Klicks ein. Millisekunden müssen größer als 0 sein, wenn Stunden/Minuten/Sekunden auf 0 stehen.\r\n\r\n" +
                    "2. KLICK-WIEDERHOLUNG\r\n" +
                    "   - Unendlich: Klickt fortlaufend, bis der Stopp-Hotkey gedrückt wird.\r\n" +
                    "   - Wiederholungsanzahl: Führt die angegebene Anzahl an Klicks aus und stoppt dann automatisch.\r\n\r\n" +
                    "3. MENSCHLICHE TOLERANZ (Neu)\r\n" +
                    "   - Aktivieren, um eine natürliche Klickverzögerung zu simulieren.\r\n" +
                    "   - Gib einen Prozentsatz ein (z.B. 10%). Das Klickintervall wird zufällig um bis zu diesen Prozentsatz verkürzt oder verlängert (z.B. 1000ms +/- 100ms).\r\n" +
                    "   - Eine Live-Vorschau der maximalen Abweichung in Millisekunden wird direkt im UI angezeigt.\r\n\r\n" +
                    "4. KLICK-OPTIONEN\r\n" +
                    "   - Maustaste: Auswahl zwischen Linksklick, Rechtsklick oder Klick mit mittlerer Taste.\r\n" +
                    "   - Klicktyp: Auswahl zwischen Einfach- oder Doppel-Klick.\r\n" +
                    "   - Current Cursor-Position: Klickt genau dort, wo sich die Maus gerade befindet.\r\n" +
                    "   - Spezifische Anwendung anvisieren: Klickt gezielt in ein bestimmtes Fenster/Prozess.\r\n\r\n" +
                    "6. ZIELANWENDUNGS-DETAILS\r\n" +
                    "   - Klick-Modus:\r\n" +
                    "     * Foreground: Holt das Fenster in den Vordergrund und klickt (sehr zuverlässig).\r\n" +
                    "     * Background: Sendet Klick-Events im Hintergrund, ohne das Fenster zu fokussieren.\r\n" +
                    "   - Koordinaten-Modus:\r\n" +
                    "     * Window-relative: Die Koordinaten wandern mit, wenn das Fenster verschoben wird.\r\n" +
                    "     * Screen position: Die Koordinaten bleiben an einer festen physischen Bildschirmposition.\r\n" +
                    "   - Pick Location: Klicken, danach über das Zielfenster hovern und klicken, um es automatisch zu verknüpfen.\r\n\r\n" +
                    "7. HOTKEYS\r\n" +
                    "   - Standard Start-Hotkey: F6\r\n" +
                    "   - Standard Stopp-Hotkey: F7\r\n" +
                    "   - Die Tasten können über den 'Hotkeys'-Button geändert werden.";

                txtHelpContent.Text = helpEN;
                btnEN.BackColor = Color.FromArgb(0, 180, 216);

                btnEN.Click += (s, e) =>
                {
                    txtHelpContent.Text = helpEN;
                    btnEN.BackColor = Color.FromArgb(0, 180, 216);
                    btnDE.BackColor = Color.FromArgb(35, 35, 35);
                };

                btnDE.Click += (s, e) =>
                {
                    txtHelpContent.Text = helpDE;
                    btnDE.BackColor = Color.FromArgb(0, 180, 216);
                    btnEN.BackColor = Color.FromArgb(35, 35, 35);
                };

                dialog.Controls.AddRange(new Control[] { topLine, btnEN, btnDE, txtHelpContent, btnClose });
                dialog.AcceptButton = btnClose;
                dialog.ShowDialog(owner);
            }
        }
    }
}
