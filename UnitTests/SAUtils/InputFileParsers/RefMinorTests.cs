using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Genome;
using IO;
using Moq;
using SAUtils.InputFileParsers.OneKGen;
using SAUtils.RefMinorDb;
using VariantAnnotation.Interface.Providers;
using VariantAnnotation.NSA;
using VariantAnnotation.Providers;
using VariantAnnotation.SA;
using Xunit;

namespace UnitTests.SAUtils.InputFileParsers
{
    public sealed class RefMinorTests
    {
        private static readonly IChromosome Chrom1 = new Chromosome("chr1", "1", 1);

        private readonly Dictionary<string, IChromosome> _chromDict = new Dictionary<string, IChromosome>
        {
            { "1", Chrom1}
        };

        private static Stream GetStream()
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);

            writer.WriteLine("##1000Genomes");
            writer.WriteLine("#CHROM\tPOS\tID\tREF\tALT\tQUAL\tFILTER\tINFO");
            writer.WriteLine("1\t15274\trs62636497\tA\tG,T\t100\tPASS\tAC=1739,3210;AF=0.347244,0.640974;AN=5008;NS=2504;DP=23255;EAS_AF=0.4812,0.5188;AMR_AF=0.2752,0.7205;AFR_AF=0.323,0.6369;EUR_AF=0.2922,0.7078;SAS_AF=0.3497,0.6472;AA=g|||;VT=SNP;MULTI_ALLELIC;EAS_AN=1008;EAS_AC=485,523;EUR_AN=1006;EUR_AC=294,712;AFR_AN=1322;AFR_AC=427,842;AMR_AN=694;AMR_AC=191,500;SAS_AN=978;SAS_AC=342,633");
            writer.WriteLine("1\t241369\trs11490246\tC\tT\t100\tPASS\tAC=5008;AF=1;AN=5008;NS=2504;DP=8951;EAS_AF=1;AMR_AF=1;AFR_AF=1;EUR_AF=1;SAS_AF=1;AA=.|||;VT=SNP;EAS_AN=1008;EAS_AC=1008;EUR_AN=1006;EUR_AC=1006;AFR_AN=1322;AFR_AC=1322;AMR_AN=694;AMR_AC=694;SAS_AN=978;SAS_AC=978");

            writer.Flush();

            stream.Position = 0;
            return stream;
        }

        [Fact]
        public void GetItems()
        {
            using (var reader = new RefMinorReader(new StreamReader(GetStream()), GetSequenceProvider()))
            {
                var items = reader.GetItems().ToList();

                Assert.Equal(3, items.Count);
            }
            
        }

        private ISequenceProvider GetSequenceProvider()
        {
            var seqProvider = new Mock<ISequenceProvider>();
            seqProvider.SetupGet(x => x.Assembly).Returns(GenomeAssembly.GRCh37);
            seqProvider.SetupGet(x => x.RefNameToChromosome).Returns(_chromDict);
            seqProvider.Setup(x => x.Sequence.Substring(15274 -1, 1)).Returns("A");
            seqProvider.Setup(x => x.Sequence.Substring(241369-1, 1)).Returns("C");

            return seqProvider.Object;
        }


        [Fact]
        public void LoopBack()
        {
            var version = new DataSourceVersion("onekgen", "v0.3", DateTime.Now.Ticks);
            using (var reader = new RefMinorReader(new StreamReader(GetStream()), GetSequenceProvider()))
            using (var stream = new MemoryStream())
            using (var indexStream = new MemoryStream())
            using (var writer = new RefMinorDbWriter(new ExtendedBinaryWriter(stream), new ExtendedBinaryWriter(indexStream), version, GetSequenceProvider(), SaCommon.SchemaVersion))
            {
                writer.Write(reader.GetItems());

                stream.Position = 0;
                indexStream.Position = 0;

                using (var dbReader = new RefMinorDbReader(stream,indexStream))
                {
                    Assert.Equal("T", dbReader.GetGlobalMajorAllele(Chrom1, 15274));
                    Assert.Null(dbReader.GetGlobalMajorAllele(Chrom1, 1524));
                }
            }
        }
    }
}