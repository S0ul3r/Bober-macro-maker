using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using WWMBoberRotations.Models;

namespace WWMBoberRotations.Services
{
    public class HotkeyManager : IDisposable
    {
        private readonly ComboExecutor _executor;
        private readonly Dictionary<string, Combo> _combos = new();
        private readonly Dictionary<int, string> _hotkeyIds = new();
        private bool _isActive;
        private IntPtr _windowHandle;
        private string _panicButton = "rmb";
        private int _nextHotkeyId = 1;
        private CancellationTokenSource? _monitoringCts;
        private CancellationTokenSource? _mouseMonitoringCts;
        private readonly Dictionary<string, bool> _mouseButtonStates = new();

        // Windows API for global hooks
        [DllImport("user32.dll")]
        private static extern short GetAsyncKeyState(int vKey);

        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        private const int WM_HOTKEY = 0x0312;
        private const int PANIC_BUTTON_CHECK_INTERVAL = 50;
        private const int PANIC_BUTTON_DEBOUNCE = 500;

        public event EventHandler<string>? StatusChanged;

        public bool IsActive => _isActive;
        public string PanicButton => _panicButton;

        public HotkeyManager(ComboExecutor executor)
        {
            _executor = executor ?? throw new ArgumentNullException(nameof(executor));
            _executor.StatusChanged += (s, msg) => StatusChanged?.Invoke(s, msg);
        }

        public void SetPanicButton(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                return;

            _panicButton = key.ToLower();
            StatusChanged?.Invoke(this, $"Panic button set to: {key}");
        }

        public void Initialize(IntPtr windowHandle)
        {
            _windowHandle = windowHandle;
        }

        public void UpdateCombos(IEnumerable<Combo> combos)
        {
            // Check for duplicate hotkeys
            var hotkeyCombos = combos.Where(c => c.IsEnabled && !string.IsNullOrEmpty(c.Hotkey)).ToList();
            var duplicates = hotkeyCombos
                .GroupBy(c => c.Hotkey!.ToLower())
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToList();

            if (duplicates.Any())
            {
                StatusChanged?.Invoke(this, $"Warning: Duplicate hotkeys detected: {string.Join(", ", duplicates)}");
            }

            // Clear and rebuild combo dictionary
            _combos.Clear();
            foreach (var combo in hotkeyCombos)
            {
                var hotkeyLower = combo.Hotkey!.ToLower();
                if (!_combos.ContainsKey(hotkeyLower)) // Only add first combo with this hotkey
                {
                    _combos[hotkeyLower] = combo;
                }
            }

            // Re-register hotkeys if system is active
            if (_isActive)
            {
                RegisterAllHotkeys();
            }
        }

        public void Start()
        {
            if (_isActive) 
                return;

            _isActive = true;
            RegisterAllHotkeys();
            StartPanicButtonMonitoring();
            
            StatusChanged?.Invoke(this, "Macro system active");
        }

        public void Stop()
        {
            if (!_isActive) 
                return;

            _isActive = false;
            StopPanicButtonMonitoring();
            StopMouseButtonMonitoring();
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
                
                // Skip mouse buttons - they cannot be registered as global hotkeys
                // They will be handled through GetAsyncKeyState monitoring
                if (KeyMapper.IsMouseButton(hotkey))
                {
                    continue;
                }
                
                var vkCode = KeyMapper.GetVirtualKeyCode(hotkey);
                
                if (vkCode != 0)
                {
                    var id = _nextHotkeyId++;
                    if (RegisterHotKey(_windowHandle, id, 0, (uint)vkCode))
                    {
                        _hotkeyIds[id] = hotkey;
                    }
                }
            }

            // Start monitoring mouse buttons if any combo uses them
            if (_combos.Keys.Any(k => KeyMapper.IsMouseButton(k)))
            {
                StartMouseButtonMonitoring();
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

        public async Task HandleHotkeyAsync(int hotkeyId)
        {
            if (!_isActive) 
                return;

            if (_hotkeyIds.TryGetValue(hotkeyId, out var hotkey) &&
                _combos.TryGetValue(hotkey, out var combo))
            {
                await _executor.ExecuteComboAsync(combo);
            }
        }

        private void StartPanicButtonMonitoring()
        {
            _monitoringCts?.Cancel();
            _monitoringCts = new CancellationTokenSource();

            Task.Run(async () => await MonitorPanicButtonAsync(_monitoringCts.Token), _monitoringCts.Token);
        }

        private void StopPanicButtonMonitoring()
        {
            _monitoringCts?.Cancel();
            _monitoringCts?.Dispose();
            _monitoringCts = null;
        }

        private async Task MonitorPanicButtonAsync(CancellationToken cancellationToken)
        {
            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    var panicKeyCode = GetPanicButtonVirtualKey();
                    
                    if (panicKeyCode != 0 && (GetAsyncKeyState(panicKeyCode) & 0x8000) != 0)
                    {
                        if (_executor.IsExecuting)
                        {
                            _executor.Stop();
                            StatusChanged?.Invoke(this, "Combo cancelled (panic button)");
                            await Task.Delay(PANIC_BUTTON_DEBOUNCE, cancellationToken);
                        }
                    }
                    
                    await Task.Delay(PANIC_BUTTON_CHECK_INTERVAL, cancellationToken);
                }
            }
            catch (OperationCanceledException)
            {
                // Expected when cancellation is requested
            }
        }

        private int GetPanicButtonVirtualKey()
        {
            var mouseCode = KeyMapper.GetMouseButtonCode(_panicButton);
            return mouseCode != 0 ? mouseCode : KeyMapper.GetVirtualKeyCode(_panicButton);
        }

        private void StartMouseButtonMonitoring()
        {
            _mouseMonitoringCts?.Cancel();
            _mouseMonitoringCts = new CancellationTokenSource();

            Task.Run(async () => await MonitorMouseButtonsAsync(_mouseMonitoringCts.Token), _mouseMonitoringCts.Token);
        }

        private void StopMouseButtonMonitoring()
        {
            _mouseMonitoringCts?.Cancel();
            _mouseMonitoringCts?.Dispose();
            _mouseMonitoringCts = null;
            _mouseButtonStates.Clear();
        }

        private async Task MonitorMouseButtonsAsync(CancellationToken cancellationToken)
        {
            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    foreach (var kvp in _combos.Where(c => KeyMapper.IsMouseButton(c.Key)))
                    {
                        var hotkey = kvp.Key;
                        var combo = kvp.Value;
                        var mouseCode = KeyMapper.GetMouseButtonCode(hotkey);

                        if (mouseCode != 0)
                        {
                            var isPressed = (GetAsyncKeyState(mouseCode) & 0x8000) != 0;
                            var wasPressed = _mouseButtonStates.GetValueOrDefault(hotkey, false);

                            // Detect rising edge (button just pressed)
                            if (isPressed && !wasPressed && !_executor.IsExecuting)
                            {
                                _ = _executor.ExecuteComboAsync(combo);
                            }

                            _mouseButtonStates[hotkey] = isPressed;
                        }
                    }

                    await Task.Delay(50, cancellationToken); // Check every 50ms
                }
            }
            catch (OperationCanceledException)
            {
                // Expected when cancellation is requested
            }
        }

        public void Dispose()
        {
            Stop();
            _monitoringCts?.Dispose();
            _mouseMonitoringCts?.Dispose();
        }
    }
}
