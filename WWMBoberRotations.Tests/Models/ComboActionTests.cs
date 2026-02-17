using WWMBoberRotations.Models;
using Xunit;

namespace WWMBoberRotations.Tests.Models
{
    public class ComboActionTests
    {
        [Fact]
        public void Duration_Negative_ClampedToZero()
        {
            var action = new ComboAction { Duration = -100 };
            Assert.Equal(0, action.Duration);
        }

        [Fact]
        public void DelayAfter_Negative_ClampedToZero()
        {
            var action = new ComboAction { DelayAfter = -50 };
            Assert.Equal(0, action.DelayAfter);
        }

        [Fact]
        public void ToString_KeyPress_FormatsCorrectly()
        {
            var action = new ComboAction { Type = ActionType.KeyPress, Key = "q" };
            Assert.Equal("Press: q", action.ToString());
        }

        [Fact]
        public void ToString_KeyPress_WithDelayAfter_IncludesDelay()
        {
            var action = new ComboAction
            {
                Type = ActionType.KeyPress,
                Key = "q",
                DelayAfter = 500
            };
            Assert.Equal("Press: q, delay 500ms", action.ToString());
        }

        [Fact]
        public void ToString_KeyHold_FormatsCorrectly()
        {
            var action = new ComboAction
            {
                Type = ActionType.KeyHold,
                Key = "space",
                Duration = 1000
            };
            Assert.Equal("Hold: space for 1000ms", action.ToString());
        }

        [Fact]
        public void ToString_MouseClick_Left_FormatsCorrectly()
        {
            var action = new ComboAction
            {
                Type = ActionType.MouseClick,
                Button = MouseButton.Left
            };
            Assert.Equal("Click: Left Mouse Button", action.ToString());
        }

        [Fact]
        public void ToString_MouseClick_XButton1_ShowsMouse4()
        {
            var action = new ComboAction
            {
                Type = ActionType.MouseClick,
                Button = MouseButton.XButton1
            };
            Assert.Equal("Click: Mouse 4", action.ToString());
        }

        [Fact]
        public void ToString_MouseClick_XButton2_ShowsMouse5()
        {
            var action = new ComboAction
            {
                Type = ActionType.MouseClick,
                Button = MouseButton.XButton2
            };
            Assert.Equal("Click: Mouse 5", action.ToString());
        }

        [Fact]
        public void ToString_Delay_FormatsCorrectly()
        {
            var action = new ComboAction
            {
                Type = ActionType.Delay,
                Duration = 300
            };
            Assert.Equal("Delay: 300ms", action.ToString());
        }
    }
}
