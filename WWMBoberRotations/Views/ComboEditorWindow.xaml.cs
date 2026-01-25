using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using WWMBoberRotations.Models;
using WWMBoberRotations.Services;
using WWMBoberRotations.ViewModels;

namespace WWMBoberRotations.Views
{
    public partial class ComboEditorWindow : Window
    {
        private readonly ComboEditorViewModel _viewModel;
        private bool _isWaitingForHotkey;
        private Point _dragStartPoint;
        private ComboAction? _draggedItem;
        private bool _isDragging;

        public Combo? Result { get; private set; }

        public ComboEditorWindow(Combo? combo = null)
        {
            InitializeComponent();

            var editCombo = combo ?? new Combo();
            _viewModel = new ComboEditorViewModel(editCombo);
            DataContext = _viewModel;

            if (combo != null)
            {
                HotkeyTextBox.Text = combo.Hotkey ?? "";
            }
        }

        private void SetHotkey_Click(object sender, RoutedEventArgs e)
        {
            _isWaitingForHotkey = true;
            HotkeyTextBox.Text = "Press a key or ESC to clear...";
            HotkeyTextBox.Focus();
        }

        private void ClearHotkey_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.Hotkey = null;
            _isWaitingForHotkey = false;
        }

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            base.OnPreviewKeyDown(e);

            if (_isWaitingForHotkey)
            {
                e.Handled = true;

                var key = e.Key == Key.System ? e.SystemKey : e.Key;

                if (key == Key.Escape)
                {
                    _viewModel.Hotkey = null;
                    _isWaitingForHotkey = false;
                    return;
                }

                if (key == Key.LeftShift || key == Key.RightShift ||
                    key == Key.LeftCtrl || key == Key.RightCtrl ||
                    key == Key.LeftAlt || key == Key.RightAlt ||
                    key == Key.LWin || key == Key.RWin)
                {
                    return;
                }

                _viewModel.Hotkey = KeyMapper.WpfKeyToString(key);
                _isWaitingForHotkey = false;
            }
        }

        protected override void OnPreviewMouseDown(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseDown(e);

            if (_isWaitingForHotkey)
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
                    _viewModel.Hotkey = mouseButton;
                    _isWaitingForHotkey = false;
                }
            }
        }

        private void ActionsListBox_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _dragStartPoint = e.GetPosition(null);
            _isDragging = false;

            var item = GetItemAtPosition(e.GetPosition(ActionsListBox));
            if (item != null && ActionsListBox.ItemsSource != null)
            {
                _draggedItem = item;
            }
        }

        private void ActionsListBox_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && _draggedItem != null && !_isDragging)
            {
                var currentPosition = e.GetPosition(null);
                var diff = _dragStartPoint - currentPosition;

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
            DropIndicator.Visibility = Visibility.Collapsed;
            
            if (e.Data.GetData(typeof(ComboAction)) is ComboAction droppedAction)
            {
                var targetAction = GetItemAtPosition(e.GetPosition(ActionsListBox));
                
                if (targetAction != null && !ReferenceEquals(droppedAction, targetAction))
                {
                    var oldIndex = _viewModel.Actions.IndexOf(droppedAction);
                    var newIndex = _viewModel.Actions.IndexOf(targetAction);

                    if (oldIndex >= 0 && newIndex >= 0)
                    {
                        _viewModel.Actions.Move(oldIndex, newIndex);
                    }
                }
            }
        }

        private void ActionsListBox_DragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetData(typeof(ComboAction)) != null)
            {
                var targetAction = GetItemAtPosition(e.GetPosition(ActionsListBox));
                if (targetAction != null)
                {
                    var container = ActionsListBox.ItemContainerGenerator.ContainerFromItem(targetAction) as ListBoxItem;
                    if (container != null)
                    {
                        var position = e.GetPosition(ActionsListBox);
                        var containerPosition = container.TranslatePoint(new Point(0, 0), ActionsListBox);
                        var containerHeight = container.ActualHeight;
                        var relativeY = position.Y - containerPosition.Y;
                        var insertBefore = relativeY < containerHeight / 2;
                        var lineY = containerPosition.Y + (insertBefore ? 9 : containerHeight + 9);
                        var listBoxWidth = ActionsListBox.ActualWidth - SystemParameters.VerticalScrollBarWidth - 20;
                        
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

        private ComboAction? GetItemAtPosition(Point position)
        {
            var hitTestResult = VisualTreeHelper.HitTest(ActionsListBox, position);
            if (hitTestResult == null) return null;

            var element = hitTestResult.VisualHit;
            while (element != null && element != ActionsListBox)
            {
                if (element is ListBoxItem listBoxItem)
                {
                    return listBoxItem.Content as ComboAction;
                }
                element = VisualTreeHelper.GetParent(element);
            }

            return null;
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

        private void ActionsListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var item = GetItemAtPosition(e.GetPosition(ActionsListBox));
            if (item != null)
            {
                _viewModel.SelectedAction = item;
                if (_viewModel.EditActionCommand.CanExecute(null))
                {
                    _viewModel.EditActionCommand.Execute(null);
                }
            }
        }
    }
}
