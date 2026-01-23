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
            _combos.Clear();
            foreach (var combo in combos.Where(c => c.IsEnabled && !string.IsNullOrEmpty(c.Hotkey)))
            {
                _combos[combo.Hotkey!.ToLower()] = combo;
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
                var vkCode = KeyMapper.GetVirtualKeyCode(hotkey);
                
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

        public void Dispose()
        {
            Stop();
            _monitoringCts?.Dispose();
        }
    }
}
