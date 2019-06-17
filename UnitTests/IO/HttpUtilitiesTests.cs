using System;
using System.IO;
using System.Net;
using System.Xml.Linq;
using ErrorHandling.Exceptions;
using UnitTests.ErrorHandling;
using Xunit;
using IO;

namespace UnitTests.IO
{
    public sealed class HttpUtilitiesTests
    {
        [Fact]
        public void IsWebProtocolErrorException_AsExpected()
        {
            Assert.False(HttpUtilities.IsWebProtocolErrorException(new Exception("An exception")));
            Assert.False(HttpUtilities.IsWebProtocolErrorException(new WebException("web exception")));
            Assert.False(HttpUtilities.IsWebProtocolErrorException(new WebException("web exception", WebExceptionStatus.ConnectFailure)));
            Assert.True(HttpUtilities.IsWebProtocolErrorException(new WebException("web exception", null, WebExceptionStatus.ProtocolError, new MockHttpWebResponse(null, HttpStatusCode.NotFound))));
            Assert.True(HttpUtilities.IsWebProtocolErrorException(new WebException("web exception", null, WebExceptionStatus.ProtocolError, new MockHttpWebResponse(null, HttpStatusCode.Forbidden))));
        }

        [Theory]
        [InlineData("InvalidAccessKeyId", "The AWS Access Key Id you provided does not exist in our records", "https://unit.test", "Something wrong.", "Authentication error while reading from https://unit.test. The AWS Access Key Id you provided does not exist in our records. Exception: Something wrong.")]
        [InlineData("AccessDenied", "Request has expired", "https://expired.url", "Something wrong again.", "The provided URL https://expired.url is expired. Exception: Something wrong again.")]
        public void ProcessHttpRequestForbiddenException_AsExpected(string errorCode, string message, string url, string exceptionMessage, string newErrorMessage)
        {
            XElement xmlMessage = new XElement("Root", new XElement("Code", errorCode), new XElement("Message", message));
            var stream = new MemoryStream();
            xmlMessage.Save(stream);
            stream.Position = 0;

            var response = new MockHttpWebResponse(stream, HttpStatusCode.Forbidden);
            var inputException = new WebException(exceptionMessage, null, WebExceptionStatus.ProtocolError, response);

            var outputException = HttpUtilities.ProcessHttpRequestWebProtocolErrorException(inputException, url);
            Assert.IsType<UserErrorException>(outputException);
            Assert.Equal(newErrorMessage, outputException.Message);
        }
    }
}
