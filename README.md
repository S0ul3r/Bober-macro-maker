# WWM Bober Rotations

**Macro automation tool for "Where Winds Meet"**

<img width="886" height="693" alt="image" src="https://github.com/user-attachments/assets/3952baf7-9867-4796-8626-7841168665e6" />

Built with C# + WPF + Material Design

[![GitHub](https://img.shields.io/badge/GitHub-Repository-blue)](https://github.com/S0ul3r/Bober-macro-maker)

---

## Overview

WWM Bober Rotations is a safe, external macro automation tool that helps you execute complex attack sequences and skill rotations in "Where Winds Meet". It simulates keyboard and mouse input at the Windows driver level - exactly like a physical keyboard - making it completely safe and external.

## Features

- **Custom Combo Creator** - Design unlimited attack sequences
- **Macro Recorder** - Record key presses and mouse clicks with automatic timing
- **Smart Input System** - Key presses, holds, mouse clicks, and delays
- **Hotkey Activation** - Trigger combos with any keyboard key or mouse button
- **Panic Button** - Instant emergency stop (configurable, default: Right Mouse Button)
- **Auto-Save** - Automatic backup every 30 seconds with crash recovery
- **Precise Timing** - Millisecond-accurate delays and holds
- **Drag-and-Drop Reordering** - Easily rearrange actions in combos
- **Beautiful Material Design UI** - Modern, professional dark theme interface
- **Import/Export** - Share combos with friends
- **100% Safe** - No injection, no memory reading, completely external
- **Single .exe** - Standalone executable, no installation needed

## Quick Start

### Download Pre-built Executable

1. Download the latest release from the `publish` folder
2. Run `WWMBoberRotations.exe`
3. Start creating combos

### Build from Source

**Requirements:**
- .NET 8.0 SDK or higher
- Windows 10/11

**Steps:**
```batch
build.bat
cd publish
WWMBoberRotations.exe
```

### Creating Your First Combo

1. Launch the application
2. Click "New Combo"
3. Enter a name (e.g., "Basic Attack")
4. Click in the Hotkey field and press a key (e.g., "1")
5. Click "Add Action" to add actions
6. Click "Save Combo"
7. Click "START MACRO SYSTEM" (button turns red)
8. Press your hotkey in-game

## Action Types

| Action | Description | Example |
|--------|-------------|---------|
| **Key Press** | Instantly press and release a key or mouse button | Q, W, E, R, Space, LMB, Mouse4 |
| **Key Hold** | Hold a key for specified duration | Charged attacks (Hold Space 1.5s) |
| **Mouse Click** | Click left/right/middle/mouse4/mouse5 button | Attack combos |
| **Delay** | Wait for specified time | Cooldown timing (Wait 500ms) |

## Supported Keys

### Keyboard Keys
- **Letters:** a-z (lowercase)
- **Numbers:** 0-9
- **Function Keys:** f1-f12
- **Common Keys:** space (or spacebar), enter (or return), tab, esc (or escape), backspace (or back), delete (or del), insert (or ins)
- **Modifier Keys:** shift (or lshift), rshift, ctrl (or control, lctrl), rctrl, alt (or lalt), ralt
- **Lock Keys:** capslock (or caps), numlock (or num), scrolllock (or scroll)
- **Arrow Keys:** up, down, left, right (or arrowup, arrowdown, arrowleft, arrowright)
- **Navigation Keys:** home, end, pageup (or pgup), pagedown (or pgdown)

### Mouse Buttons
- **lmb** - Left Mouse Button
- **rmb** - Right Mouse Button
- **mmb** - Middle Mouse Button
- **mouse4** - Side Button 1 (XButton1)
- **mouse5** - Side Button 2 (XButton2)

**Note:** Always enter keys in lowercase! Mouse buttons can be used as hotkeys and in actions.

## Panic Button

The panic button instantly stops any running combo. Perfect for emergency situations when you need to cancel a combo mid-execution, dodge an attack, or regain manual control.

**Default:** Right Mouse Button (RMB)

You can customize the panic button in the Settings section (click "Set Panic Button", then press your desired key).

## Macro Recorder

The Macro Recorder allows you to record your key presses and mouse clicks in real-time, with automatic timing capture between actions.

**How to use:**
1. Click "Record Macro" button
2. Set the recording hotkey (e.g., Insert) - this key toggles recording on/off
3. Press the record hotkey to start recording
4. Press any keys/mouse buttons you want to record
5. Press the record hotkey again to stop recording
6. Enter a combo name
7. Click "SAVE AS COMBO" to save your recording
8. You'll automatically be taken to the Combo Editor to assign a hotkey and make adjustments

**Features:**
- **Precise Timing** - Automatically captures delays between actions (minimum 50ms)
- **Easy Editing** - Recorded actions appear in a list that you can delete or rearrange
- **Drag-and-Drop** - Reorder actions by dragging them to new positions
- **Real-time Feedback** - See a visual indicator showing where actions will be moved during drag operations

## Auto-Save & Crash Recovery

The application automatically saves your combos every **30 seconds** when auto-save is enabled (default: ON).

**Features:**
- **Automatic backup** - Your work is saved in the background every 30 seconds
- **Crash recovery** - If the application or computer crashes, you'll be prompted to restore from autosave on next launch
- **No data loss** - Continue working where you left off
- **Toggle control** - Enable/disable auto-save using the switch in the UI
- **Unsaved indicator** - Visual indicator shows when you have unsaved changes

**Note:** Manual save (CTRL+S or Save button) clears the autosave and marks all changes as saved.

## Building for Distribution

### Single-File Executable

```batch
build.bat
```

Output: `publish/WWMBoberRotations.exe` (~25-35 MB)

This creates a self-contained executable that includes:
- .NET runtime
- All dependencies
- No installation required

### Advanced Build Options

**Trimmed Build (smaller size):**
```batch
dotnet publish -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true /p:PublishTrimmed=true
```

**Framework-Dependent (requires .NET installed):**
```batch
dotnet publish -c Release -r win-x64 --self-contained false
```

## License

This project is licensed under the MIT License - see the LICENSE file for details.

## Disclaimer

This software is provided for **educational purposes only**. Users are responsible for ensuring their use complies with game terms of service. The authors are not responsible for any consequences resulting from use of this software.

**This tool:**
- Does NOT inject code into games
- Does NOT read game memory
- Does NOT modify game files
- Uses standard Windows input APIs

Use responsibly and at your own risk.

## Contributing

Contributions are welcome. Feel free to:
- Report bugs
- Suggest features
- Submit pull requests
- Share combo configurations

## Support

Need help? Check these resources:
- Read this README thoroughly
- Review example combos in `Examples/`
- Check Issues section for known problems
