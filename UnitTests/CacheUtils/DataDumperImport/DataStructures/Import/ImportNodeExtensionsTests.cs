using System.IO;
using CacheUtils.DataDumperImport.DataStructures.Import;
using Xunit;

namespace UnitTests.CacheUtils.DataDumperImport.DataStructures.Import
{
    public sealed class ImportNodeExtensionsTests
    {
        [Fact]
        public void GetInt32_Nominal()
        {
            var node = new StringKeyValueNode("bob", "123");
            var observedResult = node.GetInt32();
            Assert.Equal(123, observedResult);
        }

        [Fact]
        public void GetInt32_ReturnMinusOne_WhenNull()
        {
            var node = new StringKeyValueNode("bob", null);
            var observedResult = node.GetInt32();
            Assert.Equal(-1, observedResult);
        }

        [Fact]
        public void GetInt32_ThrowException_When_NotNumber()
        {
            var node = new StringKeyValueNode("bob", "123N");

            Assert.Throws<InvalidDataException>(delegate
            {
                // ReSharper disable once UnusedVariable
                var observedResult = node.GetInt32();
            });
        }

        [Fact]
        public void GetString_ThrowException_When_NotCorrectType()
        {
            var node = new ObjectKeyValueNode("bob", null);

            Assert.Throws<InvalidDataException>(delegate
            {
                // ReSharper disable once UnusedVariable
                var observedResult = node.GetString();
            });
        }

        [Fact]
        public void GetString_ReturnNull_IfEmptyOrMinus()
        {
            var node = new StringKeyValueNode("bob", "-");
            var observedResult = node.GetString();
            Assert.Null(observedResult);

            node = new StringKeyValueNode("bob", "");
            observedResult = node.GetString();
            Assert.Null(observedResult);
        }

        [Fact]
        public void GetBool_ReturnTrue()
        {
            var node = new StringKeyValueNode("bob", "1");
            var observedResult = node.GetBool();
            Assert.True(observedResult);
        }

        [Fact]
        public void GetBool_ReturnFalse()
        {
            var node = new StringKeyValueNode("bob", "0");
            var observedResult = node.GetBool();
            Assert.False(observedResult);
        }

        [Fact]
        public void IsUndefined_ReturnTrue()
        {
            var node = new StringKeyValueNode("bob", null);
            var observedResult = node.IsUndefined();
            Assert.True(observedResult);
        }

        [Fact]
        public void IsUndefined_ReturnFalse()
        {
            var node = new StringKeyValueNode("bob", "test");
            var observedResult = node.IsUndefined();
            Assert.False(observedResult);
        }

        [Fact]
        public void IsUndefined_ReturnFalse_IncorrectType()
        {
            var node = new ObjectKeyValueNode("bob", null);
            var observedResult = node.IsUndefined();
            Assert.False(observedResult);
        }
    }
}
