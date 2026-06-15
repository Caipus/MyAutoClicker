using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Drawing;

namespace MyAutoClicker
{
    public static class Win32Helper
    {
        // Window selection / details
        [DllImport("user32.dll")]
        public static extern IntPtr WindowFromPoint(Point point);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

        [DllImport("user32.dll")]
        public static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        [DllImport("user32.dll", ExactSpelling = true)]
        public static extern IntPtr GetAncestor(IntPtr hwnd, uint flags);
        
        public const uint GA_PARENT = 1;
        public const uint GA_ROOT = 2;
        public const uint GA_ROOTOWNER = 3;

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetClientRect(IntPtr hWnd, out RECT lpRect);

        [DllImport("dwmapi.dll")]
        public static extern int DwmSetWindowAttribute(IntPtr hwnd, int dwAttribute, ref int pvAttribute, int cbAttribute);
        public const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;
        public const int DWMWA_CAPTION_COLOR           = 35;
        public const int DWMWA_TEXT_COLOR              = 36;
        public const int DWMWA_BORDER_COLOR            = 34;

        [DllImport("user32.dll")]
        public static extern bool ScreenToClient(IntPtr hWnd, ref Point lpPoint);

        [DllImport("user32.dll")]
        public static extern bool ClientToScreen(IntPtr hWnd, ref Point lpPoint);

        /// <summary>Returns the physical (unscaled) cursor position, bypassing DPI virtualization.</summary>
        [DllImport("user32.dll")]
        public static extern bool GetPhysicalCursorPos(out Point lpPoint);

        /// <summary>Converts a logical point in a window's client area to physical screen coordinates.</summary>
        [DllImport("user32.dll")]
        public static extern bool LogicalToPhysicalPointForPerMonitorDPI(IntPtr hWnd, ref Point lpPoint);

        [DllImport("shcore.dll")]
        public static extern int GetDpiForMonitor(IntPtr hmonitor, int dpiType, out uint dpiX, out uint dpiY);

        [DllImport("user32.dll")]
        public static extern IntPtr MonitorFromPoint(Point pt, uint dwFlags);
        public const uint MONITOR_DEFAULTTONEAREST = 2;

        [DllImport("user32.dll")]
        public static extern int GetSystemMetrics(int nIndex);
        public const int SM_XVIRTUALSCREEN  = 76;
        public const int SM_YVIRTUALSCREEN  = 77;
        public const int SM_CXVIRTUALSCREEN = 78;
        public const int SM_CYVIRTUALSCREEN = 79;

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool BringWindowToTop(IntPtr hWnd);

        [DllImport("user32.dll")]
        public static extern void SwitchToThisWindow(IntPtr hWnd, bool fAltTab);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool AllowSetForegroundWindow(uint dwProcessId);
        public const uint ASFW_ANY = 0xFFFFFFFF;

        [DllImport("user32.dll")]
        public static extern IntPtr SetActiveWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        public static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        public static extern bool AttachThreadInput(uint idAttach, uint idAttachTo, bool fAttach);

        [DllImport("kernel32.dll")]
        public static extern uint GetCurrentThreadId();

        [DllImport("user32.dll")]
        public static extern bool IsWindow(IntPtr hWnd);

        // Windows Message Posting (for background click)
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern bool PostMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        public const uint WM_LBUTTONDOWN = 0x0201;
        public const uint WM_LBUTTONUP = 0x0202;
        public const uint WM_RBUTTONDOWN = 0x0204;
        public const uint WM_RBUTTONUP = 0x0205;
        public const uint WM_MBUTTONDOWN = 0x0207;
        public const uint WM_MBUTTONUP = 0x0208;
        
        public const uint WM_LBUTTONDBLCLK = 0x0203;
        public const uint WM_RBUTTONDBLCLK = 0x0206;
        public const uint WM_MBUTTONDBLCLK = 0x0209;

        // SendInput (for foreground click)
        [DllImport("user32.dll", SetLastError = true)]
        public static extern uint SendInput(uint nInputs, INPUT[] pInputs, int cbSize);

        public const int INPUT_MOUSE = 0;
        public const uint MOUSEEVENTF_MOVE = 0x0001;
        public const uint MOUSEEVENTF_LEFTDOWN = 0x0002;
        public const uint MOUSEEVENTF_LEFTUP = 0x0004;
        public const uint MOUSEEVENTF_RIGHTDOWN = 0x0008;
        public const uint MOUSEEVENTF_RIGHTUP = 0x0010;
        public const uint MOUSEEVENTF_MIDDLEDOWN = 0x0020;
        public const uint MOUSEEVENTF_MIDDLEUP = 0x0040;
        public const uint MOUSEEVENTF_ABSOLUTE = 0x8000;
        public const uint MOUSEEVENTF_VIRTUALDESKTOP = 0x4000;

        // Global Hotkeys
        [DllImport("user32.dll")]
        public static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll")]
        public static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        // Child window enumeration
        public delegate bool EnumWindowProc(IntPtr hWnd, IntPtr lParam);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool EnumChildWindows(IntPtr window, EnumWindowProc callback, IntPtr lParam);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool EnumWindows(EnumWindowProc lpEnumFunc, IntPtr lParam);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool IsWindowVisible(IntPtr hWnd);

        // Structures
        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;

            public int Width => Right - Left;
            public int Height => Bottom - Top;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct INPUT
        {
            public int type;
            public MouseKeybdhardwareInput Union;
        }

        [StructLayout(LayoutKind.Explicit)]
        public struct MouseKeybdhardwareInput
        {
            [FieldOffset(0)]
            public MOUSEINPUT mi;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MOUSEINPUT
        {
            public int dx;
            public int dy;
            public int mouseData;
            public int dwFlags;
            public int time;
            public IntPtr dwExtraInfo;
        }

        // Native methods for anti-cheat safe process name retrieval
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr OpenProcess(uint processAccess, bool bInheritHandle, uint processId);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern bool QueryFullProcessImageName(IntPtr hProcess, int dwFlags, StringBuilder lpExeName, ref int lpdwSize);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool CloseHandle(IntPtr hObject);

        public const uint PROCESS_QUERY_LIMITED_INFORMATION = 0x1000;

        // Helper methods to get window information
        public static string GetWindowTitle(IntPtr hWnd)
        {
            StringBuilder sb = new StringBuilder(256);
            GetWindowText(hWnd, sb, sb.Capacity);
            return sb.ToString();
        }

        public static string GetWindowClassName(IntPtr hWnd)
        {
            StringBuilder sb = new StringBuilder(256);
            GetClassName(hWnd, sb, sb.Capacity);
            return sb.ToString();
        }

        public static string GetProcessNameFromWindow(IntPtr hWnd)
        {
            GetWindowThreadProcessId(hWnd, out uint processId);
            if (processId == 0) return string.Empty;

            // Try opening with limited query info first (not blocked by Roblox Hyperion/Byfron anti-cheat)
            IntPtr hProcess = OpenProcess(PROCESS_QUERY_LIMITED_INFORMATION, false, processId);
            if (hProcess != IntPtr.Zero)
            {
                StringBuilder sb = new StringBuilder(1024);
                int size = sb.Capacity;
                if (QueryFullProcessImageName(hProcess, 0, sb, ref size))
                {
                    CloseHandle(hProcess);
                    string fullPath = sb.ToString();
                    return System.IO.Path.GetFileNameWithoutExtension(fullPath);
                }
                CloseHandle(hProcess);
            }

            // Fallback (might fail with Access Denied for protected processes like Roblox)
            try
            {
                using (var proc = System.Diagnostics.Process.GetProcessById((int)processId))
                {
                    return proc.ProcessName;
                }
            }
            catch
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Aggressively forces a window to the foreground using all available Win32 techniques,
        /// bypassing the Windows foreground lock that blocks background process focus changes.
        /// Returns true if the window is confirmed as foreground, false on timeout.
        /// </summary>
        public static bool ForceSetForegroundWindow(IntPtr hWnd)
        {
            if (GetForegroundWindow() == hWnd) return true;

            GetWindowThreadProcessId(hWnd, out uint targetPid);
            IntPtr fgWnd = GetForegroundWindow();
            uint fgThreadId = GetWindowThreadProcessId(fgWnd, out _);
            uint myThreadId = GetCurrentThreadId();

            // 1. Allow the target process to set the foreground
            AllowSetForegroundWindow(targetPid);
            AllowSetForegroundWindow(ASFW_ANY);

            // 2. Attach our input thread to the current foreground thread
            bool attached = false;
            if (fgThreadId != 0 && fgThreadId != myThreadId)
                attached = AttachThreadInput(myThreadId, fgThreadId, true);

            // 3. Try all available focus methods
            BringWindowToTop(hWnd);
            SwitchToThisWindow(hWnd, true);
            SetActiveWindow(hWnd);
            SetForegroundWindow(hWnd);

            if (attached)
                AttachThreadInput(myThreadId, fgThreadId, false);

            // 4. Verify focus actually transferred (poll up to 50ms in 5ms steps)
            for (int i = 0; i < 10; i++)
            {
                if (GetForegroundWindow() == hWnd) return true;
                System.Threading.Thread.Sleep(5);
            }

            return GetForegroundWindow() == hWnd;
        }
    }
}
