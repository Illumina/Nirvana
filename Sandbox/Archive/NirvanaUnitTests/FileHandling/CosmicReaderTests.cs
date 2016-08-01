using System.Collections.Generic;
using System.IO;
using Illumina.VariantAnnotation.DataStructures.SupplementaryAnnotations;
using Xunit;

namespace NirvanaUnitTests.FileHandling
{
    public sealed class CosmicReaderTest
    {
        
        private static IEnumerable<CosmicItem> CreateTruthCosmicItemSequence()
        {
            yield return new CosmicItem("14", 81610259, "COSN26416", "A", "G", "TSHR", "adenoma-nodule-goitre", "thyroid", "");
            yield return new CosmicItem("17", 7577520, "COSM11929", "AT", "GA", "TP53", "lymphoid_neoplasm", "haematopoietic_and_lymphoid_tissue", "");
            yield return new CosmicItem("3", 41266082, "COSM27285", "C", "T", "CTNNB1", "other", "liver", "");
            yield return new CosmicItem("3", 178936116, "COSN27489", "GT", "C", "PIK3CA", "carcinoma", "liver", "");
            yield return new CosmicItem("3", 178916648, "COSN27496", "G", "A", "PIK3CA", "carcinoma", "salivary_gland", "");
            yield return new CosmicItem("4", 178916648, "COSN27497", "G", "A", null, null, null, null);
            yield return new CosmicItem("7", 55242484, "COSM29274", "T", "C", "EGFR", "carcinoma", "thyroid", "");
        }

        [Fact]
        public void TestCosmicReader()
        {
            var nonCodingVcf = new FileInfo(@"Resources\TestCosmicParser.NonCoding.vcf");
            var codingVcf    = new FileInfo(@"Resources\TestCosmicParser.Coding.vcf");
            var tsv          = new FileInfo(@"Resources\TestCosmicParser.tsv");

            var cosmicReader = new CosmicReader(nonCodingVcf, codingVcf, tsv);
            // Assert.True(cosmicReader.SequenceEqual(CreateTruthCosmicItemSequence()));
            
            var trueCosmicItems = CreateTruthCosmicItemSequence().GetEnumerator();
            trueCosmicItems.MoveNext();

            foreach (var cosmicItem in cosmicReader)
            {
                var isEqual = cosmicItem.Equals(trueCosmicItems.Current);
                Assert.True(isEqual);
                trueCosmicItems.MoveNext();
            }
        }
    }
}