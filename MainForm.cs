using System;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace MyAutoClicker
{
    public class MainForm : Form
    {
        private ClickerSettings settings = new ClickerSettings();
        private IntPtr activeTargetHWnd = IntPtr.Zero;
        private uint activeTargetPID = 0;
        private bool isProgrammaticTextChange = false;
 
         // UI Components
         private Panel headerPanel = null!;
         private Label titleLabel = null!;
         
         private Panel leftPanel = null!;
         private Label intervalHeader = null!;
         private NumericUpDown numHours = null!;
         private NumericUpDown numMinutes = null!;
         private NumericUpDown numSeconds = null!;
         private NumericUpDown numMilliseconds = null!;
         
         private Label repeatHeader = null!;
         private RadioButton rbInfinite = null!;
         private RadioButton rbTimes = null!;
         private NumericUpDown numRepeatCount = null!;
         
         private Label toleranceHeader = null!;
         private CheckBox chkUseRandomTolerance = null!;
         private NumericUpDown numRandomTolerancePercent = null!;
         private Label lblPercentSign = null!;
         private Label lblTolerancePreview = null!;
 
         private Panel rightPanel = null!;
         private Label optionsHeader = null!;
         private ComboBox comboMouseButton = null!;
         private ComboBox comboClickType = null!;
         
         private Label positionHeader = null!;
         private RadioButton rbCurrentPos = null!;
         private RadioButton rbTargetApp = null!;
 
         private Panel targetPanel = null!;
         private Label targetHeader = null!;
         private TextBox txtTargetApp = null!;
         private TextBox txtTargetTitle = null!;
         private NumericUpDown numTargetX = null!;
         private NumericUpDown numTargetY = null!;
         private Button btnPickTarget = null!;
         private ComboBox comboClickMode = null!;
         private ComboBox comboCoordMode = null!;   // "Window" or "Screen"
         private Label lblCoordsMode = null!;
 
         private Panel controlsPanel = null!;
         private Button btnStart = null!;
         private Button btnStop = null!;
         private Button btnHotkeys = null!;
         private Button btnSaveSettings = null!;
         private Button btnResetSettings = null!;
         private Label lblTargetStatus = null!;
 
         private StatusStrip statusStrip = null!;
         private ToolStripStatusLabel lblStatus = null!;
 
         // Threading & Execution State
         private Thread? clickThread = null;
        private volatile bool isClicking = false;

        // Hotkey Identifiers
        private const int START_HOTKEY_ID = 1;
        private const int STOP_HOTKEY_ID = 2;

        public MainForm()
        {
            InitializeComponent();
            ApplyDarkTitleBar();
            LoadApplicationSettings();
            RegisterGlobalHotkeys();
            UpdateAdminStatus();
        }

        /// <summary>
        /// Colors the native Win32 title bar to match the dark theme using DWM APIs.
        /// Requires Windows 11 (Build 22000+) for DWMWA_CAPTION_COLOR; silently ignored on older Windows.
        /// </summary>
        private void ApplyDarkTitleBar()
        {
            try
            {
                // Enable immersive dark mode (dark title bar text/icons on Win10+)
                int darkMode = 1;
                Win32Helper.DwmSetWindowAttribute(this.Handle, Win32Helper.DWMWA_USE_IMMERSIVE_DARK_MODE,
                    ref darkMode, sizeof(int));

                // Set caption (title bar) background to match our header color: RGB(25,25,25)
                // DWM uses COLORREF = 0x00BBGGRR
                int captionColor = 25 | (25 << 8) | (25 << 16); // #191919
                Win32Helper.DwmSetWindowAttribute(this.Handle, Win32Helper.DWMWA_CAPTION_COLOR,
                    ref captionColor, sizeof(int));

                // Set title text to white: RGB(220,220,220)
                int textColor = 220 | (220 << 8) | (220 << 16);
                Win32Helper.DwmSetWindowAttribute(this.Handle, Win32Helper.DWMWA_TEXT_COLOR,
                    ref textColor, sizeof(int));

                // Set border color to the cyan accent: RGB(0,180,216) → 0x00D8B400
                int borderColor = 0 | (180 << 8) | (216 << 16);
                Win32Helper.DwmSetWindowAttribute(this.Handle, Win32Helper.DWMWA_BORDER_COLOR,
                    ref borderColor, sizeof(int));
            }
            catch
            {
                // DWM not available (older OS or high-contrast mode) — ignore silently
            }
        }

        private void UpdateAdminStatus()
        {
            bool isAdmin = IsRunningAsAdmin();
            lblStatus.Text = isAdmin ? "Status: Idle (Admin Mode)" : "Status: Idle (Non-Admin - target restricted)";
            if (!isAdmin)
            {
                lblStatus.ForeColor = Color.Orange;
            }
        }

        private bool IsRunningAsAdmin()
        {
            using (var identity = System.Security.Principal.WindowsIdentity.GetCurrent())
            {
                var principal = new System.Security.Principal.WindowsPrincipal(identity);
                return principal.IsInRole(System.Security.Principal.WindowsBuiltInRole.Administrator);
            }
        }

        private void InitializeComponent()
        {
            // Main Form configuration
            this.Text = "Neo Auto Clicker";
            this.ClientSize = new Size(580, 680);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.BackColor = Color.FromArgb(20, 20, 20);
            this.ForeColor = Color.FromArgb(220, 220, 220);
            this.StartPosition = FormStartPosition.CenterScreen;

            // 1. Header Panel
            headerPanel = new Panel { Size = new Size(580, 50), Location = new Point(0, 0), BackColor = Color.FromArgb(25, 25, 25) };
            titleLabel = new Label
            {
                Text = "NEO AUTO CLICKER",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 180, 216),
                Location = new Point(15, 12),
                AutoSize = true
            };
            headerPanel.Controls.Add(titleLabel);

            // ℹ About — Label is used instead of Button to avoid content clipping in the header
            var btnAbout = new Label
            {
                Text = "ℹ",
                Size = new Size(30, 30),
                Location = new Point(580 - 38, 10),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.FromArgb(0, 180, 216),
                BackColor = Color.FromArgb(25, 25, 25),
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                Cursor = Cursors.Hand,
                TabStop = false,
                TextAlign = ContentAlignment.MiddleCenter
            };
            btnAbout.Click += (s, e) => ShowAboutDialog();
            btnAbout.MouseEnter += (s, e) => btnAbout.BackColor = Color.FromArgb(42, 42, 42);
            btnAbout.MouseLeave += (s, e) => btnAbout.BackColor = Color.FromArgb(25, 25, 25);
            headerPanel.Controls.Add(btnAbout);

            headerPanel.Paint += (s, e) =>
            {
                // Bottom border line for header
                e.Graphics.DrawLine(new Pen(Color.FromArgb(0, 180, 216), 2), 0, 49, 580, 49);
            };

            // 2. Left Panel (Interval & Repeat)
            leftPanel = new Panel { Size = new Size(255, 340), Location = new Point(15, 65), BackColor = Color.FromArgb(28, 28, 28) };
            AddPanelBorder(leftPanel);

            intervalHeader = new Label
            {
                Text = "CLICK INTERVAL",
                Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 180, 216),
                Location = new Point(15, 15),
                AutoSize = true
            };

            var lblHrs = new Label { Text = "Hours:", Location = new Point(15, 45), AutoSize = true, ForeColor = Color.White };
            numHours = CreateNumericInput(15, 65, 95, 0, 99);

            var lblMins = new Label { Text = "Minutes:", Location = new Point(130, 45), AutoSize = true, ForeColor = Color.White };
            numMinutes = CreateNumericInput(130, 65, 95, 0, 59);

            var lblSecs = new Label { Text = "Seconds:", Location = new Point(15, 105), AutoSize = true, ForeColor = Color.White };
            numSeconds = CreateNumericInput(15, 125, 95, 0, 59);

            var lblMs = new Label { Text = "Milliseconds:", Location = new Point(130, 105), AutoSize = true, ForeColor = Color.White };
            numMilliseconds = CreateNumericInput(130, 125, 95, 0, 99999);

            repeatHeader = new Label
            {
                Text = "CLICK REPEAT",
                Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 180, 216),
                Location = new Point(15, 185),
                AutoSize = true
            };

            rbInfinite = new RadioButton
            {
                Text = "Infinite (Until stopped)",
                Location = new Point(15, 210),
                Size = new Size(220, 26),
                Checked = true,
                ForeColor = Color.White
            };

            rbTimes = new RadioButton
            {
                Text = "Repeat count:",
                Location = new Point(15, 240),
                Size = new Size(110, 26),
                ForeColor = Color.White
            };

            numRepeatCount = CreateNumericInput(130, 238, 95, 1, 999999);
            numRepeatCount.Value = 10;
            numRepeatCount.Enabled = false;

            rbTimes.CheckedChanged += (s, e) => numRepeatCount.Enabled = rbTimes.Checked;
            rbInfinite.CheckedChanged += (s, e) => numRepeatCount.Enabled = !rbInfinite.Checked;

            toleranceHeader = new Label
            {
                Text = "HUMAN TOLERANCE",
                Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 180, 216),
                Location = new Point(15, 275),
                AutoSize = true
            };

            chkUseRandomTolerance = new CheckBox
            {
                Text = "Enable:",
                Location = new Point(15, 300),
                Size = new Size(70, 26),
                ForeColor = Color.White
            };

            numRandomTolerancePercent = CreateNumericInput(85, 298, 55, 0, 99);
            numRandomTolerancePercent.Value = 10;
            numRandomTolerancePercent.Enabled = false;

            lblPercentSign = new Label
            {
                Text = "%",
                Location = new Point(142, 301),
                AutoSize = true,
                ForeColor = Color.White
            };

            lblTolerancePreview = new Label
            {
                Text = "(+/- 0 ms)",
                Location = new Point(160, 301),
                AutoSize = true,
                ForeColor = Color.FromArgb(155, 155, 155)
            };

            chkUseRandomTolerance.CheckedChanged += (s, e) => {
                numRandomTolerancePercent.Enabled = chkUseRandomTolerance.Checked;
                UpdateTolerancePreview();
            };
            numRandomTolerancePercent.ValueChanged += (s, e) => UpdateTolerancePreview();
            numHours.ValueChanged += (s, e) => UpdateTolerancePreview();
            numMinutes.ValueChanged += (s, e) => UpdateTolerancePreview();
            numSeconds.ValueChanged += (s, e) => UpdateTolerancePreview();
            numMilliseconds.ValueChanged += (s, e) => UpdateTolerancePreview();

            leftPanel.Controls.AddRange(new Control[] {
                intervalHeader, lblHrs, numHours, lblMins, numMinutes, lblSecs, numSeconds, lblMs, numMilliseconds,
                repeatHeader, rbInfinite, rbTimes, numRepeatCount,
                toleranceHeader, chkUseRandomTolerance, numRandomTolerancePercent, lblPercentSign, lblTolerancePreview
            });

            // 3. Right Panel (Options & Position selection)
            rightPanel = new Panel { Size = new Size(280, 340), Location = new Point(285, 65), BackColor = Color.FromArgb(28, 28, 28) };
            AddPanelBorder(rightPanel);

            optionsHeader = new Label
            {
                Text = "CLICK OPTIONS",
                Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 180, 216),
                Location = new Point(15, 15),
                AutoSize = true
            };

            var lblBtn = new Label { Text = "Mouse Button:", Location = new Point(15, 45), AutoSize = true, ForeColor = Color.White };
            comboMouseButton = CreateComboBox(15, 65, 250);
            comboMouseButton.Items.AddRange(new object[] { "Left", "Right", "Middle" });
            comboMouseButton.SelectedIndex = 0;

            var lblType = new Label { Text = "Click Type:", Location = new Point(15, 105), AutoSize = true, ForeColor = Color.White };
            comboClickType = CreateComboBox(15, 125, 250);
            comboClickType.Items.AddRange(new object[] { "Single", "Double" });
            comboClickType.SelectedIndex = 0;

            positionHeader = new Label
            {
                Text = "CLICK POSITION",
                Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 180, 216),
                Location = new Point(15, 185),
                AutoSize = true
            };

            rbCurrentPos = new RadioButton
            {
                Text = "Current Cursor Position",
                Location = new Point(15, 210),
                Size = new Size(250, 26),   // 26px: prevents descenders (p, g, y) from being clipped
                Checked = true,
                ForeColor = Color.White
            };

            rbTargetApp = new RadioButton
            {
                Text = "Target Specific Application",
                Location = new Point(15, 240),
                Size = new Size(250, 26),   // 26px: prevents descenders (p, g, y) from being clipped
                ForeColor = Color.White
            };

            rbTargetApp.CheckedChanged += (s, e) => {
                UpdateTargetPanelUI();
                UpdateTargetStatus();
            };
            rbCurrentPos.CheckedChanged += (s, e) => UpdateTargetStatus();

            rightPanel.Controls.AddRange(new Control[] {
                optionsHeader, lblBtn, comboMouseButton, lblType, comboClickType,
                positionHeader, rbCurrentPos, rbTargetApp
            });

            // 4. Target Panel (Target details enabled when rbTargetApp is checked)
            targetPanel = new Panel { Size = new Size(550, 175), Location = new Point(15, 415), BackColor = Color.FromArgb(28, 28, 28) };
            AddPanelBorder(targetPanel);

            targetHeader = new Label
            {
                Text = "TARGET APPLICATION DETAILS",
                Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 180, 216),
                Location = new Point(15, 12),
                AutoSize = true
            };

            var lblProc = new Label { Text = "Process:", Location = new Point(15, 35), AutoSize = true, ForeColor = Color.White };
            txtTargetApp = new TextBox
            {
                Location = new Point(95, 32),
                Size = new Size(270, 23),
                BackColor = Color.FromArgb(40, 40, 40),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            var lblTitle = new Label { Text = "Title:", Location = new Point(15, 65), AutoSize = true, ForeColor = Color.White };
            txtTargetTitle = new TextBox
            {
                Location = new Point(95, 62),
                Size = new Size(270, 23),
                BackColor = Color.FromArgb(40, 40, 40),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            // Coord label shortened to "X/Y:" so the row stays uncluttered
            var lblCoords = new Label { Text = "X/Y:", Location = new Point(15, 97), AutoSize = true, ForeColor = Color.White };
            numTargetX = CreateNumericInput(95, 92, 70, -99999, 99999);
            numTargetY = CreateNumericInput(175, 92, 70, -99999, 99999);

            lblCoordsMode = new Label
            {
                Text = "(window-relative)",
                Font = new Font("Segoe UI", 7.5F, FontStyle.Regular),
                ForeColor = Color.FromArgb(155, 155, 155),  // visible but subdued
                Location = new Point(95, 117),              // below the spinners, not beside
                AutoSize = true
            };

            lblTargetStatus = new Label
            {
                Text = "Status: Using current cursor position",
                Font = new Font("Segoe UI", 8.5F, FontStyle.Regular), // no italic — better readability
                ForeColor = Color.FromArgb(185, 185, 185),            // was Color.Gray (128) — much more readable
                Location = new Point(15, 140),
                Size = new Size(360, 22)
            };

            // --- Right column (x=390): Click Mode, Coord Mode, Pick Location ---
            var lblMode = new Label { Text = "Click Mode:", Location = new Point(390, 12), Size = new Size(145, 18), ForeColor = Color.White };
            comboClickMode = CreateComboBox(390, 30, 145);
            comboClickMode.Items.AddRange(new object[] { "Background", "Foreground" });
            comboClickMode.SelectedIndex = 1;

            var lblCoordModeLabel = new Label { Text = "Coord Mode:", Location = new Point(390, 60), Size = new Size(145, 18), ForeColor = Color.White };
            comboCoordMode = CreateComboBox(390, 78, 145);
            comboCoordMode.Items.AddRange(new object[] { "Window-relative", "Screen position" });
            comboCoordMode.SelectedIndex = 0;
            comboCoordMode.SelectedIndexChanged += (s, e) => {
                bool isScreen = comboCoordMode.SelectedIndex == 1;
                lblCoordsMode.Text = isScreen ? "(screen coords)" : "(window-relative)";
            };

            btnPickTarget = new Button { Text = "Pick Location", Location = new Point(390, 115), Size = new Size(145, 45) };
            ApplyModernStyle(btnPickTarget);
            btnPickTarget.Click += BtnPickTarget_Click;

            txtTargetApp.TextChanged += (s, e) => {
                if (!isProgrammaticTextChange)
                {
                    activeTargetHWnd = IntPtr.Zero;
                    activeTargetPID = 0;
                }
                UpdateTargetStatus();
            };

            txtTargetTitle.TextChanged += (s, e) => {
                if (!isProgrammaticTextChange)
                {
                    activeTargetHWnd = IntPtr.Zero;
                    activeTargetPID = 0;
                }
                UpdateTargetStatus();
            };

            targetPanel.Controls.AddRange(new Control[] {
                targetHeader, lblProc, txtTargetApp, lblTitle, txtTargetTitle,
                lblCoords, numTargetX, numTargetY, lblCoordsMode, lblTargetStatus,
                lblMode, comboClickMode, lblCoordModeLabel, comboCoordMode, btnPickTarget
            });

            // 5. Control Buttons Panel (Bottom)
            controlsPanel = new Panel { Size = new Size(550, 45), Location = new Point(15, 605) };
            
            btnStart       = new Button { Text = "Start (F6)",    Location = new Point(0,   0), Size = new Size(105, 38) };
            ApplyModernStyle(btnStart);
            btnStart.Click += (s, e) => StartClicking();

            btnStop        = new Button { Text = "Stop (F7)",     Location = new Point(112, 0), Size = new Size(105, 38), Enabled = false };
            ApplyModernStyle(btnStop);
            btnStop.ForeColor = Color.FromArgb(90, 90, 90); // starts disabled — dim color
            btnStop.EnabledChanged += (s, e) =>
                btnStop.ForeColor = btnStop.Enabled ? Color.White : Color.FromArgb(90, 90, 90);
            btnStop.Click += (s, e) => StopClicking();

            btnHotkeys     = new Button { Text = "Hotkeys",       Location = new Point(224, 0), Size = new Size(100, 38) };
            ApplyModernStyle(btnHotkeys);
            btnHotkeys.Click += (s, e) => ShowHotkeyDialog();

            btnSaveSettings = new Button { Text = "Save Settings", Location = new Point(331, 0), Size = new Size(105, 38) };
            ApplyModernStyle(btnSaveSettings);
            btnSaveSettings.Click += (s, e) => SaveApplicationSettings();

            btnResetSettings = new Button { Text = "Reset",        Location = new Point(443, 0), Size = new Size(107, 38) };
            ApplyModernStyle(btnResetSettings);
            btnResetSettings.Click += (s, e) => ResetApplicationSettings();

            controlsPanel.Controls.AddRange(new Control[] { btnStart, btnStop, btnHotkeys, btnSaveSettings, btnResetSettings });

            // 6. Status Strip
            statusStrip = new StatusStrip { BackColor = Color.FromArgb(25, 25, 25), SizingGrip = false };
            lblStatus = new ToolStripStatusLabel { Text = "Status: Idle", ForeColor = Color.FromArgb(180, 180, 180) };
            statusStrip.Items.Add(lblStatus);

            // Add all controls to form
            this.Controls.AddRange(new Control[] { headerPanel, leftPanel, rightPanel, targetPanel, controlsPanel, statusStrip });

            // Set up form closing clean up
            this.FormClosing += (s, e) => {
                StopClicking();
                UnregisterGlobalHotkeys();
                SaveApplicationSettings();
            };
        }

        private NumericUpDown CreateNumericInput(int x, int y, int width, int min, int max)
        {
            var num = new NumericUpDown
            {
                Location = new Point(x, y),
                Width = width,
                Minimum = min,
                Maximum = max,
                BackColor = Color.FromArgb(40, 40, 40),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };
            return num;
        }

        private ComboBox CreateComboBox(int x, int y, int width)
        {
            var combo = new ComboBox
            {
                Location = new Point(x, y),
                Width = width,
                BackColor = Color.FromArgb(40, 40, 40),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                DropDownStyle = ComboBoxStyle.DropDownList,
                // Explicit font guarantees identical height on all combos regardless of DPI/theme
                Font = new Font("Segoe UI", 9F)
            };
            return combo;
        }

        private void AddPanelBorder(Panel p)
        {
            p.Paint += (s, e) => {
                ControlPaint.DrawBorder(e.Graphics, p.ClientRectangle, Color.FromArgb(45, 45, 45), ButtonBorderStyle.Solid);
            };
        }

        private void ApplyModernStyle(Button btn)
        {
            btn.FlatStyle = FlatStyle.Flat;
            btn.ForeColor = Color.White;
            btn.BackColor = Color.FromArgb(35, 35, 35);
            btn.FlatAppearance.BorderColor = Color.FromArgb(60, 60, 60);
            btn.FlatAppearance.BorderSize = 1;
            btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(0, 180, 216);
            btn.FlatAppearance.MouseDownBackColor = Color.FromArgb(0, 150, 180);
            btn.Font = new Font("Segoe UI", 9F, FontStyle.Bold);

            // Custom Paint: WinForms draws disabled-button text as near-black on dark themes.
            // We override it to draw explicit gray text with the correct dark background.
            btn.Paint += (s, e) =>
            {
                if (btn.Enabled) return;   // let WinForms handle the normal (enabled) state

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

        // --- Settings Operations ---
        private void LoadApplicationSettings()
        {
            settings = SettingsManager.LoadSettings();

            // Apply to UI
            numHours.Value = settings.Hours;
            numMinutes.Value = settings.Minutes;
            numSeconds.Value = settings.Seconds;
            numMilliseconds.Value = settings.Milliseconds;
            chkUseRandomTolerance.Checked = settings.UseRandomTolerance;
            numRandomTolerancePercent.Value = settings.RandomTolerancePercent;
            numRandomTolerancePercent.Enabled = settings.UseRandomTolerance;
            UpdateTolerancePreview();

            rbInfinite.Checked = settings.InfiniteRepeat;
            rbTimes.Checked = !settings.InfiniteRepeat;
            numRepeatCount.Value = settings.RepeatCount;
            numRepeatCount.Enabled = !settings.InfiniteRepeat;

            comboMouseButton.SelectedItem = settings.MouseButton;
            comboClickType.SelectedItem = settings.ClickType;

            if (string.IsNullOrEmpty(settings.ProcessName) && string.IsNullOrEmpty(settings.WindowTitle))
            {
                rbCurrentPos.Checked = true;
                rbTargetApp.Checked = false;
            }
            else
            {
                rbCurrentPos.Checked = settings.ClickMode == "CurrentCursor"; // Custom toggle logic
                rbTargetApp.Checked = !rbCurrentPos.Checked;

                isProgrammaticTextChange = true;
                txtTargetApp.Text = settings.ProcessName;
                txtTargetTitle.Text = settings.WindowTitle;
                isProgrammaticTextChange = false;

                numTargetX.Value = settings.RelativeX;
                numTargetY.Value = settings.RelativeY;
                
                int modeIdx = comboClickMode.FindStringExact(settings.ClickMode);
                if (modeIdx >= 0) comboClickMode.SelectedIndex = modeIdx;

                // CoordMode
                bool isScreenMode = settings.CoordMode == "Screen";
                comboCoordMode.SelectedIndex = isScreenMode ? 1 : 0;
                lblCoordsMode.Text = isScreenMode ? "(screen coords)" : "(window-relative)";
            }
            UpdateTargetPanelUI();
            UpdateTargetStatus();

            btnStart.Text = $"Start ({settings.StartHotkey})";
            btnStop.Text = $"Stop ({settings.StopHotkey})";
            
            lblStatus.Text = "Settings loaded.";
        }

        private void SaveApplicationSettings()
        {
            // Read from UI
            settings.Hours = (int)numHours.Value;
            settings.Minutes = (int)numMinutes.Value;
            settings.Seconds = (int)numSeconds.Value;
            settings.Milliseconds = (int)numMilliseconds.Value;
            settings.UseRandomTolerance = chkUseRandomTolerance.Checked;
            settings.RandomTolerancePercent = (int)numRandomTolerancePercent.Value;

            settings.InfiniteRepeat = rbInfinite.Checked;
            settings.RepeatCount = (int)numRepeatCount.Value;

            settings.MouseButton = comboMouseButton.SelectedItem?.ToString() ?? "Left";
            settings.ClickType = comboClickType.SelectedItem?.ToString() ?? "Single";

            settings.ProcessName = txtTargetApp.Text;
            settings.WindowTitle = txtTargetTitle.Text;
            settings.RelativeX = (int)numTargetX.Value;
            settings.RelativeY = (int)numTargetY.Value;
            settings.CoordMode = (comboCoordMode.SelectedIndex == 1) ? "Screen" : "Window";

            if (rbTargetApp.Checked)
            {
                settings.ClickMode = comboClickMode.SelectedItem?.ToString() ?? "Background";
            }
            else
            {
                settings.ClickMode = "CurrentCursor";
            }

            SettingsManager.SaveSettings(settings);
            lblStatus.Text = "Settings saved successfully.";
        }

        private void ResetApplicationSettings()
        {
            settings = new ClickerSettings();
            
            numHours.Value = settings.Hours;
            numMinutes.Value = settings.Minutes;
            numSeconds.Value = settings.Seconds;
            numMilliseconds.Value = settings.Milliseconds;
            chkUseRandomTolerance.Checked = settings.UseRandomTolerance;
            numRandomTolerancePercent.Value = settings.RandomTolerancePercent;
            numRandomTolerancePercent.Enabled = settings.UseRandomTolerance;
            UpdateTolerancePreview();

            rbInfinite.Checked = settings.InfiniteRepeat;
            rbTimes.Checked = !settings.InfiniteRepeat;
            numRepeatCount.Value = settings.RepeatCount;

            comboMouseButton.SelectedIndex = 0;
            comboClickType.SelectedIndex = 0;

            rbCurrentPos.Checked = true;
            UpdateTargetPanelUI();

            isProgrammaticTextChange = true;
            txtTargetApp.Text = string.Empty;
            txtTargetTitle.Text = string.Empty;
            isProgrammaticTextChange = false;

            numTargetX.Value = 0;
            numTargetY.Value = 0;
            comboClickMode.SelectedIndex = 1;
            comboCoordMode.SelectedIndex = 0;
            lblCoordsMode.Text = "(window-relative)";

            UnregisterGlobalHotkeys();
            settings.StartHotkey = Keys.F6;
            settings.StopHotkey = Keys.F7;
            RegisterGlobalHotkeys();

            btnStart.Text = $"Start ({settings.StartHotkey})";
            btnStop.Text = $"Stop ({settings.StopHotkey})";

            UpdateTargetStatus();
            lblStatus.Text = "Settings reset to defaults.";
        }

        // --- Window / Location Picker ---
        private void BtnPickTarget_Click(object? sender, EventArgs e)
        {
            this.Hide();
            Thread.Sleep(250); // let main window hide completely

            using (var selector = new TargetSelectorForm())
            {
                bool screenMode = comboCoordMode.SelectedIndex == 1;
                selector.UseScreenCoords = screenMode;

                if (selector.ShowDialog(this) == DialogResult.OK)
                {
                    activeTargetHWnd = selector.TargetHWnd;
                    Win32Helper.GetWindowThreadProcessId(activeTargetHWnd, out activeTargetPID);

                    settings.ProcessName = selector.ProcessName;
                    settings.WindowTitle = selector.WindowTitle;
                    settings.WindowClassName = selector.WindowClassName;
                    settings.RelativeX = selector.RelativeCoordinate.X;
                    settings.RelativeY = selector.RelativeCoordinate.Y;
                    settings.CoordMode = screenMode ? "Screen" : "Window";

                    isProgrammaticTextChange = true;
                    txtTargetApp.Text = selector.ProcessName;
                    txtTargetTitle.Text = selector.WindowTitle;
                    isProgrammaticTextChange = false;

                    numTargetX.Value = selector.RelativeCoordinate.X;
                    numTargetY.Value = selector.RelativeCoordinate.Y;
                    lblCoordsMode.Text = screenMode ? "(screen coords)" : "(window-relative)";

                    lblStatus.Text = screenMode
                        ? "Target selected (screen position fixed)."
                        : "Target selected (window-relative, follows window).";
                }
                else
                {
                    lblStatus.Text = "Picking cancelled.";
                }
            }

            this.Show();
        }

        // --- Clicker Execution Loop ---
        private void StartClicking()
        {
            if (isClicking) return;

            // Apply quick validations
            int intervalMs = ((int)numHours.Value * 3600000) + 
                             ((int)numMinutes.Value * 60000) + 
                             ((int)numSeconds.Value * 1000) + 
                             (int)numMilliseconds.Value;
            if (intervalMs <= 0)
            {
                MessageBox.Show("Click interval must be greater than 0.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Sync settings temporarily
            settings.Hours = (int)numHours.Value;
            settings.Minutes = (int)numMinutes.Value;
            settings.Seconds = (int)numSeconds.Value;
            settings.Milliseconds = (int)numMilliseconds.Value;
            settings.UseRandomTolerance = chkUseRandomTolerance.Checked;
            settings.RandomTolerancePercent = (int)numRandomTolerancePercent.Value;
            settings.InfiniteRepeat = rbInfinite.Checked;
            settings.RepeatCount = (int)numRepeatCount.Value;
            settings.MouseButton = comboMouseButton.SelectedItem?.ToString() ?? "Left";
            settings.ClickType = comboClickType.SelectedItem?.ToString() ?? "Single";
            if (rbTargetApp.Checked)
            {
                settings.ClickMode = comboClickMode.SelectedItem?.ToString() ?? "Background";
                settings.ProcessName = txtTargetApp.Text;
                settings.WindowTitle = txtTargetTitle.Text;
                settings.RelativeX = (int)numTargetX.Value;
                settings.RelativeY = (int)numTargetY.Value;
            }

            isClicking = true;
            btnStart.Enabled = false;
            btnStop.Enabled = true;
            lblStatus.Text = "Status: Clicking...";

            clickThread = new Thread(ClickLoop)
            {
                IsBackground = true
            };
            clickThread.Start();
        }

        private void StopClicking()
        {
            if (!isClicking) return;

            isClicking = false;
            if (clickThread != null && clickThread.IsAlive)
            {
                clickThread.Join(200); // Wait briefly for clean stop
            }

            btnStart.Enabled = true;
            btnStop.Enabled = false;
            lblStatus.Text = "Status: Idle";
        }

        private void ClickLoop()
        {
            int clicksPerformed = 0;
            int intervalMs = (settings.Hours * 3600000) + 
                             (settings.Minutes * 60000) + 
                             (settings.Seconds * 1000) + 
                             settings.Milliseconds;
            if (intervalMs <= 0) intervalMs = 1;

            Random rand = new Random();

            while (isClicking)
            {
                IntPtr targetHwnd = IntPtr.Zero;
                IntPtr topLevelHwnd = IntPtr.Zero;

                if (rbTargetApp.Checked)
                {
                    targetHwnd = ResolveTargetWindow();
                    if (targetHwnd == IntPtr.Zero)
                    {
                        this.BeginInvoke(new Action(() =>
                        {
                            StopClicking();
                            MessageBox.Show("Target window not found! Please check that the application is running.", "Target Lost", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }));
                        break;
                    }
                    topLevelHwnd = Win32Helper.GetAncestor(targetHwnd, Win32Helper.GA_ROOT);
                    if (topLevelHwnd == IntPtr.Zero) topLevelHwnd = targetHwnd;
                }

                // Simulate
                try
                {
                    if (rbCurrentPos.Checked)
                    {
                        DoForegroundClickAtCurrentPos();
                    }
                    else
                    {
                        if (settings.ClickMode == "Background")
                        {
                            DoBackgroundClick(targetHwnd, settings.RelativeX, settings.RelativeY, settings.MouseButton, settings.ClickType);
                        }
                        else
                        {
                            DoForegroundClickAtWindow(targetHwnd, topLevelHwnd, settings.RelativeX, settings.RelativeY, settings.MouseButton, settings.ClickType);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Click simulation error: {ex.Message}");
                }

                clicksPerformed++;

                if (!settings.InfiniteRepeat && clicksPerformed >= settings.RepeatCount)
                {
                    this.BeginInvoke(new Action(() => StopClicking()));
                    break;
                }

                // Smooth responsive sleeping
                int currentIntervalMs = intervalMs;
                if (settings.UseRandomTolerance && settings.RandomTolerancePercent > 0)
                {
                    int maxToleranceMs = (int)Math.Round((double)intervalMs * settings.RandomTolerancePercent / 100.0);
                    if (maxToleranceMs > 0)
                    {
                        int tolerance = rand.Next(-maxToleranceMs, maxToleranceMs + 1);
                        currentIntervalMs = Math.Max(1, intervalMs + tolerance);
                    }
                }

                int elapsed = 0;
                const int sleepChunk = 25;
                while (isClicking && elapsed < currentIntervalMs)
                {
                    int toSleep = Math.Min(sleepChunk, currentIntervalMs - elapsed);
                    Thread.Sleep(toSleep);
                    elapsed += toSleep;
                }
            }
        }

        private void UpdateTolerancePreview()
        {
            if (chkUseRandomTolerance == null || numRandomTolerancePercent == null || lblTolerancePreview == null)
                return;

            if (!chkUseRandomTolerance.Checked)
            {
                lblTolerancePreview.Text = "(+/- 0 ms)";
                return;
            }

            int intervalMs = ((int)numHours.Value * 3600000) + 
                             ((int)numMinutes.Value * 60000) + 
                             ((int)numSeconds.Value * 1000) + 
                             (int)numMilliseconds.Value;

            int pct = (int)numRandomTolerancePercent.Value;
            int toleranceMs = (int)Math.Round((double)intervalMs * pct / 100.0);
            lblTolerancePreview.Text = $"(+/- {toleranceMs} ms)";
        }

        private IntPtr FindWindowByProcessName(string processName, string windowTitle)
        {
            IntPtr bestMatch = IntPtr.Zero;
            bool processSpecified = !string.IsNullOrEmpty(processName);
            bool titleSpecified = !string.IsNullOrEmpty(windowTitle);

            if (!processSpecified && !titleSpecified)
                return IntPtr.Zero;

            // 1. Try to find a window matching both criteria (visible and root-level)
            Win32Helper.EnumWindows((hWnd, lParam) =>
            {
                IntPtr root = Win32Helper.GetAncestor(hWnd, Win32Helper.GA_ROOT);
                if (root == hWnd && Win32Helper.IsWindowVisible(hWnd))
                {
                    bool procMatch = true;
                    if (processSpecified)
                    {
                        string actualProc = Win32Helper.GetProcessNameFromWindow(hWnd);
                        if (string.IsNullOrEmpty(actualProc) || 
                            actualProc.IndexOf(processName, StringComparison.OrdinalIgnoreCase) < 0)
                        {
                            procMatch = false;
                        }
                    }

                    bool titleMatch = true;
                    if (titleSpecified)
                    {
                        string actualTitle = Win32Helper.GetWindowTitle(hWnd);
                        if (string.IsNullOrEmpty(actualTitle) || 
                            actualTitle.IndexOf(windowTitle, StringComparison.OrdinalIgnoreCase) < 0)
                        {
                            titleMatch = false;
                        }
                    }

                    if (procMatch && titleMatch)
                    {
                        bestMatch = hWnd;
                        return false; // Stop enum
                    }
                }
                return true; // Continue
            }, IntPtr.Zero);

            // 2. Fallback: if we specified both but failed, check if we can match by Title alone (visible and root-level)
            if (bestMatch == IntPtr.Zero && processSpecified && titleSpecified)
            {
                Win32Helper.EnumWindows((hWnd, lParam) =>
                {
                    IntPtr root = Win32Helper.GetAncestor(hWnd, Win32Helper.GA_ROOT);
                    if (root == hWnd && Win32Helper.IsWindowVisible(hWnd))
                    {
                        string actualTitle = Win32Helper.GetWindowTitle(hWnd);
                        if (!string.IsNullOrEmpty(actualTitle) && 
                            actualTitle.IndexOf(windowTitle, StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            bestMatch = hWnd;
                            return false; // Stop enum
                        }
                    }
                    return true;
                }, IntPtr.Zero);
            }

            // 3. Fallback: if we specified both but failed, check if we can match by Process Name alone (visible and root-level)
            if (bestMatch == IntPtr.Zero && processSpecified && titleSpecified)
            {
                Win32Helper.EnumWindows((hWnd, lParam) =>
                {
                    IntPtr root = Win32Helper.GetAncestor(hWnd, Win32Helper.GA_ROOT);
                    if (root == hWnd && Win32Helper.IsWindowVisible(hWnd))
                    {
                        string actualProc = Win32Helper.GetProcessNameFromWindow(hWnd);
                        if (!string.IsNullOrEmpty(actualProc) && 
                            actualProc.IndexOf(processName, StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            bestMatch = hWnd;
                            return false; // Stop enum
                        }
                    }
                    return true;
                }, IntPtr.Zero);
            }

            // 4. Absolute Fallback: Search all windows (even invisible/child windows) by Title alone
            if (bestMatch == IntPtr.Zero && titleSpecified)
            {
                Win32Helper.EnumWindows((hWnd, lParam) =>
                {
                    string actualTitle = Win32Helper.GetWindowTitle(hWnd);
                    if (!string.IsNullOrEmpty(actualTitle) && 
                        actualTitle.IndexOf(windowTitle, StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        bestMatch = hWnd;
                        return false;
                    }
                    return true;
                }, IntPtr.Zero);
            }

            // 5. Absolute Fallback: Search all windows by Process Name alone
            if (bestMatch == IntPtr.Zero && processSpecified)
            {
                Win32Helper.EnumWindows((hWnd, lParam) =>
                {
                    string actualProc = Win32Helper.GetProcessNameFromWindow(hWnd);
                    if (!string.IsNullOrEmpty(actualProc) && 
                        actualProc.IndexOf(processName, StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        bestMatch = hWnd;
                        return false;
                    }
                    return true;
                }, IntPtr.Zero);
            }

            return bestMatch;
        }

        private IntPtr ResolveTargetWindow()
        {
            // 1. If we have a cached handle, check if it's still valid
            if (activeTargetHWnd != IntPtr.Zero && Win32Helper.IsWindow(activeTargetHWnd))
            {
                // Verify the process ID is still the same (prevent recycled handle issues)
                Win32Helper.GetWindowThreadProcessId(activeTargetHWnd, out uint currentPid);
                if (currentPid == activeTargetPID && activeTargetPID != 0)
                {
                    return activeTargetHWnd;
                }
            }

            // 2. If no valid cached handle, resolve by Title and/or Process Name
            if (string.IsNullOrEmpty(settings.ProcessName) && string.IsNullOrEmpty(settings.WindowTitle))
                return IntPtr.Zero;

            IntPtr mainHwnd = FindWindowByProcessName(settings.ProcessName, settings.WindowTitle);
            if (mainHwnd != IntPtr.Zero)
            {
                // Update cached top-level handle and PID
                activeTargetHWnd = mainHwnd;
                Win32Helper.GetWindowThreadProcessId(activeTargetHWnd, out activeTargetPID);

                // If selected child window class was set, try to find the matching child
                if (!string.IsNullOrEmpty(settings.WindowClassName))
                {
                    string mainClass = Win32Helper.GetWindowClassName(mainHwnd);
                    if (mainClass != settings.WindowClassName)
                    {
                        IntPtr childHwnd = FindChildWindow(mainHwnd, settings.WindowClassName);
                        if (childHwnd != IntPtr.Zero)
                        {
                            activeTargetHWnd = childHwnd;
                            Win32Helper.GetWindowThreadProcessId(activeTargetHWnd, out activeTargetPID);
                            return childHwnd;
                        }
                    }
                }
                return mainHwnd;
            }

            return IntPtr.Zero;
        }

        private IntPtr FindChildWindow(IntPtr parent, string className)
        {
            IntPtr result = IntPtr.Zero;
            Win32Helper.EnumChildWindows(parent, (hWnd, lParam) =>
            {
                if (Win32Helper.GetWindowClassName(hWnd) == className)
                {
                    result = hWnd;
                    return false; // Stop enum
                }
                return true; // Continue
            }, IntPtr.Zero);
            return result;
        }

        private void DoForegroundClickAtCurrentPos()
        {
            uint downFlag = Win32Helper.MOUSEEVENTF_LEFTDOWN;
            uint upFlag = Win32Helper.MOUSEEVENTF_LEFTUP;

            if (settings.MouseButton == "Right")
            {
                downFlag = Win32Helper.MOUSEEVENTF_RIGHTDOWN;
                upFlag = Win32Helper.MOUSEEVENTF_RIGHTUP;
            }
            else if (settings.MouseButton == "Middle")
            {
                downFlag = Win32Helper.MOUSEEVENTF_MIDDLEDOWN;
                upFlag = Win32Helper.MOUSEEVENTF_MIDDLEUP;
            }

            try
            {
                SendMouseInput(downFlag);
                Thread.Sleep(10);
            }
            finally
            {
                SendMouseInput(upFlag);
            }

            if (settings.ClickType == "Double")
            {
                Thread.Sleep(100);
                try
                {
                    SendMouseInput(downFlag);
                    Thread.Sleep(10);
                }
                finally
                {
                    SendMouseInput(upFlag);
                }
            }
        }

        private void UpdateTargetPanelUI()
        {
            bool enabled = rbTargetApp.Checked;
            btnPickTarget.Enabled = enabled;
            comboClickMode.Enabled = enabled;
            // NOTE: comboCoordMode is intentionally NOT disabled — Coord Mode applies regardless
            // of whether we are targeting a specific app or current cursor position.
            txtTargetApp.Enabled = enabled;
            txtTargetTitle.Enabled = enabled;
            numTargetX.Enabled = enabled;
            numTargetY.Enabled = enabled;

            Color headerColor = enabled ? Color.FromArgb(0, 180, 216) : Color.FromArgb(0, 130, 155);
            targetHeader.ForeColor = headerColor;
        }

        private void UpdateTargetStatus()
        {
            if (lblTargetStatus == null) return;

            if (!rbTargetApp.Checked)
            {
                lblTargetStatus.Text = "Status: Using current cursor position";
                lblTargetStatus.ForeColor = Color.Gray;
                return;
            }

            // If we have a valid cached active window handle, use it for real-time status
            if (activeTargetHWnd != IntPtr.Zero && Win32Helper.IsWindow(activeTargetHWnd))
            {
                string proc = Win32Helper.GetProcessNameFromWindow(activeTargetHWnd);
                string title = Win32Helper.GetWindowTitle(activeTargetHWnd);
                if (string.IsNullOrEmpty(title))
                {
                    // If it's a child window, get parent title
                    IntPtr parent = Win32Helper.GetAncestor(activeTargetHWnd, Win32Helper.GA_ROOT);
                    if (parent != IntPtr.Zero) title = Win32Helper.GetWindowTitle(parent);
                }
                lblTargetStatus.Text = $"Status: Linked to \"{title}\" ({proc}.exe)";
                lblTargetStatus.ForeColor = Color.FromArgb(0, 180, 216); // Cyan
                return;
            }

            string processName = txtTargetApp.Text;
            string windowTitle = txtTargetTitle.Text;

            if (string.IsNullOrEmpty(processName) && string.IsNullOrEmpty(windowTitle))
            {
                lblTargetStatus.Text = "Status: Enter Process Name or Window Title";
                lblTargetStatus.ForeColor = Color.Orange;
                return;
            }

            IntPtr hwnd = FindWindowByProcessName(processName, windowTitle);
            if (hwnd != IntPtr.Zero)
            {
                string proc = Win32Helper.GetProcessNameFromWindow(hwnd);
                string title = Win32Helper.GetWindowTitle(hwnd);
                lblTargetStatus.Text = $"Status: Found \"{title}\" ({proc}.exe)";
                lblTargetStatus.ForeColor = Color.FromArgb(0, 180, 216); // Cyan
            }
            else
            {
                lblTargetStatus.Text = "Status: Target window not found";
                lblTargetStatus.ForeColor = Color.FromArgb(239, 71, 111); // Red/Pink
            }
        }

        /// <summary>
        /// Returns the true physical pixel position of the mouse, bypassing DPI virtualization.
        /// Essential for multi-monitor setups with different DPI/scaling per screen.
        /// </summary>
        private static Point GetPhysicalMousePosition()
        {
            if (Win32Helper.GetPhysicalCursorPos(out Point pt))
                return pt;
            return Cursor.Position; // fallback
        }

        /// <summary>
        /// Returns the physical pixel bounds of the entire virtual desktop (all monitors combined).
        /// Uses SM_CXVIRTUALSCREEN / SM_CYVIRTUALSCREEN via GetSystemMetrics for physical pixels.
        /// </summary>
        private static Rectangle GetPhysicalVirtualScreen()
        {
            // GetSystemMetrics always returns physical pixels when the process is DPI-aware
            int left   = Win32Helper.GetSystemMetrics(Win32Helper.SM_XVIRTUALSCREEN);
            int top    = Win32Helper.GetSystemMetrics(Win32Helper.SM_YVIRTUALSCREEN);
            int width  = Win32Helper.GetSystemMetrics(Win32Helper.SM_CXVIRTUALSCREEN);
            int height = Win32Helper.GetSystemMetrics(Win32Helper.SM_CYVIRTUALSCREEN);
            if (width <= 0 || height <= 0)
                return SystemInformation.VirtualScreen; // safe fallback
            return new Rectangle(left, top, width, height);
        }

        private void SendMouseInputAbsolute(uint dwFlags, int x, int y)
        {
            Rectangle virtualScreen = GetPhysicalVirtualScreen();
            int dx = ((x - virtualScreen.Left) * 65536) / virtualScreen.Width;
            int dy = ((y - virtualScreen.Top) * 65536) / virtualScreen.Height;

            var input = new Win32Helper.INPUT
            {
                type = Win32Helper.INPUT_MOUSE,
                Union = new Win32Helper.MouseKeybdhardwareInput
                {
                    mi = new Win32Helper.MOUSEINPUT
                    {
                        dx = dx,
                        dy = dy,
                        mouseData = 0,
                        dwFlags = (int)(dwFlags | Win32Helper.MOUSEEVENTF_ABSOLUTE | Win32Helper.MOUSEEVENTF_MOVE | Win32Helper.MOUSEEVENTF_VIRTUALDESKTOP),
                        time = 0,
                        dwExtraInfo = IntPtr.Zero
                    }
                }
            };
            Win32Helper.SendInput(1, new[] { input }, Marshal.SizeOf(typeof(Win32Helper.INPUT)));
        }

        private void DoForegroundClickAtWindow(IntPtr targetHwnd, IntPtr topLevelHwnd, int rx, int ry, string button, string type)
        {
            // Step 1: Save physical cursor position and previous foreground window
            Point prevCursorPos = GetPhysicalMousePosition();
            IntPtr prevForeground = Win32Helper.GetForegroundWindow();

            // Step 2: Convert to physical screen coordinates based on the selected mode
            bool screenAbsolute = settings.CoordMode == "Screen";
            Point screenPt;
            if (screenAbsolute)
            {
                // Screen mode: rx/ry ARE already physical screen coords — use them directly.
                // The click always lands on the same fixed screen position regardless of window location.
                screenPt = new Point(rx, ry);
            }
            else
            {
                // Window-relative mode: rx/ry are client coords relative to the target window.
                // ClientToScreen converts using the CURRENT window position at click time,
                // so the click follows the window even if it has been moved.
                screenPt = new Point(rx, ry);
                Win32Helper.ClientToScreen(targetHwnd, ref screenPt);
            }

            Rectangle virtualScreen = GetPhysicalVirtualScreen();
            int targetDx = ((screenPt.X - virtualScreen.Left) * 65536) / virtualScreen.Width;
            int targetDy = ((screenPt.Y - virtualScreen.Top) * 65536) / virtualScreen.Height;
            int restoreDx = ((prevCursorPos.X - virtualScreen.Left) * 65536) / virtualScreen.Width;
            int restoreDy = ((prevCursorPos.Y - virtualScreen.Top) * 65536) / virtualScreen.Height;

            // Step 3: MOVE mouse to the target position on the correct monitor.
            // Separate SendInput call so OS registers the new position before the click.
            var moveInput = MakeMouseInput(targetDx, targetDy,
                Win32Helper.MOUSEEVENTF_ABSOLUTE | Win32Helper.MOUSEEVENTF_MOVE | Win32Helper.MOUSEEVENTF_VIRTUALDESKTOP);
            Win32Helper.SendInput(1, new[] { moveInput }, Marshal.SizeOf(typeof(Win32Helper.INPUT)));

            // Step 4: Let Windows process the move (update hover state, window under cursor)
            Thread.Sleep(8);

            // Step 5: Bring the target window to front now that cursor is already over it.
            Win32Helper.ForceSetForegroundWindow(topLevelHwnd);

            // Step 6: Send the click(s)
            SendClickInputs(targetDx, targetDy, button, type);

            // Step 7: Restore mouse to original position
            var restoreMove = MakeMouseInput(restoreDx, restoreDy,
                Win32Helper.MOUSEEVENTF_ABSOLUTE | Win32Helper.MOUSEEVENTF_MOVE | Win32Helper.MOUSEEVENTF_VIRTUALDESKTOP);
            Win32Helper.SendInput(1, new[] { restoreMove }, Marshal.SizeOf(typeof(Win32Helper.INPUT)));

            // Step 8: Restore focus to previous window
            if (prevForeground != IntPtr.Zero && prevForeground != this.Handle && prevForeground != topLevelHwnd)
                Win32Helper.ForceSetForegroundWindow(prevForeground);
        }

        /// <summary>Builds a single MOUSE INPUT struct.</summary>
        private static Win32Helper.INPUT MakeMouseInput(int dx, int dy, uint flags, uint mouseData = 0)
        {
            return new Win32Helper.INPUT
            {
                type = Win32Helper.INPUT_MOUSE,
                Union = new Win32Helper.MouseKeybdhardwareInput
                {
                    mi = new Win32Helper.MOUSEINPUT
                    {
                        dx = dx, dy = dy,
                        mouseData = (int)mouseData,
                        dwFlags = (int)flags,
                        time = 0,
                        dwExtraInfo = IntPtr.Zero
                    }
                }
            };
        }

        /// <summary>Sends mouse button down/up (and optional double-click) at the given normalized coords.</summary>
        private void SendClickInputs(int dx, int dy, string button, string type)
        {
            uint downFlag = Win32Helper.MOUSEEVENTF_LEFTDOWN;
            uint upFlag   = Win32Helper.MOUSEEVENTF_LEFTUP;
            if (button == "Right")  { downFlag = Win32Helper.MOUSEEVENTF_RIGHTDOWN;  upFlag = Win32Helper.MOUSEEVENTF_RIGHTUP; }
            else if (button == "Middle") { downFlag = Win32Helper.MOUSEEVENTF_MIDDLEDOWN; upFlag = Win32Helper.MOUSEEVENTF_MIDDLEUP; }

            uint absFlags = Win32Helper.MOUSEEVENTF_ABSOLUTE | Win32Helper.MOUSEEVENTF_VIRTUALDESKTOP;

            if (type == "Double")
            {
                var inputs = new[]
                {
                    MakeMouseInput(dx, dy, downFlag | absFlags),
                    MakeMouseInput(dx, dy, upFlag   | absFlags),
                    MakeMouseInput(dx, dy, downFlag | absFlags),
                    MakeMouseInput(dx, dy, upFlag   | absFlags),
                };
                Win32Helper.SendInput((uint)inputs.Length, inputs, Marshal.SizeOf(typeof(Win32Helper.INPUT)));
            }
            else
            {
                var inputs = new[]
                {
                    MakeMouseInput(dx, dy, downFlag | absFlags),
                    MakeMouseInput(dx, dy, upFlag   | absFlags),
                };
                Win32Helper.SendInput((uint)inputs.Length, inputs, Marshal.SizeOf(typeof(Win32Helper.INPUT)));
            }
        }

        private void DoBackgroundClick(IntPtr targetHwnd, int rx, int ry, string button, string type)
        {
            IntPtr lParam = (IntPtr)((ry << 16) | (rx & 0xFFFF));
            uint downMsg = Win32Helper.WM_LBUTTONDOWN;
            uint upMsg = Win32Helper.WM_LBUTTONUP;
            IntPtr wParamDown = (IntPtr)0x0001; // MK_LBUTTON

            if (button == "Right")
            {
                downMsg = Win32Helper.WM_RBUTTONDOWN;
                upMsg = Win32Helper.WM_RBUTTONUP;
                wParamDown = (IntPtr)0x0002;
            }
            else if (button == "Middle")
            {
                downMsg = Win32Helper.WM_MBUTTONDOWN;
                upMsg = Win32Helper.WM_MBUTTONUP;
                wParamDown = (IntPtr)0x0010;
            }

            // 1. Send WM_MOUSEMOVE to target coordinate
            Win32Helper.PostMessage(targetHwnd, 0x0200, IntPtr.Zero, lParam); // 0x0200 = WM_MOUSEMOVE
            Thread.Sleep(5);

            // 2. Send WM_SETCURSOR to update target's internal hover state
            IntPtr setCursorLParam = (IntPtr)((downMsg << 16) | 1); // HTCLIENT = 1
            Win32Helper.PostMessage(targetHwnd, 0x0020, targetHwnd, setCursorLParam); // 0x0020 = WM_SETCURSOR
            Thread.Sleep(5);

            // 3. Send click events
            if (type == "Single")
            {
                Win32Helper.PostMessage(targetHwnd, downMsg, wParamDown, lParam);
                Thread.Sleep(15);
                Win32Helper.PostMessage(targetHwnd, upMsg, IntPtr.Zero, lParam);
            }
            else
            {
                Win32Helper.PostMessage(targetHwnd, downMsg, wParamDown, lParam);
                Thread.Sleep(15);
                Win32Helper.PostMessage(targetHwnd, upMsg, IntPtr.Zero, lParam);
                Thread.Sleep(100);
                
                Win32Helper.PostMessage(targetHwnd, 0x0200, IntPtr.Zero, lParam); // WM_MOUSEMOVE
                Thread.Sleep(5);
                Win32Helper.PostMessage(targetHwnd, downMsg, wParamDown, lParam);
                Thread.Sleep(15);
                Win32Helper.PostMessage(targetHwnd, upMsg, IntPtr.Zero, lParam);
            }
        }

        private void SendMouseInput(uint dwFlags)
        {
            var input = new Win32Helper.INPUT
            {
                type = Win32Helper.INPUT_MOUSE,
                Union = new Win32Helper.MouseKeybdhardwareInput
                {
                    mi = new Win32Helper.MOUSEINPUT
                    {
                        dx = 0,
                        dy = 0,
                        mouseData = 0,
                        dwFlags = (int)dwFlags,
                        time = 0,
                        dwExtraInfo = IntPtr.Zero
                    }
                }
            };
            Win32Helper.SendInput(1, new[] { input }, Marshal.SizeOf(typeof(Win32Helper.INPUT)));
        }

        // --- Hotkey Configuration Sub-Form ---
        private void ShowHotkeyDialog()
        {
            using (Form dialog = new Form())
            {
                dialog.Text = "Configure Hotkeys";
                dialog.Size = new Size(300, 200);
                dialog.FormBorderStyle = FormBorderStyle.FixedDialog;
                dialog.MaximizeBox = false;
                dialog.MinimizeBox = false;
                dialog.StartPosition = FormStartPosition.CenterParent;
                dialog.BackColor = Color.FromArgb(20, 20, 20);
                dialog.ForeColor = Color.White;

                Label lblStart = new Label { Text = "Start Hotkey:", Location = new Point(20, 30), Size = new Size(100, 20), Font = new Font("Segoe UI", 9, FontStyle.Bold) };
                TextBox txtStart = new TextBox
                {
                    Location = new Point(130, 27),
                    Size = new Size(120, 23),
                    BackColor = Color.FromArgb(30, 30, 30),
                    ForeColor = Color.White,
                    BorderStyle = BorderStyle.FixedSingle,
                    ReadOnly = true,
                    Text = settings.StartHotkey.ToString()
                };

                Label lblStop = new Label { Text = "Stop Hotkey:", Location = new Point(20, 70), Size = new Size(100, 20), Font = new Font("Segoe UI", 9, FontStyle.Bold) };
                TextBox txtStop = new TextBox
                {
                    Location = new Point(130, 67),
                    Size = new Size(120, 23),
                    BackColor = Color.FromArgb(30, 30, 30),
                    ForeColor = Color.White,
                    BorderStyle = BorderStyle.FixedSingle,
                    ReadOnly = true,
                    Text = settings.StopHotkey.ToString()
                };

                Keys startKey = settings.StartHotkey;
                Keys stopKey = settings.StopHotkey;

                txtStart.KeyDown += (s, e) =>
                {
                    e.SuppressKeyPress = true;
                    startKey = e.KeyCode;
                    txtStart.Text = startKey.ToString();
                };

                txtStop.KeyDown += (s, e) =>
                {
                    e.SuppressKeyPress = true;
                    stopKey = e.KeyCode;
                    txtStop.Text = stopKey.ToString();
                };

                Button btnSave = new Button { Text = "Save", Location = new Point(60, 120), Size = new Size(80, 30), DialogResult = DialogResult.OK };
                Button btnCancel = new Button { Text = "Cancel", Location = new Point(150, 120), Size = new Size(80, 30), DialogResult = DialogResult.Cancel };

                ApplyModernStyle(btnSave);
                ApplyModernStyle(btnCancel);

                dialog.Controls.AddRange(new Control[] { lblStart, txtStart, lblStop, txtStop, btnSave, btnCancel });

                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    UnregisterGlobalHotkeys();
                    
                    settings.StartHotkey = startKey;
                    settings.StopHotkey = stopKey;
                    
                    RegisterGlobalHotkeys();

                    btnStart.Text = $"Start ({settings.StartHotkey})";
                    btnStop.Text = $"Stop ({settings.StopHotkey})";

                    SettingsManager.SaveSettings(settings);
                    lblStatus.Text = "Hotkeys updated.";
                }
            }
        }

        // --- Global Hotkey Registration ---
        private void RegisterGlobalHotkeys()
        {
            bool startOk = Win32Helper.RegisterHotKey(this.Handle, START_HOTKEY_ID, 0, (uint)settings.StartHotkey);
            bool stopOk = Win32Helper.RegisterHotKey(this.Handle, STOP_HOTKEY_ID, 0, (uint)settings.StopHotkey);

            if (!startOk || !stopOk)
            {
                lblStatus.Text = "Error: Some hotkeys could not be registered.";
            }
        }

        private void UnregisterGlobalHotkeys()
        {
            Win32Helper.UnregisterHotKey(this.Handle, START_HOTKEY_ID);
            Win32Helper.UnregisterHotKey(this.Handle, STOP_HOTKEY_ID);
        }

        protected override void WndProc(ref Message m)
        {
            const int WM_HOTKEY = 0x0312;
            if (m.Msg == WM_HOTKEY)
            {
                int id = m.WParam.ToInt32();
                if (id == START_HOTKEY_ID)
                {
                    StartClicking();
                }
                else if (id == STOP_HOTKEY_ID)
                {
                    StopClicking();
                }
            }
            base.WndProc(ref m);
        }

        // --- About Dialog ---
        private void ShowAboutDialog()
        {
            var about = new Form
            {
                Text = "About Neo Auto Clicker",
                FormBorderStyle = FormBorderStyle.FixedSingle,
                MaximizeBox = false,
                MinimizeBox = false,
                StartPosition = FormStartPosition.CenterParent,
                ClientSize = new Size(380, 315),
                BackColor = Color.FromArgb(20, 20, 20),
                ForeColor = Color.White
            };

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
            catch { }

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
            about.ShowDialog(this);
        }
    }
}
