using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using WWMBoberRotations.Models;

namespace WWMBoberRotations.Services
{
    public class InputSimulatorService
    {
        // SendInput structures
        [StructLayout(LayoutKind.Sequential)]
        private struct INPUT
        {
            public uint Type;
            public INPUTUNION Data;
        }

        [StructLayout(LayoutKind.Explicit)]
        private struct INPUTUNION
        {
            [FieldOffset(0)]
            public MOUSEINPUT Mouse;
            [FieldOffset(0)]
            public KEYBDINPUT Keyboard;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MOUSEINPUT
        {
            public int dx;
            public int dy;
            public uint mouseData;
            public uint dwFlags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct KEYBDINPUT
        {
            public ushort wVk;
            public ushort wScan;
            public uint dwFlags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        // Windows API
        [DllImport("user32.dll", SetLastError = true)]
        private static extern uint SendInput(uint nInputs, INPUT[] pInputs, int cbSize);

        [DllImport("user32.dll")]
        private static extern void mouse_event(uint dwFlags, int dx, int dy, uint dwData, int dwExtraInfo);

        [DllImport("user32.dll")]
        private static extern uint MapVirtualKey(uint uCode, uint uMapType);

        private const uint INPUT_KEYBOARD = 1;
        private const uint KEYEVENTF_KEYDOWN = 0x0000;
        private const uint KEYEVENTF_KEYUP = 0x0002;
        private const uint KEYEVENTF_SCANCODE = 0x0008;
        private const uint MAPVK_VK_TO_VSC = 0;
        
        private const uint MOUSEEVENTF_MIDDLEDOWN = 0x0020;
        private const uint MOUSEEVENTF_MIDDLEUP = 0x0040;
        private const uint MOUSEEVENTF_XDOWN = 0x0080;
        private const uint MOUSEEVENTF_XUP = 0x0100;
        private const uint XBUTTON1 = 0x0001;
        private const uint XBUTTON2 = 0x0002;
        private const int MOUSE_CLICK_DELAY = 50;
        private const int KEY_PRESS_DELAY = 50;
        private const int KEY_RELEASE_DELAY = 30;

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
            
            // Otherwise treat it as a keyboard key - use SendInput with SCAN CODES
            var virtualKey = KeyMapper.GetVirtualKeyCode(key);
            ushort vkCode = (ushort)virtualKey;
            ushort scanCode = (ushort)MapVirtualKey(vkCode, MAPVK_VK_TO_VSC);
            
            // Create key down input with SCANCODE flag
            var inputDown = new INPUT
            {
                Type = INPUT_KEYBOARD,
                Data = new INPUTUNION
                {
                    Keyboard = new KEYBDINPUT
                    {
                        wVk = 0,  // Set to 0 when using scan code
                        wScan = scanCode,
                        dwFlags = KEYEVENTF_SCANCODE,  // Use scan code instead of virtual key
                        time = 0,
                        dwExtraInfo = IntPtr.Zero
                    }
                }
            };

            // Create key up input with SCANCODE flag
            var inputUp = new INPUT
            {
                Type = INPUT_KEYBOARD,
                Data = new INPUTUNION
                {
                    Keyboard = new KEYBDINPUT
                    {
                        wVk = 0,  // Set to 0 when using scan code
                        wScan = scanCode,
                        dwFlags = KEYEVENTF_SCANCODE | KEYEVENTF_KEYUP,  // Scan code + key up
                        time = 0,
                        dwExtraInfo = IntPtr.Zero
                    }
                }
            };

            // Send key down
            SendInput(1, new[] { inputDown }, Marshal.SizeOf(typeof(INPUT)));
            Thread.Sleep(KEY_PRESS_DELAY);

            // Send key up
            SendInput(1, new[] { inputUp }, Marshal.SizeOf(typeof(INPUT)));
            Thread.Sleep(KEY_RELEASE_DELAY);
        }

        private void ExecuteMouseButtonClick(string mouseButton)
        {
            var lowerButton = mouseButton.ToLower();
            
            switch (lowerButton)
            {
                case "lmb":
                case "leftclick":
                case "leftmouse":
                    mouse_event(0x0002, 0, 0, 0, 0); // MOUSEEVENTF_LEFTDOWN
                    Thread.Sleep(MOUSE_CLICK_DELAY);
                    mouse_event(0x0004, 0, 0, 0, 0); // MOUSEEVENTF_LEFTUP
                    Thread.Sleep(30);
                    break;
                    
                case "rmb":
                case "rightclick":
                case "rightmouse":
                    mouse_event(0x0008, 0, 0, 0, 0); // MOUSEEVENTF_RIGHTDOWN
                    Thread.Sleep(MOUSE_CLICK_DELAY);
                    mouse_event(0x0010, 0, 0, 0, 0); // MOUSEEVENTF_RIGHTUP
                    Thread.Sleep(30);
                    break;
                    
                case "mmb":
                case "middleclick":
                case "middlemouse":
                    ClickMiddleButton();
                    Thread.Sleep(30);
                    break;
                    
                case "mouse4":
                case "xbutton1":
                    ClickXButton(XBUTTON1);
                    Thread.Sleep(30);
                    break;
                    
                case "mouse5":
                case "xbutton2":
                    ClickXButton(XBUTTON2);
                    Thread.Sleep(30);
                    break;
            }
        }

        private async Task HoldKeyAsync(string key, int duration, CancellationToken cancellationToken)
        {
            var virtualKey = KeyMapper.GetVirtualKeyCode(key);
            ushort vkCode = (ushort)virtualKey;
            ushort scanCode = (ushort)MapVirtualKey(vkCode, MAPVK_VK_TO_VSC);
            
            // Create key down input with SCANCODE
            var inputDown = new INPUT
            {
                Type = INPUT_KEYBOARD,
                Data = new INPUTUNION
                {
                    Keyboard = new KEYBDINPUT
                    {
                        wVk = 0,
                        wScan = scanCode,
                        dwFlags = KEYEVENTF_SCANCODE,
                        time = 0,
                        dwExtraInfo = IntPtr.Zero
                    }
                }
            };

            // Send key down
            SendInput(1, new[] { inputDown }, Marshal.SizeOf(typeof(INPUT)));
            
            try
            {
                await Task.Delay(duration, cancellationToken);
            }
            finally
            {
                // Create key up input with SCANCODE
                var inputUp = new INPUT
                {
                    Type = INPUT_KEYBOARD,
                    Data = new INPUTUNION
                    {
                        Keyboard = new KEYBDINPUT
                        {
                            wVk = 0,
                            wScan = scanCode,
                            dwFlags = KEYEVENTF_SCANCODE | KEYEVENTF_KEYUP,
                            time = 0,
                            dwExtraInfo = IntPtr.Zero
                        }
                    }
                };

                // Send key up
                SendInput(1, new[] { inputUp }, Marshal.SizeOf(typeof(INPUT)));
            }
        }

        private void ClickMouse(Models.MouseButton button)
        {
            switch (button)
            {
                case Models.MouseButton.Left:
                    mouse_event(0x0002, 0, 0, 0, 0); // MOUSEEVENTF_LEFTDOWN
                    Thread.Sleep(MOUSE_CLICK_DELAY);
                    mouse_event(0x0004, 0, 0, 0, 0); // MOUSEEVENTF_LEFTUP
                    break;
                case Models.MouseButton.Right:
                    mouse_event(0x0008, 0, 0, 0, 0); // MOUSEEVENTF_RIGHTDOWN
                    Thread.Sleep(MOUSE_CLICK_DELAY);
                    mouse_event(0x0010, 0, 0, 0, 0); // MOUSEEVENTF_RIGHTUP
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
