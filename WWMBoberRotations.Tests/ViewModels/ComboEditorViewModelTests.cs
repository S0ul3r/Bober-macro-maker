using System.Windows.Input;
using WWMBoberRotations.Models;
using WWMBoberRotations.ViewModels;
using Xunit;

namespace WWMBoberRotations.Tests.ViewModels
{
    public class ComboEditorViewModelTests
    {
        [Fact]
        public void Name_ReflectsComboName()
        {
            var combo = new Combo { Name = "Original" };
            var vm = new ComboEditorViewModel(combo);
            Assert.Equal("Original", vm.Name);

            vm.Name = "Updated";
            Assert.Equal("Updated", vm.Name);
            Assert.Equal("Updated", combo.Name);
        }

        [Fact]
        public void Hotkey_ReflectsComboHotkey()
        {
            var combo = new Combo { Hotkey = "f1" };
            var vm = new ComboEditorViewModel(combo);
            Assert.Equal("f1", vm.Hotkey);

            vm.Hotkey = "f2";
            Assert.Equal("f2", vm.Hotkey);
            Assert.Equal("f2", combo.Hotkey);
        }

        [Fact]
        public void IsEnabled_ReflectsComboIsEnabled()
        {
            var combo = new Combo { IsEnabled = true };
            var vm = new ComboEditorViewModel(combo);
            Assert.True(vm.IsEnabled);

            vm.IsEnabled = false;
            Assert.False(vm.IsEnabled);
            Assert.False(combo.IsEnabled);
        }

        [Fact]
        public void Actions_IsSameCollectionAsCombo()
        {
            var combo = new Combo();
            var action = new ComboAction { Type = ActionType.KeyPress, Key = "q" };
            combo.Actions.Add(action);

            var vm = new ComboEditorViewModel(combo);
            Assert.Same(combo.Actions, vm.Actions);
            Assert.Single(vm.Actions);
        }

        [Fact]
        public void GetCombo_ReturnsNewInstanceWithSameData()
        {
            var combo = new Combo
            {
                Name = "Test",
                Hotkey = "x",
                IsEnabled = false
            };
            combo.Actions.Add(new ComboAction { Type = ActionType.KeyPress, Key = "e" });

            var vm = new ComboEditorViewModel(combo);
            var result = vm.GetCombo();

            Assert.NotSame(combo, result);
            Assert.Equal(combo.Name, result.Name);
            Assert.Equal(combo.Hotkey, result.Hotkey);
            Assert.Equal(combo.IsEnabled, result.IsEnabled);
            Assert.Single(result.Actions);
            Assert.Equal("e", result.Actions[0].Key);
        }

        [Fact]
        public void DeleteActionCommand_WhenNoSelection_CanExecuteFalse()
        {
            var combo = new Combo();
            combo.Actions.Add(new ComboAction { Type = ActionType.KeyPress, Key = "q" });
            var vm = new ComboEditorViewModel(combo);
            vm.SelectedAction = null;
            Assert.False(vm.DeleteActionCommand.CanExecute(null));
        }

        [Fact]
        public void DeleteActionCommand_WhenSelected_CanExecuteTrue()
        {
            var combo = new Combo();
            var action = new ComboAction { Type = ActionType.KeyPress, Key = "q" };
            combo.Actions.Add(action);
            var vm = new ComboEditorViewModel(combo);
            vm.SelectedAction = action;
            Assert.True(vm.DeleteActionCommand.CanExecute(null));
        }

        [Fact]
        public void DeleteActionCommand_Execute_RemovesSelectedAction()
        {
            var combo = new Combo();
            var action = new ComboAction { Type = ActionType.KeyPress, Key = "q" };
            combo.Actions.Add(action);
            var vm = new ComboEditorViewModel(combo);
            vm.SelectedAction = action;

            vm.DeleteActionCommand.Execute(null);

            Assert.Empty(combo.Actions);
        }

        [Fact]
        public void MoveUpCommand_WhenFirstItem_CanExecuteFalse()
        {
            var combo = new Combo();
            var a1 = new ComboAction { Type = ActionType.KeyPress, Key = "1" };
            combo.Actions.Add(a1);
            combo.Actions.Add(new ComboAction { Type = ActionType.KeyPress, Key = "2" });
            var vm = new ComboEditorViewModel(combo);
            vm.SelectedAction = a1;
            Assert.False(vm.MoveUpCommand.CanExecute(null));
        }

        [Fact]
        public void MoveDownCommand_WhenLastItem_CanExecuteFalse()
        {
            var combo = new Combo();
            combo.Actions.Add(new ComboAction { Type = ActionType.KeyPress, Key = "1" });
            var a2 = new ComboAction { Type = ActionType.KeyPress, Key = "2" };
            combo.Actions.Add(a2);
            var vm = new ComboEditorViewModel(combo);
            vm.SelectedAction = a2;
            Assert.False(vm.MoveDownCommand.CanExecute(null));
        }

        [Fact]
        public void MoveDownCommand_Execute_MovesItemDown()
        {
            var combo = new Combo();
            var a1 = new ComboAction { Type = ActionType.KeyPress, Key = "1" };
            var a2 = new ComboAction { Type = ActionType.KeyPress, Key = "2" };
            combo.Actions.Add(a1);
            combo.Actions.Add(a2);
            var vm = new ComboEditorViewModel(combo);
            vm.SelectedAction = a1;

            vm.MoveDownCommand.Execute(null);

            Assert.Same(a2, combo.Actions[0]);
            Assert.Same(a1, combo.Actions[1]);
        }

        [Fact]
        public void MoveUpCommand_Execute_MovesItemUp()
        {
            var combo = new Combo();
            var a1 = new ComboAction { Type = ActionType.KeyPress, Key = "1" };
            var a2 = new ComboAction { Type = ActionType.KeyPress, Key = "2" };
            combo.Actions.Add(a1);
            combo.Actions.Add(a2);
            var vm = new ComboEditorViewModel(combo);
            vm.SelectedAction = a2;

            vm.MoveUpCommand.Execute(null);

            Assert.Same(a2, combo.Actions[0]);
            Assert.Same(a1, combo.Actions[1]);
        }
    }
}
