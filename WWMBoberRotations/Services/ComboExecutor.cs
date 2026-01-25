using System;
using System.Threading;
using System.Threading.Tasks;
using WWMBoberRotations.Models;

namespace WWMBoberRotations.Services
{
    public class ComboExecutor
    {
        private readonly InputSimulatorService _inputSimulator;
        private CancellationTokenSource? _currentExecutionCts;
        private bool _isExecuting;

        public event EventHandler<string>? StatusChanged;

        public bool IsExecuting => _isExecuting;
        public InputSimulatorService InputSimulator => _inputSimulator;

        public ComboExecutor()
        {
            _inputSimulator = new InputSimulatorService();
        }

        public async Task ExecuteComboAsync(Combo combo)
        {
            if (_isExecuting)
            {
                Stop();
                await Task.Delay(100);
            }

            _currentExecutionCts = new CancellationTokenSource();
            _isExecuting = true;

            try
            {
                StatusChanged?.Invoke(this, $"Executing: {combo.Name}");

                foreach (var action in combo.Actions)
                {
                    if (_currentExecutionCts.Token.IsCancellationRequested)
                        break;

                    await _inputSimulator.ExecuteActionAsync(action, _currentExecutionCts.Token);
                    
                    if (action.DelayAfter > 0 && !_currentExecutionCts.Token.IsCancellationRequested)
                    {
                        await Task.Delay(action.DelayAfter, _currentExecutionCts.Token);
                    }
                }

                StatusChanged?.Invoke(this, "Combo completed");
            }
            catch (OperationCanceledException)
            {
                StatusChanged?.Invoke(this, "Combo cancelled");
            }
            catch (Exception ex)
            {
                StatusChanged?.Invoke(this, $"Error: {ex.Message}");
            }
            finally
            {
                _isExecuting = false;
                _currentExecutionCts?.Dispose();
                _currentExecutionCts = null;
            }
        }

        public void Stop()
        {
            _currentExecutionCts?.Cancel();
            StatusChanged?.Invoke(this, "Stopping combo...");
        }
    }
}
