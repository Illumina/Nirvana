using System;
using System.Collections.Generic;
using System.Text;
using Cloud;
using ErrorHandling.Exceptions;
using Xunit;

namespace UnitTests.Cloud
{
    public class SaUrlsTests
    {
        [Theory]
        [InlineData("test.nsa", "test.idx", "test.nsi", "test.nga")]
        [InlineData(null, "test.idx", "test.nsi", "test.nga")]
        [InlineData("test.nsa", "test.idx", null, "test.nga")]
        [InlineData("test.nsa", "test.idx", "test.nsi", null)]
        [InlineData(null, "test.idx", null, null)]
        [InlineData(null, null, null, null)]
        [InlineData("test.nsa", null, null, null)]
        public void SetSaType_InvalidValues_ThrowException(string nsaUrl, string idxUrl, string nsiUrl, string ngaUrl)
        {
            var saUrls = new SaUrls{nsaUrl = nsaUrl, idxUrl = idxUrl, nsiUrl = nsiUrl, ngaUrl = ngaUrl};
            Assert.Throws<UserErrorException>(() => saUrls.GetSaType());
        }

        [Theory]
        [InlineData("test.nsa", "test.idx", null, null, CustomSaType.Nsa)]
        [InlineData(null, null, "test.nsi", null, CustomSaType.Nsi)]
        [InlineData(null, null, null, "test.nga", CustomSaType.Nga)]
        public void SetSaType_AsExpected(string nsaUrl, string idxUrl, string nsiUrl, string ngaUrl, CustomSaType expectSaType)
        {
            var saUrls = new SaUrls { nsaUrl = nsaUrl, idxUrl = idxUrl, nsiUrl = nsiUrl, ngaUrl = ngaUrl };
            Assert.Equal(expectSaType, saUrls.SaType);
        }
    }
}
