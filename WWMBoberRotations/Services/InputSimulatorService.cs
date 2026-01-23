using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using WindowsInput;
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
        private const int MOUSE_CLICK_DELAY = 50;

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
            // Check if it's a mouse button first
            if (KeyMapper.IsMouseButton(key))
            {
                ExecuteMouseButtonClick(key);
                return;
            }
            
            // Otherwise treat it as a keyboard key
            var virtualKey = KeyMapper.ToVirtualKeyCode(key);
            _simulator.Keyboard.KeyPress(virtualKey);
        }

        private void ExecuteMouseButtonClick(string mouseButton)
        {
            var lowerButton = mouseButton.ToLower();
            
            switch (lowerButton)
            {
                case "lmb":
                case "leftclick":
                case "leftmouse":
                    _simulator.Mouse.LeftButtonClick();
                    break;
                    
                case "rmb":
                case "rightclick":
                case "rightmouse":
                    _simulator.Mouse.RightButtonClick();
                    break;
                    
                case "mmb":
                case "middleclick":
                case "middlemouse":
                    ClickMiddleButton();
                    break;
                    
                case "mouse4":
                case "xbutton1":
                    ClickXButton(XBUTTON1);
                    break;
                    
                case "mouse5":
                case "xbutton2":
                    ClickXButton(XBUTTON2);
                    break;
            }
        }

        private async Task HoldKeyAsync(string key, int duration, CancellationToken cancellationToken)
        {
            var virtualKey = KeyMapper.ToVirtualKeyCode(key);
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
            Thread.Sleep(MOUSE_CLICK_DELAY);
            mouse_event(MOUSEEVENTF_MIDDLEUP, 0, 0, 0, 0);
        }

        private void ClickXButton(uint button)
        {
            mouse_event(MOUSEEVENTF_XDOWN, 0, 0, button, 0);
            Thread.Sleep(MOUSE_CLICK_DELAY);
            mouse_event(MOUSEEVENTF_XUP, 0, 0, button, 0);
        }

        private static async Task DelayAsync(int duration, CancellationToken cancellationToken)
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
    }
}
