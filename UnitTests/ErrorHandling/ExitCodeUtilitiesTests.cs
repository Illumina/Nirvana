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
    }
}
