﻿using System.IO;
using ErrorHandling.Exceptions;
using Genome;
using IO;
using Moq;
using UnitTests.SAUtils.InputFileParsers;
using UnitTests.TestUtilities;
using VariantAnnotation.Interface.Positions;
using VariantAnnotation.Interface.Providers;
using Vcf;
using Vcf.VariantCreator;
using Xunit;

namespace UnitTests.Vcf
{
    public sealed class VcfReaderTests
    {
        private MemoryStream _ms;
        private StreamWriter _streamWriter;
        private readonly VariantId _vidCreator = new VariantId();

        private void AddLines(string[] lines)
        {
            _ms = new MemoryStream();
            _streamWriter = new StreamWriter(_ms);
            foreach (string headline in lines)
            {
                _streamWriter.WriteLine(headline);
            }
            _streamWriter.Flush();

            _ms.Position = 0;
        }

        [Fact]
        public void ValidateVcfHeader_ExceptionThrown_NoFileFormat()
        {
            var headers = new[] { "##Some comments", "#CHROM	POS	ID	REF	ALT	QUAL	FILTER	INFO	FORMAT	NHL-16	NHL-17" };
            AddLines(headers);
            var seqProvider = ParserTestUtils.GetSequenceProvider(1000, "A", 'T', ChromosomeUtilities.RefNameToChromosome);
            var reader = FileUtilities.GetStreamReader(_ms);
            Assert.Throws<UserErrorException>(() => VcfReader.Create(reader, reader, seqProvider, null, new NullVcfFilter(), _vidCreator, null));
        }

        [Fact]
        public void ValidateVcfHeader_ExceptionThrown_NoChromHeaderLine()
        {
            var headers = new[] { "##fileformat=VCFv4.1", "##FILTER=<ID=PASS,Description=\"All filters passed\">", "##fileDate=20160920" };
            AddLines(headers);
            var seqProvider = ParserTestUtils.GetSequenceProvider(1000, "A", 'T', ChromosomeUtilities.RefNameToChromosome);
            var reader = FileUtilities.GetStreamReader(_ms);
            Assert.Throws<UserErrorException>(() => VcfReader.Create(reader, reader, seqProvider, null, new NullVcfFilter(), _vidCreator, null));
        }

        [Fact]
        public void Sample_names_are_reported()
        {
            var headers = new[] { "##fileformat=VCFv4.1", "##FILTER=<ID=PASS,Description=\"All filters passed\">", "##fileDate=20160920", "#CHROM	POS	ID	REF	ALT	QUAL	FILTER	INFO	FORMAT	NHL-16	NHL-17" };
            AddLines(headers);
            string[] samples;
            var seqProvider = ParserTestUtils.GetSequenceProvider(1000, "A", 'T', ChromosomeUtilities.RefNameToChromosome);

            using (var reader = FileUtilities.GetStreamReader(_ms))
            using (var vcfReader = VcfReader.Create(reader, reader, seqProvider, null, new NullVcfFilter(), _vidCreator, null))
            {
                samples = vcfReader.GetSampleNames();
            }

            Assert.Equal(new[] { "NHL-16", "NHL-17" }, samples);
        }

        [Fact]
        public void GetChromAndLengthInfo_ReturnEmptyArray_NoProperPrefix()
        {
            Assert.Empty(VcfReader.GetChromAndLengthInfo("##fileformat=VCFv"));
        }

        [Fact]
        public void GetChromAndLengthInfo_ReturnEmptyArray_NoChromInfo()
        {
            Assert.Empty(VcfReader.GetChromAndLengthInfo("##contig=<ID>"));
        }

        [Fact]
        public void GetChromAndLengthInfo_ReturnEmptyArray_NoLengthInfo()
        {
            Assert.Empty(VcfReader.GetChromAndLengthInfo("##contig=<ID=chr1>"));
        }

        [Theory]
        [InlineData("##contig=<ID=chr1,length=343>")]
        [InlineData("##contig=<ID=X,length=1239495")]
        public void CheckContigId_IncorrectAutoAndSexChromLength_ThrowException(string contigLine)
        {
            var headers = new[] { "##fileformat=VCFv4.1", contigLine, "#CHROM	POS	ID	REF	ALT	QUAL	FILTER	INFO	FORMAT" };
            AddLines(headers);
            var seqProvider = ParserTestUtils.GetSequenceProvider(1000, "A", 'T', ChromosomeUtilities.RefNameToChromosome);

            using (var reader = FileUtilities.GetStreamReader(_ms))
                Assert.Throws<UserErrorException>(() => VcfReader.Create(reader, reader, seqProvider, null, new NullVcfFilter(), _vidCreator, null));
        }

        [Theory]
        [InlineData("##contig=<ID=unknown_contig,length=1232455>")]
        [InlineData("##contig=<ID=random_chrom,length=98772>")]
        public void CheckContigId_InferredAssemblyIsUnknown_GivenIrregularChrom(string contigLine)
        {
            var headers = new[] { "##fileformat=VCFv4.1", contigLine, "#CHROM	POS	ID	REF	ALT	QUAL	FILTER	INFO	FORMAT" };
            AddLines(headers);
            var seqProvider = ParserTestUtils.GetSequenceProvider(1000, "A", 'T', ChromosomeUtilities.RefNameToChromosome);

            using (var reader = FileUtilities.GetStreamReader(_ms))
            using (var vcfReader = VcfReader.Create(reader, reader, seqProvider, null, new NullVcfFilter(), _vidCreator, null))
            {
                Assert.Equal(GenomeAssembly.Unknown, vcfReader.InferredGenomeAssembly);
            }
        }

        [Theory]
        [InlineData("##contig=<ID=chrM,length=16569>")]
        [InlineData("##contig=<ID=MT,length=16569>")]
        public void CheckContigId_IsRcrsMitochondrionTrue_InferredAssemblyIsUnknown_GivenRcrsChrMLength(string contigLine)
        {
            var headers = new[] { "##fileformat=VCFv4.1", contigLine, "#CHROM	POS	ID	REF	ALT	QUAL	FILTER	INFO	FORMAT" };
            AddLines(headers);
            var seqProvider = ParserTestUtils.GetSequenceProvider(1000, "A", 'T', ChromosomeUtilities.RefNameToChromosome);
            using (var reader = FileUtilities.GetStreamReader(_ms))
            using (var vcfReader = VcfReader.Create(reader, reader, seqProvider, null, new NullVcfFilter(), _vidCreator, null))
            {
                Assert.Equal(GenomeAssembly.Unknown, vcfReader.InferredGenomeAssembly);
                Assert.True(vcfReader.IsRcrsMitochondrion);
            }
        }

        [Theory]
        [InlineData("##contig=<ID=chrM,length=1234>")]
        [InlineData("##contig=<ID=MT,length=5678>")]
        public void CheckContigId_IsRcrsMitochondrionFalse_InferredAssemblyIsUnknown_GivenNonRcrsChrMLength(string contigLine)
        {
            var headers = new[] { "##fileformat=VCFv4.1", contigLine, "#CHROM	POS	ID	REF	ALT	QUAL	FILTER	INFO	FORMAT" };
            AddLines(headers);
            var seqProvider = ParserTestUtils.GetSequenceProvider(1000, "A", 'T', ChromosomeUtilities.RefNameToChromosome);

            using (var reader = FileUtilities.GetStreamReader(_ms))
            using (var vcfReader = VcfReader.Create(reader, reader, seqProvider, null, new NullVcfFilter(), _vidCreator, null))
            {
                Assert.Equal(GenomeAssembly.Unknown, vcfReader.InferredGenomeAssembly);
                Assert.False(vcfReader.IsRcrsMitochondrion);
            }
        }

        [Theory]
        [InlineData("##contig=<ID=chr1,length=248956422>", new[] { "chr1", "248956422" })]
        [InlineData("##contig=<ID=2,length=242193529>", new[] { "2", "242193529" })]
        [InlineData("##contig=<ID=chrM,length=16569>", new[] { "chrM", "16569" })]
        [InlineData("##contig=<ID=MT,length=16569>", new[] { "MT", "16569" })]
        public void GetChromAndLength_AsExpect(string line, string[] info)
        {
            Assert.Equal(info, VcfReader.GetChromAndLengthInfo(line));
        }

        [Fact]
        public void GetNextPosition()
        {
            const string vcfLine = "chr1	13133	.	T	C	36.00	PASS	SNVSB=0.0;SNVHPOL=4	GT:GQ:GQX:DP:DPF:AD	0/1:62:20:7:1:3,4";
            var lines = new[]
            {
                "##fileformat=VCFv4.1", "##FILTER=<ID=PASS,Description=\"All filters passed\">", "##fileDate=20160920",
                "#CHROM	POS	ID	REF	ALT	QUAL	FILTER	INFO	FORMAT	NHL-16", vcfLine
            };

            AddLines(lines);

            var refMinorProvider = new Mock<IRefMinorProvider>();
            var seqProvider = ParserTestUtils.GetSequenceProvider(13133, "T", 'A', ChromosomeUtilities.RefNameToChromosome);
            IPosition observedResult;

            using (var reader = FileUtilities.GetStreamReader(_ms))
            using (var vcfReader = VcfReader.Create(reader, reader, seqProvider, refMinorProvider.Object, new NullVcfFilter(), _vidCreator, null))
            {
                observedResult = vcfReader.GetNextPosition();
            }

            var expectedResult = PositionPool.Get(ChromosomeUtilities.Chr1, 13133, 13133, "T", new[] { "C" }, 36, new[] { "PASS" }, null,
                null, null, vcfLine.Split('\t'), new[] { false }, false);

            Assert.NotNull(observedResult);
            Assert.Equal(expectedResult.End, observedResult.End);
            Assert.Equal(expectedResult.AltAlleles, observedResult.AltAlleles);
            Assert.Equal(expectedResult.Filters, observedResult.Filters);
            Assert.Equal(expectedResult.Quality, observedResult.Quality);
            Assert.Equal(expectedResult.VcfFields, observedResult.VcfFields);
            
            PositionPool.Return(expectedResult);
        }

        [Fact]
        public void CheckSampleConsistency_oneSample()
        {
            const string vcfLine1 = "chr1	13133	.	T	C	36.00	PASS	SNVSB=0.0;SNVHPOL=4	GT:GQ:GQX:DP:DPF:AD	0/1:62:20:7:1:3,4";
            const string vcfLine2 = "chr1	13133	.	T	A	36.00	PASS	SNVSB=0.0;SNVHPOL=4	GT:GQ:GQX:DP:DPF:AD";
            var lines = new[]
            {
                "##fileformat=VCFv4.1", "##FILTER=<ID=PASS,Description=\"All filters passed\">", "##fileDate=20160920",
                "#CHROM	POS	ID	REF	ALT	QUAL	FILTER	INFO	FORMAT	NHL-16", vcfLine1, vcfLine2
            };

            AddLines(lines);
            
            var refMinorProvider = new Mock<IRefMinorProvider>();
            var seqProvider = ParserTestUtils.GetSequenceProvider(13133, "T", 'A', ChromosomeUtilities.RefNameToChromosome);

            using (var reader = FileUtilities.GetStreamReader(_ms))
            using (var vcfReader = VcfReader.Create(reader, reader, seqProvider, refMinorProvider.Object, new NullVcfFilter(), _vidCreator, null))
            {
                //first line is valid. So, no exception
                Assert.NotNull(vcfReader.GetNextPosition()); 
                // second line has invalid number of sample fields, so it will throw exception
                Assert.Throws<UserErrorException>(()=>vcfReader.GetNextPosition());
            }
            
        }

        [Fact]
        public void CheckSampleConsistency_noSample()
        {
            const string vcfLine1 = "chr1	13133	.	T	C	36.00	PASS	SNVSB=0.0;SNVHPOL=4";
            const string vcfLine2 = "chr1	13133	.	T	A	36.00	PASS	SNVSB=0.0;SNVHPOL=4	GT:GQ:GQX:DP:DPF:AD	0/1:62:20:7:1:3,4";
            var lines = new[]
            {
                "##fileformat=VCFv4.1", "##FILTER=<ID=PASS,Description=\"All filters passed\">", "##fileDate=20160920",
                "#CHROM	POS	ID	REF	ALT	QUAL	FILTER	INFO", vcfLine1, vcfLine2
            };

            AddLines(lines);
            
            var refMinorProvider = new Mock<IRefMinorProvider>();
            var seqProvider = ParserTestUtils.GetSequenceProvider(13133, "T", 'A', ChromosomeUtilities.RefNameToChromosome);

            using (var reader = FileUtilities.GetStreamReader(_ms))
            using (var vcfReader = VcfReader.Create(reader, reader, seqProvider, refMinorProvider.Object, new NullVcfFilter(), _vidCreator, null))
            {
                //first line is valid. So, no exception
                Assert.NotNull(vcfReader.GetNextPosition());
                // second line has invalid number of sample fields, so it will throw exception
                Assert.Throws<UserErrorException>(() => vcfReader.GetNextPosition());
            }
        }
    }
}