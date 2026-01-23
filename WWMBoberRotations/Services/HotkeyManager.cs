using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Interop;
using WWMBoberRotations.Models;

namespace WWMBoberRotations.Services
{
    public class HotkeyManager : IDisposable
    {
        private readonly ComboExecutor _executor;
        private readonly Dictionary<string, Combo> _combos = new();
        private bool _isActive;
        private IntPtr _windowHandle;
        private string _panicButton = "rmb"; // Default panic button

        // Windows API for global hooks
        [DllImport("user32.dll")]
        private static extern short GetAsyncKeyState(int vKey);

        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        private const int WM_HOTKEY = 0x0312;
        private readonly Dictionary<int, string> _hotkeyIds = new();
        private int _nextHotkeyId = 1;

        public event EventHandler<string>? StatusChanged;

        public bool IsActive => _isActive;
        public string PanicButton => _panicButton;

        public HotkeyManager(ComboExecutor executor)
        {
            _executor = executor;
            _executor.StatusChanged += (s, msg) => StatusChanged?.Invoke(s, msg);
        }

        public void SetPanicButton(string key)
        {
            _panicButton = key.ToLower();
            StatusChanged?.Invoke(this, $"Panic button set to: {key}");
        }

        public void Initialize(IntPtr windowHandle)
        {
            _windowHandle = windowHandle;
        }

        public void UpdateCombos(IEnumerable<Combo> combos)
        {
            _combos.Clear();
            foreach (var combo in combos.Where(c => c.IsEnabled && !string.IsNullOrEmpty(c.Hotkey)))
            {
                _combos[combo.Hotkey!.ToLower()] = combo;
            }
        }

        public void Start()
        {
            if (_isActive) return;

            _isActive = true;
            RegisterAllHotkeys();
            StartMouseMonitoring();
            
            StatusChanged?.Invoke(this, "Macro system active");
        }

        public void Stop()
        {
            if (!_isActive) return;

            _isActive = false;
            UnregisterAllHotkeys();
            _executor.Stop();
            
            StatusChanged?.Invoke(this, "Macro system stopped");
        }

        private void RegisterAllHotkeys()
        {
            UnregisterAllHotkeys();

            foreach (var kvp in _combos)
            {
                var hotkey = kvp.Key;
                var vkCode = GetVirtualKeyCode(hotkey);
                
                if (vkCode != 0)
                {
                    var id = _nextHotkeyId++;
                    RegisterHotKey(_windowHandle, id, 0, (uint)vkCode);
                    _hotkeyIds[id] = hotkey;
                }
            }
        }

        private void UnregisterAllHotkeys()
        {
            foreach (var id in _hotkeyIds.Keys)
            {
                UnregisterHotKey(_windowHandle, id);
            }
            _hotkeyIds.Clear();
        }

        public async void HandleHotkey(int hotkeyId)
        {
            if (!_isActive) return;

            if (_hotkeyIds.TryGetValue(hotkeyId, out var hotkey))
            {
                if (_combos.TryGetValue(hotkey, out var combo))
                {
                    await _executor.ExecuteComboAsync(combo);
                }
            }
        }

        private void StartMouseMonitoring()
        {
            // Start background task to monitor panic button
            Task.Run(async () =>
            {
                while (_isActive)
                {
                    // Get virtual key code for panic button
                    int panicKeyCode = GetPanicButtonVirtualKey();
                    
                    if (panicKeyCode != 0 && (GetAsyncKeyState(panicKeyCode) & 0x8000) != 0)
                    {
                        if (_executor.IsExecuting)
                        {
                            _executor.Stop();
                            StatusChanged?.Invoke(this, "Combo cancelled (panic button)");
                            await Task.Delay(500); // Debounce
                        }
                    }
                    
                    await Task.Delay(50); // Check every 50ms
                }
            });
        }

        private int GetPanicButtonVirtualKey()
        {
            return _panicButton switch
            {
                "lmb" or "leftclick" or "leftmouse" => 0x01,
                "rmb" or "rightclick" or "rightmouse" => 0x02,
                "mmb" or "middleclick" or "middlemouse" => 0x04,
                "mouse4" or "xbutton1" => 0x05,
                "mouse5" or "xbutton2" => 0x06,
                _ => GetVirtualKeyCode(_panicButton)
            };
        }

        private int GetVirtualKeyCode(string key)
        {
            if (string.IsNullOrEmpty(key)) return 0;

            var lowerKey = key.ToLower();

            // Numbers
            if (lowerKey.Length == 1)
            {
                var c = lowerKey[0];
                if (c >= '0' && c <= '9') return 0x30 + (c - '0');
                if (c >= 'a' && c <= 'z') return 0x41 + (c - 'a');
            }

            // Special keys
            return lowerKey switch
            {
                // Common keys
                "space" or "spacebar" => 0x20,
                "enter" or "return" => 0x0D,
                "tab" => 0x09,
                "esc" or "escape" => 0x1B,
                "backspace" or "back" => 0x08,
                "delete" or "del" => 0x2E,
                "insert" or "ins" => 0x2D,
                
                // Modifier keys
                "shift" or "lshift" => 0x10,
                "rshift" => 0xA1,
                "ctrl" or "control" or "lctrl" => 0x11,
                "rctrl" => 0xA3,
                "alt" or "lalt" => 0x12,
                "ralt" => 0xA5,
                
                // Lock keys
                "capslock" or "caps" => 0x14,
                "numlock" or "num" => 0x90,
                "scrolllock" or "scroll" => 0x91,
                
                // Arrow keys
                "up" or "arrowup" or "uparrow" => 0x26,
                "down" or "arrowdown" or "downarrow" => 0x28,
                "left" or "arrowleft" or "leftarrow" => 0x25,
                "right" or "arrowright" or "rightarrow" => 0x27,
                
                // Navigation keys
                "home" => 0x24,
                "end" => 0x23,
                "pageup" or "pgup" => 0x21,
                "pagedown" or "pgdown" => 0x22,
                
                // Function keys
                "f1" => 0x70,
                "f2" => 0x71,
                "f3" => 0x72,
                "f4" => 0x73,
                "f5" => 0x74,
                "f6" => 0x75,
                "f7" => 0x76,
                "f8" => 0x77,
                "f9" => 0x78,
                "f10" => 0x79,
                "f11" => 0x7A,
                "f12" => 0x7B,
                
                _ => 0
            };
        }

        public void Dispose()
        {
            Stop();
        }
    }
}
