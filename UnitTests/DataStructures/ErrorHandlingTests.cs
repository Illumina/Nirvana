using System;
using System.IO;
using ErrorHandling.DataStructures;
using ErrorHandling.Exceptions;
using ErrorHandling.Utilities;
using Xunit;

namespace UnitTests.DataStructures
{
    public sealed class ErrorHandlingTests
    {
        [Fact]
        public void AccessDenied()
        {
            var exception = new UnauthorizedAccessException();
            var observedExitCode = ExitCodeUtilities.ShowException(exception);

            Assert.Equal((int)ExitCodes.AccessDenied, observedExitCode);
        }

        [Fact]
        public void BadArguments()
        {
            var exception = new ArgumentNullException();
            var observedExitCode = ExitCodeUtilities.ShowException(exception);

            Assert.Equal((int)ExitCodes.BadArguments, observedExitCode);
        }

        [Fact]
        public void BadFormat()
        {
            var exception = new FormatException();
            var observedExitCode = ExitCodeUtilities.ShowException(exception);

            Assert.Equal((int)ExitCodes.BadFormat, observedExitCode);
        }

        [Fact]
        public void CallNotImplemented()
        {
            var exception = new NotImplementedException();
            var observedExitCode = ExitCodeUtilities.ShowException(exception);

            Assert.Equal((int)ExitCodes.CallNotImplemented, observedExitCode);
        }

        [Fact]
        public void FileNotFound()
        {
            var exception = new FileNotFoundException();
            var observedExitCode = ExitCodeUtilities.ShowException(exception);

            Assert.Equal((int)ExitCodes.FileNotFound, observedExitCode);
        }

        [Fact]
        public void FileNotSorted()
        {
            var exception = new FileNotSortedException("test");
            var observedExitCode = ExitCodeUtilities.ShowException(exception);

            Assert.Equal((int)ExitCodes.FileNotSorted, observedExitCode);
        }

        [Fact]
        public void InvalidData()
        {
            var exception = new InvalidDataException();
            var observedExitCode = ExitCodeUtilities.ShowException(exception);

            Assert.Equal((int)ExitCodes.InvalidData, observedExitCode);
        }

        [Fact]
        public void InvalidFileFormat()
        {
            var exception = new InvalidFileFormatException("test");
            var observedExitCode = ExitCodeUtilities.ShowException(exception);

            Assert.Equal((int)ExitCodes.InvalidFileFormat, observedExitCode);
        }

        [Fact]
        public void InvalidFunction()
        {
            var exception = new GeneralException("test");
            var observedExitCode = ExitCodeUtilities.ShowException(exception);

            Assert.Equal((int)ExitCodes.InvalidFunction, observedExitCode);

            var exception2 = new Exception();
            observedExitCode = ExitCodeUtilities.ShowException(exception2);

            Assert.Equal((int)ExitCodes.InvalidFunction, observedExitCode);

            var exception3 = new InvalidOperationException();
            observedExitCode = ExitCodeUtilities.ShowException(exception3);

            Assert.Equal((int)ExitCodes.InvalidFunction, observedExitCode);
        }

        [Fact]
        public void MissingCompressionLibrary()
        {
            var exception = new MissingCompressionLibraryException("test");
            var observedExitCode = ExitCodeUtilities.ShowException(exception);

            Assert.Equal((int)ExitCodes.MissingCompressionLibrary, observedExitCode);
        }

        [Fact]
        public void OutofMemory()
        {
            var exception = new OutOfMemoryException();
            var observedExitCode = ExitCodeUtilities.ShowException(exception);

            Assert.Equal((int)ExitCodes.OutofMemory, observedExitCode);
        }

        [Fact]
        public void SharingViolation()
        {
            var exception = new ProcessLockedFileException("test");
            var observedExitCode = ExitCodeUtilities.ShowException(exception);

            Assert.Equal((int)ExitCodes.SharingViolation, observedExitCode);
        }

        [Fact]
        public void UserError()
        {
            var exception = new UserErrorException("test");
            var observedExitCode = ExitCodeUtilities.ShowException(exception);

            Assert.Equal((int)ExitCodes.UserError, observedExitCode);
        }
    }
}