using System.Collections.Generic;
using System.IO;
using System.Text;
using VariantAnnotation.GeneAnnotation;
using VariantAnnotation.IO;
using Xunit;

namespace UnitTests.VariantAnnotation.GeneAnnotation
{
    public sealed class OmimEntryTests
    {
        [Fact]
        public void ReadAndWriteTest()
        {
            var omimEntry = new OmimEntry("gene1", "gene for test", 123345,
                new List<OmimEntry.Phenotype>
                {
                    new OmimEntry.Phenotype(23456, "test phenotype1", OmimEntry.Mapping.mapping_of_the_wildtype_gene,
                        OmimEntry.Comments.nondiseases, new HashSet<string> {"dominant"}),
                    new OmimEntry.Phenotype(23467, "test phenotype2", OmimEntry.Mapping.chromosome_deletion_or_duplication_syndrome,
                        OmimEntry.Comments.contribute_to_susceptibility_to_multifactorial_disorders_or_to_susceptibility_to_infection, new HashSet<string> {"recessive","autosomal"})
                });

            var ms = new MemoryStream();
            OmimEntry observedEntry;
            using (var writer = new ExtendedBinaryWriter(ms, Encoding.UTF8, true))
            using (var reader = new ExtendedBinaryReader(ms))
            {
                omimEntry.Write(writer);
                ms.Position = 0;
                observedEntry = OmimEntry.Read(reader);
            }

            Assert.Equal(omimEntry.GeneSymbol, observedEntry.GeneSymbol);
            Assert.Equal("{\"mimNumber\":123345,\"description\":\"gene for test\",\"phenotypes\":[{\"phenotype\":\"test phenotype1\",\"mapping\":\"mapping of the wildtype gene\",\"inheritances\":[\"dominant\"],\"comments\":\"nondiseases\"},{\"phenotype\":\"test phenotype2\",\"mapping\":\"chromosome deletion or duplication syndrome\",\"inheritances\":[\"recessive\",\"autosomal\"],\"comments\":\"contribute to susceptibility to multifactorial disorders or to susceptibility to infection\"}]}", observedEntry.ToString());
        }
    }
}