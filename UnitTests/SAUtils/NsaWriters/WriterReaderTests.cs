using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using ErrorHandling.Exceptions;
using Genome;
using IO;
using Moq;
using SAUtils;
using SAUtils.DataStructures;
using SAUtils.InputFileParsers.ClinVar;
using UnitTests.TestDataStructures;
using UnitTests.TestUtilities;
using VariantAnnotation.Interface.Providers;
using VariantAnnotation.NSA;
using VariantAnnotation.Providers;
using VariantAnnotation.SA;
using Xunit;

namespace UnitTests.SAUtils.NsaWriters
{
    public sealed class WriterReaderTests
    {
        private static ClinVarItem GetClinVarItem(IChromosome chromosome,
            int position,
            int stop,
            string referenceAllele,
            string altAllele,
            IEnumerable<string> alleleOrigins,
            string variantType,
            string id,
            string reviewStatus,
            IEnumerable<string> medGenIds,
            IEnumerable<string> omimIds,
            IEnumerable<string> orphanetIds,
            IEnumerable<string> phenotypes,
            string significance,
            IEnumerable<long> pubMedIds,
            long lastUpdatedDate = long.MinValue)
        {
            string[][] values = {
                new[] {id},
                new[] {ReviewStatusMapping.FormatReviewStatus(reviewStatus)},
                alleleOrigins.ToArray(),
                phenotypes.ToArray(),
                medGenIds.ToArray(),
                omimIds.ToArray(),
                orphanetIds.ToArray(),
                new[] {significance},
                new[] {new DateTime(lastUpdatedDate).ToString("yyyy-MM-dd")},
                pubMedIds.OrderBy(x => x).Select(x => x.ToString()).ToArray()
            };
            
            return new ClinVarItem(chromosome, position, stop, referenceAllele, altAllele, variantType, values, ClinVarSchema.Get());
        } 

        private IEnumerable<ClinVarItem> GetClinvarItems()
        {
            var clinvarItems = new List<ClinVarItem>
            {
                GetClinVarItem(ChromosomeUtilities.Chr1, 100, 100, "T", "A", new[] {"origin1"}, "SNV", "RCV0001", "", new[] {"medgen1"}, new[] {"omim1"}, new[] {"orpha1"}, new[] {"phenotype1"}, "significance", new[] {10024875684920}, 658794146787),
                GetClinVarItem(ChromosomeUtilities.Chr1, 101, 101, "A", "", new[] {"origin1"}, "del", "RCV00011", "no_assertion", new[] {"medgen1"}, new[] {"omim1"}, new[] {"orpha1"}, new[] {"phenotype1"}, "significance", new[] {10024875684920}, 658794146787),
                GetClinVarItem(ChromosomeUtilities.Chr1, 106, 106, "C", "", new[] {"origin5"}, "del", "RCV0005", "classified by multiple submitters", new[] {"medgen5"}, new[] {"omim5"}, new[] {"orpha5"}, new[] {"phenotype5"}, "significance5", new[] {10024255684920}, 658794187787),
                GetClinVarItem(ChromosomeUtilities.Chr2, 200, 200, "G", "A",
                    new[] {"origin21"}, "SNV", "RCV20001", "criteria provided, multiple submitters, no conflicts", new[] {"medgen20"}, new[] {"omim20"}, new[] {"orpha20"}, new[] {"phenotype20"}, "significance20", new[] {10024875684480}, 669794146787),
                GetClinVarItem(ChromosomeUtilities.Chr2, 205, 205, "T", "C", new[] {"origin25"}, "ins", "RCV20005", "reviewed by expert panel", new[] {"medgen25"}, new[] {"omim25"}, new[] {"orpha25"}, new[] {"phenotype25"}, "significance25", new[] {10024255684925}, 658794187287)
            };

            return clinvarItems;
        }

        private ISequenceProvider GetSequenceProvider()
        {
            var sequence = new SimpleSequence(new string('A', 99)+"TAGTCGGTTAA" + new string('A', 89)+"GCCCAT");
            
            //return seqProvider.Object;
            var refNameToChrom = new Dictionary<string, IChromosome>
            {
                { "1", ChromosomeUtilities.Chr1},
                {"2", ChromosomeUtilities.Chr2 }
            };
            return new SimpleSequenceProvider(GenomeAssembly.GRCh37, sequence, refNameToChrom);
        }

        
        [Fact]
        public void Write_clinvar_basic()
        {
            var version = new DataSourceVersion("source1", "v1", DateTime.Now.Ticks, "description");

            using (var saStream = new MemoryStream())
            using (var indexStream = new MemoryStream())
            {
                using (var extWriter = new ExtendedBinaryWriter(saStream, Encoding.UTF8, true))
                using (var indexExtWriter = new ExtendedBinaryWriter(indexStream, Encoding.UTF8, true))
                {
                    var saWriter = new NsaWriter(extWriter, indexExtWriter, version, GetSequenceProvider(), "clinvar", false, true, SaCommon.SchemaVersion, false, true, 1024);
                    saWriter.Write(GetClinvarItems());
                }

                saStream.Position = 0;
                indexStream.Position = 0;

                using (var extReader = new ExtendedBinaryReader(saStream))
                {
                    var saReader = new NsaReader(extReader, indexStream, 1024);
                    Assert.Equal(GenomeAssembly.GRCh37, saReader.Assembly);
                    Assert.Equal(version.ToString(), saReader.Version.ToString());
                    saReader.PreLoad(ChromosomeUtilities.Chr1, new List<int> { 100, 101, 106 });
                    var annotations = saReader.GetAnnotation(ChromosomeUtilities.Chr1, 100).ToList();

                    Assert.Equal("T", annotations[0].refAllele);
                    Assert.Equal("A", annotations[0].altAllele);
                    Assert.Equal("\"id\":\"RCV0001\",\"reviewStatus\":\"no assertion provided\",\"alleleOrigins\":[\"origin1\"],\"refAllele\":\"T\",\"altAllele\":\"A\",\"phenotypes\":[\"phenotype1\"],\"medGenIds\":[\"medgen1\"],\"omimIds\":[\"omim1\"],\"orphanetIds\":[\"orpha1\"],\"significance\":[\"significance\"],\"lastUpdatedDate\":\"0001-01-01\",\"pubMedIds\":[\"10024875684920\"]", annotations[0].annotation);

                    annotations = saReader.GetAnnotation(ChromosomeUtilities.Chr1, 101).ToList();
                    Assert.Equal("A", annotations[0].refAllele);
                    Assert.Equal("", annotations[0].altAllele);
                    Assert.Equal("\"id\":\"RCV00011\",\"reviewStatus\":\"no assertion provided\",\"alleleOrigins\":[\"origin1\"],\"refAllele\":\"A\",\"altAllele\":\"-\",\"phenotypes\":[\"phenotype1\"],\"medGenIds\":[\"medgen1\"],\"omimIds\":[\"omim1\"],\"orphanetIds\":[\"orpha1\"],\"significance\":[\"significance\"],\"lastUpdatedDate\":\"0001-01-01\",\"pubMedIds\":[\"10024875684920\"]", annotations[0].annotation);

                    saReader.PreLoad(ChromosomeUtilities.Chr2, new List<int> { 200, 205 });
                    var (refAllele, altAllele, annotation) = saReader.GetAnnotation(ChromosomeUtilities.Chr2, 200).First();
                    Assert.Equal("G", refAllele);
                    Assert.Equal("A", altAllele);
                    Assert.NotNull(annotation);
                }
            }

        }


        private IEnumerable<DbSnpItem> GetDbsnpItems(int count)
        {
            var items = new List<DbSnpItem>();
            var position = 100;
            for (int i = 0; i < count; i++, position+=5)
            {
                items.Add(new DbSnpItem(ChromosomeUtilities.Chr1, position, position, "A", "C"));
            }

            return items;
        }

        private ISequenceProvider GetAllASequenceProvider()
        {
            var seqProvider = new Mock<ISequenceProvider>();
            seqProvider.SetupGet(x => x.Assembly).Returns(GenomeAssembly.GRCh37);
            seqProvider.Setup(x => x.Sequence.Substring(It.IsAny<int>(), 1)).Returns("A");
            
            return seqProvider.Object;
        }

        [Fact]
        public void Preload()
        {
            var version = new DataSourceVersion("source1", "v1", DateTime.Now.Ticks, "description");

            using (var saStream = new MemoryStream())
            using (var indexStream = new MemoryStream())
            {
                using (var extWriter = new ExtendedBinaryWriter(saStream, Encoding.UTF8, true))
                using (var indexExtWriter = new ExtendedBinaryWriter(indexStream, Encoding.UTF8, true))
                {
                    var saWriter = new NsaWriter(extWriter, indexExtWriter, version, GetAllASequenceProvider(), "dbsnp", true, true, SaCommon.SchemaVersion, false, true, 1024);
                    saWriter.Write(GetDbsnpItems(1000));
                }

                saStream.Position = 0;
                indexStream.Position = 0;

                using (var extReader = new ExtendedBinaryReader(saStream))
                {
                    var saReader = new NsaReader(extReader, indexStream, 1024);
                    saReader.PreLoad(ChromosomeUtilities.Chr1, GetPositions(50, 1000));

                    Assert.Null(saReader.GetAnnotation(ChromosomeUtilities.Chr1, 90));//before any SA existed
                    Assert.NotNull(saReader.GetAnnotation(ChromosomeUtilities.Chr1, 100));//first entry of first block
                    Assert.NotNull(saReader.GetAnnotation(ChromosomeUtilities.Chr1, 480));//last query of first block
                    Assert.Null(saReader.GetAnnotation(ChromosomeUtilities.Chr1, 488));//between first and second block
                    Assert.NotNull(saReader.GetAnnotation(ChromosomeUtilities.Chr1, 490));//first entry of second block
                }
            }
        }

        private List<int> GetPositions(int start, int count)
        {
            var positions = new List<int>();
            for (var i = 0; i < count; i++, start+=2)
            {
                positions.Add(start);
            }

            return positions;
        }

        [Fact]
        public void WrongRefAllele_ThrowUserException()
        {
            var customItem = new CustomItem(ChromosomeUtilities.Chr1, 100, "A", "T", null, null, null);

            Assert.Throws<UserErrorException>(() => WriteCustomSaItem(customItem));
        }

        private void WriteCustomSaItem(CustomItem customItem)
        {
            using (var saStream = new MemoryStream())
            using (var indexStream = new MemoryStream())
            using (var saWriter = new NsaWriter(
                new ExtendedBinaryWriter(saStream),
                new ExtendedBinaryWriter(indexStream),
                new DataSourceVersion("customeSa", "test", DateTime.Now.Ticks),
                GetSequenceProvider(),
                "customeSa", false, true, SaCommon.SchemaVersion, false, false))
            {
                saWriter.Write(new[] { customItem });
            }
        }
    }
}