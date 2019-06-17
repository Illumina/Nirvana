using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Xml.Linq;
using ErrorHandling.Exceptions;
using Xunit;
using static ErrorHandling.ExceptionUtilities;

namespace UnitTests.ErrorHandling
{
    public sealed class ExceptionUtilitiesTests
    {
        private readonly Exception _generalException = new Exception("first level", new Exception("second level", new Exception("third level")));
        private readonly Exception _taskCancellation1 = new Exception("first level", new TaskCanceledException("second level", new Exception("third level")));
        private readonly Exception _taskCancellation2 = new Exception("first level", new Exception("second level", new TaskCanceledException("third level")));
        private const string ExceptionMessage = "Something wrong.";
        private const string HttpRequestUrl = "http://unit.test";

        [Fact]
        public void HasException_AsExpected()
        {
            Assert.False(HasException<TaskCanceledException>(_generalException));
            Assert.True(HasException<TaskCanceledException>(_taskCancellation1));
            Assert.True(HasException<TaskCanceledException>(_taskCancellation2));
        }

        [Fact]
        public void GetInnermostException_AsExpected()
        {
            Assert.Equal("third level", GetInnermostException(_generalException).Message);
            Assert.Equal("third level", GetInnermostException(_taskCancellation1).Message);
            Assert.Equal("third level", GetInnermostException(_taskCancellation2).Message);
        }
    }

    public sealed class MockHttpWebResponse : HttpWebResponse
    {
        private readonly Stream _stream;
        public override HttpStatusCode StatusCode { get; }

        public MockHttpWebResponse(Stream stream, HttpStatusCode statusCode)
        {
            _stream = stream;
            StatusCode = statusCode;
        }

        public override Stream GetResponseStream() => _stream;
    }
}