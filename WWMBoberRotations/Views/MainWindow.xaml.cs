using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using WWMBoberRotations.ViewModels;

namespace WWMBoberRotations.Views
{
    public partial class MainWindow : Window
    {
        private readonly MainViewModel _viewModel;
        private const int WM_HOTKEY = 0x0312;

        public MainWindow()
        {
            InitializeComponent();
            
            _viewModel = new MainViewModel();
            DataContext = _viewModel;

            // Subscribe to system active changes to update button appearance
            _viewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(MainViewModel.IsSystemActive))
                {
                    UpdateStartStopButton();
                }
            };

            Loaded += MainWindow_Loaded;
            Closing += MainWindow_Closing;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // Initialize hotkey manager with window handle
            var helper = new WindowInteropHelper(this);
            _viewModel.InitializeHotkeyManager(helper.Handle);

            // Hook into Windows message loop for hotkey handling
            var source = HwndSource.FromHwnd(helper.Handle);
            source?.AddHook(WndProc);
        }

        private void MainWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            _viewModel.Cleanup();
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == WM_HOTKEY)
            {
                int hotkeyId = wParam.ToInt32();
                _viewModel.HandleHotkey(hotkeyId);
                handled = true;
            }
            return IntPtr.Zero;
        }

        private void UpdateStartStopButton()
        {
            Dispatcher.Invoke(() =>
            {
                if (_viewModel.IsSystemActive)
                {
                    StartStopButton.Content = new System.Windows.Controls.StackPanel
                    {
                        Orientation = System.Windows.Controls.Orientation.Horizontal,
                        Children =
                        {
                            new MaterialDesignThemes.Wpf.PackIcon
                            {
                                Kind = MaterialDesignThemes.Wpf.PackIconKind.Stop,
                                Width = 24,
                                Height = 24,
                                Margin = new Thickness(0, 0, 10, 0)
                            },
                            new System.Windows.Controls.TextBlock
                            {
                                Text = "STOP MACRO SYSTEM",
                                FontSize = 16,
                                FontWeight = FontWeights.Bold
                            }
                        }
                    };
                    StartStopButton.Background = new SolidColorBrush(Color.FromRgb(211, 47, 47)); // Red
                }
                else
                {
                    StartStopButton.Content = new System.Windows.Controls.StackPanel
                    {
                        Orientation = System.Windows.Controls.Orientation.Horizontal,
                        Children =
                        {
                            new MaterialDesignThemes.Wpf.PackIcon
                            {
                                Kind = MaterialDesignThemes.Wpf.PackIconKind.Play,
                                Width = 24,
                                Height = 24,
                                Margin = new Thickness(0, 0, 10, 0)
                            },
                            new System.Windows.Controls.TextBlock
                            {
                                Text = "START MACRO SYSTEM",
                                FontSize = 16,
                                FontWeight = FontWeights.Bold
                            }
                        }
                    };
                    StartStopButton.ClearValue(BackgroundProperty); // Reset to default
                }
            });
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (_viewModel.IsWaitingForPanicButton)
            {
                e.Handled = true;
                string keyName = e.Key.ToString().ToLower();
                _viewModel.OnKeyPressed(keyName);
            }
        }

        private void Window_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (_viewModel.IsWaitingForPanicButton)
            {
                e.Handled = true;
                string buttonName = e.ChangedButton switch
                {
                    MouseButton.Left => "lmb",
                    MouseButton.Right => "rmb",
                    MouseButton.Middle => "mmb",
                    MouseButton.XButton1 => "mouse4",
                    MouseButton.XButton2 => "mouse5",
                    _ => "unknown"
                };
                _viewModel.OnKeyPressed(buttonName);
            }
        }
    }
}
