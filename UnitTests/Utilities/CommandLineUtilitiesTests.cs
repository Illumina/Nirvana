using System;
using CommandLine.Utilities;
using Xunit;

namespace UnitTests.Utilities
{
    public class CommandLineUtilitiesTests
    {
        [Fact]
        public void DisplayBannerLongAuthorName()
        {
            Assert.Throws<InvalidOperationException>(
                () =>
                    CommandLineUtilities.DisplayBanner(
                        "Lopado­temacho­selacho­galeo­kranio­leipsano­drim­hypo­trimmato­silphio­parao­melito­katakechy­meno­kichl­epi­kossypho­phatto­perister­alektryon­opte­kephallio­kigklo­peleio­lagoio­siraio­baphe­tragano­pterygon"));
        }

        [Fact]
        public void GetAssemblyCopyright()
        {
            Assert.Equal("(c) 2017 Illumina, Inc.", CommandLineUtilities.Copyright);
        }

        [Fact]
        public void GetAssemblyVersion()
        {
            Assert.StartsWith("15.0.0", CommandLineUtilities.Version);
        }

        [Fact]
        public void GetAssemblyTitle()
        {
            Assert.StartsWith("testhost", CommandLineUtilities.Title);
        }
    }
}
