using System;
using System.IO;
using IO;
using Moq;
using Xunit;

namespace UnitTests.IO
{
    public sealed class PersistentStreamTests
    {
        private static IMockConnector GetMockConnector()
        {
            var buffer = new byte[ushort.MaxValue];
            var random = new Random();

            random.NextBytes(buffer);
            
            var mockConnector = new Mock<IMockConnector>();
            mockConnector.SetupSequence(x => x.ConnectorFunc(0))
                .Throws(new IOException())
                .Throws(new IOException())
                .Returns(new MemoryStream(buffer))
                .Throws(new IOException());

            return mockConnector.Object;
        }

        [Fact]
        public void Connect_failed_due_to_too_few_retries()
        {
            var connector = GetMockConnector();

            Assert.Throws<IOException>(() => ConnectUtilities.ConnectWithRetries(connector.ConnectorFunc, 0, 2));

        }

        [Fact]
        public void Connect_success_with_enough_retries()
        {
            var connector = GetMockConnector();

            Assert.NotNull(ConnectUtilities.ConnectWithRetries(connector.ConnectorFunc, 0, 4));

        }

        private static IMockConnector Connecter_returns_null_streams()
        {
            var buffer = new byte[ushort.MaxValue];
            var random = new Random();

            random.NextBytes(buffer);

            var mockConnector = new Mock<IMockConnector>();
            mockConnector.SetupSequence(x => x.ConnectorFunc(0))
                .Returns((Stream)null)
                .Returns((Stream)null)
                .Returns(new MemoryStream(buffer))
                .Returns((Stream)null);

            return mockConnector.Object;
        }

        [Fact]
        public void Persistent_stream_read()
        {
            var pStream = new PersistentStream(null, Connecter_returns_null_streams().ConnectorFunc, 0);

            //since the stream is null, reading will cause an exception and reconnect will be invoked
            Assert.NotEqual(0, pStream.Read(new byte[100], 0, 100));
        }

        private static IMockConnector Connecter_throws_exception()
        {
            var buffer = new byte[ushort.MaxValue];
            var random = new Random();

            random.NextBytes(buffer);

            var mockConnector = new Mock<IMockConnector>();
            mockConnector.SetupSequence(x => x.ConnectorFunc(0))
                .Throws(new IOException())
                .Returns(new MemoryStream(buffer));

            return mockConnector.Object;
        }

        [Fact]
        public void Persistent_stream_connection_exception()
        {
            var pStream = new PersistentStream(null, Connecter_throws_exception().ConnectorFunc, 0);

            //since the stream is null, reading will cause an exception and reconnect will be invoked
            Assert.NotEqual(0, pStream.Read(new byte[100], 0, 100));
        }
    }
}