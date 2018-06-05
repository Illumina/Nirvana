using System.Collections.Generic;
using System.IO;
using System.Linq;
using Genome;
using SAUtils.ExtractCosmicSvs;
using UnitTests.TestUtilities;
using Xunit;

namespace UnitTests.SAUtils.InputFileParsers
{
    public sealed class CosmicCnvReaderTests
    {
        [Fact]
        public void GetColumnIndices_valid_header()
        {
            const string header = @"CNV_ID	ID_GENE	gene_name	ID_SAMPLE	ID_TUMOUR	Primary site	Site subtype 1	Site subtype 2	Site subtype 3	Primary histology	Histology subtype 1	Histology subtype 2	Histology subtype 3	SAMPLE_NAME	TOTAL_CN	MINOR_ALLELE	MUT_TYPE	ID_STUDY	GRCh	Chromosome:G_Start..G_Stop";

            var readStream = ResourceUtilities.GetReadStream(Resources.TopPath("SA\\CosmicCNV.tsv"));

            var cnvReader = new CosmicCnvReader(readStream,
                new Dictionary<string, IChromosome> {{"W", new Chromosome("chrW", "W", 1)}},
                GenomeAssembly.GRCh37);

            cnvReader.GetColumnIndices(header);
            //we do not need an assert because not getting an exception in the last line means pass
        }

        [Fact]
        public void GetColumnIndices_missing_column()
        {
            const string header = @"CNV_ID	ID_GENE	gene_name	ID_SAMPLE	ID_TUMOUR	Primary site	Site subtype 1	Site subtype 2	Site subtype 3	Primary histology	Histology subtype 1	Histology subtype 2	Histology subtype 3	SAMPLE_NAME	TOTAL_CN	MINOR_ALLELE	MUT_TYPE	ID_STUDY	Chromosome:G_Start..G_Stop";

            var readStream = ResourceUtilities.GetReadStream(Resources.TopPath("SA\\CosmicCNV.tsv"));

            var cnvReader = new CosmicCnvReader(readStream,
                new Dictionary<string, IChromosome> { { "W", new Chromosome("chrW", "W", 1) } },
                GenomeAssembly.GRCh37);

            Assert.Throws<InvalidDataException>(()=>cnvReader.GetColumnIndices(header));
        }

        [Fact]
        public void GetEntries()
        {
            var readStream = ResourceUtilities.GetReadStream(Resources.TopPath("SA\\CosmicCNV.tsv"));

            var cnvReader = new CosmicCnvReader(readStream,
                new Dictionary<string, IChromosome>
                {
                    { "17", new Chromosome("chr17", "17", 1) },
                    { "Y", new Chromosome("chrY", "Y", 2) },
                    { "MT", new Chromosome("chrM", "MT", 3) }
                },
                GenomeAssembly.GRCh37);

            var cnvItems = cnvReader.GetEntries();

            Assert.Equal(5, cnvItems.Count());
        }
    }
}