using System.Collections.Generic;

namespace WWMBoberRotations.Services
{
    /// <summary>
    /// Centralized key mapping service for converting string key names to virtual key codes
    /// </summary>
    public static class KeyMapper
    {
        private static readonly Dictionary<string, int> _virtualKeyCodes = new()
        {
            ["space"] = 0x20,
            ["spacebar"] = 0x20,
            ["enter"] = 0x0D,
            ["return"] = 0x0D,
            ["tab"] = 0x09,
            ["esc"] = 0x1B,
            ["escape"] = 0x1B,
            ["backspace"] = 0x08,
            ["back"] = 0x08,
            ["delete"] = 0x2E,
            ["del"] = 0x2E,
            ["insert"] = 0x2D,
            ["ins"] = 0x2D,
            ["shift"] = 0x10,
            ["lshift"] = 0x10,
            ["rshift"] = 0xA1,
            ["ctrl"] = 0x11,
            ["control"] = 0x11,
            ["lctrl"] = 0x11,
            ["rctrl"] = 0xA3,
            ["alt"] = 0x12,
            ["lalt"] = 0x12,
            ["ralt"] = 0xA5,
            ["capslock"] = 0x14,
            ["caps"] = 0x14,
            ["numlock"] = 0x90,
            ["num"] = 0x90,
            ["scrolllock"] = 0x91,
            ["scroll"] = 0x91,
            ["up"] = 0x26,
            ["arrowup"] = 0x26,
            ["uparrow"] = 0x26,
            ["down"] = 0x28,
            ["arrowdown"] = 0x28,
            ["downarrow"] = 0x28,
            ["left"] = 0x25,
            ["arrowleft"] = 0x25,
            ["leftarrow"] = 0x25,
            ["right"] = 0x27,
            ["arrowright"] = 0x27,
            ["rightarrow"] = 0x27,
            ["home"] = 0x24,
            ["end"] = 0x23,
            ["pageup"] = 0x21,
            ["pgup"] = 0x21,
            ["pagedown"] = 0x22,
            ["pgdown"] = 0x22,
            ["f1"] = 0x70,
            ["f2"] = 0x71,
            ["f3"] = 0x72,
            ["f4"] = 0x73,
            ["f5"] = 0x74,
            ["f6"] = 0x75,
            ["f7"] = 0x76,
            ["f8"] = 0x77,
            ["f9"] = 0x78,
            ["f10"] = 0x79,
            ["f11"] = 0x7A,
            ["f12"] = 0x7B,
        };

        private static readonly Dictionary<string, int> _mouseButtons = new()
        {
            ["lmb"] = 0x01,
            ["leftclick"] = 0x01,
            ["leftmouse"] = 0x01,
            ["rmb"] = 0x02,
            ["rightclick"] = 0x02,
            ["rightmouse"] = 0x02,
            ["mmb"] = 0x04,
            ["middleclick"] = 0x04,
            ["middlemouse"] = 0x04,
            ["mouse4"] = 0x05,
            ["xbutton1"] = 0x05,
            ["mouse5"] = 0x06,
            ["xbutton2"] = 0x06,
        };

        private static readonly System.Collections.Concurrent.ConcurrentDictionary<string, string> _lowerCaseCache = new();

        private static string CachedToLower(string key)
        {
            if (string.IsNullOrEmpty(key))
                return string.Empty;
            
            return _lowerCaseCache.GetOrAdd(key, k => k.ToLower());
        }

        public static int GetVirtualKeyCode(string key)
        {
            if (string.IsNullOrEmpty(key))
                return 0;

            var lowerKey = CachedToLower(key);

            if (_virtualKeyCodes.TryGetValue(lowerKey, out var code))
                return code;

            if (lowerKey.Length == 1)
            {
                var c = lowerKey[0];
                if (c >= '0' && c <= '9')
                    return 0x30 + (c - '0');
                if (c >= 'a' && c <= 'z')
                    return 0x41 + (c - 'a');
            }

            return 0;
        }

        public static int GetMouseButtonCode(string button)
        {
            if (string.IsNullOrEmpty(button))
                return 0;

            var lowerButton = CachedToLower(button);
            return _mouseButtons.TryGetValue(lowerButton, out var code) ? code : 0;
        }

        public static bool IsMouseButton(string key)
        {
            return !string.IsNullOrEmpty(key) && _mouseButtons.ContainsKey(CachedToLower(key));
        }

        /// <summary>
        /// Gets all mouse button virtual key codes for monitoring/recording
        /// </summary>
        public static Dictionary<int, string> GetAllMouseButtonCodes()
        {
            return new Dictionary<int, string>
            {
                [0x01] = "lmb",
                [0x02] = "rmb",
                [0x04] = "mmb",
                [0x05] = "mouse4",
                [0x06] = "mouse5"
            };
        }

        /// <summary>
        /// Converts WPF Key enum to lowercase string representation
        /// </summary>
        public static string WpfKeyToString(System.Windows.Input.Key key)
        {
            return key switch
            {
                System.Windows.Input.Key.Space => "space",
                System.Windows.Input.Key.Return or System.Windows.Input.Key.Enter => "enter",
                System.Windows.Input.Key.Escape => "esc",
                System.Windows.Input.Key.Tab => "tab",
                System.Windows.Input.Key.Back => "backspace",
                System.Windows.Input.Key.Delete => "delete",
                System.Windows.Input.Key.Insert => "insert",
                System.Windows.Input.Key.CapsLock => "capslock",
                System.Windows.Input.Key.NumLock => "numlock",
                System.Windows.Input.Key.Scroll => "scrolllock",
                System.Windows.Input.Key.Up => "up",
                System.Windows.Input.Key.Down => "down",
                System.Windows.Input.Key.Left => "left",
                System.Windows.Input.Key.Right => "right",
                System.Windows.Input.Key.Home => "home",
                System.Windows.Input.Key.End => "end",
                System.Windows.Input.Key.PageUp => "pageup",
                System.Windows.Input.Key.PageDown => "pagedown",
                System.Windows.Input.Key.LeftShift => "lshift",
                System.Windows.Input.Key.RightShift => "rshift",
                System.Windows.Input.Key.LeftCtrl => "lctrl",
                System.Windows.Input.Key.RightCtrl => "rctrl",
                System.Windows.Input.Key.LeftAlt => "lalt",
                System.Windows.Input.Key.RightAlt => "ralt",
                >= System.Windows.Input.Key.F1 and <= System.Windows.Input.Key.F12 => 
                    $"f{(int)key - (int)System.Windows.Input.Key.F1 + 1}",
                >= System.Windows.Input.Key.D0 and <= System.Windows.Input.Key.D9 =>
                    ((char)('0' + (key - System.Windows.Input.Key.D0))).ToString(),
                >= System.Windows.Input.Key.A and <= System.Windows.Input.Key.Z =>
                    ((char)('a' + (key - System.Windows.Input.Key.A))).ToString(),
                _ => key.ToString().ToLower()
            };
        }
    }
}
