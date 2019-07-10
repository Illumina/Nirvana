using System;
using System.Collections.Generic;
using System.Text;
using Nirvana;
using Xunit;

namespace UnitTests.Nirvana
{
    public sealed class ProviderUtilitiesTests
    {
        [Fact]
        public void GetNsaProvider_NoSaFile_ReturnNull()
        {
            var annotationFiles = new AnnotationFiles();
            var nsaProvider = ProviderUtilities.GetNsaProvider(annotationFiles);

            Assert.Null(nsaProvider);
        }
    }
}
