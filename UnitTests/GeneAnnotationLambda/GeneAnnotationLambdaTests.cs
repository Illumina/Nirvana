using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cloud.Messages.Gene;
using UnitTests.TestUtilities;
using Xunit;

namespace UnitTests.GeneAnnotationLambda
{
    public sealed class GeneAnnotationLambdaTests
    {
        private readonly string _manifestPath  = Resources.TopPath("manifest.txt");
        private readonly string _customNgaPath = Resources.TopPath("custom_gene.nga");
        private readonly string _prefix        = Resources.Top + Path.DirectorySeparatorChar;

        [Fact]
        public void GetNgaFiles_AsExpected()
        {
            IEnumerable<string> ngaFiles = global::GeneAnnotationLambda.GeneAnnotationLambda.GetNgaFileList(_manifestPath, _prefix, new[] { _customNgaPath });

            IEnumerable<string> expectedFiles = new[]
            {
                "ClinGen_Dosage_Sensitivity_Map_20190507.nga",
                "gnomAD_gene_scores_2.1.nga",
                "OMIM_20190812.nga",
                "custom_gene.nga"
            }.Select(Resources.TopPath);

            Assert.Equal(expectedFiles, ngaFiles);
        }

        [Fact]
        public void GetGeneAnnotation_AsExpected()
        {
            var input = new GeneConfig
            {
                id = "test",
                geneSymbols = new[] { "TP53", "ZIC2", "LOC645752" },
                ngaUrls = new[] { _customNgaPath }
            };

            string responseString = global::GeneAnnotationLambda.GeneAnnotationLambda.GetGeneAnnotation(input, _manifestPath, _prefix);

            Assert.Contains("header", responseString);
            Assert.Contains("TP53", responseString);
            Assert.Contains("ZIC2", responseString);
            Assert.Contains("clingenDosageSensitivityMap", responseString);
            Assert.Contains("gnomAD", responseString);
            Assert.Contains("omim", responseString);
            Assert.Contains("InternalGeneAnnotation", responseString);
            Assert.DoesNotContain("LOC645752", responseString);
        }
    }
}
