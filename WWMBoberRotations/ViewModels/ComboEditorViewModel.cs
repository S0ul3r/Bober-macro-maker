using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using WWMBoberRotations.Models;
using WWMBoberRotations.Views;

namespace WWMBoberRotations.ViewModels
{
    public class ComboEditorViewModel : ViewModelBase
    {
        private readonly Combo _combo;
        private ComboAction? _selectedAction;

        public string Name
        {
            get => _combo.Name;
            set
            {
                _combo.Name = value;
                OnPropertyChanged();
            }
        }

        public string? Hotkey
        {
            get => _combo.Hotkey;
            set
            {
                _combo.Hotkey = value;
                OnPropertyChanged();
            }
        }

        public bool IsEnabled
        {
            get => _combo.IsEnabled;
            set
            {
                _combo.IsEnabled = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<ComboAction> Actions => _combo.Actions;

        public ComboAction? SelectedAction
        {
            get => _selectedAction;
            set => SetProperty(ref _selectedAction, value);
        }

        public ICommand AddActionCommand { get; }
        public ICommand EditActionCommand { get; }
        public ICommand DeleteActionCommand { get; }
        public ICommand MoveUpCommand { get; }
        public ICommand MoveDownCommand { get; }

        public ComboEditorViewModel(Combo combo)
        {
            _combo = combo;

            AddActionCommand = new RelayCommand(AddAction);
            EditActionCommand = new RelayCommand(EditAction, () => SelectedAction != null);
            DeleteActionCommand = new RelayCommand(DeleteAction, () => SelectedAction != null);
            MoveUpCommand = new RelayCommand(MoveUp, CanMoveUp);
            MoveDownCommand = new RelayCommand(MoveDown, CanMoveDown);
        }

        private void AddAction()
        {
            var editor = new ActionEditorWindow();
            if (editor.ShowDialog() == true && editor.Result != null)
            {
                Actions.Add(editor.Result);
            }
        }

        private void EditAction()
        {
            if (SelectedAction == null) return;

            var editor = new ActionEditorWindow(SelectedAction);
            if (editor.ShowDialog() == true)
            {
                var index = Actions.IndexOf(SelectedAction);
                Actions[index] = editor.Result!;
                OnPropertyChanged(nameof(Actions));
            }
        }

        private void DeleteAction()
        {
            if (SelectedAction == null) return;
            Actions.Remove(SelectedAction);
        }

        private void MoveUp()
        {
            if (SelectedAction == null) return;

            var index = Actions.IndexOf(SelectedAction);
            if (index > 0)
            {
                Actions.Move(index, index - 1);
            }
        }

        private void MoveDown()
        {
            if (SelectedAction == null) return;

            var index = Actions.IndexOf(SelectedAction);
            if (index < Actions.Count - 1)
            {
                Actions.Move(index, index + 1);
            }
        }

        private bool CanMoveUp()
        {
            return SelectedAction != null && Actions.IndexOf(SelectedAction) > 0;
        }

        private bool CanMoveDown()
        {
            return SelectedAction != null && Actions.IndexOf(SelectedAction) < Actions.Count - 1;
        }

        public Combo GetCombo()
        {
            var result = new Combo
            {
                Name = _combo.Name,
                Hotkey = _combo.Hotkey,
                IsEnabled = _combo.IsEnabled
            };
            
            foreach (var action in _combo.Actions)
            {
                result.Actions.Add(action);
            }
            
            return result;
        }
    }
}
