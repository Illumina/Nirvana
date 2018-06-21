using System;
using System.IO;
using CommandLine.NDesk.Options;
using Xunit;

namespace UnitTests.CommandLine.NDesk.Options
{
    public sealed class OptionsTests
    {
        [Fact]
        public void Should_ThrowException_When_PrototypeNull()
        {
            Assert.Throws<ArgumentNullException>(delegate
            {
                // ReSharper disable once UnusedVariable
                var option = new DefaultOption(null, null);
            });
        }

        [Fact]
        public void Should_ThrowException_When_PrototypeEmpty()
        {
            Assert.Throws<ArgumentException>(delegate
            {
                // ReSharper disable once UnusedVariable
                var option = new DefaultOption("", null);
            });
        }

        [Fact]
        public void Should_ThrowException_When_OptionNameEmpty()
        {
            Assert.Throws<InvalidDataException>(delegate
            {
                // ReSharper disable once UnusedVariable
                var option = new DefaultOption("a|b||c=", null);
            });
        }

        [Fact]
        public void Should_ThrowException_When_OptionTypesConflict()
        {
            Assert.Throws<InvalidDataException>(delegate
            {
                // ReSharper disable once UnusedVariable
                var option = new DefaultOption("a=|b:", null);
            });
        }

        [Fact]
        public void Should_ThrowException_When_DefaultHandlerRequiresValue()
        {
            Assert.Throws<ArgumentException>(delegate
            {
                // ReSharper disable once UnusedVariable
                var option = new DefaultOption("<>=", null);
            });
        }

        [Fact]
        public void Should_ThrowException_When_DefaultHandlerRequiresValues()
        {
            Assert.Throws<ArgumentException>(delegate
            {
                // ReSharper disable once UnusedVariable
                var option = new DefaultOption("<>:", null);
            });

            Assert.Throws<ArgumentException>(delegate
            {
                // ReSharper disable once UnusedVariable
                var option = new DefaultOption("t|<>=", null, 2);
            });
        }

        [Fact]
        public void Should_Not_ThrowException()
        {
            // ReSharper disable NotAccessedVariable
            // ReSharper disable RedundantAssignment
            var ex = Record.Exception(() =>
            {
                var option = new DefaultOption("a|b=", null, 2);                
                option     = new DefaultOption("t|<>=", null, 1);
                option     = new DefaultOption("a", null, 0);
            });
            // ReSharper restore RedundantAssignment
            // ReSharper restore NotAccessedVariable

            Assert.Null(ex);
        }

        [Fact]
        public void Should_ThrowException_When_MaxValueCountOutOfRange()
        {
            Assert.Throws<ArgumentOutOfRangeException>(delegate
            {
                // ReSharper disable once UnusedVariable
                var option = new DefaultOption("a", null, -1);
            });
        }

        [Fact]
        public void Should_ThrowException_When_MaxValueCountZero_And_RequiredType()
        {
            Assert.Throws<ArgumentException>(delegate
            {
                // ReSharper disable once UnusedVariable
                var option = new DefaultOption("a=", null, 0);
            });
        }

        [Fact]
        public void Should_ThrowException_With_IllFormedSeparator()
        {
            Assert.Throws<ArgumentException>(delegate
            {
                // ReSharper disable once UnusedVariable
                var option = new DefaultOption("a={", null);
            });

            Assert.Throws<ArgumentException>(delegate
            {
                // ReSharper disable once UnusedVariable
                var option = new DefaultOption("a=}", null);
            });

            Assert.Throws<ArgumentException>(delegate
            {
                // ReSharper disable once UnusedVariable
                var option = new DefaultOption("a={{}}", null);
            });

            Assert.Throws<ArgumentException>(delegate
            {
                // ReSharper disable once UnusedVariable
                var option = new DefaultOption("a={}}", null);
            });

            Assert.Throws<ArgumentException>(delegate
            {
                // ReSharper disable once UnusedVariable
                var option = new DefaultOption("a={}{", null);
            });
        }

        [Fact]
        public void Should_ThrowException_When_CannotProvideSeparatorsWhenTakingOneValue()
        {
            Assert.Throws<InvalidDataException>(delegate
            {
                // ReSharper disable once UnusedVariable
                var option = new DefaultOption("a==", null);
            });

            Assert.Throws<InvalidDataException>(delegate
            {
                // ReSharper disable once UnusedVariable
                var option = new DefaultOption("a={}", null);
            });

            Assert.Throws<InvalidDataException>(delegate
            {
                // ReSharper disable once UnusedVariable
                var option = new DefaultOption("a=+-*/", null);
            });
        }

        private sealed class DefaultOption : Option
        {
            public DefaultOption(string prototypes, string description)
                : base(prototypes, description, 1)
            {}

            public DefaultOption(string prototypes, string description, int c)
                : base(prototypes, description, c)
            {}

            protected override void OnParseComplete(OptionContext c)
            {
                throw new NotImplementedException();
            }
        }
    }
}
