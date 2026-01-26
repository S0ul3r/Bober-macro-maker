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
    public class MainViewModel : ViewModelBase, IDisposable
    {
        private readonly ComboStorageService _storageService;
        private readonly ComboExecutor _executor;
        private readonly HotkeyManager _hotkeyManager;
        
        private string _statusMessage = "";
        private bool _isSystemActive;
        private Combo? _selectedCombo;
        private string _panicButtonText = "Panic Button: rmb";
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
        public ICommand RecordMacroCommand { get; }
        public ICommand DuplicateComboCommand { get; }
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
            Combos.CollectionChanged += OnCombosChanged;

            ToggleSystemCommand = new RelayCommand(ToggleSystem);
            NewComboCommand = new RelayCommand(NewCombo);
            EditComboCommand = new RelayCommand(EditCombo, () => SelectedCombo != null);
            RecordMacroCommand = new RelayCommand(RecordMacro);
            DuplicateComboCommand = new RelayCommand(DuplicateCombo, () => SelectedCombo != null);
            DeleteComboCommand = new RelayCommand(DeleteCombo, () => SelectedCombo != null);
            SaveCombosCommand = new RelayCommand(SaveCombos);
            LoadCombosCommand = new RelayCommand(LoadCombos);
            ExportCommand = new RelayCommand(ExportCombos);
            ImportCommand = new RelayCommand(ImportCombos);
            SetPanicButtonCommand = new RelayCommand(SetPanicButton);

            CheckForAutoSave();
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
            if (editor.ShowDialog() == true && editor.Result != null)
            {
                UpdateComboFromEditorResult(SelectedCombo, editor.Result);
                MarkAsChanged();
                RefreshComboInList(SelectedCombo);
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

        private void DuplicateCombo()
        {
            if (SelectedCombo == null) return;

            var duplicate = new Combo
            {
                Name = SelectedCombo.Name + " - Copy",
                Hotkey = null,
                IsEnabled = SelectedCombo.IsEnabled
            };

            foreach (var action in SelectedCombo.Actions)
            {
                duplicate.Actions.Add(action);
            }

            Combos.Add(duplicate);
            SelectedCombo = duplicate;
            MarkAsChanged();
            StatusMessage = $"Duplicated combo: {duplicate.Name}";
            Logger.Info($"Duplicated combo: {SelectedCombo.Name} -> {duplicate.Name}");
        }

        private void RecordMacro()
        {
            var recorderWindow = new MacroRecorderWindow
            {
                Owner = Application.Current.MainWindow
            };

            if (recorderWindow.ShowDialog() == true && recorderWindow.RecordedCombo != null)
            {
                var combo = recorderWindow.RecordedCombo;
                Combos.Add(combo);
                SelectedCombo = combo;
                MarkAsChanged();
                StatusMessage = $"Recorded macro '{combo.Name}' with {combo.Actions.Count} actions";
                OpenComboEditorForRecordedMacro(combo);
            }
        }

        private void OpenComboEditorForRecordedMacro(Combo combo)
        {
            var editorWindow = new ComboEditorWindow(combo)
            {
                Owner = Application.Current.MainWindow
            };

            if (editorWindow.ShowDialog() == true && editorWindow.Result != null)
            {
                UpdateComboFromEditorResult(combo, editorWindow.Result);
                MarkAsChanged();
                RefreshComboInList(combo);
            }
        }

        private void UpdateComboFromEditorResult(Combo target, Combo source)
        {
            target.Name = source.Name;
            target.Hotkey = source.Hotkey;
            target.IsEnabled = source.IsEnabled;
            target.Actions.Clear();
            foreach (var action in source.Actions)
            {
                target.Actions.Add(action);
            }
        }

        private void RefreshComboInList(Combo combo)
        {
            var index = Combos.IndexOf(combo);
            if (index >= 0)
            {
                Combos.RemoveAt(index);
                Combos.Insert(index, combo);
                SelectedCombo = combo;
            }
        }

        private void SaveCombos()
        {
            try
            {
                _storageService.SaveCombos(Combos);
                HasUnsavedChanges = false;
                StatusMessage = "Combos saved successfully";
                Logger.Info($"Saved {Combos.Count} combos");
            }
            catch (System.IO.IOException ex)
            {
                Logger.Error("Failed to save combos - file access error", ex);
                MessageBox.Show($"Failed to save combos: {ex.Message}\n\nMake sure the file is not open in another program.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (UnauthorizedAccessException ex)
            {
                Logger.Error("Failed to save combos - access denied", ex);
                MessageBox.Show("Access denied. Try running the application as administrator.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
                Logger.Info($"Restored {Combos.Count} combos from autosave");
            }
            catch (System.IO.IOException ex)
            {
                Logger.Error("Failed to load autosave - file access error", ex);
                MessageBox.Show($"Failed to load autosave: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                LoadCombos();
            }
            catch (Newtonsoft.Json.JsonException ex)
            {
                Logger.Error("Failed to load autosave - invalid JSON format", ex);
                MessageBox.Show("Autosave file is corrupted. Loading normal save instead.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
                Logger.Info($"Loaded {Combos.Count} combos");
            }
            catch (System.IO.IOException ex)
            {
                Logger.Error("Failed to load combos - file access error", ex);
                MessageBox.Show($"Failed to load combos: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Newtonsoft.Json.JsonException ex)
            {
                Logger.Error("Failed to load combos - invalid JSON format", ex);
                MessageBox.Show("Save file is corrupted. Starting with empty combo list.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
                    Logger.Info($"Exported {Combos.Count} combos to {dialog.FileName}");
                    MessageBox.Show("Combos exported successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (System.IO.IOException ex)
                {
                    Logger.Error("Failed to export combos - file access error", ex);
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
                    Logger.Info($"Imported {imported.Count} combos from {dialog.FileName}");
                    MessageBox.Show($"Imported {imported.Count} combos successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (System.IO.FileNotFoundException)
                {
                    MessageBox.Show("File not found.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                catch (Newtonsoft.Json.JsonException ex)
                {
                    Logger.Error("Failed to import combos - invalid JSON format", ex);
                    MessageBox.Show("Invalid file format. Please select a valid combo export file.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                catch (System.IO.IOException ex)
                {
                    Logger.Error("Failed to import combos - file access error", ex);
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

        public void Dispose()
        {
            Cleanup();
        }

        private void Cleanup()
        {
            StopAutoSave();
            _hotkeyManager.Stop();
            _hotkeyManager.Dispose();
            
            Logger.Info("MainViewModel disposed");
        }
    }
}
