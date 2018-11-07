using System;
using System.Net;
using IO;
using Moq;
using UnitTests.Cloud;
using Xunit;

namespace UnitTests.IO
{
    public sealed class FailureRecoveryTests
    {
        private const int ExpectedReturnValue = 1;

        [Fact]
        public void CallWithRetryTests_ReturnCorrectValue_WithSufficientRetries()
        {
            var mockClient = CreateMockClient();
            var returnedFileSize = FailureRecovery.CallWithRetry(mockClient.IntMethod, out int retryCounter, 3);
            Assert.Equal(ExpectedReturnValue, returnedFileSize);
            Assert.Equal(2, retryCounter);
        }

        [Fact]
        public void CallWithRetryTests_ReturnCorrectValue_WithMoreThanSufficientRetries()
        {
            var mockClient = CreateMockClient();
            var returnedFileSize = FailureRecovery.CallWithRetry(mockClient.IntMethod, out int retryCounter, 4);
            Assert.Equal(ExpectedReturnValue, returnedFileSize);
            Assert.Equal(2, retryCounter);
        }

        [Fact]
        public void CallWithRetryTests_ThrowException_WithInsufficientRetries()
        {
            var mockClient = CreateMockClient();

            Assert.Throws<AggregateException>(() =>
                FailureRecovery.CallWithRetry(mockClient.IntMethod, out int _, 2));
        }

        private IMockInterface CreateMockClient()
        {
            var mockClient = new Mock<IMockInterface>();
            mockClient.SetupSequence(x => x.IntMethod())
                .Throws(new WebException())
                .Throws(new WebException())
                .Returns(ExpectedReturnValue)
                .Throws(new WebException());

            return mockClient.Object;
        }
    }
}   