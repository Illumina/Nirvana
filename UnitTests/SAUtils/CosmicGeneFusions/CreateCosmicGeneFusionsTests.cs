using System;
using SAUtils.CosmicGeneFusions;
using VariantAnnotation.Providers;
using Xunit;

namespace UnitTests.SAUtils.CosmicGeneFusions
{
    public sealed class CreateCosmicGeneFusionsTests
    {
        [Fact]
        public void CreateDataSourceVersion_ExpectedResults()
        {
            const string expectedName             = "COSMIC gene fusions";
            const string expectedDescription      = "manually curated somatic gene fusions";
            const string expectedVersion          = "94";
            const string releaseDate              = "2021-05-28";
            long         expectedReleaseDateTicks = DateTime.Parse(releaseDate).Ticks;

            DataSourceVersion actualDataSourceVersion = CreateCosmicGeneFusions.CreateDataSourceVersion(expectedVersion, releaseDate);

            Assert.Equal(expectedName,             actualDataSourceVersion.Name);
            Assert.Equal(expectedDescription,      actualDataSourceVersion.Description);
            Assert.Equal(expectedVersion,          actualDataSourceVersion.Version);
            Assert.Equal(expectedReleaseDateTicks, actualDataSourceVersion.ReleaseDateTicks);
        }
    }
}