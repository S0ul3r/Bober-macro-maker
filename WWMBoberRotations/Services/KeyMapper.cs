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
            // Common keys
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
            
            // Modifier keys
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
            
            // Lock keys
            ["capslock"] = 0x14,
            ["caps"] = 0x14,
            ["numlock"] = 0x90,
            ["num"] = 0x90,
            ["scrolllock"] = 0x91,
            ["scroll"] = 0x91,
            
            // Arrow keys
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
            
            // Navigation keys
            ["home"] = 0x24,
            ["end"] = 0x23,
            ["pageup"] = 0x21,
            ["pgup"] = 0x21,
            ["pagedown"] = 0x22,
            ["pgdown"] = 0x22,
            
            // Function keys
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

        public static int GetVirtualKeyCode(string key)
        {
            if (string.IsNullOrEmpty(key))
                return 0;

            var lowerKey = key.ToLower();

            // Check special keys dictionary
            if (_virtualKeyCodes.TryGetValue(lowerKey, out var code))
                return code;

            // Check if it's a single character (number or letter)
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

            var lowerButton = button.ToLower();
            return _mouseButtons.TryGetValue(lowerButton, out var code) ? code : 0;
        }

        public static bool IsMouseButton(string key)
        {
            return !string.IsNullOrEmpty(key) && _mouseButtons.ContainsKey(key.ToLower());
        }
    }
}
