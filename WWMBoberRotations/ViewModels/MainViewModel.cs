using System;
using System.Collections.ObjectModel;
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

            // Load combos
            LoadCombos();
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
                SaveCombos();
            }
        }

        private void EditCombo()
        {
            if (SelectedCombo == null) return;

            var editor = new ComboEditorWindow(SelectedCombo);
            if (editor.ShowDialog() == true)
            {
                SaveCombos();
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
                SaveCombos();
            }
        }

        private void SaveCombos()
        {
            try
            {
                _storageService.SaveCombos(Combos);
                StatusMessage = "Combos saved successfully";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to save combos: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
                StatusMessage = $"Loaded {Combos.Count} combos";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load combos: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
                    SaveCombos();
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
            _hotkeyManager.Stop();
            _hotkeyManager.Dispose();
        }
    }
}
