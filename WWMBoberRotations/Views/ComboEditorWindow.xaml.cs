using System.Windows;
using System.Windows.Input;
using WWMBoberRotations.Models;
using WWMBoberRotations.ViewModels;

namespace WWMBoberRotations.Views
{
    public partial class ComboEditorWindow : Window
    {
        private readonly ComboEditorViewModel _viewModel;

        public Combo? Result { get; private set; }

        public ComboEditorWindow(Combo? combo = null)
        {
            InitializeComponent();

            var editCombo = combo ?? new Combo();
            _viewModel = new ComboEditorViewModel(editCombo);
            DataContext = _viewModel;
        }

        private void HotkeyTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            e.Handled = true;

            var key = e.Key == Key.System ? e.SystemKey : e.Key;

            // Ignore modifier keys by themselves
            if (key == Key.LeftShift || key == Key.RightShift ||
                key == Key.LeftCtrl || key == Key.RightCtrl ||
                key == Key.LeftAlt || key == Key.RightAlt ||
                key == Key.LWin || key == Key.RWin)
            {
                return;
            }

            _viewModel.Hotkey = KeyToString(key);
            HotkeyTextBox.CaretIndex = HotkeyTextBox.Text.Length;
        }

        private void ClearHotkey_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.Hotkey = null;
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_viewModel.Name))
            {
                MessageBox.Show("Please enter a combo name.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (_viewModel.Actions.Count == 0)
            {
                MessageBox.Show("Please add at least one action.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            Result = _viewModel.GetCombo();
            DialogResult = true;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private string KeyToString(Key key)
        {
            return key switch
            {
                Key.Space => "space",
                Key.Enter or Key.Return => "enter",
                Key.Tab => "tab",
                Key.LeftShift or Key.RightShift => "shift",
                Key.LeftCtrl or Key.RightCtrl => "ctrl",
                Key.LeftAlt or Key.RightAlt => "alt",
                Key.Escape => "esc",
                Key.Up => "up",
                Key.Down => "down",
                Key.Left => "left",
                Key.Right => "right",
                Key.F1 => "f1",
                Key.F2 => "f2",
                Key.F3 => "f3",
                Key.F4 => "f4",
                Key.F5 => "f5",
                Key.F6 => "f6",
                Key.F7 => "f7",
                Key.F8 => "f8",
                Key.F9 => "f9",
                Key.F10 => "f10",
                Key.F11 => "f11",
                Key.F12 => "f12",
                >= Key.D0 and <= Key.D9 => ((char)('0' + (key - Key.D0))).ToString(),
                >= Key.A and <= Key.Z => ((char)('a' + (key - Key.A))).ToString(),
                _ => key.ToString().ToLower()
            };
        }
    }
}
