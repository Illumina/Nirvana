using System;
using System.Collections;
using System.Collections.Generic;
using ErrorHandling;
using ErrorHandling.Exceptions;
using Xunit;

namespace UnitTests.ErrorHandling.Exceptions
{
    public sealed class ExceptionsTests
    {
        private sealed class ExceptionGenerator : IEnumerable<object[]>
        {
            private readonly List<object[]> _data = new List<object[]>
            {
                new object[] { new CompressionException("test"),               ExitCodes.Compression},
                new object[] { new FileNotSortedException("test"),             ExitCodes.FileNotSorted},
                new object[] { new InvalidFileFormatException("test"),         ExitCodes.InvalidFileFormat},
                new object[] { new MissingCompressionLibraryException("test"), ExitCodes.MissingCompressionLibrary},
                new object[] { new ProcessLockedFileException("test"),         ExitCodes.SharingViolation},
                new object[] { new UserErrorException("test"),                 ExitCodes.UserError}
            };

            public IEnumerator<object[]> GetEnumerator() => _data.GetEnumerator();
            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }

        [Theory]
        [ClassData(typeof(ExceptionGenerator))]
        public void Check_ExceptionToExitCode_Mapping(Exception ex, ExitCodes expectedExitCode)
        {
            ExitCodes observedExitCode = ExitCodeUtilities.GetExitCode(ex.GetType());
            Assert.Equal(expectedExitCode, observedExitCode);
        }
    }
}
