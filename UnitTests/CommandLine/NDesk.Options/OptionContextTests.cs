using System;
using CommandLine.NDesk.Options;
using Xunit;

namespace UnitTests.CommandLine.NDesk.Options
{
    public sealed class OptionContextTests
    {
        private readonly OptionSet _optionSet;

        public OptionContextTests()
        {
            _optionSet = new OptionSet
            {
                { "a=", "test", v => { /* ignore */ } }
            };
        }

        [Fact]
        public void Should_ThrowException_When_ContextIsEmpty()
        {
            var optionContext = new OptionContext();

            Assert.Throws<InvalidOperationException>(delegate
            {
                // ReSharper disable once UnusedVariable
                string ignore = optionContext.OptionValues[0];
            });
        }

        [Fact]
        public void Should_ThrowException_When_IndexGreaterThanLength()
        {
            var optionContext = new OptionContext { Option = _optionSet[0] };

            Assert.Throws<ArgumentOutOfRangeException>(delegate
            {
                // ReSharper disable once UnusedVariable
                string ignore = optionContext.OptionValues[2];
            });
        }

        [Fact]
        public void Should_ThrowException_When_RequiredValueMissing()
        {
            var optionContext = new OptionContext { Option = _optionSet[0], OptionName = "-a" };

            Assert.Throws<OptionException>(delegate
            {
                // ReSharper disable once UnusedVariable
                string ignore = optionContext.OptionValues[0];
            });
        }
    }
}
