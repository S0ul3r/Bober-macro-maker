using System.Collections.ObjectModel;
using System.ComponentModel;

namespace WWMBoberRotations.Models
{
    public class Combo : INotifyPropertyChanged
    {
        private string _name = "New Combo";
        private string? _hotkey;
        private bool _isEnabled = true;

        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                OnPropertyChanged(nameof(Name));
            }
        }

        public string? Hotkey
        {
            get => _hotkey;
            set
            {
                _hotkey = value;
                OnPropertyChanged(nameof(Hotkey));
            }
        }

        public bool IsEnabled
        {
            get => _isEnabled;
            set
            {
                _isEnabled = value;
                OnPropertyChanged(nameof(IsEnabled));
            }
        }

        public ObservableCollection<ComboAction> Actions { get; set; } = new();

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public override string ToString()
        {
            var status = IsEnabled ? "✓" : "✗";
            var hotkey = string.IsNullOrEmpty(Hotkey) ? "[No Hotkey]" : $"[{Hotkey}]";
            return $"{status} {Name} {hotkey} - {Actions.Count} actions";
        }
    }
}
