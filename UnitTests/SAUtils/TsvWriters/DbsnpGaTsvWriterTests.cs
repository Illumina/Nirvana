using System.Collections.Generic;
using System.IO;
using Moq;
using SAUtils.DataStructures;
using SAUtils.InputFileParsers.IntermediateAnnotation;
using SAUtils.TsvWriters;
using UnitTests.TestDataStructures;
using VariantAnnotation.Interface.Providers;
using VariantAnnotation.Interface.Sequence;
using VariantAnnotation.Providers;
using VariantAnnotation.Sequence;
using Xunit;

namespace UnitTests.SAUtils.TsvWriters
{
    public sealed class DbsnpGaTsvWriterTests
    {
        [Fact]
        public void Same_Frequency_ref_and_alt_and_ref_is_preferred_return_ref_as_gloabal_major()
        {

            var alleleFreqDict = new Dictionary<string,double>
            {
                {"A",0.4 },
                {"G",0.4 },
                {"C",0.2 }
            };
            var mostFrequent = DbsnpGaTsvWriter.GetMostFrequentAllele(alleleFreqDict, "A");
            Assert.Equal("A",mostFrequent);
        }

        [Fact]
        public void Same_Frequency_ref_and_alt_and_ref_is_not_preferred_return_alt_as_gloabal_major()
        {

            var alleleFreqDict = new Dictionary<string, double>
            {
                {"A",0.4 },
                {"G",0.4 },
                {"C",0.2 }
            };
            var mostFrequent = DbsnpGaTsvWriter.GetMostFrequentAllele(alleleFreqDict, "A",false);
            Assert.Equal("G", mostFrequent);
        }

        [Fact]
        public void same_Frequency_alts_return_the_first_one()
        {
            var alleleFreqDict = new Dictionary<string, double>
            {
                {"A",0.2 },
                {"G",0.4 },
                {"T",0.4 }
            };
            var mostFrequent = DbsnpGaTsvWriter.GetMostFrequentAllele(alleleFreqDict, "A");
            Assert.Equal("G", mostFrequent);
        }


        [Fact]
        private void DbsnpGaTsvWriter_write_sa_item()
        {
            var chromosome = new Chromosome("chr1", "1", 0);
            var chromDict = new Dictionary<string,IChromosome>
            {
                {"chr1",chromosome },
                {"1" ,chromosome}
            };

            var randomDbsnpPath = Path.GetTempPath();
            var sequenceProvider = new Mock<ISequenceProvider>();
            var simpleSequence = new SimpleSequence("ATGCGGT", 99);
            sequenceProvider.SetupGet(x => x.Sequence).Returns(simpleSequence);
            sequenceProvider.Setup(x => x.RefNameToChromosome).Returns(chromDict);
            var dataVersion = new DataSourceVersion("dbsnp", "77", 123456);
            var dbsnpWriter = new SaTsvWriter(randomDbsnpPath, dataVersion, "GRCh37",10,"dbsnp","dbsnp",true,sequenceProvider.Object);
            var globalAlleleWriter = new SaTsvWriter(randomDbsnpPath, dataVersion, "GRCh37", 10, "globalAllele", "GMAF", true, sequenceProvider.Object);

            using (var dbsnpGaTsvWriter = new DbsnpGaTsvWriter(dbsnpWriter, globalAlleleWriter))
            {
                var dbSnpItemsPos100 = new List<SupplementaryDataItem>
                {
                    new DbSnpItem(chromosome,100,123456,"A",0.2,"G",0.4),
                    new DbSnpItem(chromosome,100,123458,"A",0.2,"T",0.4)
                };
                var dbSnpItemsPos103 = new List<SupplementaryDataItem>
                {
                    new DbSnpItem(chromosome,103,134567,"C",0.5,"A",0.5)
                };

                var dbSnpItemsPos104 = new List<SupplementaryDataItem>
                {
                    new DbSnpItem(chromosome,104,134590,"G",double.MinValue,"A",0.75)
                };
                var dbSnpItemsPos106 = new List<SupplementaryDataItem>
                {
                    new DbSnpItem(chromosome,106,134257,"T",0.3,"G",0.45),
                    new DbSnpItem(chromosome,106,126753,"T",0.3,"A",0.25)
                };

                dbsnpGaTsvWriter.WritePosition(dbSnpItemsPos100);
                dbsnpGaTsvWriter.WritePosition(dbSnpItemsPos103);
                dbsnpGaTsvWriter.WritePosition(dbSnpItemsPos104);
                dbsnpGaTsvWriter.WritePosition(dbSnpItemsPos106);

            }

            var dbsnpFile = Path.Combine(randomDbsnpPath, "dbsnp_77.tsv.gz");
            var globalAlleleFile = Path.Combine(randomDbsnpPath, "globalAllele_77.tsv.gz");
            var tsvReader = new ParallelSaTsvReader(dbsnpFile);

            using (var tsvEnumerator = tsvReader.GetItems("1").GetEnumerator())
            {
                Assert.True(tsvEnumerator.MoveNext());
                Assert.Equal("\"ids\":[\"rs123456\"]", tsvEnumerator.Current.JsonStrings[0]);
                Assert.True(tsvEnumerator.MoveNext());
                Assert.Equal("\"ids\":[\"rs123458\"]", tsvEnumerator.Current.JsonStrings[0]);
            }

            var globalAlleleReader = new ParallelSaTsvReader(globalAlleleFile);
            var globalAlleleEnumerator = globalAlleleReader.GetItems("1").GetEnumerator();
            Assert.True(globalAlleleEnumerator.MoveNext());
            Assert.Equal(100,globalAlleleEnumerator.Current.Position);
            Assert.Equal("\"globalMinorAllele\":\"T\",\"globalMinorAlleleFrequency\":0.4", globalAlleleEnumerator.Current.JsonStrings[0]);
            Assert.True(globalAlleleEnumerator.MoveNext());
            Assert.Equal(103, globalAlleleEnumerator.Current.Position);
            Assert.Equal("\"globalMinorAllele\":\"A\",\"globalMinorAlleleFrequency\":0.5", globalAlleleEnumerator.Current.JsonStrings[0]);

            Assert.True(globalAlleleEnumerator.MoveNext());
            Assert.Equal(104, globalAlleleEnumerator.Current.Position);
            Assert.Equal("", globalAlleleEnumerator.Current.JsonStrings[0]);

            Assert.True(globalAlleleEnumerator.MoveNext());
            Assert.Equal(106, globalAlleleEnumerator.Current.Position);
            Assert.Equal("\"globalMinorAllele\":\"T\",\"globalMinorAlleleFrequency\":0.3", globalAlleleEnumerator.Current.JsonStrings[0]);

            globalAlleleEnumerator.Dispose();
            File.Delete(dbsnpFile);
            File.Delete(globalAlleleFile);
        }

       

    }
}