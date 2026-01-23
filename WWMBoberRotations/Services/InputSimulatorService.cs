using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using WindowsInput;
using WindowsInput.Native;
using WWMBoberRotations.Models;

namespace WWMBoberRotations.Services
{
    public class InputSimulatorService
    {
        private readonly IInputSimulator _simulator;

        // Windows API for mouse events
        [DllImport("user32.dll")]
        private static extern void mouse_event(uint dwFlags, int dx, int dy, uint dwData, int dwExtraInfo);

        private const uint MOUSEEVENTF_MIDDLEDOWN = 0x0020;
        private const uint MOUSEEVENTF_MIDDLEUP = 0x0040;
        private const uint MOUSEEVENTF_XDOWN = 0x0080;
        private const uint MOUSEEVENTF_XUP = 0x0100;
        private const uint XBUTTON1 = 0x0001;
        private const uint XBUTTON2 = 0x0002;

        public InputSimulatorService()
        {
            _simulator = new InputSimulator();
        }

        public async Task ExecuteActionAsync(ComboAction action, CancellationToken cancellationToken)
        {
            switch (action.Type)
            {
                case ActionType.KeyPress:
                    PressKeyOrMouseButton(action.Key!);
                    break;

                case ActionType.KeyHold:
                    await HoldKeyAsync(action.Key!, action.Duration, cancellationToken);
                    break;

                case ActionType.MouseClick:
                    ClickMouse(action.Button);
                    break;

                case ActionType.Delay:
                    await DelayAsync(action.Duration, cancellationToken);
                    break;
            }
        }

        private void PressKeyOrMouseButton(string key)
        {
            var lowerKey = key.ToLower();
            
            // Check if it's a mouse button
            switch (lowerKey)
            {
                case "lmb":
                case "leftclick":
                case "leftmouse":
                    _simulator.Mouse.LeftButtonClick();
                    return;
                    
                case "rmb":
                case "rightclick":
                case "rightmouse":
                    _simulator.Mouse.RightButtonClick();
                    return;
                    
                case "mmb":
                case "middleclick":
                case "middlemouse":
                    ClickMiddleButton();
                    return;
                    
                case "mouse4":
                case "xbutton1":
                    ClickXButton(XBUTTON1);
                    return;
                    
                case "mouse5":
                case "xbutton2":
                    ClickXButton(XBUTTON2);
                    return;
            }
            
            // Otherwise treat it as a keyboard key
            var virtualKey = ParseKey(key);
            _simulator.Keyboard.KeyPress(virtualKey);
        }

        private async Task HoldKeyAsync(string key, int duration, CancellationToken cancellationToken)
        {
            var virtualKey = ParseKey(key);
            _simulator.Keyboard.KeyDown(virtualKey);
            
            try
            {
                await Task.Delay(duration, cancellationToken);
            }
            finally
            {
                _simulator.Keyboard.KeyUp(virtualKey);
            }
        }

        private void ClickMouse(Models.MouseButton button)
        {
            switch (button)
            {
                case Models.MouseButton.Left:
                    _simulator.Mouse.LeftButtonClick();
                    break;
                case Models.MouseButton.Right:
                    _simulator.Mouse.RightButtonClick();
                    break;
                case Models.MouseButton.Middle:
                    ClickMiddleButton();
                    break;
                case Models.MouseButton.XButton1:
                    ClickXButton(XBUTTON1);
                    break;
                case Models.MouseButton.XButton2:
                    ClickXButton(XBUTTON2);
                    break;
            }
        }

        private void ClickMiddleButton()
        {
            mouse_event(MOUSEEVENTF_MIDDLEDOWN, 0, 0, 0, 0);
            Thread.Sleep(50);
            mouse_event(MOUSEEVENTF_MIDDLEUP, 0, 0, 0, 0);
        }

        private void ClickXButton(uint button)
        {
            mouse_event(MOUSEEVENTF_XDOWN, 0, 0, button, 0);
            Thread.Sleep(50);
            mouse_event(MOUSEEVENTF_XUP, 0, 0, button, 0);
        }

        private async Task DelayAsync(int duration, CancellationToken cancellationToken)
        {
            // Break delay into smaller chunks for better responsiveness
            const int chunkSize = 100;
            var remaining = duration;

            while (remaining > 0 && !cancellationToken.IsCancellationRequested)
            {
                var delay = Math.Min(remaining, chunkSize);
                await Task.Delay(delay, cancellationToken);
                remaining -= delay;
            }
        }

        private VirtualKeyCode ParseKey(string key)
        {
            var lowerKey = key.ToLower();

            // Special keys
            return lowerKey switch
            {
                // Common keys
                "space" or "spacebar" => VirtualKeyCode.SPACE,
                "enter" or "return" => VirtualKeyCode.RETURN,
                "tab" => VirtualKeyCode.TAB,
                "esc" or "escape" => VirtualKeyCode.ESCAPE,
                "backspace" or "back" => VirtualKeyCode.BACK,
                "delete" or "del" => VirtualKeyCode.DELETE,
                "insert" or "ins" => VirtualKeyCode.INSERT,
                
                // Modifier keys
                "shift" or "lshift" => VirtualKeyCode.SHIFT,
                "rshift" => VirtualKeyCode.RSHIFT,
                "ctrl" or "control" or "lctrl" => VirtualKeyCode.CONTROL,
                "rctrl" => VirtualKeyCode.RCONTROL,
                "alt" or "lalt" => VirtualKeyCode.MENU,
                "ralt" => VirtualKeyCode.RMENU,
                
                // Lock keys
                "capslock" or "caps" => VirtualKeyCode.CAPITAL,
                "numlock" or "num" => VirtualKeyCode.NUMLOCK,
                "scrolllock" or "scroll" => VirtualKeyCode.SCROLL,
                
                // Arrow keys
                "up" or "arrowup" or "uparrow" => VirtualKeyCode.UP,
                "down" or "arrowdown" or "downarrow" => VirtualKeyCode.DOWN,
                "left" or "arrowleft" or "leftarrow" => VirtualKeyCode.LEFT,
                "right" or "arrowright" or "rightarrow" => VirtualKeyCode.RIGHT,
                
                // Navigation keys
                "home" => VirtualKeyCode.HOME,
                "end" => VirtualKeyCode.END,
                "pageup" or "pgup" => VirtualKeyCode.PRIOR,
                "pagedown" or "pgdown" => VirtualKeyCode.NEXT,
                
                // Function keys
                "f1" => VirtualKeyCode.F1,
                "f2" => VirtualKeyCode.F2,
                "f3" => VirtualKeyCode.F3,
                "f4" => VirtualKeyCode.F4,
                "f5" => VirtualKeyCode.F5,
                "f6" => VirtualKeyCode.F6,
                "f7" => VirtualKeyCode.F7,
                "f8" => VirtualKeyCode.F8,
                "f9" => VirtualKeyCode.F9,
                "f10" => VirtualKeyCode.F10,
                "f11" => VirtualKeyCode.F11,
                "f12" => VirtualKeyCode.F12,
                
                _ => ParseRegularKey(lowerKey)
            };
        }

        private VirtualKeyCode ParseRegularKey(string key)
        {
            if (string.IsNullOrEmpty(key)) return VirtualKeyCode.SPACE;

            var c = key[0];
            
            // Numbers
            if (c >= '0' && c <= '9')
                return (VirtualKeyCode)(0x30 + (c - '0'));
            
            // Letters
            if (c >= 'a' && c <= 'z')
                return (VirtualKeyCode)(0x41 + (c - 'a'));

            return VirtualKeyCode.SPACE;
        }
    }
}
