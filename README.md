# Neo Auto Clicker

A lightweight, feature-rich auto-clicker for Windows with multi-monitor support, window targeting, and background clicking — built in C# / WinForms (.NET 10).

![Platform](https://img.shields.io/badge/platform-Windows-blue)
![.NET](https://img.shields.io/badge/.NET-10-purple)
![Version](https://img.shields.io/badge/version-1.0.0-cyan)

---

## Features

- **Click Interval** — Set hours, minutes, seconds, milliseconds between clicks
- **Click Repeat** — Infinite loop or fixed repeat count
- **Mouse Button** — Left, Right, or Middle click
- **Click Type** — Single or Double click
- **Click Position Modes:**
  - **Current Cursor Position** — Clicks wherever your mouse is; just press F6
  - **Target Specific Application** — Clicks a fixed location in a target window, regardless of where your mouse is
- **Click Modes (Target App):**
  - **Background** — Sends `WM_LBUTTONDOWN/UP` directly to the window; no focus required
  - **Foreground** — Moves the mouse to the target and forces focus; required for some games (e.g. Roblox)
- **Coordinate Modes:**
  - **Window-relative** — Click position follows the window if it's moved
  - **Screen position** — Click position is fixed to absolute screen coordinates
- **Pick Location** — Visual point-and-click selector to capture a target window and coordinates
- **Global Hotkeys** — F6 to start, F7 to stop (configurable)
- **Save / Reset Settings** — Persisted as JSON
- **Dark UI** — Themed title bar via DWM API, dark controls throughout
- **Admin Mode** — Requests elevation for reliable hotkey registration and foreground click support

---

## Requirements

- Windows 10 / 11 (64-bit)
- [.NET 10 SDK](https://dotnet.microsoft.com/download) *(for building from source)*

The published executable is **self-contained** — no .NET runtime needed to run it.

---

## Build from Source

```batch
build.bat
```

Or manually:

```bash
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
```

Output: `MyAutoClicker.exe` (single file, ~12 MB)

---

## Usage

### Current Cursor Position (simple mode)

1. Set your desired click interval
2. Press **F6** (or click **Start**) — the clicker fires at your current mouse position
3. Press **F7** (or click **Stop**) to stop

### Target Specific Application (advanced mode)

1. Select **Target Specific Application**
2. Click **Pick Location** and click on the desired spot in your target window
3. Choose **Click Mode** (Background for most apps, Foreground for games)
4. Choose **Coord Mode** (Window-relative to follow the window, Screen position for fixed coordinates)
5. Press **F6** to start — clicks fire at the target regardless of where your mouse is

---

## Project Structure

```
MyAutoClicker/
├── MainForm.cs           # Main UI and click logic
├── Win32Helper.cs        # P/Invoke declarations (SendMessage, mouse_event, DWM, etc.)
├── TargetSelectorForm.cs # Pick Location overlay dialog
├── SettingsManager.cs    # JSON settings persistence
├── Program.cs            # Entry point
├── MyAutoClicker.csproj  # Project file (.NET 10, WinForms)
├── app.manifest          # PerMonitorV2 DPI awareness + admin elevation
├── build.bat             # One-click build script
└── .gitignore
```

---

## License

MIT — free to use and modify.
