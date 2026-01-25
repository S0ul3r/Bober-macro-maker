using System;
using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using WWMBoberRotations.Models;

namespace WWMBoberRotations.Services
{
    public class InputSimulatorService
    {
        private readonly ConcurrentDictionary<int, DateTime> _simulatedInputs = new();
        private const int SIMULATION_TIMEOUT_MS = 100;
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
        
        private const int MOUSE_CLICK_DELAY = 20;
        private const int KEY_PRESS_DELAY = 20;

        public bool IsSimulatingInput(int keyCode)
        {
            if (_simulatedInputs.TryGetValue(keyCode, out var timestamp))
            {
                var elapsed = (DateTime.UtcNow - timestamp).TotalMilliseconds;
                if (elapsed < SIMULATION_TIMEOUT_MS)
                {
                    return true;
                }
                else
                {
                    _simulatedInputs.TryRemove(keyCode, out _);
                }
            }
            return false;
        }

        public async Task ExecuteActionAsync(ComboAction action, CancellationToken cancellationToken)
        {
            switch (action.Type)
            {
                case ActionType.KeyPress:
                    await PressKeyOrMouseButtonAsync(action.Key!, cancellationToken);
                    break;

                case ActionType.KeyHold:
                    await HoldKeyAsync(action.Key!, action.Duration, cancellationToken);
                    break;

                case ActionType.MouseClick:
                    await ClickMouseAsync(action.Button, cancellationToken);
                    break;

                case ActionType.Delay:
                    await DelayAsync(action.Duration, cancellationToken);
                    break;
            }
        }

        private async Task PressKeyOrMouseButtonAsync(string key, CancellationToken cancellationToken)
        {
            if (KeyMapper.IsMouseButton(key))
            {
                await ExecuteMouseButtonClickAsync(key, cancellationToken);
                return;
            }
            
            var virtualKey = KeyMapper.GetVirtualKeyCode(key);
            ushort vkCode = (ushort)virtualKey;
            ushort scanCode = (ushort)MapVirtualKey(vkCode, MAPVK_VK_TO_VSC);
            
            _simulatedInputs[vkCode] = DateTime.UtcNow;
            
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

            SendInput(1, new[] { inputDown }, Marshal.SizeOf(typeof(INPUT)));
            await Task.Delay(KEY_PRESS_DELAY, cancellationToken);
            SendInput(1, new[] { inputUp }, Marshal.SizeOf(typeof(INPUT)));
        }

        private async Task ExecuteMouseButtonClickAsync(string mouseButton, CancellationToken cancellationToken)
        {
            var lowerButton = mouseButton.ToLower();
            int mouseCode = 0;
            
            switch (lowerButton)
            {
                case "lmb":
                case "leftclick":
                case "leftmouse":
                    mouseCode = 0x01;
                    _simulatedInputs[mouseCode] = DateTime.UtcNow;
                    mouse_event(0x0002, 0, 0, 0, 0);
                    await Task.Delay(MOUSE_CLICK_DELAY, cancellationToken);
                    mouse_event(0x0004, 0, 0, 0, 0);
                    break;
                    
                case "rmb":
                case "rightclick":
                case "rightmouse":
                    mouseCode = 0x02;
                    _simulatedInputs[mouseCode] = DateTime.UtcNow;
                    mouse_event(0x0008, 0, 0, 0, 0);
                    await Task.Delay(MOUSE_CLICK_DELAY, cancellationToken);
                    mouse_event(0x0010, 0, 0, 0, 0);
                    break;
                    
                case "mmb":
                case "middleclick":
                case "middlemouse":
                    mouseCode = 0x04;
                    _simulatedInputs[mouseCode] = DateTime.UtcNow;
                    await ClickMiddleButtonAsync(cancellationToken);
                    break;
                    
                case "mouse4":
                case "xbutton1":
                    mouseCode = 0x05;
                    _simulatedInputs[mouseCode] = DateTime.UtcNow;
                    await ClickXButtonAsync(XBUTTON1, cancellationToken);
                    break;
                    
                case "mouse5":
                case "xbutton2":
                    mouseCode = 0x06;
                    _simulatedInputs[mouseCode] = DateTime.UtcNow;
                    await ClickXButtonAsync(XBUTTON2, cancellationToken);
                    break;
            }
        }

        private async Task HoldKeyAsync(string key, int duration, CancellationToken cancellationToken)
        {
            var virtualKey = KeyMapper.GetVirtualKeyCode(key);
            ushort vkCode = (ushort)virtualKey;
            ushort scanCode = (ushort)MapVirtualKey(vkCode, MAPVK_VK_TO_VSC);
            
            _simulatedInputs[vkCode] = DateTime.UtcNow;
            
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

            SendInput(1, new[] { inputDown }, Marshal.SizeOf(typeof(INPUT)));
            
            try
            {
                await Task.Delay(duration, cancellationToken);
            }
            finally
            {
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

                SendInput(1, new[] { inputUp }, Marshal.SizeOf(typeof(INPUT)));
            }
        }

        private async Task ClickMouseAsync(Models.MouseButton button, CancellationToken cancellationToken)
        {
            int mouseCode = 0;
            switch (button)
            {
                case Models.MouseButton.Left:
                    mouseCode = 0x01;
                    _simulatedInputs[mouseCode] = DateTime.UtcNow;
                    mouse_event(0x0002, 0, 0, 0, 0);
                    await Task.Delay(MOUSE_CLICK_DELAY, cancellationToken);
                    mouse_event(0x0004, 0, 0, 0, 0);
                    break;
                case Models.MouseButton.Right:
                    mouseCode = 0x02;
                    _simulatedInputs[mouseCode] = DateTime.UtcNow;
                    mouse_event(0x0008, 0, 0, 0, 0);
                    await Task.Delay(MOUSE_CLICK_DELAY, cancellationToken);
                    mouse_event(0x0010, 0, 0, 0, 0);
                    break;
                case Models.MouseButton.Middle:
                    mouseCode = 0x04;
                    _simulatedInputs[mouseCode] = DateTime.UtcNow;
                    await ClickMiddleButtonAsync(cancellationToken);
                    break;
                case Models.MouseButton.XButton1:
                    mouseCode = 0x05;
                    _simulatedInputs[mouseCode] = DateTime.UtcNow;
                    await ClickXButtonAsync(XBUTTON1, cancellationToken);
                    break;
                case Models.MouseButton.XButton2:
                    mouseCode = 0x06;
                    _simulatedInputs[mouseCode] = DateTime.UtcNow;
                    await ClickXButtonAsync(XBUTTON2, cancellationToken);
                    break;
            }
        }

        private async Task ClickMiddleButtonAsync(CancellationToken cancellationToken)
        {
            mouse_event(MOUSEEVENTF_MIDDLEDOWN, 0, 0, 0, 0);
            await Task.Delay(MOUSE_CLICK_DELAY, cancellationToken);
            mouse_event(MOUSEEVENTF_MIDDLEUP, 0, 0, 0, 0);
        }

        private async Task ClickXButtonAsync(uint button, CancellationToken cancellationToken)
        {
            mouse_event(MOUSEEVENTF_XDOWN, 0, 0, button, 0);
            await Task.Delay(MOUSE_CLICK_DELAY, cancellationToken);
            mouse_event(MOUSEEVENTF_XUP, 0, 0, button, 0);
        }

        private static async Task DelayAsync(int duration, CancellationToken cancellationToken)
        {
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
