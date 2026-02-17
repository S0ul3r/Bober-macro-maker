using System.Globalization;
using System.Windows;
using WWMBoberRotations.Converters;
using Xunit;

namespace WWMBoberRotations.Tests.Converters
{
    public class StringNotEmptyToVisibilityConverterTests
    {
        private readonly StringNotEmptyToVisibilityConverter _converter = new();

        [Fact]
        public void Convert_NonEmptyString_ReturnsVisible()
        {
            var result = _converter.Convert("hello", typeof(Visibility), null!, CultureInfo.InvariantCulture);
            Assert.Equal(Visibility.Visible, result);
        }

        [Fact]
        public void Convert_EmptyString_ReturnsCollapsed()
        {
            var result = _converter.Convert("", typeof(Visibility), null!, CultureInfo.InvariantCulture);
            Assert.Equal(Visibility.Collapsed, result);
        }

        [Fact]
        public void Convert_Null_ReturnsCollapsed()
        {
            var result = _converter.Convert(null!, typeof(Visibility), null!, CultureInfo.InvariantCulture);
            Assert.Equal(Visibility.Collapsed, result);
        }

        [Fact]
        public void Convert_WhitespaceOnly_ReturnsVisible()
        {
            var result = _converter.Convert("   ", typeof(Visibility), null!, CultureInfo.InvariantCulture);
            Assert.Equal(Visibility.Visible, result);
        }

        [Fact]
        public void ConvertBack_ThrowsNotImplementedException()
        {
            Assert.Throws<System.NotImplementedException>(() =>
                _converter.ConvertBack(Visibility.Visible, typeof(string), null!, CultureInfo.InvariantCulture));
        }
    }
}
