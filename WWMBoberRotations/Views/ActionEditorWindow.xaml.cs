using System;
using System.Windows;
using System.Windows.Controls;
using MaterialDesignThemes.Wpf;
using WWMBoberRotations.Models;
using WWMBoberRotations.Services;

namespace WWMBoberRotations.Views
{
    public partial class ActionEditorWindow : Window
    {
        private ComboAction? _action;
        private TextBox? _keyTextBox;
        private TextBox? _durationTextBox;
        private TextBox? _delayAfterTextBox;
        private ComboBox? _mouseButtonCombo;

        public ComboAction? Result { get; private set; }

        public ActionEditorWindow(ComboAction? action = null)
        {
            InitializeComponent();
            _action = action;

            if (_action != null)
            {
                LoadAction(_action);
            }
            else
            {
                ActionTypeCombo.SelectedIndex = 0;
            }
        }

        private void LoadAction(ComboAction action)
        {
            ActionTypeCombo.SelectedIndex = action.Type switch
            {
                ActionType.KeyPress => 0,
                ActionType.KeyHold => 1,
                ActionType.MouseClick => 2,
                ActionType.Delay => 3,
                _ => 0
            };
        }

        private void ActionTypeCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ActionTypeCombo.SelectedItem is ComboBoxItem item)
            {
                var type = item.Tag.ToString();
                ConfigureUI(type);
            }
        }

        private void ConfigureUI(string? type)
        {
            ConfigPanel.Children.Clear();

            switch (type)
            {
                case "KeyPress":
                    ConfigureKeyPressUI();
                    break;
                case "KeyHold":
                    ConfigureKeyHoldUI();
                    break;
                case "MouseClick":
                    ConfigureMouseClickUI();
                    break;
                case "Delay":
                    ConfigureDelayUI();
                    break;
            }
        }

        private void ConfigureKeyPressUI()
        {
            var stack = new StackPanel();

            var label = new TextBlock
            {
                Text = "Key to press:",
                Margin = new Thickness(0, 0, 0, 10)
            };
            stack.Children.Add(label);

            var inputRow = new Grid();
            inputRow.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            inputRow.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            inputRow.Margin = new Thickness(0, 0, 0, 10);

            _keyTextBox = new TextBox
            {
                Style = (Style)FindResource("MaterialDesignOutlinedTextBox"),
                Margin = new Thickness(0, 0, 10, 0)
            };
            HintAssist.SetHint(_keyTextBox, "Enter key (e.g., q, w, e, space, f1)");
            
            if (_action?.Type == ActionType.KeyPress)
                _keyTextBox.Text = _action.Key;
            
            Grid.SetColumn(_keyTextBox, 0);
            inputRow.Children.Add(_keyTextBox);

            var captureButton = new Button
            {
                Content = "Capture Key",
                Style = (Style)FindResource("MaterialDesignRaisedButton"),
                Height = 40,
                Padding = new Thickness(15, 10, 15, 10)
            };
            captureButton.Click += (s, e) => CaptureKeyPress();
            Grid.SetColumn(captureButton, 1);
            inputRow.Children.Add(captureButton);

            stack.Children.Add(inputRow);
            AddDelayAfterField(stack);

            var info = new TextBlock
            {
                Text = "Keyboard: a-z, 0-9, space, enter, tab, esc, backspace, delete, insert\n" +
                       "Modifiers: shift, ctrl, alt, rshift, rctrl, ralt\n" +
                       "Lock keys: capslock, numlock, scrolllock\n" +
                       "Arrows: up, down, left, right (or arrowup, arrowdown, etc.)\n" +
                       "Navigation: home, end, pageup, pagedown\n" +
                       "Function: f1-f12\n" +
                       "Mouse: lmb, rmb, mmb, mouse4, mouse5",
                TextWrapping = TextWrapping.Wrap,
                Foreground = System.Windows.Media.Brushes.Gray,
                FontSize = 10
            };
            stack.Children.Add(info);

            ConfigPanel.Children.Add(stack);
        }

        private void ConfigureKeyHoldUI()
        {
            var stack = new StackPanel();

            var label1 = new TextBlock
            {
                Text = "Key to hold:",
                Margin = new Thickness(0, 0, 0, 10)
            };
            stack.Children.Add(label1);

            var inputRow = new Grid();
            inputRow.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            inputRow.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            inputRow.Margin = new Thickness(0, 0, 0, 15);

            _keyTextBox = new TextBox
            {
                Style = (Style)FindResource("MaterialDesignOutlinedTextBox"),
                Margin = new Thickness(0, 0, 10, 0)
            };
            HintAssist.SetHint(_keyTextBox, "Enter key");
            
            if (_action?.Type == ActionType.KeyHold)
                _keyTextBox.Text = _action.Key;
            
            Grid.SetColumn(_keyTextBox, 0);
            inputRow.Children.Add(_keyTextBox);

            var captureButton = new Button
            {
                Content = "Capture Key",
                Style = (Style)FindResource("MaterialDesignRaisedButton"),
                Height = 40,
                Padding = new Thickness(15, 10, 15, 10)
            };
            captureButton.Click += (s, e) => CaptureKeyPress();
            Grid.SetColumn(captureButton, 1);
            inputRow.Children.Add(captureButton);

            stack.Children.Add(inputRow);
            AddDelayAfterField(stack);

            var label2 = new TextBlock
            {
                Text = "Duration (milliseconds):",
                Margin = new Thickness(0, 0, 0, 10)
            };
            stack.Children.Add(label2);

            _durationTextBox = new TextBox
            {
                Style = (Style)FindResource("MaterialDesignOutlinedTextBox")
            };
            HintAssist.SetHint(_durationTextBox, "Duration in ms (e.g., 500, 1000)");
            
            if (_action?.Type == ActionType.KeyHold)
                _durationTextBox.Text = _action.Duration.ToString();
            else
                _durationTextBox.Text = "500";
            
            stack.Children.Add(_durationTextBox);

            ConfigPanel.Children.Add(stack);
        }

        private void ConfigureMouseClickUI()
        {
            var stack = new StackPanel();

            var label = new TextBlock
            {
                Text = "Mouse button:",
                Margin = new Thickness(0, 0, 0, 10)
            };
            stack.Children.Add(label);

            _mouseButtonCombo = new ComboBox
            {
                Style = (Style)FindResource("MaterialDesignOutlinedComboBox")
            };
            HintAssist.SetHint(_mouseButtonCombo, "Select button");

            _mouseButtonCombo.Items.Add(new ComboBoxItem { Content = "Left", Tag = "Left" });
            _mouseButtonCombo.Items.Add(new ComboBoxItem { Content = "Right", Tag = "Right" });
            _mouseButtonCombo.Items.Add(new ComboBoxItem { Content = "Middle", Tag = "Middle" });
            _mouseButtonCombo.Items.Add(new ComboBoxItem { Content = "Mouse 4 (XButton1)", Tag = "XButton1" });
            _mouseButtonCombo.Items.Add(new ComboBoxItem { Content = "Mouse 5 (XButton2)", Tag = "XButton2" });

            if (_action?.Type == ActionType.MouseClick)
            {
                _mouseButtonCombo.SelectedIndex = _action.Button switch
                {
                    MouseButton.Left => 0,
                    MouseButton.Right => 1,
                    MouseButton.Middle => 2,
                    MouseButton.XButton1 => 3,
                    MouseButton.XButton2 => 4,
                    _ => 0
                };
            }
            else
            {
                _mouseButtonCombo.SelectedIndex = 0;
            }

            stack.Children.Add(_mouseButtonCombo);
            AddDelayAfterField(stack);
            ConfigPanel.Children.Add(stack);
        }

        private void ConfigureDelayUI()
        {
            var stack = new StackPanel();

            var label = new TextBlock
            {
                Text = "Delay duration (milliseconds):",
                Margin = new Thickness(0, 0, 0, 10)
            };
            stack.Children.Add(label);

            _durationTextBox = new TextBox
            {
                Style = (Style)FindResource("MaterialDesignOutlinedTextBox")
            };
            HintAssist.SetHint(_durationTextBox, "Duration in ms (e.g., 500, 1000)");
            
            if (_action?.Type == ActionType.Delay)
                _durationTextBox.Text = _action.Duration.ToString();
            else
                _durationTextBox.Text = "1000";
            
            stack.Children.Add(_durationTextBox);

            ConfigPanel.Children.Add(stack);
        }

        private void AddDelayAfterField(StackPanel stack)
        {
            var label = new TextBlock
            {
                Text = "Delay after action (milliseconds):",
                Margin = new Thickness(0, 15, 0, 10)
            };
            stack.Children.Add(label);

            _delayAfterTextBox = new TextBox
            {
                Style = (Style)FindResource("MaterialDesignOutlinedTextBox")
            };
            HintAssist.SetHint(_delayAfterTextBox, "Delay in ms (e.g., 100, 500, 1000)");
            _delayAfterTextBox.Text = _action?.DelayAfter.ToString() ?? "0";
            
            stack.Children.Add(_delayAfterTextBox);
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var selectedItem = ActionTypeCombo.SelectedItem as ComboBoxItem;
                var type = selectedItem?.Tag.ToString();

                var action = new ComboAction();
                int delayAfter = 0;
                if (!string.IsNullOrWhiteSpace(_delayAfterTextBox?.Text))
                {
                    if (!int.TryParse(_delayAfterTextBox.Text, out delayAfter) || delayAfter < 0 || delayAfter > 300000)
                    {
                        MessageBox.Show("Delay must be between 0ms and 300000ms.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                }

                switch (type)
                {
                    case "KeyPress":
                        if (string.IsNullOrWhiteSpace(_keyTextBox?.Text))
                        {
                            MessageBox.Show("Please enter a key.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }
                        action.Type = ActionType.KeyPress;
                        action.Key = _keyTextBox.Text.Trim().ToLower();
                        action.DelayAfter = delayAfter;
                        break;

                    case "KeyHold":
                        if (string.IsNullOrWhiteSpace(_keyTextBox?.Text))
                        {
                            MessageBox.Show("Please enter a key.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }
                        if (!int.TryParse(_durationTextBox?.Text, out int holdDuration) || holdDuration < 50 || holdDuration > 60000)
                        {
                            MessageBox.Show("Please enter a valid duration (50ms - 60000ms).", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }
                        action.Type = ActionType.KeyHold;
                        action.Key = _keyTextBox.Text.Trim().ToLower();
                        action.Duration = holdDuration;
                        action.DelayAfter = delayAfter;
                        break;

                    case "MouseClick":
                        var mouseItem = _mouseButtonCombo?.SelectedItem as ComboBoxItem;
                        action.Type = ActionType.MouseClick;
                        action.Button = mouseItem?.Tag?.ToString() switch
                        {
                            "Right" => MouseButton.Right,
                            "Middle" => MouseButton.Middle,
                            "XButton1" => MouseButton.XButton1,
                            "XButton2" => MouseButton.XButton2,
                            _ => MouseButton.Left
                        };
                        action.DelayAfter = delayAfter;
                        break;

                    case "Delay":
                        if (!int.TryParse(_durationTextBox?.Text, out int delayDuration) || delayDuration < 10 || delayDuration > 300000)
                        {
                            MessageBox.Show("Please enter a valid duration (10ms - 300000ms).", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }
                        action.Type = ActionType.Delay;
                        action.Duration = delayDuration;
                        break;

                    default:
                        MessageBox.Show("Please select an action type.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                }

                Result = action;
                DialogResult = true;
                Close();
            }
            catch (FormatException ex)
            {
                Logger.Error("Invalid input format in action editor", ex);
                MessageBox.Show("Invalid input format. Please check your entries.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (ArgumentException ex)
            {
                Logger.Error("Invalid argument in action editor", ex);
                MessageBox.Show($"Invalid input: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                Logger.Error("Unexpected error in action editor", ex);
                MessageBox.Show($"Error saving action: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void CaptureKeyPress()
        {
            if (_keyTextBox == null) return;

            _keyTextBox.Text = "Press any key...";
            _keyTextBox.IsReadOnly = true;
            Focus();
            PreviewKeyDown -= ActionEditorWindow_PreviewKeyDown;
            PreviewKeyDown += ActionEditorWindow_PreviewKeyDown;
        }

        private void ActionEditorWindow_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (_keyTextBox?.IsReadOnly != true) return;

            PreviewKeyDown -= ActionEditorWindow_PreviewKeyDown;
            string keyName = Services.KeyMapper.WpfKeyToString(e.Key == System.Windows.Input.Key.System ? e.SystemKey : e.Key);

            _keyTextBox.Text = keyName;
            _keyTextBox.IsReadOnly = false;

            e.Handled = true;
        }

        protected override void OnPreviewMouseDown(System.Windows.Input.MouseButtonEventArgs e)
        {
            base.OnPreviewMouseDown(e);

            if (_keyTextBox?.IsReadOnly == true)
            {
                e.Handled = true;

                var mouseButton = e.ChangedButton switch
                {
                    System.Windows.Input.MouseButton.Left => "lmb",
                    System.Windows.Input.MouseButton.Right => "rmb",
                    System.Windows.Input.MouseButton.Middle => "mmb",
                    System.Windows.Input.MouseButton.XButton1 => "mouse4",
                    System.Windows.Input.MouseButton.XButton2 => "mouse5",
                    _ => null
                };

                if (mouseButton != null)
                {
                    PreviewKeyDown -= ActionEditorWindow_PreviewKeyDown;
                    _keyTextBox.Text = mouseButton;
                    _keyTextBox.IsReadOnly = false;
                }
            }
        }
    }
}
