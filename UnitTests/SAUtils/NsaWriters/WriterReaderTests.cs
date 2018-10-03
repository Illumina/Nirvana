using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Genome;
using IO;
using Moq;
using SAUtils;
using SAUtils.DataStructures;
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
                new ClinVarItem(_chrom1, 100, 100, "T", "A", new[] {"origin1"}, "SNV", "RCV0001", ReviewStatus.no_assertion, new[] {"medgen1"}, new[] {"omim1"}, new[] {"orpha1"}, new[] {"phenotype1"}, "significance", new[] {10024875684920}, 658794146787),
                new ClinVarItem(_chrom1, 100, 101, "TA", "A", new[] {"origin1"}, "SNV", "RCV00011", ReviewStatus.no_assertion, new[] {"medgen1"}, new[] {"omim1"}, new[] {"orpha1"}, new[] {"phenotype1"}, "significance", new[] {10024875684920}, 658794146787),
                new ClinVarItem(_chrom1, 105, 106, "TC", "T", new[] {"origin5"}, "del", "RCV0005", ReviewStatus.multiple_submitters, new[] {"medgen5"}, new[] {"omim5"}, new[] {"orpha5"}, new[] {"phenotype5"}, "significance5", new[] {10024255684920}, 658794187787),
                new ClinVarItem(_chrom2, 200, 200, "G", "A",
                    new[] {"origin21"}, "SNV", "RCV20001", ReviewStatus.multiple_submitters_no_conflict, new[] {"medgen20"}, new[] {"omim20"}, new[] {"orpha20"}, new[] {"phenotype20"}, "significance20", new[] {10024875684480}, 669794146787),
                new ClinVarItem(_chrom2, 205, 205, "T", "C", new[] {"origin25"}, "ins", "RCV20005", ReviewStatus.expert_panel, new[] {"medgen25"}, new[] {"omim25"}, new[] {"orpha25"}, new[] {"phenotype25"}, "significance25", new[] {10024255684925}, 658794187287)
            };

            return clinvarItems;
        }

        private ISequenceProvider GetSequenceProvider()
        {
            var seqProvider = new Mock<ISequenceProvider>();
            seqProvider.SetupGet(x => x.Assembly).Returns(GenomeAssembly.GRCh37);
            seqProvider.Setup(x => x.Sequence.Substring(100 -1, 1)).Returns("T");
            seqProvider.Setup(x => x.Sequence.Substring(100 - 1, 2)).Returns("TA");
            seqProvider.Setup(x => x.Sequence.Substring(105 - 1, 2)).Returns("TC");
            seqProvider.Setup(x => x.Sequence.Substring(200 - 1, 1)).Returns("G");
            seqProvider.Setup(x => x.Sequence.Substring(205 - 1, 1)).Returns("T");

            return seqProvider.Object;
        }

        [Fact]
        public void PreCache_and_annotate()
        {
            var version = new DataSourceVersion("source1", "v1", DateTime.Now.Ticks, "description");

            using (var saStream = new MemoryStream())
            using (var indexStream = new MemoryStream())
            {
                using (var extWriter = new ExtendedBinaryWriter(saStream, Encoding.UTF8, true))
                using (var indexExtWriter = new ExtendedBinaryWriter(indexStream, Encoding.UTF8, true))
                {
                    var saWriter = new NsaWriter(extWriter, indexExtWriter, version, GetSequenceProvider(), "clinvar",
                        false, true, SaCommon.SchemaVersion, false, 1024);
                    saWriter.Write(GetClinvarItems());
                }

                saStream.Position = 0;
                indexStream.Position = 0;

                using (var extReader = new ExtendedBinaryReader(saStream))
                {
                    var saReader = new NsaReader(extReader, indexStream, 1024);
                    Assert.Equal(GenomeAssembly.GRCh37, saReader.Assembly);
                    Assert.Equal(version.ToString(), saReader.Version.ToString());
                    saReader.PreLoad(_chrom1, new List<int>(){100,105});

                    var annotations = saReader.GetAnnotation(_chrom1, 100).ToList();

                    Assert.Equal("T", annotations[0].refAllele);
                    Assert.Equal("A", annotations[0].altAllele);
                    Assert.Equal(
                        "\"id\":\"RCV0001\",\"reviewStatus\":\"no assertion provided\",\"alleleOrigins\":[\"origin1\"],\"refAllele\":\"T\",\"altAllele\":\"A\",\"phenotypes\":[\"phenotype1\"],\"medGenIds\":[\"medgen1\"],\"omimIds\":[\"omim1\"],\"orphanetIds\":[\"orpha1\"],\"significance\":\"significance\",\"lastUpdatedDate\":\"0001-01-01\",\"pubMedIds\":[\"10024875684920\"]",
                        annotations[0].annotation);

                    Assert.Equal("T", annotations[1].refAllele);
                    Assert.Equal("", annotations[1].altAllele);
                    Assert.Equal(
                        "\"id\":\"RCV00011\",\"reviewStatus\":\"no assertion provided\",\"alleleOrigins\":[\"origin1\"],\"refAllele\":\"T\",\"altAllele\":\"-\",\"phenotypes\":[\"phenotype1\"],\"medGenIds\":[\"medgen1\"],\"omimIds\":[\"omim1\"],\"orphanetIds\":[\"orpha1\"],\"significance\":\"significance\",\"lastUpdatedDate\":\"0001-01-01\",\"pubMedIds\":[\"10024875684920\"]",
                        annotations[1].annotation);

                    saReader.PreLoad(_chrom2, new List<int>() { 200, 205 });
                    var (refAllele, altAllele, annotation) = saReader.GetAnnotation(_chrom2, 200).First();
                    Assert.Equal("G", refAllele);
                    Assert.Equal("A", altAllele);
                    Assert.NotNull(annotation);
                }
            }
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
                    var saWriter = new NsaWriter(extWriter, indexExtWriter, version, GetSequenceProvider(), "clinvar", false, true, SaCommon.SchemaVersion, false, 1024);
                    saWriter.Write(GetClinvarItems());
                }

                saStream.Position = 0;
                indexStream.Position = 0;

                using (var extReader = new ExtendedBinaryReader(saStream))
                {
                    var saReader = new NsaReader(extReader, indexStream, 1024);
                    Assert.Equal(GenomeAssembly.GRCh37, saReader.Assembly);
                    Assert.Equal(version.ToString(), saReader.Version.ToString());
                    var annotations = saReader.GetAnnotation(_chrom1, 100).ToList();

                    Assert.Equal("T", annotations[0].refAllele);
                    Assert.Equal("A", annotations[0].altAllele);
                    Assert.Equal("\"id\":\"RCV0001\",\"reviewStatus\":\"no assertion provided\",\"alleleOrigins\":[\"origin1\"],\"refAllele\":\"T\",\"altAllele\":\"A\",\"phenotypes\":[\"phenotype1\"],\"medGenIds\":[\"medgen1\"],\"omimIds\":[\"omim1\"],\"orphanetIds\":[\"orpha1\"],\"significance\":\"significance\",\"lastUpdatedDate\":\"0001-01-01\",\"pubMedIds\":[\"10024875684920\"]", annotations[0].annotation);

                    Assert.Equal("T", annotations[1].refAllele);
                    Assert.Equal("", annotations[1].altAllele);
                    Assert.Equal("\"id\":\"RCV00011\",\"reviewStatus\":\"no assertion provided\",\"alleleOrigins\":[\"origin1\"],\"refAllele\":\"T\",\"altAllele\":\"-\",\"phenotypes\":[\"phenotype1\"],\"medGenIds\":[\"medgen1\"],\"omimIds\":[\"omim1\"],\"orphanetIds\":[\"orpha1\"],\"significance\":\"significance\",\"lastUpdatedDate\":\"0001-01-01\",\"pubMedIds\":[\"10024875684920\"]", annotations[1].annotation);

                    var (refAllele,  altAllele,  annotation) = saReader.GetAnnotation(_chrom2, 200).First();
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
                    var saWriter = new NsaWriter(extWriter, indexExtWriter, version, GetAllASequenceProvider(), "dbsnp", true, true, SaCommon.SchemaVersion, false, 1024);
                    saWriter.Write(GetDbsnpItems(1000));
                }

                saStream.Position = 0;
                indexStream.Position = 0;

                using (var extReader = new ExtendedBinaryReader(saStream))
                {
                    var saReader = new NsaReader(extReader, indexStream, 1024);
                    saReader.PreLoad(_chrom1, GetPositions(50, 1000));

                    Assert.Null(saReader.GetAnnotation(_chrom1, 90));//before any SA existed
                    Assert.NotNull(saReader.GetAnnotation(_chrom1, 100));//first entry of first block
                    Assert.NotNull(saReader.GetAnnotation(_chrom1, 480));//last query of first block
                    Assert.Null(saReader.GetAnnotation(_chrom1, 488));//between first and second block
                    Assert.NotNull(saReader.GetAnnotation(_chrom1, 490));//first entry of second block
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
    }
}