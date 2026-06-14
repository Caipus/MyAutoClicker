using System;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace MyAutoClicker
{
    public class TargetSelectorForm : Form
    {
        // Selected values
        public IntPtr TargetHWnd { get; private set; } = IntPtr.Zero;
        public IntPtr TopLevelHWnd { get; private set; } = IntPtr.Zero;
        public string ProcessName { get; private set; } = string.Empty;
        public string WindowTitle { get; private set; } = string.Empty;
        public string WindowClassName { get; private set; } = string.Empty;
        public Point RelativeCoordinate { get; private set; } = Point.Empty;

        /// <summary>
        /// When true, RelativeCoordinate stores raw physical screen pixels.
        /// When false (default), RelativeCoordinate stores window-client-relative pixels.
        /// Set this before ShowDialog().
        /// </summary>
        public bool UseScreenCoords { get; set; } = false;

        // Current state for rendering
        private IntPtr hoveredHWnd = IntPtr.Zero;
        private Win32Helper.RECT hoveredRect;
        private Point currentMousePos = Point.Empty;

        private Brush infoBgBrush = new SolidBrush(Color.FromArgb(220, 30, 30, 30));
        private Pen borderPen = new Pen(Color.Cyan, 3);
        private Font infoFont = new Font("Segoe UI", 9, FontStyle.Regular);
        private Font headerFont = new Font("Segoe UI", 9, FontStyle.Bold);

        public TargetSelectorForm()
        {
            // Set up form properties to cover all screens transparently
            this.FormBorderStyle = FormBorderStyle.None;
            this.StartPosition = FormStartPosition.Manual;
            this.Bounds = SystemInformation.VirtualScreen;
            this.ShowInTaskbar = false;
            this.TopMost = true;
            
            // Minimal opacity to capture mouse input while letting the desktop be fully visible
            this.Opacity = 0.30;
            this.BackColor = Color.FromArgb(10, 10, 10);
            
            this.DoubleBuffered = true;
            this.Cursor = Cursors.Cross;

            // Handle keys like Escape to cancel
            this.KeyPreview = true;
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (e.KeyCode == Keys.Escape)
            {
                this.DialogResult = DialogResult.Cancel;
                this.Close();
            }
        }

        private IntPtr GetWindowUnderPoint(Point pt, IntPtr ignoreHWnd)
        {
            IntPtr foundHWnd = IntPtr.Zero;
            Win32Helper.EnumWindows((hWnd, lParam) =>
            {
                if (hWnd != ignoreHWnd && Win32Helper.IsWindowVisible(hWnd))
                {
                    Win32Helper.RECT rect;
                    if (Win32Helper.GetWindowRect(hWnd, out rect))
                    {
                        if (pt.X >= rect.Left && pt.X <= rect.Right &&
                            pt.Y >= rect.Top && pt.Y <= rect.Bottom)
                        {
                            foundHWnd = hWnd;
                            return false; // Stop enumeration
                        }
                    }
                }
                return true; // Continue
            }, IntPtr.Zero);
            return foundHWnd;
        }

        private IntPtr GetChildWindowUnderPoint(IntPtr parentHWnd, Point pt)
        {
            IntPtr childHWnd = parentHWnd;
            bool foundChild = true;
            while (foundChild)
            {
                foundChild = false;
                IntPtr temp = IntPtr.Zero;
                Win32Helper.EnumChildWindows(childHWnd, (hWnd, lParam) =>
                {
                    if (Win32Helper.IsWindowVisible(hWnd))
                    {
                        Win32Helper.RECT rect;
                        if (Win32Helper.GetWindowRect(hWnd, out rect))
                        {
                            if (pt.X >= rect.Left && pt.X <= rect.Right &&
                                pt.Y >= rect.Top && pt.Y <= rect.Bottom)
                            {
                                temp = hWnd;
                            }
                        }
                    }
                    return true;
                }, IntPtr.Zero);

                if (temp != IntPtr.Zero && temp != childHWnd)
                {
                    childHWnd = temp;
                    foundChild = true;
                }
            }
            return childHWnd;
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            currentMousePos = Cursor.Position;

            // Find top level window under mouse, ignoring overlay
            IntPtr topLvl = GetWindowUnderPoint(currentMousePos, this.Handle);
            if (topLvl != IntPtr.Zero)
            {
                // Find deepest child window control under mouse
                IntPtr child = GetChildWindowUnderPoint(topLvl, currentMousePos);
                hoveredHWnd = (child != IntPtr.Zero) ? child : topLvl;
                Win32Helper.GetWindowRect(hoveredHWnd, out hoveredRect);
            }
            else
            {
                hoveredHWnd = IntPtr.Zero;
            }

            this.Invalidate();
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);

            if (hoveredHWnd != IntPtr.Zero)
            {
                TargetHWnd = hoveredHWnd;
                TopLevelHWnd = Win32Helper.GetAncestor(TargetHWnd, Win32Helper.GA_ROOT);
                if (TopLevelHWnd == IntPtr.Zero) TopLevelHWnd = TargetHWnd;

                ProcessName = Win32Helper.GetProcessNameFromWindow(TopLevelHWnd);
                WindowTitle = Win32Helper.GetWindowTitle(TopLevelHWnd);
                WindowClassName = Win32Helper.GetWindowClassName(TargetHWnd);

                // Capture coordinate based on selected mode
                Win32Helper.GetPhysicalCursorPos(out Point physicalPt);
                if (UseScreenCoords)
                {
                    // Screen mode: store the raw physical screen position.
                    // The click will always land on this exact screen pixel.
                    RelativeCoordinate = physicalPt;
                }
                else
                {
                    // Window-relative mode: convert to window client coordinates.
                    // At click time, ClientToScreen will reverse this, so the click
                    // follows the window even if it is moved later.
                    Win32Helper.ScreenToClient(TargetHWnd, ref physicalPt);
                    RelativeCoordinate = physicalPt;
                }

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            else
            {
                this.DialogResult = DialogResult.Cancel;
                this.Close();
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            Graphics g = e.Graphics;

            // Draw targeted window border
            if (hoveredHWnd != IntPtr.Zero)
            {
                // Convert screen bounds to client bounds of this overlay form
                int left = hoveredRect.Left - this.Bounds.X;
                int top = hoveredRect.Top - this.Bounds.Y;
                int width = hoveredRect.Width;
                int height = hoveredRect.Height;

                g.DrawRectangle(borderPen, left, top, width, height);

                // Draw coordinate crosshair at target point
                int clientMouseX = currentMousePos.X - this.Bounds.X;
                int clientMouseY = currentMousePos.Y - this.Bounds.Y;
                g.DrawLine(Pens.Cyan, clientMouseX - 15, clientMouseY, clientMouseX + 15, clientMouseY);
                g.DrawLine(Pens.Cyan, clientMouseX, clientMouseY - 15, clientMouseX, clientMouseY + 15);

                // Prepare info panel texts
                string pName = Win32Helper.GetProcessNameFromWindow(hoveredHWnd);
                string wTitle = Win32Helper.GetWindowTitle(hoveredHWnd);
                if (wTitle.Length > 30) wTitle = wTitle.Substring(0, 27) + "...";
                string wClass = Win32Helper.GetWindowClassName(hoveredHWnd);

                Point relPos = currentMousePos;
                Win32Helper.ScreenToClient(hoveredHWnd, ref relPos);

                string infoText = 
                    $"App: {pName}.exe\n" +
                    $"Title: {wTitle}\n" +
                    $"Class: {wClass}\n" +
                    $"Coords: X={relPos.X}, Y={relPos.Y} (relative)";

                // Draw floating information panel next to cursor
                Size panelSize = new Size(240, 85);
                int panelX = clientMouseX + 15;
                int panelY = clientMouseY + 15;

                // Adjust panel position if it goes off screen boundaries
                if (panelX + panelSize.Width > this.ClientRectangle.Width)
                {
                    panelX = clientMouseX - panelSize.Width - 15;
                }
                if (panelY + panelSize.Height > this.ClientRectangle.Height)
                {
                    panelY = clientMouseY - panelSize.Height - 15;
                }

                Rectangle panelRect = new Rectangle(panelX, panelY, panelSize.Width, panelSize.Height);
                g.FillRectangle(infoBgBrush, panelRect);
                g.DrawRectangle(Pens.Cyan, panelRect);

                g.DrawString("Target Window Info (ESC to Cancel)", headerFont, Brushes.Cyan, panelX + 8, panelY + 6);
                g.DrawString(infoText, infoFont, Brushes.White, panelX + 8, panelY + 22);
            }
            else
            {
                // Display general prompt if not hovering anything
                int clientMouseX = currentMousePos.X - this.Bounds.X;
                int clientMouseY = currentMousePos.Y - this.Bounds.Y;
                
                string prompt = "Hover over a window to select click target.\nClick to select. ESC to cancel.";
                Size promptSize = new Size(260, 45);
                int px = clientMouseX + 15;
                int py = clientMouseY + 15;
                
                if (px + promptSize.Width > this.ClientRectangle.Width) px = clientMouseX - promptSize.Width - 15;
                if (py + promptSize.Height > this.ClientRectangle.Height) py = clientMouseY - promptSize.Height - 15;
                
                Rectangle promptRect = new Rectangle(px, py, promptSize.Width, promptSize.Height);
                g.FillRectangle(infoBgBrush, promptRect);
                g.DrawRectangle(Pens.White, promptRect);
                g.DrawString(prompt, infoFont, Brushes.White, px + 8, py + 8);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                infoBgBrush?.Dispose();
                borderPen?.Dispose();
                infoFont?.Dispose();
                headerFont?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
