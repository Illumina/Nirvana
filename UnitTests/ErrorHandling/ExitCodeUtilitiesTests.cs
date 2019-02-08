using System;
using System.Threading;
using ErrorHandling;
using ErrorHandling.Exceptions;
using Xunit;

namespace UnitTests.ErrorHandling
{
    public sealed class ExitCodeUtilitiesTests
    {
        [Fact]
        public void ShowException_CompressionException_CheckExitCode()
        {
            var compressionException = new CompressionException("test");
            compressionException.Data[ExitCodeUtilities.VcfLine] = "chr1\t100\tA\tC";
            var exitCode = ExitCodeUtilities.ShowException(compressionException);
            Assert.Equal(ExitCodes.Compression, exitCode);
        }

        [Fact]
        public void ShowException_UnknownException_ExitCode_ShouldBeOne()
        {
            var unknownException = new AbandonedMutexException();
            var exitCode = ExitCodeUtilities.ShowException(unknownException);
            Assert.Equal(ExitCodes.InvalidFunction, exitCode);
        }

        [Fact]
        public void ShowException_AggregateException_ExitCode_ShouldBeOne()
        {
            // TODO: It would be great to verify which exception was shown
            var refNullException   = new NullReferenceException();
            var aggregateException = new AggregateException(refNullException);
            var exitCode           = ExitCodeUtilities.ShowException(aggregateException);
            Assert.Equal(ExitCodes.InvalidFunction, exitCode);
        }
    }
}
