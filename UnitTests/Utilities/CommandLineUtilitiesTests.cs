using System.Collections.Generic;
using ErrorHandling.Exceptions;
using VariantAnnotation.Utilities;
using Xunit;

namespace UnitTests.Utilities
{
    public class CommandLineUtilitiesTests
    {
        [Fact]
        public void DisplayBanner()
        {
            string result;

            using (var redirector = new ConsoleRedirector())
            {
                CommandLineUtilities.DisplayBanner("BOB");
                result = redirector.ToString();
                redirector.Close();
            }

            Assert.Contains("testhost", result);
            Assert.Contains("BOB", result);
            Assert.Contains("(c) 2017 Illumina, Inc.", result);
            Assert.Contains("15.0.0", result);
        }

        [Fact]
        public void DisplayBannerLongAuthorName()
        {
            Assert.Throws<GeneralException>(
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

        [Fact]
        public void ShowUnsupportedOptions()
        {
            var unsupportedOptions = new List<string> { "no_crash", "go_faster" };
            string result = GetUnsupportedOptions(unsupportedOptions);

            Assert.Contains("no_crash", result);
            Assert.Contains("go_faster", result);
            Assert.Contains("Unsupported options:", result);
        }

        [Fact]
        public void ShowUnsupportedOptionsEmpty()
        {
            string result = GetUnsupportedOptions(null);
            Assert.Equal("", result);

            var unsupportedOptions = new List<string>();
            result = GetUnsupportedOptions(unsupportedOptions);
            Assert.Equal("", result);
        }

        private static string GetUnsupportedOptions(List<string> unsupportedOptions)
        {
            string result;

            using (var redirector = new ConsoleRedirector())
            {
                CommandLineUtilities.ShowUnsupportedOptions(unsupportedOptions);
                result = redirector.ToString();
                redirector.Close();
            }

            return result;
        }
    }
}
