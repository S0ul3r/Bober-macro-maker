using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using WWMBoberRotations.Models;

namespace WWMBoberRotations.Services
{
    public class MacroRecorder
    {
        [DllImport("user32.dll")]
        private static extern short GetAsyncKeyState(int vKey);

        private const int KEY_POLL_INTERVAL = 10; // Poll every 10ms for responsive detection
        private const int MIN_DELAY_TO_RECORD = 50; // Minimum delay to record (avoid spam)
        
        private readonly List<ComboAction> _recordedActions = new();
        private readonly HashSet<int> _pressedKeys = new();
        private CancellationTokenSource? _recordingCts;
        private Stopwatch? _timingSw;
        private bool _isRecording;
        private string _stopHotkey = "insert";

        public event EventHandler<ComboAction>? ActionRecorded;
        public event EventHandler<string>? StatusChanged;

        public bool IsRecording => _isRecording;
        public IReadOnlyList<ComboAction> RecordedActions => _recordedActions.AsReadOnly();

        public void SetStopHotkey(string hotkey)
        {
            _stopHotkey = hotkey?.ToLower() ?? "insert";
        }

        public async Task StartRecordingAsync()
        {
            if (_isRecording)
                return;

            _recordedActions.Clear();
            _pressedKeys.Clear();
            _isRecording = true;
            _timingSw = Stopwatch.StartNew();
            _recordingCts = new CancellationTokenSource();

            StatusChanged?.Invoke(this, "Recording started - press keys to record");

            await Task.Run(() => RecordingLoop(_recordingCts.Token));
        }

        public void StopRecording()
        {
            if (!_isRecording)
                return;

            _isRecording = false;
            _recordingCts?.Cancel();
            _timingSw?.Stop();
            
            StatusChanged?.Invoke(this, $"Recording stopped - {_recordedActions.Count} actions recorded");
        }

        private void RecordingLoop(CancellationToken cancellationToken)
        {
            var lastActionTime = 0L;

            try
            {
                while (!cancellationToken.IsCancellationRequested && _isRecording)
                {
                    // Check all possible keys (but NOT the stop hotkey - it's handled by the window)
                    CheckAndRecordKeys(ref lastActionTime);
                    CheckAndRecordMouseButtons(ref lastActionTime);

                    Thread.Sleep(KEY_POLL_INTERVAL);
                }
            }
            catch (Exception ex)
            {
                StatusChanged?.Invoke(this, $"Recording error: {ex.Message}");
                StopRecording();
            }
        }

        private void CheckAndRecordKeys(ref long lastActionTime)
        {
            // Check A-Z
            for (int i = 0x41; i <= 0x5A; i++)
            {
                CheckKey(i, ((char)i).ToString().ToLower(), ref lastActionTime);
            }

            // Check 0-9
            for (int i = 0x30; i <= 0x39; i++)
            {
                CheckKey(i, ((char)i).ToString(), ref lastActionTime);
            }

            // Check special keys
            var specialKeys = new Dictionary<int, string>
            {
                [0x20] = "space",
                [0x0D] = "enter",
                [0x09] = "tab",
                [0x1B] = "esc",
                [0x10] = "shift",
                [0x11] = "ctrl",
                [0x12] = "alt",
                [0x26] = "up",
                [0x28] = "down",
                [0x25] = "left",
                [0x27] = "right",
                [0x70] = "f1",
                [0x71] = "f2",
                [0x72] = "f3",
                [0x73] = "f4",
                [0x74] = "f5",
                [0x75] = "f6",
                [0x76] = "f7",
                [0x77] = "f8",
                [0x78] = "f9",
                [0x79] = "f10",
                [0x7A] = "f11",
                [0x7B] = "f12",
            };

            foreach (var kvp in specialKeys)
            {
                CheckKey(kvp.Key, kvp.Value, ref lastActionTime);
            }
        }

        private void CheckAndRecordMouseButtons(ref long lastActionTime)
        {
            var mouseButtons = new Dictionary<int, string>
            {
                [0x01] = "lmb",
                [0x02] = "rmb",
                [0x04] = "mmb",
                [0x05] = "mouse4",
                [0x06] = "mouse5",
            };

            foreach (var kvp in mouseButtons)
            {
                CheckKey(kvp.Key, kvp.Value, ref lastActionTime);
            }
        }

        private void CheckKey(int keyCode, string keyName, ref long lastActionTime)
        {
            // Skip the stop hotkey - don't record it
            var stopKeyCode = GetStopHotkeyCode();
            if (keyCode == stopKeyCode)
                return;

            var isPressed = IsKeyPressed(keyCode);

            // Detect key press (rising edge)
            if (isPressed && !_pressedKeys.Contains(keyCode))
            {
                _pressedKeys.Add(keyCode);

                // Add delay if this is not the first action
                if (_recordedActions.Count > 0 && _timingSw != null)
                {
                    var currentTime = _timingSw.ElapsedMilliseconds;
                    var delay = (int)(currentTime - lastActionTime);

                    if (delay >= MIN_DELAY_TO_RECORD)
                    {
                        var delayAction = new ComboAction
                        {
                            Type = ActionType.Delay,
                            Duration = delay
                        };
                        _recordedActions.Add(delayAction);
                        ActionRecorded?.Invoke(this, delayAction);
                    }

                    lastActionTime = currentTime;
                }
                else if (_timingSw != null)
                {
                    lastActionTime = _timingSw.ElapsedMilliseconds;
                }

                // Add key press action
                var keyAction = new ComboAction
                {
                    Type = ActionType.KeyPress,
                    Key = keyName
                };
                _recordedActions.Add(keyAction);
                ActionRecorded?.Invoke(this, keyAction);
            }
            // Detect key release (falling edge)
            else if (!isPressed && _pressedKeys.Contains(keyCode))
            {
                _pressedKeys.Remove(keyCode);
            }
        }

        private bool IsKeyPressed(int keyCode)
        {
            return (GetAsyncKeyState(keyCode) & 0x8000) != 0;
        }

        private int GetStopHotkeyCode()
        {
            var mouseCode = KeyMapper.GetMouseButtonCode(_stopHotkey);
            return mouseCode != 0 ? mouseCode : KeyMapper.GetVirtualKeyCode(_stopHotkey);
        }

        public List<ComboAction> GetRecordedActions()
        {
            return new List<ComboAction>(_recordedActions);
        }

        public void RemoveActionAt(int index)
        {
            if (index >= 0 && index < _recordedActions.Count)
            {
                _recordedActions.RemoveAt(index);
            }
        }

        public void InsertActionAt(int index, ComboAction action)
        {
            if (index >= 0 && index <= _recordedActions.Count)
            {
                _recordedActions.Insert(index, action);
            }
        }

        public void ClearRecording()
        {
            _recordedActions.Clear();
            _pressedKeys.Clear();
        }
    }
}
