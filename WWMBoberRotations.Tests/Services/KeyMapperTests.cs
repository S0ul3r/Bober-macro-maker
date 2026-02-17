using System.Windows.Input;
using WWMBoberRotations.Services;
using Xunit;

namespace WWMBoberRotations.Tests.Services
{
    public class KeyMapperTests
    {
        [Theory]
        [InlineData(null, 0)]
        [InlineData("", 0)]
        public void GetVirtualKeyCode_NullOrEmpty_ReturnsZero(string? key, int expected)
        {
            Assert.Equal(expected, KeyMapper.GetVirtualKeyCode(key!));
        }

        [Theory]
        [InlineData("a", 0x41)]
        [InlineData("A", 0x41)]
        [InlineData("z", 0x5A)]
        [InlineData("q", 0x51)]
        public void GetVirtualKeyCode_Letter_ReturnsCorrectCode(string key, int expectedVk)
        {
            Assert.Equal(expectedVk, KeyMapper.GetVirtualKeyCode(key));
        }

        [Theory]
        [InlineData("0", 0x30)]
        [InlineData("9", 0x39)]
        [InlineData("5", 0x35)]
        public void GetVirtualKeyCode_Digit_ReturnsCorrectCode(string key, int expectedVk)
        {
            Assert.Equal(expectedVk, KeyMapper.GetVirtualKeyCode(key));
        }

        [Theory]
        [InlineData("space", 0x20)]
        [InlineData("SPACE", 0x20)]
        [InlineData("enter", 0x0D)]
        [InlineData("tab", 0x09)]
        [InlineData("esc", 0x1B)]
        [InlineData("shift", 0x10)]
        [InlineData("ctrl", 0x11)]
        [InlineData("f1", 0x70)]
        [InlineData("f12", 0x7B)]
        [InlineData("up", 0x26)]
        [InlineData("down", 0x28)]
        [InlineData("left", 0x25)]
        [InlineData("right", 0x27)]
        public void GetVirtualKeyCode_SpecialKeys_ReturnsCorrectCode(string key, int expectedVk)
        {
            Assert.Equal(expectedVk, KeyMapper.GetVirtualKeyCode(key));
        }

        [Fact]
        public void GetVirtualKeyCode_UnknownKey_ReturnsZero()
        {
            Assert.Equal(0, KeyMapper.GetVirtualKeyCode("unknownkey"));
        }

        [Theory]
        [InlineData(null, 0)]
        [InlineData("", 0)]
        public void GetMouseButtonCode_NullOrEmpty_ReturnsZero(string? button, int expected)
        {
            Assert.Equal(expected, KeyMapper.GetMouseButtonCode(button!));
        }

        [Theory]
        [InlineData("lmb", 0x01)]
        [InlineData("LMB", 0x01)]
        [InlineData("rmb", 0x02)]
        [InlineData("mmb", 0x04)]
        [InlineData("mouse4", 0x05)]
        [InlineData("mouse5", 0x06)]
        [InlineData("xbutton1", 0x05)]
        [InlineData("xbutton2", 0x06)]
        public void GetMouseButtonCode_ValidButton_ReturnsCorrectCode(string button, int expected)
        {
            Assert.Equal(expected, KeyMapper.GetMouseButtonCode(button));
        }

        [Fact]
        public void GetMouseButtonCode_Unknown_ReturnsZero()
        {
            Assert.Equal(0, KeyMapper.GetMouseButtonCode("unknown"));
        }

        [Theory]
        [InlineData("lmb", true)]
        [InlineData("rmb", true)]
        [InlineData("mouse4", true)]
        [InlineData("q", false)]
        [InlineData("space", false)]
        [InlineData(null, false)]
        [InlineData("", false)]
        public void IsMouseButton_ReturnsExpected(string? key, bool expected)
        {
            Assert.Equal(expected, KeyMapper.IsMouseButton(key!));
        }

        [Fact]
        public void GetAllMouseButtonCodes_ReturnsFiveEntries()
        {
            var codes = KeyMapper.GetAllMouseButtonCodes();
            Assert.Equal(5, codes.Count);
            Assert.Equal("lmb", codes[0x01]);
            Assert.Equal("rmb", codes[0x02]);
            Assert.Equal("mmb", codes[0x04]);
            Assert.Equal("mouse4", codes[0x05]);
            Assert.Equal("mouse5", codes[0x06]);
        }

        [Theory]
        [InlineData(Key.Space, "space")]
        [InlineData(Key.Enter, "enter")]
        [InlineData(Key.Escape, "esc")]
        [InlineData(Key.Tab, "tab")]
        [InlineData(Key.Up, "up")]
        [InlineData(Key.Down, "down")]
        [InlineData(Key.Left, "left")]
        [InlineData(Key.Right, "right")]
        [InlineData(Key.F1, "f1")]
        [InlineData(Key.F12, "f12")]
        [InlineData(Key.A, "a")]
        [InlineData(Key.Z, "z")]
        [InlineData(Key.D0, "0")]
        [InlineData(Key.D9, "9")]
        public void WpfKeyToString_CommonKeys_ReturnsExpected(Key key, string expected)
        {
            Assert.Equal(expected, KeyMapper.WpfKeyToString(key));
        }
    }
}
