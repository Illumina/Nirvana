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

        [Fact]
        public void IsHttpRequestForbiddenException_AsExpected()
        {
            Assert.False(IsWebProtocolErrorException(new Exception("An exception")));
            Assert.False(IsWebProtocolErrorException(new WebException("web exception")));
            Assert.False(IsWebProtocolErrorException(new WebException("web exception", WebExceptionStatus.ConnectFailure)));
            Assert.True(IsWebProtocolErrorException(new WebException("web exception", null, WebExceptionStatus.ProtocolError, new MockHttpWebResponse(null, HttpStatusCode.NotFound))));
            Assert.True(IsWebProtocolErrorException(new WebException("web exception", null, WebExceptionStatus.ProtocolError, new MockHttpWebResponse(null, HttpStatusCode.Forbidden))));
        }

        [Theory]
        [InlineData("Access denied", "https://unit.test", "Something wrong.", "Access denied when reading from https://unit.test. Something wrong.")]
        [InlineData("Request has expired", "https://expired.url", "Something wrong again.", "The provided URL https://expired.url is expired. Something wrong again.")]
        public void ProcessHttpRequestForbiddenException_AsExpected(string message, string url, string exceptionMessage, string newErrorMessage)
        {
            XElement xmlMessage = new XElement("Root", new XElement("Message", message));
            var stream = new MemoryStream();
            xmlMessage.Save(stream);
            stream.Position = 0;

            var response = new MockHttpWebResponse(stream, HttpStatusCode.Forbidden);
            var inputException = new WebException(exceptionMessage, null, WebExceptionStatus.ProtocolError, response);

            var outputException = ProcessHttpRequestForbiddenException(inputException, url);
            Assert.IsType<UserErrorException>(outputException);
            Assert.Equal(newErrorMessage, outputException.Message);
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