using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Genome;
using SAUtils.DataStructures;
using VariantAnnotation.Interface.SA;
using VariantAnnotation.NSA;
using VariantAnnotation.Providers;
using VariantAnnotation.SA;
using Variants;
using Xunit;

namespace UnitTests.SAUtils.NsaWriters
{
    public sealed class IntervalWriterReaderTests
    {
        private static IEnumerable<ClinGenItem> GetClinGenItems()
        {
            var chrom1 = new Chromosome("chr1", "1", 0);
            var chrom2 = new Chromosome("chr2", "2", 1);

            return new[]
            {
                new ClinGenItem("cg1",chrom1, 145, 2743, VariantType.copy_number_gain, 3, 0, ClinicalInterpretation.likely_benign,true, new HashSet<string> {"phenotype1", "phenotype2"}, new HashSet<string> {"pid1", "pid2"} ),
                new ClinGenItem("cg2",chrom1, 14585, 5872743, VariantType.copy_number_loss, 0, 5, ClinicalInterpretation.likely_pathogenic,true, new HashSet<string> {"phenotype3", "phenotype5"}, new HashSet<string> {"pid3", "pid5"} ),
                new ClinGenItem("cg3",chrom2, 45759, 8792743, VariantType.deletion, 3, 0, ClinicalInterpretation.pathogenic,true, new HashSet<string> {"phenotype1", "phenotype4"}, new HashSet<string> {"pid1", "pid4"} ),
                new ClinGenItem("cg4",chrom2, 5589745, 7987923, VariantType.insertion, 3, 0, ClinicalInterpretation.uncertain_significance, true, new HashSet<string> {"phenotype10", "phenotype14"}, new HashSet<string> {"pid10", "pid14"} )
            };
        }

        [Fact]
        public void Readback_clingen()
        {
            var chrom1 = new Chromosome("chr1", "1", 0);
            var version = new DataSourceVersion("source1", "v1", DateTime.Now.Ticks, "description");

            using (var saStream = new MemoryStream())
            {
                using (var extWriter = new BinaryWriter(saStream, Encoding.UTF8, true))
                using(var siWriter = new NsiWriter(extWriter, version, GenomeAssembly.GRCh37, "clingen",
                    ReportFor.StructuralVariants, SaCommon.SchemaVersion))
                {
                    siWriter.Write(GetClinGenItems());
                }
                saStream.Position = 0;

                var siReader = new NsiReader(saStream);
                var annotations = siReader.GetAnnotation(new Variant(chrom1, 100, 14590, "", "<DEL>", VariantType.deletion, "1:100:14590:del", false, false, false, null, null, null)).ToArray();

                string[] expected = {
                    "\"chromosome\":\"1\",\"begin\":145,\"end\":2743,\"variantType\":\"copy_number_gain\",\"id\":\"cg1\",\"clinicalInterpretation\":\"likely benign\",\"phenotypes\":[\"phenotype1\",\"phenotype2\"],\"phenotypeIds\":[\"pid1\",\"pid2\"],\"observedGains\":3,\"validated\":true,\"reciprocalOverlap\":0.17935,\"annotationOverlap\":1",
                    "\"chromosome\":\"1\",\"begin\":14585,\"end\":5872743,\"variantType\":\"copy_number_loss\",\"id\":\"cg2\",\"clinicalInterpretation\":\"likely pathogenic\",\"phenotypes\":[\"phenotype3\",\"phenotype5\"],\"phenotypeIds\":[\"pid3\",\"pid5\"],\"observedLosses\":5,\"validated\":true,\"reciprocalOverlap\":0,\"annotationOverlap\":0"
                };
                
                Assert.Equal(2, annotations.Length);
                Assert.Equal(expected, annotations);
            }
        }
    }
}