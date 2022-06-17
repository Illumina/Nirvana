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
        [InlineData("InvalidAccessKeyId", "The AWS Access Key Id you provided does not exist in our records", "https://unit.test/bob.vcf.gz", "Something wrong.", "Authentication error while reading from URL for bob.vcf.gz.")]
        [InlineData("AccessDenied", "Request has expired", "https://expired.url/bob.vcf.gz", "Something wrong again.", "The provided URL for bob.vcf.gz has expired.")]
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

        [Fact]
        public void ValidateUrl_invalid_user_provided()
        {
            Assert.Throws<UserErrorException>(() =>
                HttpUtilities.ValidateUrl(
                    "https://nirvana-annotations.s3.us-west-2.amazonaws.com/645778a7d475ac437d15765ef3c6f50c-OMIM/0/OMIM_20191004.nga"));
        }

        [Fact]
        public void ValidateUrl_invalid_deployment()
        {
            Assert.Throws<DeploymentErrorException>(() =>
                HttpUtilities.ValidateUrl(
                    "https://nirvana-annotations.s3.us-west-2.amazonaws.com/645778a7d475ac437d15765ef3c6f50c-OMIM/0/OMIM_20191004.nga", false));
        }

        [Fact]
        public void ValidateUrl_valid()
        {
            HttpUtilities.ValidateUrl(
                    "https://nirvana-annotations.s3.us-west-2.amazonaws.com/645778a7d475ac437d15765ef3c6f50c-OMIM/6/OMIM_20191004.nga", false);
        }

    }
}
