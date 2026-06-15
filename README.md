# Neo Auto Clicker

A lightweight, feature-rich auto-clicker for Windows with multi-monitor support, window targeting, and background clicking — built in C# / WinForms (.NET 10).

[![Download Latest](https://img.shields.io/badge/Download-Latest%20Release-green?style=for-the-badge&logo=github)](https://github.com/Caipus/MyAutoClicker/releases/latest)

---

## 📥 Download

If you just want to run the application, you do **not** need to install any SDK or compile the code:

1. Go to the [Releases](https://github.com/Caipus/MyAutoClicker/releases) page.
2. Under the latest version, click on **`NeoAutoClicker.exe`** (or the `.zip` package if your browser blocks direct executable downloads) to download it.
3. Open the downloaded file and start clicking!

---

## Features

- **Click Interval** — Set hours, minutes, seconds, milliseconds between clicks
- **Click Repeat** — Infinite loop or fixed repeat count
- **Human Tolerance (Humanize)** — Optional random interval tolerance (+/- %) to mimic human click accuracy (e.g. 1000ms +/- 10%), with a live millisecond offset preview.
- **Bilingual User Help** — Click the `?` icon in the header to open a bilingual help dialog switchable between English and German.
- **Mouse Button** — Left, Right, or Middle click
- **Click Type** — Single or Double click
- **Click Position Modes:**
  - **Current Cursor Position** — Clicks wherever your mouse is; just press F6
  - **Target Specific Application** — Clicks a fixed location in a target window, regardless of where your mouse is
- **Click Modes (Target App):**
  - **Background** — Sends `WM_LBUTTONDOWN/UP` directly to the window; no focus required
  - **Foreground** — Moves the mouse to the target and forces focus (default); required for some games (e.g. Roblox)
- **Coordinate Modes:**
  - **Window-relative** — Click position follows the window if it's moved
  - **Screen position** — Click position is fixed to absolute screen coordinates
- **Pick Location** — Visual point-and-click selector to capture a target window and coordinates
- **Global Hotkeys** — F6 to start, F7 to stop (configurable)
- **Save / Reset Settings** — Persisted as JSON
- **Dark UI** — Themed title bar via DWM API, dark controls throughout
- **No admin required** — Runs with normal user rights; no UAC prompt on launch

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

Output: `NeoAutoClicker.exe` (single file, ~12 MB)

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

### Human Tolerance (Humanize)

1. Check **Enable:** under the **HUMAN TOLERANCE** section at the bottom-left.
2. Set a percentage (e.g., 10%).
3. The clicker will randomly vary the click interval by up to that percentage (e.g., at a 1000ms base and 10% tolerance, clicks will fire randomly between 900ms and 1100ms).
4. The live millisecond offset range is displayed as `(+/- X ms)`.

### Bilingual Help

Click the `?` icon in the top-right header to open the bilingual user help. Use the **English** or **Deutsch** buttons to switch between languages dynamically.

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
├── app.manifest          # PerMonitorV2 DPI awareness (no admin elevation)
├── build.bat             # One-click build script
└── .gitignore
```

---

## License

MIT — free to use and modify.
