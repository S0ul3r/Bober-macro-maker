using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using WWMBoberRotations.Models;
using WWMBoberRotations.Services;

namespace WWMBoberRotations.Views
{
    public partial class MacroRecorderWindow : Window
    {
        private readonly MacroRecorder _recorder;
        private readonly ObservableCollection<ActionDisplayItem> _displayActions;
        private bool _isWaitingForRecordHotkey;
        private string _comboName = string.Empty;
        private bool _lastHotkeyState;

        public string ComboName
        {
            get => _comboName;
            set
            {
                _comboName = value;
                UpdateSaveButtonState();
            }
        }

        public string RecordHotkey { get; set; } = "insert";
        public Combo? RecordedCombo { get; private set; }

        public MacroRecorderWindow()
        {
            InitializeComponent();
            DataContext = this;

            _recorder = new MacroRecorder();
            _recorder.ActionRecorded += OnActionRecorded;
            _recorder.StatusChanged += OnStatusChanged;

            _displayActions = new ObservableCollection<ActionDisplayItem>();
            ActionsListBox.ItemsSource = _displayActions;

            RecordHotkeyTextBox.Text = RecordHotkey;
            ComboNameTextBox.TextChanged += (s, e) => ComboName = ComboNameTextBox.Text;

            // Start monitoring for record hotkey
            StartRecordHotkeyMonitoring();
        }

        private void StartRecordHotkeyMonitoring()
        {
            CompositionTarget.Rendering += OnRendering;
        }

        private void OnRendering(object? sender, System.EventArgs e)
        {
            if (_isWaitingForRecordHotkey)
                return;

            // Check if record hotkey is pressed
            var keyCode = GetHotkeyCode(RecordHotkey);
            if (keyCode == 0)
                return;

            var isPressed = IsKeyPressed(keyCode);

            // Detect rising edge (key just pressed)
            if (isPressed && !_lastHotkeyState)
            {
                _ = ToggleRecordingAsync();
            }

            _lastHotkeyState = isPressed;
        }

        private async System.Threading.Tasks.Task ToggleRecordingAsync()
        {
            if (_recorder.IsRecording)
            {
                // Stop recording
                _recorder.StopRecording();
                UpdateSaveButtonState();
            }
            else
            {
                // Start recording
                _displayActions.Clear();
                ActionCountText.Text = " (0)";
                _recorder.SetStopHotkey(RecordHotkey);
                await _recorder.StartRecordingAsync();

                // Recording stopped by hotkey or manual stop
                UpdateSaveButtonState();
            }
        }

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern short GetAsyncKeyState(int vKey);

        private bool IsKeyPressed(int keyCode)
        {
            return (GetAsyncKeyState(keyCode) & 0x8000) != 0;
        }

        private int GetHotkeyCode(string hotkey)
        {
            var mouseCode = KeyMapper.GetMouseButtonCode(hotkey);
            return mouseCode != 0 ? mouseCode : KeyMapper.GetVirtualKeyCode(hotkey);
        }

        private void OnActionRecorded(object? sender, ComboAction action)
        {
            Dispatcher.InvokeAsync(() =>
            {
                _displayActions.Add(new ActionDisplayItem(action));
                ActionCountText.Text = $" ({_displayActions.Count})";
                UpdateButtonStates();
                
                // Auto-scroll to bottom
                if (ActionsListBox.Items.Count > 0)
                {
                    ActionsListBox.ScrollIntoView(ActionsListBox.Items[ActionsListBox.Items.Count - 1]);
                }
            });
        }

        private void OnStatusChanged(object? sender, string status)
        {
            Dispatcher.InvokeAsync(() =>
            {
                StatusText.Text = status;
            });
        }

        private void SetRecordHotkey_Click(object sender, RoutedEventArgs e)
        {
            _isWaitingForRecordHotkey = true;
            RecordHotkeyTextBox.Text = "Press a key or ESC to clear...";
            RecordHotkeyTextBox.Focus();
        }

        private void ClearRecordHotkey_Click(object sender, RoutedEventArgs e)
        {
            RecordHotkey = "insert";
            RecordHotkeyTextBox.Text = RecordHotkey;
            _isWaitingForRecordHotkey = false;
        }

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            base.OnPreviewKeyDown(e);

            if (_isWaitingForRecordHotkey)
            {
                e.Handled = true;

                var key = e.Key == Key.System ? e.SystemKey : e.Key;

                // ESC clears the hotkey
                if (key == Key.Escape)
                {
                    ClearRecordHotkey_Click(this, new RoutedEventArgs());
                    return;
                }

                // Ignore modifier keys
                if (key == Key.LeftShift || key == Key.RightShift ||
                    key == Key.LeftCtrl || key == Key.RightCtrl ||
                    key == Key.LeftAlt || key == Key.RightAlt ||
                    key == Key.LWin || key == Key.RWin)
                {
                    return;
                }

                RecordHotkey = KeyToString(key);
                RecordHotkeyTextBox.Text = RecordHotkey;
                _isWaitingForRecordHotkey = false;
            }
        }

        protected override void OnPreviewMouseDown(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseDown(e);

            if (_isWaitingForRecordHotkey)
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
                    RecordHotkey = mouseButton;
                    RecordHotkeyTextBox.Text = mouseButton;
                    _isWaitingForRecordHotkey = false;
                }
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(ComboName))
            {
                MessageBox.Show("Please enter a combo name.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (_recorder.RecordedActions.Count == 0)
            {
                MessageBox.Show("No actions recorded.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            RecordedCombo = new Combo
            {
                Name = ComboName,
                IsEnabled = true,
                Hotkey = null,
                Actions = new ObservableCollection<ComboAction>(_recorder.GetRecordedActions())
            };

            DialogResult = true;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            if (_recorder.IsRecording)
            {
                _recorder.StopRecording();
            }

            DialogResult = false;
            Close();
        }

        private void UpdateSaveButtonState()
        {
            SaveButton.IsEnabled = !string.IsNullOrWhiteSpace(ComboName) && _recorder.RecordedActions.Count > 0;
        }

        private void UpdateButtonStates()
        {
            ClearAllButton.IsEnabled = _displayActions.Count > 0;
            UpdateSaveButtonState();
        }

        private void ClearAll_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(
                "Are you sure you want to clear all recorded actions?",
                "Clear All",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question
            );

            if (result == MessageBoxResult.Yes)
            {
                _displayActions.Clear();
                _recorder.ClearRecording();
                ActionCountText.Text = " (0)";
                UpdateButtonStates();
            }
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

        #region Drag and Drop

        private Point _dragStartPoint;
        private ActionDisplayItem? _draggedItem;
        private bool _isDragging;

        private void ActionsListBox_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _dragStartPoint = e.GetPosition(null);
            _isDragging = false;
            
            // Get the item being clicked
            var item = GetItemAtPosition(e.GetPosition(ActionsListBox));
            if (item != null)
            {
                _draggedItem = item;
            }
            
            // Don't handle the event - let ListBox handle selection
        }

        private void ActionsListBox_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && _draggedItem != null && !_isDragging)
            {
                Point currentPosition = e.GetPosition(null);
                Vector diff = _dragStartPoint - currentPosition;

                // Only start drag if moved beyond threshold
                if (Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance ||
                    Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance)
                {
                    _isDragging = true;
                    DragDrop.DoDragDrop(ActionsListBox, _draggedItem, DragDropEffects.Move);
                    _draggedItem = null;
                    _isDragging = false;
                }
            }
        }

        private void ActionsListBox_Drop(object sender, DragEventArgs e)
        {
            // Hide drop indicator
            DropIndicator.Visibility = Visibility.Collapsed;
            
            if (e.Data.GetDataPresent(typeof(ActionDisplayItem)))
            {
                var droppedItem = e.Data.GetData(typeof(ActionDisplayItem)) as ActionDisplayItem;
                var targetItem = GetItemAtPosition(e.GetPosition(ActionsListBox));

                if (droppedItem != null && targetItem != null && droppedItem != targetItem)
                {
                    int oldIndex = _displayActions.IndexOf(droppedItem);
                    int newIndex = _displayActions.IndexOf(targetItem);

                    if (oldIndex >= 0 && newIndex >= 0)
                    {
                        // Move in display collection
                        _displayActions.Move(oldIndex, newIndex);
                        
                        // Move in recorder's internal list
                        var actions = _recorder.GetRecordedActions();
                        if (oldIndex < actions.Count && newIndex < actions.Count)
                        {
                            var action = actions[oldIndex];
                            _recorder.RemoveActionAt(oldIndex);
                            
                            // Adjust index if moving down
                            if (newIndex > oldIndex)
                                newIndex--;
                            
                            _recorder.InsertActionAt(newIndex, action);
                        }
                    }
                }
            }
        }

        private void ActionsListBox_DragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(ActionDisplayItem)))
            {
                var targetItem = GetItemAtPosition(e.GetPosition(ActionsListBox));
                if (targetItem != null)
                {
                    var container = ActionsListBox.ItemContainerGenerator.ContainerFromItem(targetItem) as ListBoxItem;
                    if (container != null)
                    {
                        var position = e.GetPosition(ActionsListBox);
                        var containerPosition = container.TranslatePoint(new Point(0, 0), ActionsListBox);
                        var containerHeight = container.ActualHeight;
                        
                        // Show line above or below depending on mouse position
                        var relativeY = position.Y - containerPosition.Y;
                        var insertBefore = relativeY < containerHeight / 2;
                        
                        // Calculate Y position for the line - add offset to position between elements
                        var lineY = containerPosition.Y + (insertBefore ? 9 : containerHeight + 9);
                        
                        // Set the width to match the ListBox content width (excluding scrollbar)
                        var listBoxWidth = ActionsListBox.ActualWidth - SystemParameters.VerticalScrollBarWidth - 30; // 30 for margins
                        
                        DropIndicator.Visibility = Visibility.Visible;
                        DropIndicator.Width = listBoxWidth;
                        Canvas.SetLeft(DropIndicator, 0);
                        Canvas.SetTop(DropIndicator, lineY);
                    }
                }
                e.Effects = DragDropEffects.Move;
                e.Handled = true;
            }
        }

        private ActionDisplayItem? GetItemAtPosition(Point position)
        {
            var hitTestResult = VisualTreeHelper.HitTest(ActionsListBox, position);
            if (hitTestResult == null)
                return null;

            var element = hitTestResult.VisualHit;
            while (element != null && element != ActionsListBox)
            {
                if (element is ListBoxItem listBoxItem)
                {
                    return listBoxItem.DataContext as ActionDisplayItem;
                }
                element = VisualTreeHelper.GetParent(element);
            }

            return null;
        }

        #endregion

        protected override void OnClosed(System.EventArgs e)
        {
            base.OnClosed(e);
            CompositionTarget.Rendering -= OnRendering;
        }

        private class ActionDisplayItem
        {
            public ActionType Type { get; }
            public string Description { get; }

            public ActionDisplayItem(ComboAction action)
            {
                Type = action.Type;
                Description = action.Type switch
                {
                    ActionType.KeyPress => $"- Press '{action.Key}'",
                    ActionType.Delay => $"- Wait {action.Duration}ms",
                    ActionType.KeyHold => $"- Hold '{action.Key}' for {action.Duration}ms",
                    ActionType.MouseClick => $"- Click {action.Button}",
                    _ => ""
                };
            }
        }
    }
}
