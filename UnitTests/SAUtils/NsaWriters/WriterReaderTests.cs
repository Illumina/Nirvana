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
using VariantAnnotation.Interface.Providers;
using VariantAnnotation.NSA;
using VariantAnnotation.Providers;
using VariantAnnotation.SA;
using Xunit;

namespace UnitTests.SAUtils.NsaWriters
{
    public sealed class WriterReaderTests
    {
        private readonly IChromosome _chrom1 = new Chromosome("chr1", "1", 0);
        private readonly IChromosome _chrom2 = new Chromosome("chr2", "2", 1);

        private IEnumerable<ClinVarItem> GetClinvarItems()
        {
            var clinvarItems = new List<ClinVarItem>
            {
                new ClinVarItem(_chrom1, 100, 100, "T", "A", ClinVarSchema.Get(), new[] {"origin1"}, "SNV", "RCV0001",null, ReviewStatus.no_assertion, new[] {"medgen1"}, new[] {"omim1"}, new[] {"orpha1"}, new[] {"phenotype1"}, new []{"significance"}, new[] {10024875684920}, 658794146787),

                new ClinVarItem(_chrom1, 101, 101, "A", "", ClinVarSchema.Get(),new[] {"origin1"}, "del", "RCV00011", 101 ,ReviewStatus.no_assertion, new[] {"medgen1"}, new[] {"omim1"}, new[] {"orpha1"}, new[] {"phenotype1"}, new []{"significance"}, new[] {10024875684920}, 658794146787),

                new ClinVarItem(_chrom1, 106, 106, "C", "",ClinVarSchema.Get(), new[] {"origin5"}, "del", "RCV0005", null, ReviewStatus.multiple_submitters, new[] {"medgen5"}, new[] {"omim5"}, new[] {"orpha5"}, new[] {"phenotype5"}, new []{"significance5"}, new[] {10024255684920}, 658794187787),

                new ClinVarItem(_chrom2, 200, 200, "G", "A", ClinVarSchema.Get(),
                    new[] {"origin21"}, "SNV", "RCV20001",null, ReviewStatus.multiple_submitters_no_conflict, new[] {"medgen20"}, new[] {"omim20"}, new[] {"orpha20"}, new[] {"phenotype20"}, new []{"significance20"}, new[] {10024875684480}, 669794146787),

                new ClinVarItem(_chrom2, 205, 205, "T", "C",  ClinVarSchema.Get(), new[] {"origin25"}, "ins", "RCV20005", null, ReviewStatus.expert_panel, new[] {"medgen25"}, new[] {"omim25"}, new[] {"orpha25"}, new[] {"phenotype25"}, new []{"significance25"}, new[] {10024255684925}, 658794187287)
            };

            return clinvarItems;
        }

        private ISequenceProvider GetSequenceProvider()
        {
            var sequence = new SimpleSequence(new string('A', 99)+"TAGTCGGTTAA" + new string('A', 89)+"GCCCAT");
            
            //return seqProvider.Object;
            var refNameToChrom = new Dictionary<string, IChromosome>
            {
                { "1", _chrom1},
                {"2", _chrom2 }
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
                    var saWriter = new NsaWriter(extWriter, indexExtWriter, version, GetSequenceProvider(), "clinvar", false, true, SaCommon.SchemaVersion, false, true, false, 1024);
                    saWriter.Write(GetClinvarItems());
                }

                saStream.Position = 0;
                indexStream.Position = 0;

                using (var saReader = new NsaReader(saStream, indexStream, 1024))
                {
                    Assert.Equal(GenomeAssembly.GRCh37, saReader.Assembly);
                    Assert.Equal(version.ToString(), saReader.Version.ToString());
                    saReader.PreLoad(_chrom1, new List<int> { 100, 101, 106 });
                    var annotations = saReader.GetAnnotation(100).ToList();

                    Assert.Equal("T", annotations[0].refAllele);
                    Assert.Equal("A", annotations[0].altAllele);
                    Assert.Equal("\"id\":\"RCV0001\",\"reviewStatus\":\"no assertion provided\",\"alleleOrigins\":[\"origin1\"],\"refAllele\":\"T\",\"altAllele\":\"A\",\"phenotypes\":[\"phenotype1\"],\"medGenIds\":[\"medgen1\"],\"omimIds\":[\"omim1\"],\"orphanetIds\":[\"orpha1\"],\"significance\":[\"significance\"],\"lastUpdatedDate\":\"0001-01-01\",\"pubMedIds\":[\"10024875684920\"]", annotations[0].annotation);

                    annotations = saReader.GetAnnotation(101).ToList();
                    Assert.Equal("A", annotations[0].refAllele);
                    Assert.Equal("", annotations[0].altAllele);
                    Assert.Equal("\"id\":\"RCV00011\",\"variationId\":101,\"reviewStatus\":\"no assertion provided\",\"alleleOrigins\":[\"origin1\"],\"refAllele\":\"A\",\"altAllele\":\"-\",\"phenotypes\":[\"phenotype1\"],\"medGenIds\":[\"medgen1\"],\"omimIds\":[\"omim1\"],\"orphanetIds\":[\"orpha1\"],\"significance\":[\"significance\"],\"lastUpdatedDate\":\"0001-01-01\",\"pubMedIds\":[\"10024875684920\"]", annotations[0].annotation);

                    saReader.PreLoad(_chrom2, new List<int> { 200, 205 });
                    var (refAllele, altAllele, annotation) = saReader.GetAnnotation(200).First();
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
                items.Add(new DbSnpItem(_chrom1, position, position, "A", "C"));
            }

            return items;
        }

        private static ISequenceProvider GetAllASequenceProvider()
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
                    var saWriter = new NsaWriter(extWriter, indexExtWriter, version, GetAllASequenceProvider(), "dbsnp", true, true, SaCommon.SchemaVersion, false, true, false, 1024);
                    saWriter.Write(GetDbsnpItems(1000));
                }

                saStream.Position = 0;
                indexStream.Position = 0;

                using (var saReader = new NsaReader(saStream, indexStream, 1024))
                {
                    saReader.PreLoad(_chrom1, GetPositions(50, 1000));

                    Assert.Null(saReader.GetAnnotation(90));//before any SA existed
                    Assert.NotNull(saReader.GetAnnotation(100));//first entry of first block
                    Assert.NotNull(saReader.GetAnnotation(480));//last query of first block
                    Assert.Null(saReader.GetAnnotation(488));//between first and second block
                    Assert.NotNull(saReader.GetAnnotation(490));//first entry of second block
                }
            }
        }

        private static List<int> GetPositions(int start, int count)
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
            var customItem = new CustomItem(_chrom1, 100, "A", "T", null, null, null);

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