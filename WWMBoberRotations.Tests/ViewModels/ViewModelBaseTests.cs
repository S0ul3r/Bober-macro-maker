using System.ComponentModel;
using WWMBoberRotations.ViewModels;
using Xunit;

namespace WWMBoberRotations.Tests.ViewModels
{
    public class ViewModelBaseTests
    {
        private class ConcreteViewModel : ViewModelBase
        {
            private string? _name;
            public string? Name
            {
                get => _name;
                set => SetProperty(ref _name, value);
            }
        }

        [Fact]
        public void SetProperty_WhenValueChanges_RaisesPropertyChanged()
        {
            var vm = new ConcreteViewModel();
            var raised = false;
            vm.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == nameof(ConcreteViewModel.Name))
                    raised = true;
            };

            vm.Name = "Test";

            Assert.True(raised);
            Assert.Equal("Test", vm.Name);
        }

        [Fact]
        public void SetProperty_WhenValueSame_DoesNotRaisePropertyChanged()
        {
            var vm = new ConcreteViewModel();
            vm.Name = "Same";
            var raised = false;
            vm.PropertyChanged += (_, _) => raised = true;

            vm.Name = "Same";

            Assert.False(raised);
        }

        [Fact]
        public void OnPropertyChanged_RaisesEvent()
        {
            var vm = new ConcreteViewModel();
            string? raisedName = null;
            vm.PropertyChanged += (_, e) => raisedName = e.PropertyName;

            vm.Name = "NewName";

            Assert.Equal(nameof(ConcreteViewModel.Name), raisedName);
            Assert.Equal("NewName", vm.Name);
        }
    }
}
