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

        private const int KEY_POLL_INTERVAL = 10;
        private const int MIN_DELAY_TO_RECORD = 10;
        private const int EXECUTION_KEY_PRESS_DELAY = 20;
        
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
            var lastActionPressTime = 0L;

            try
            {
                while (!cancellationToken.IsCancellationRequested && _isRecording)
                {
                    CheckAndRecordKeys(ref lastActionPressTime);
                    CheckAndRecordMouseButtons(ref lastActionPressTime);
                    Thread.Sleep(KEY_POLL_INTERVAL);
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Recording loop error", ex);
                StatusChanged?.Invoke(this, $"Recording error: {ex.Message}");
                StopRecording();
            }
        }

        private void CheckAndRecordKeys(ref long lastActionTime)
        {
            for (int i = 0x41; i <= 0x5A; i++)
            {
                CheckKey(i, ((char)i).ToString().ToLower(), ref lastActionTime);
            }

            for (int i = 0x30; i <= 0x39; i++)
            {
                CheckKey(i, ((char)i).ToString(), ref lastActionTime);
            }

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
            var mouseButtons = KeyMapper.GetAllMouseButtonCodes();
            foreach (var kvp in mouseButtons)
            {
                CheckKey(kvp.Key, kvp.Value, ref lastActionTime);
            }
        }

        private void CheckKey(int keyCode, string keyName, ref long lastActionPressTime)
        {
            var stopKeyCode = GetStopHotkeyCode();
            if (keyCode == stopKeyCode)
                return;

            var isPressed = IsKeyPressed(keyCode);

            if (isPressed && !_pressedKeys.Contains(keyCode))
            {
                _pressedKeys.Add(keyCode);
                var currentTime = _timingSw?.ElapsedMilliseconds ?? 0;

                if (_recordedActions.Count > 0 && lastActionPressTime > 0)
                {
                    var rawDelay = (int)(currentTime - lastActionPressTime);
                    var compensatedDelay = Math.Max(0, rawDelay - EXECUTION_KEY_PRESS_DELAY);

                    if (compensatedDelay >= MIN_DELAY_TO_RECORD || rawDelay >= MIN_DELAY_TO_RECORD)
                    {
                        var lastAction = _recordedActions[_recordedActions.Count - 1];
                        lastAction.DelayAfter = compensatedDelay;
                    }
                }

                lastActionPressTime = currentTime;

                var keyAction = new ComboAction
                {
                    Type = ActionType.KeyPress,
                    Key = keyName,
                    DelayAfter = 0
                };
                _recordedActions.Add(keyAction);
                ActionRecorded?.Invoke(this, keyAction);
            }
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
