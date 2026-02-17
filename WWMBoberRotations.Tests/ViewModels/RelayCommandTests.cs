using System;
using System.Windows.Input;
using WWMBoberRotations.ViewModels;
using Xunit;

namespace WWMBoberRotations.Tests.ViewModels
{
    public class RelayCommandTests
    {
        [Fact]
        public void Constructor_NullExecute_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new RelayCommand((Action<object?>)null!));
        }

        [Fact]
        public void Execute_InvokesAction()
        {
            var invoked = false;
            var cmd = new RelayCommand(_ => invoked = true);
            cmd.Execute(null);
            Assert.True(invoked);
        }

        [Fact]
        public void Execute_WithParameter_PassesParameter()
        {
            object? received = null;
            var cmd = new RelayCommand(p => received = p);
            cmd.Execute("hello");
            Assert.Equal("hello", received);
        }

        [Fact]
        public void CanExecute_WhenNoCanExecuteFunc_ReturnsTrue()
        {
            var cmd = new RelayCommand(_ => { });
            Assert.True(cmd.CanExecute(null));
        }

        [Fact]
        public void CanExecute_WhenFuncReturnsTrue_ReturnsTrue()
        {
            var cmd = new RelayCommand(_ => { }, _ => true);
            Assert.True(cmd.CanExecute(null));
        }

        [Fact]
        public void CanExecute_WhenFuncReturnsFalse_ReturnsFalse()
        {
            var cmd = new RelayCommand(_ => { }, _ => false);
            Assert.False(cmd.CanExecute(null));
        }

        [Fact]
        public void RelayCommand_ActionOverload_ExecuteInvokesAction()
        {
            var invoked = false;
            var cmd = new RelayCommand(() => invoked = true);
            cmd.Execute(null);
            Assert.True(invoked);
        }

        [Fact]
        public void RelayCommand_ActionOverload_CanExecuteUsesFunc()
        {
            var cmd = new RelayCommand(() => { }, () => false);
            Assert.False(cmd.CanExecute(null));
        }
    }
}
