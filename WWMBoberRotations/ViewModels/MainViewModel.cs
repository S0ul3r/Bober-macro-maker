using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;
using WWMBoberRotations.Models;
using WWMBoberRotations.Services;
using WWMBoberRotations.Views;

namespace WWMBoberRotations.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private readonly ComboStorageService _storageService;
        private readonly ComboExecutor _executor;
        private readonly HotkeyManager _hotkeyManager;
        
        private string _statusMessage = "Ready";
        private bool _isSystemActive;
        private Combo? _selectedCombo;
        private string _panicButtonText = "Panic Button: rmb (click to change)";
        private bool _isWaitingForPanicButton;
        private bool _isAutoSaveEnabled = true;
        private CancellationTokenSource? _autoSaveCts;
        private bool _hasUnsavedChanges;

        private const int AUTO_SAVE_INTERVAL_SECONDS = 30;

        public ObservableCollection<Combo> Combos { get; } = new();

        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        public bool IsSystemActive
        {
            get => _isSystemActive;
            set => SetProperty(ref _isSystemActive, value);
        }

        public Combo? SelectedCombo
        {
            get => _selectedCombo;
            set => SetProperty(ref _selectedCombo, value);
        }

        public string PanicButtonText
        {
            get => _panicButtonText;
            set => SetProperty(ref _panicButtonText, value);
        }

        public bool IsAutoSaveEnabled
        {
            get => _isAutoSaveEnabled;
            set
            {
                if (SetProperty(ref _isAutoSaveEnabled, value))
                {
                    if (value)
                        StartAutoSave();
                    else
                        StopAutoSave();
                }
            }
        }

        public bool HasUnsavedChanges
        {
            get => _hasUnsavedChanges;
            set => SetProperty(ref _hasUnsavedChanges, value);
        }

        public ICommand ToggleSystemCommand { get; }
        public ICommand NewComboCommand { get; }
        public ICommand EditComboCommand { get; }
        public ICommand DeleteComboCommand { get; }
        public ICommand SaveCombosCommand { get; }
        public ICommand LoadCombosCommand { get; }
        public ICommand ExportCommand { get; }
        public ICommand ImportCommand { get; }
        public ICommand SetPanicButtonCommand { get; }

        public MainViewModel()
        {
            _storageService = new ComboStorageService();
            _executor = new ComboExecutor();
            _hotkeyManager = new HotkeyManager(_executor);

            _hotkeyManager.StatusChanged += (s, msg) => StatusMessage = msg;

            // Monitor collection changes for auto-save
            Combos.CollectionChanged += OnCombosChanged;

            // Commands
            ToggleSystemCommand = new RelayCommand(ToggleSystem);
            NewComboCommand = new RelayCommand(NewCombo);
            EditComboCommand = new RelayCommand(EditCombo, () => SelectedCombo != null);
            DeleteComboCommand = new RelayCommand(DeleteCombo, () => SelectedCombo != null);
            SaveCombosCommand = new RelayCommand(SaveCombos);
            LoadCombosCommand = new RelayCommand(LoadCombos);
            ExportCommand = new RelayCommand(ExportCombos);
            ImportCommand = new RelayCommand(ImportCombos);
            SetPanicButtonCommand = new RelayCommand(SetPanicButton);

            // Check for autosave before loading
            CheckForAutoSave();
            
            // Start auto-save timer
            if (IsAutoSaveEnabled)
                StartAutoSave();
        }

        public void InitializeHotkeyManager(IntPtr windowHandle)
        {
            _hotkeyManager.Initialize(windowHandle);
        }

        public async Task HandleHotkeyAsync(int hotkeyId)
        {
            await _hotkeyManager.HandleHotkeyAsync(hotkeyId);
        }

        private void ToggleSystem()
        {
            if (IsSystemActive)
            {
                _hotkeyManager.Stop();
                IsSystemActive = false;
            }
            else
            {
                _hotkeyManager.UpdateCombos(Combos);
                _hotkeyManager.Start();
                IsSystemActive = true;
            }
        }

        private void NewCombo()
        {
            var editor = new ComboEditorWindow();
            if (editor.ShowDialog() == true && editor.Result != null)
            {
                Combos.Add(editor.Result);
                MarkAsChanged();
            }
        }

        private void EditCombo()
        {
            if (SelectedCombo == null) return;

            var editor = new ComboEditorWindow(SelectedCombo);
            if (editor.ShowDialog() == true)
            {
                MarkAsChanged();
            }
        }

        private void DeleteCombo()
        {
            if (SelectedCombo == null) return;

            var result = MessageBox.Show(
                $"Are you sure you want to delete '{SelectedCombo.Name}'?",
                "Delete Combo",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question
            );

            if (result == MessageBoxResult.Yes)
            {
                Combos.Remove(SelectedCombo);
                MarkAsChanged();
            }
        }

        private void SaveCombos()
        {
            try
            {
                _storageService.SaveCombos(Combos);
                HasUnsavedChanges = false;
                StatusMessage = "Combos saved successfully";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to save combos: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CheckForAutoSave()
        {
            if (_storageService.HasAutoSave())
            {
                var autoSaveTime = _storageService.GetAutoSaveTime();
                var message = autoSaveTime.HasValue 
                    ? $"An autosave was found from {autoSaveTime.Value:yyyy-MM-dd HH:mm:ss}.\n\nDo you want to restore it?"
                    : "An autosave was found. Do you want to restore it?";

                var result = MessageBox.Show(
                    message,
                    "Restore Autosave",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question
                );

                if (result == MessageBoxResult.Yes)
                {
                    LoadAutoSave();
                    return;
                }
                else
                {
                    _storageService.DeleteAutoSave();
                }
            }

            LoadCombos();
        }

        private void LoadAutoSave()
        {
            try
            {
                var combos = _storageService.LoadAutoSave();
                Combos.Clear();
                foreach (var combo in combos)
                {
                    Combos.Add(combo);
                }
                HasUnsavedChanges = true;
                StatusMessage = $"Restored {Combos.Count} combos from autosave";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load autosave: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                LoadCombos();
            }
        }

        private void LoadCombos()
        {
            try
            {
                var combos = _storageService.LoadCombos();
                Combos.Clear();
                foreach (var combo in combos)
                {
                    Combos.Add(combo);
                }
                HasUnsavedChanges = false;
                StatusMessage = $"Loaded {Combos.Count} combos";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load combos: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OnCombosChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            MarkAsChanged();
        }

        private void MarkAsChanged()
        {
            HasUnsavedChanges = true;
        }

        private void StartAutoSave()
        {
            StopAutoSave();
            _autoSaveCts = new CancellationTokenSource();
            
            Task.Run(async () => await AutoSaveLoopAsync(_autoSaveCts.Token), _autoSaveCts.Token);
        }

        private void StopAutoSave()
        {
            _autoSaveCts?.Cancel();
            _autoSaveCts?.Dispose();
            _autoSaveCts = null;
        }

        private async Task AutoSaveLoopAsync(CancellationToken cancellationToken)
        {
            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    await Task.Delay(TimeSpan.FromSeconds(AUTO_SAVE_INTERVAL_SECONDS), cancellationToken);
                    
                    if (HasUnsavedChanges && Combos.Count > 0)
                    {
                        await Application.Current.Dispatcher.InvokeAsync(() =>
                        {
                            _storageService.AutoSaveCombos(Combos);
                            StatusMessage = $"Auto-saved at {DateTime.Now:HH:mm:ss}";
                        });
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // Expected when cancellation is requested
            }
        }

        private void ExportCombos()
        {
            var dialog = new SaveFileDialog
            {
                Filter = "JSON Files (*.json)|*.json",
                DefaultExt = ".json",
                FileName = "combos.json"
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    _storageService.ExportCombos(Combos, dialog.FileName);
                    MessageBox.Show("Combos exported successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to export combos: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void ImportCombos()
        {
            var dialog = new OpenFileDialog
            {
                Filter = "JSON Files (*.json)|*.json",
                DefaultExt = ".json"
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    var imported = _storageService.ImportCombos(dialog.FileName);
                    foreach (var combo in imported)
                    {
                        Combos.Add(combo);
                    }
                    MarkAsChanged();
                    MessageBox.Show($"Imported {imported.Count} combos successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to import combos: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void SetPanicButton()
        {
            _isWaitingForPanicButton = true;
            PanicButtonText = "Press a key...";
        }

        public void OnKeyPressed(string key)
        {
            if (_isWaitingForPanicButton)
            {
                _isWaitingForPanicButton = false;
                _hotkeyManager.SetPanicButton(key);
                PanicButtonText = $"Panic Button: {key} (click to change)";
            }
        }

        public bool IsWaitingForPanicButton => _isWaitingForPanicButton;

        public void Cleanup()
        {
            StopAutoSave();
            _hotkeyManager.Stop();
            _hotkeyManager.Dispose();
        }
    }
}
