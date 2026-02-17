using WWMBoberRotations.Models;
using Xunit;

namespace WWMBoberRotations.Tests.Models
{
    public class ComboTests
    {
        [Fact]
        public void Name_Setter_RaisesPropertyChanged()
        {
            var combo = new Combo();
            var raised = false;
            combo.PropertyChanged += (_, e) => raised = e.PropertyName == nameof(Combo.Name);

            combo.Name = "TestCombo";

            Assert.True(raised);
            Assert.Equal("TestCombo", combo.Name);
        }

        [Fact]
        public void Hotkey_Setter_RaisesPropertyChanged()
        {
            var combo = new Combo();
            var raised = false;
            combo.PropertyChanged += (_, e) => raised = e.PropertyName == nameof(Combo.Hotkey);

            combo.Hotkey = "f1";

            Assert.True(raised);
            Assert.Equal("f1", combo.Hotkey);
        }

        [Fact]
        public void IsEnabled_Setter_RaisesPropertyChanged()
        {
            var combo = new Combo();
            var raised = false;
            combo.PropertyChanged += (_, e) => raised = e.PropertyName == nameof(Combo.IsEnabled);

            combo.IsEnabled = false;

            Assert.True(raised);
            Assert.False(combo.IsEnabled);
        }

        [Fact]
        public void Actions_Initialized_Empty()
        {
            var combo = new Combo();
            Assert.NotNull(combo.Actions);
            Assert.Empty(combo.Actions);
        }

        [Fact]
        public void ToString_WithHotkey_FormatsCorrectly()
        {
            var combo = new Combo
            {
                Name = "MyCombo",
                Hotkey = "q",
                IsEnabled = true
            };
            combo.Actions.Add(new ComboAction { Type = ActionType.KeyPress, Key = "e" });

            var s = combo.ToString();

            Assert.Contains("✓", s);
            Assert.Contains("MyCombo", s);
            Assert.Contains("[q]", s);
            Assert.Contains("1 actions", s);
        }

        [Fact]
        public void ToString_NoHotkey_ShowsNoHotkey()
        {
            var combo = new Combo { Name = "X", Hotkey = null };
            var s = combo.ToString();
            Assert.Contains("[No Hotkey]", s);
        }

        [Fact]
        public void ToString_Disabled_ShowsCross()
        {
            var combo = new Combo { Name = "X", IsEnabled = false };
            var s = combo.ToString();
            Assert.Contains("✗", s);
        }
    }
}
