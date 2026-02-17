# WWM Bober Rotations

[![GitHub](https://img.shields.io/badge/GitHub-Repository-blue)](https://github.com/S0ul3r/Bober-macro-maker)

**Macro automation tool for "Where Winds Meet"** — safe, external, no injection or memory reading.

<img width="986" height="793" alt="image" src="https://github.com/user-attachments/assets/001b1c1f-1b58-477f-96a7-851a11051c67" />

---

## What it does

- **Custom combos** — Key presses, holds, mouse clicks, delays. Assign any hotkey (keyboard or mouse).
- **Macro recorder** — Record your inputs with timing, then save as a combo and edit.
- **Panic button** — Stops any running combo (default: Right Mouse Button). Set your own in the app.
- **Auto-save** — Saves every 30 seconds; on crash you can restore from autosave.
- **Single .exe** — No install. Build once, run from `publish\WWMBoberRotations.exe`.

---

## Quick start

**Run the app**

- **Pre-built:** Get the latest from [Releases](https://github.com/S0ul3r/Bober-macro-maker/releases) or run `publish\WWMBoberRotations.exe` after building.
- **Build yourself:** Need .NET 8 SDK + Windows. From the project folder run:
  ```batch
  build.bat
  ```
  Then run `publish\WWMBoberRotations.exe`.

**First combo**

1. Click **New Combo**, name it, click **Set Hotkey** and press a key or mouse button.
2. Click **Add Action** and add key presses, holds, mouse clicks or delays. Set "Delay after" on each action if needed.
3. Save the combo.
4. Click **START MACRO SYSTEM** (button turns red), then use your hotkey in-game.

Double-click a combo or an action in the list to edit. Use **Duplicate Combo** to copy one. **Record Macro** records key/mouse input with timing and saves it as a combo.

---

## Action types

| Type        | Description                    |
|------------|--------------------------------|
| Key Press  | Press and release (key or mouse). |
| Key Hold   | Hold key for X ms.              |
| Mouse Click| Left / Right / Middle / Mouse4 / Mouse5. |
| Delay      | Wait X ms.                     |

Each action has a **Delay after** field so you don’t need separate delay steps.

---

## Keys and mouse

- **Keyboard:** a–z, 0–9, space, enter, tab, esc, f1–f12, arrows, modifiers (shift, ctrl, alt), etc. Use **lowercase** in the app.
- **Mouse:** `lmb`, `rmb`, `mmb`, `mouse4`, `mouse5` — work in actions and as hotkeys.

---

## Macro recorder

1. Click **Record Macro**.
2. Set the record hotkey (e.g. Insert) — same key starts and stops recording.
3. Press the hotkey to start, perform your inputs, press again to stop.
4. Enter a name and click **SAVE AS COMBO** — you’ll get the combo editor to set hotkey and tweak.

---

## Panic button & auto-save

- **Panic button:** Stops the current combo. Default is RMB; change with **Set Panic Button**.
- **Auto-save:** Every 30 s when enabled. If the app crashes, you’ll be asked to restore from autosave on next start.

---

## Build (single .exe)

```batch
build.bat
```

Output: `publish\WWMBoberRotations.exe` (self-contained, no .NET install needed on the PC).

---

## License & disclaimer

MIT License — see LICENSE.

**Educational use only.** Check the game’s terms of service. This tool does **not** inject code, read memory, or modify game files; it uses normal Windows input. Use at your own risk.
