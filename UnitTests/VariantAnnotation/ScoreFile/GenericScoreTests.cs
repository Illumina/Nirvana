using System;
using System.Collections.Generic;
using System.IO;
using ErrorHandling.Exceptions;
using Genome;
using IO;
using IO.v2;
using Moq;
using SAUtils.GenericScore;
using SAUtils.GenericScore.GenericScoreParser;
using UnitTests.TestDataStructures;
using UnitTests.TestUtilities;
using VariantAnnotation.GenericScore;
using VariantAnnotation.Interface.Providers;
using VariantAnnotation.Providers;
using VariantAnnotation.SA;
using Variants;
using Xunit;

namespace UnitTests.VariantAnnotation.ScoreFile
{
    public sealed class GenericScoreTests
    {
        [Fact]
        public void TestScoreReader()
        {
            var      version     = new DataSourceVersion("source1", "v1", DateTime.Now.Ticks, "description");
            string[] nucleotides = {"A", "C", "G", "T"};
            var writerSettings = new WriterSettings(
                10_000,
                nucleotides,
                false,
                EncoderType.ZeroToOne,
                new ZeroToOneScoreEncoder(2, 1.0),
                new ScoreJsonEncoder("TestKey", "TestSubKey"),
                new SaItemValidator(true, true)
            );

            using (var saStream = new MemoryStream())
            using (var indexStream = new MemoryStream())
            {
                using (var saWriter = new ScoreFileWriter(
                           writerSettings,
                           saStream,
                           indexStream,
                           version,
                           GetAllASequenceProvider(),
                           SaCommon.SchemaVersion,
                           skipIncorrectRefEntries: false,
                           leaveOpen: true
                       ))
                {
                    var items = GetSaItems(1000);
                    saWriter.Write(items);
                }

                saStream.Position    = 0;
                indexStream.Position = 0;

                var saReader = ScoreReader.Read(saStream, indexStream);

                // before any SA existed
                Assert.True(double.IsNaN(saReader.GetScore(0, 90, "C")));
                // first entry of first block
                Assert.False(double.IsNaN(saReader.GetScore(0, 100, "C")));
                // last query of first block
                Assert.False(double.IsNaN(saReader.GetScore(0, 480, "C")));
                // between first and second block
                Assert.True(double.IsNaN(saReader.GetScore(0, 488, "C")));
                // first entry of second block
                Assert.False(double.IsNaN(saReader.GetScore(0, 490, "C")));
                // unknown allele
                Assert.True(double.IsNaN(saReader.GetScore(0, 490, "K")));
            }
        }

        [Fact]
        public void TestParRegion()
        {
            var      version     = new DataSourceVersion("source1", "v1", DateTime.Now.Ticks, "description");
            string[] nucleotides = {"A", "C", "G", "T"};
            var writerSettings = new WriterSettings(
                10_000,
                nucleotides,
                false,
                EncoderType.ZeroToOne,
                new ZeroToOneScoreEncoder(2, 1.0),
                new ScoreJsonEncoder("TestKey", "TestSubKey"),
                new SaItemValidator(true, true)
            );

            var count = 1000;

            using (var saStream = new MemoryStream())
            using (var indexStream = new MemoryStream())
            {
                using (var saWriter = new ScoreFileWriter(
                           writerSettings,
                           saStream,
                           indexStream,
                           version,
                           GetAllASequenceProvider(),
                           SaCommon.SchemaVersion,
                           skipIncorrectRefEntries: false,
                           leaveOpen: true
                       ))
                {
                    IEnumerable<GenericScoreItem> items = GetParRegionItems(count);
                    saWriter.Write(items);
                }

                saStream.Position    = 0;
                indexStream.Position = 0;

                var saReader = ScoreReader.Read(saStream, indexStream);

                var position = 10_010;
                for (int i = 0; i < count; i++, position += 2)
                {
                    Assert.False(double.IsNaN(saReader.GetScore(23, position, "C")));
                }
            }
        }

        [Fact]
        public void TestWriteUnknownAllele()
        {
            var      version     = new DataSourceVersion("source1", "v1", DateTime.Now.Ticks, "description");
            string[] nucleotides = {"A", "C", "G", "T"};
            var writerSettings = new WriterSettings(
                10_000,
                nucleotides,
                false,
                EncoderType.ZeroToOne,
                new ZeroToOneScoreEncoder(2, 1.0),
                new ScoreJsonEncoder("TestKey", "TestSubKey"),
                new SaItemValidator(true, true)
            );

            var position = 10_010;
            using (var saStream = new MemoryStream())
            using (var indexStream = new MemoryStream())
            {
                using (var saWriter = new ScoreFileWriter(
                           writerSettings,
                           saStream,
                           indexStream,
                           version,
                           GetAllASequenceProvider(),
                           SaCommon.SchemaVersion,
                           skipIncorrectRefEntries: false,
                           leaveOpen: true
                       ))
                {
                    IEnumerable<GenericScoreItem> items = new List<GenericScoreItem>
                    {
                        new(ChromosomeUtilities.Chr1, position, "A", "K", 0.5),
                    };
                    saWriter.Write(items);


                    saStream.Position    = 0;
                    indexStream.Position = 0;

                    var saReader = ScoreReader.Read(saStream, indexStream);
                    Assert.True(double.IsNaN(saReader.GetScore(ChromosomeUtilities.Chr1.Index, position, "A")));
                    Assert.True(double.IsNaN(saReader.GetScore(ChromosomeUtilities.Chr1.Index, position, "C")));
                    Assert.True(double.IsNaN(saReader.GetScore(ChromosomeUtilities.Chr1.Index, position, "G")));
                    Assert.True(double.IsNaN(saReader.GetScore(ChromosomeUtilities.Chr1.Index, position, "T")));
                }
            }
        }

        [Fact]
        public void TestOutOfOrderWriting()
        {
            var      version     = new DataSourceVersion("source1", "v1", DateTime.Now.Ticks, "description");
            string[] nucleotides = {"A", "C", "G", "T"};
            var writerSettings = new WriterSettings(
                10_000,
                nucleotides,
                false,
                EncoderType.ZeroToOne,
                new ZeroToOneScoreEncoder(2, 1.0),
                new ScoreJsonEncoder("TestKey", "TestSubKey"),
                new SaItemValidator(true, true)
            );

            var position = 10_010;
            using (var saStream = new MemoryStream())
            using (var indexStream = new MemoryStream())
            {
                using (var saWriter = new ScoreFileWriter(
                           writerSettings,
                           saStream,
                           indexStream,
                           version,
                           GetAllASequenceProvider(),
                           SaCommon.SchemaVersion,
                           skipIncorrectRefEntries: false,
                           leaveOpen: true
                       ))
                {
                    IEnumerable<GenericScoreItem> items = new List<GenericScoreItem>
                    {
                        new(ChromosomeUtilities.Chr1, position, "A", "C", 0.5),
                        new(ChromosomeUtilities.Chr1, position - 1, "A", "G", 0.5),
                    };

                    Assert.Throws<UserErrorException>(() => saWriter.Write(items));
                }
            }
        }

        [Fact]
        public void TestParRegion2()
        {
            var      version     = new DataSourceVersion("source1", "v1", DateTime.Now.Ticks, "description");
            string[] nucleotides = {"A", "C", "G", "T"};
            var writerSettings = new WriterSettings(
                10_000,
                nucleotides,
                false,
                EncoderType.ZeroToOne,
                new ZeroToOneScoreEncoder(2, 1.0),
                new ScoreJsonEncoder("TestKey", "TestSubKey"),
                new SaItemValidator(true, true)
            );

            var position = 10_010;
            using (var dataStream = new MemoryStream())
            using (var indexStream = new MemoryStream())
            {
                using (var saWriter = new ScoreFileWriter(
                           writerSettings,
                           dataStream,
                           indexStream,
                           version,
                           GetAllASequenceProvider(),
                           SaCommon.SchemaVersion,
                           skipIncorrectRefEntries: false,
                           leaveOpen: true
                       ))
                {
                    IEnumerable<GenericScoreItem> items = new List<GenericScoreItem>
                    {
                        new(ChromosomeUtilities.ChrY, position, "N", "C", 0.5),
                    };
                    saWriter.Write(items);
                }

                dataStream.Position  = 0;
                indexStream.Position = 0;

                var saReader = ScoreReader.Read(dataStream, indexStream);
                Assert.Equal(0.5, saReader.GetScore(23, position, "C"));
            }
        }

        [Fact]
        public void SchemaVersionTest()
        {
            var      version     = new DataSourceVersion("source1", "v1", DateTime.Now.Ticks, "description");
            string[] nucleotides = {"A", "C", "G", "T"};
            var writerSettings = new WriterSettings(
                10_000,
                nucleotides,
                false,
                EncoderType.ZeroToOne,
                new ZeroToOneScoreEncoder(2, 1.0),
                new ScoreJsonEncoder("TestKey", "TestSubKey"),
                new SaItemValidator(true, true)
            );

            var position = 10_010;
            using (var dataStream = new MemoryStream())
            using (var indexStream = new MemoryStream())
            {
                using (var saWriter = new ScoreFileWriter(
                           writerSettings,
                           dataStream,
                           indexStream,
                           version,
                           GetAllASequenceProvider(),
                           SaCommon.SchemaVersion + SaCommon.SchemaVersion,
                           skipIncorrectRefEntries: false,
                           leaveOpen: true
                       ))
                {
                    IEnumerable<GenericScoreItem> items = new List<GenericScoreItem>
                    {
                        new(ChromosomeUtilities.Chr1, position, "A", "C", 0.5),
                    };
                    saWriter.Write(items);
                }

                dataStream.Position  = 0;
                indexStream.Position = 0;

                Assert.Throws<UserErrorException>(() => ScoreReader.Read(dataStream, indexStream));
            }
        }

        [Fact]
        public void TestHeader()
        {
            var testData = new List<(FileType GsaIndex, uint GuardInt, ushort)>
            {
                (FileType.GsaIndex, SaCommon.GuardInt, 1),  // Incorrect File Type
                (FileType.GsaWriter, SaCommon.GuardInt, 2), // Incorrect File Format Version
                (FileType.GsaWriter, 2, 1)                  // Incorrect Guard Int
            };

            foreach ((FileType fileType, uint guardInt, ushort fileFormatVersion) in testData)
            {
                var writerStream = PrepareHeaderTestData(fileType, guardInt, fileFormatVersion);
                Assert.Throws<UserErrorException>(() => ScoreReader.Read(writerStream, null));
            }
        }

        private MemoryStream PrepareHeaderTestData(FileType fileType, uint guardInt, ushort fileFormatVersion)
        {
            var writerStream = new MemoryStream();
            var writer       = new ExtendedBinaryWriter(writerStream, System.Text.Encoding.Default);
            var header       = new Header(fileType, fileFormatVersion);

            header.Write(writer);
            writer.WriteOpt(1); // FilePairId
            writer.Write(guardInt);
            writerStream.Position = 0;

            return writerStream;
        }

        // [Fact]
        // TODO Understand what this test is doing
        // public void RemoveConflictingItems()
        // {
        //     const int blockLength = 10_000;
        //     string[]  nucleotides = {"A", "C", "G", "T"};
        //     var       version     = new DataSourceVersion("source1", "v1", DateTime.Now.Ticks, "description");
        //
        //     using (var saStream = new MemoryStream())
        //     using (var indexStream = new MemoryStream())
        //     using (var saWriter = new ScoreFileWriter(saStream, indexStream, version, GetAllASequenceProvider(), "dbsnp",
        //                SaCommon.SchemaVersion, nucleotides, blockLength, GenomeAssembly.GRCh37, 1, false, true, false))
        //     {
        //         Assert.Equal(0, saWriter.Write(GetConflictingGnomadItems()));
        //     }
        // }

        private static IEnumerable<GenericScoreItem> GetSaItems(int count)
        {
            var items    = new List<GenericScoreItem>();
            var position = 100;
            var random   = new Random();
            for (int i = 0; i < count; i++, position += 5)
            {
                double score = Math.Round(random.NextDouble(), 2);
                items.Add(new GenericScoreItem(ChromosomeUtilities.Chr1, position, "A", "C", score));
            }

            return items;
        }

        private static IEnumerable<GenericScoreItem> GetParRegionItems(int count)
        {
            var items    = new List<GenericScoreItem>();
            var position = 10_010;
            var random   = new Random();
            for (int i = 0; i < count; i++, position += 2)
            {
                double score = Math.Round(random.NextDouble(), 2);
                items.Add(new GenericScoreItem(ChromosomeUtilities.ChrY, position, "A", "C", score));
            }

            return items;
        }

        [Fact]
        public void WrongRefAllele_ThrowUserException()
        {
            var saItem = new GenericScoreItem(ChromosomeUtilities.Chr1, 100, "C", "T", 0.9);
            Assert.Throws<InvalidDataException>(() => WriteCustomSaItem(saItem, false));
            WriteCustomSaItem(saItem, true);
        }


        private static void WriteCustomSaItem(GenericScoreItem customItem, bool skipIncorrectRefEntries)
        {
            const int blockLength = 10_000;
            string[]  nucleotides = {"A", "C", "G", "T"};
            var       version     = new DataSourceVersion("source1", "v1", DateTime.Now.Ticks, "description");

            var writerSettings = new WriterSettings(
                blockLength,
                nucleotides,
                false,
                EncoderType.ZeroToOne,
                new ZeroToOneScoreEncoder(2, 1.0),
                new ScoreJsonEncoder("TestKey", "TestSubKey"),
                new SaItemValidator(true, !skipIncorrectRefEntries)
            );

            using (var writeStream = new MemoryStream())
            using (var indexStream = new MemoryStream())
            using (var scoreFileWriter = new ScoreFileWriter(
                       writerSettings,
                       writeStream,
                       indexStream,
                       version,
                       GetAllASequenceProvider(),
                       SaCommon.SchemaVersion,
                       skipIncorrectRefEntries,
                       true
                   ))
            {
                scoreFileWriter.Write(new[] {customItem});
            }
        }

        private static Stream GetChr22_17467787_17467799_genome()
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);

            writer.WriteLine("##gnomAD");
            writer.WriteLine("#CHROM\tPOS\tID\tREF\tALT\tQUAL\tFILTER\tINFO");
            writer.WriteLine(
                "22\t17467787\trs1013532764\tAAAAG\tA\t5607.38\tPASS\tAC=9;AN=7342;AF=0.00122582;rf_tp_probability=0.526938;FS=1.835;InbreedingCoeff=-0.0586;MQ=60.31;MQRankSum=-0.363;QD=12.01;ReadPosRankSum=0.416;SOR=0.869;BaseQRankSum=0.067;ClippingRankSum=0.263;DP=659925;VQSLOD=-0.9495;VQSR_culprit=FS;variant_type=indel;allele_type=del;n_alt_alleles=1;pab_max=0.864166;gq_hist_alt_bin_freq=0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|17;gq_hist_all_bin_freq=2625|6415|2399|2552|894|245|475|590|299|567|573|228|560|58|171|68|135|8|78|194;dp_hist_alt_bin_freq=0|0|0|2|4|6|2|2|0|1|0|0|0|0|0|0|0|0|0|0;dp_hist_alt_n_larger=0;dp_hist_all_bin_freq=4|18|221|1132|2818|4248|4392|3451|2107|976|414|186|95|56|40|33|32|20|18|17;dp_hist_all_n_larger=32;ab_hist_alt_bin_freq=0|0|0|0|0|0|2|1|4|1|2|5|2|0|0|0|0|0|0|0;AC_nfe_seu=0;AN_nfe_seu=38;AF_nfe_seu=0;nhomalt_nfe_seu=0;controls_AC_afr_male=1;controls_AN_afr_male=132;controls_AF_afr_male=0.00757576;controls_nhomalt_afr_male=0;non_topmed_AC_amr=1;non_topmed_AN_amr=168;non_topmed_AF_amr=0.00595238;non_topmed_nhomalt_amr=0;AC_raw=9;AN_raw=29502;AF_raw=0.000305064;nhomalt_raw=0;AC_fin_female=0;AN_fin_female=598;AF_fin_female=0;nhomalt_fin_female=0;non_neuro_AC_asj_female=0;non_neuro_AN_asj_female=12;non_neuro_AF_asj_female=0;non_neuro_nhomalt_asj_female=0;non_neuro_AC_afr_male=1;non_neuro_AN_afr_male=154;non_neuro_AF_afr_male=0.00649351;non_neuro_nhomalt_afr_male=0;AC_afr_male=1;AN_afr_male=446;AF_afr_male=0.00224215;nhomalt_afr_male=0;AC_afr=2;AN_afr=756;AF_afr=0.0026455;nhomalt_afr=0;non_neuro_AC_afr_female=1;non_neuro_AN_afr_female=164;non_neuro_AF_afr_female=0.00609756;non_neuro_nhomalt_afr_female=0;non_topmed_AC_amr_female=1;non_topmed_AN_amr_female=72;non_topmed_AF_amr_female=0.0138889;non_topmed_nhomalt_amr_female=0;non_topmed_AC_oth_female=2;non_topmed_AN_oth_female=110;non_topmed_AF_oth_female=0.0181818;non_topmed_nhomalt_oth_female=0;AC_eas_female=0;AN_eas_female=12;AF_eas_female=0;nhomalt_eas_female=0;AC_afr_female=1;AN_afr_female=310;AF_afr_female=0.00322581;nhomalt_afr_female=0;non_neuro_AC_female=2;non_neuro_AN_female=2324;non_neuro_AF_female=0.000860585;non_neuro_nhomalt_female=0;controls_AC_afr=1;controls_AN_afr=228;controls_AF_afr=0.00438596;controls_nhomalt_afr=0;AC_nfe_onf=1;AN_nfe_onf=628;AF_nfe_onf=0.00159236;nhomalt_nfe_onf=0;controls_AC_fin_male=0;controls_AN_fin_male=200;controls_AF_fin_male=0;controls_nhomalt_fin_male=0;non_neuro_AC_nfe_nwe=2;non_neuro_AN_nfe_nwe=2582;non_neuro_AF_nfe_nwe=0.000774593;non_neuro_nhomalt_nfe_nwe=0;AC_fin_male=0;AN_fin_male=526;AF_fin_male=0;nhomalt_fin_male=0;AC_nfe_female=0;AN_nfe_female=2104;AF_nfe_female=0;nhomalt_nfe_female=0;AC_amr=1;AN_amr=178;AF_amr=0.00561798;nhomalt_amr=0;non_topmed_AC_nfe_male=3;non_topmed_AN_nfe_male=1778;non_topmed_AF_nfe_male=0.00168729;non_topmed_nhomalt_nfe_male=0;AC_eas=0;AN_eas=48;AF_eas=0;nhomalt_eas=0;nhomalt=0;non_neuro_AC_nfe_female=0;non_neuro_AN_nfe_female=1840;non_neuro_AF_nfe_female=0;non_neuro_nhomalt_nfe_female=0;non_neuro_AC_afr=2;non_neuro_AN_afr=318;non_neuro_AF_afr=0.00628931;non_neuro_nhomalt_afr=0;controls_AC_raw=2;controls_AN_raw=10110;controls_AF_raw=0.000197824;controls_nhomalt_raw=0;controls_AC_male=2;controls_AN_male=1340;controls_AF_male=0.00149254;controls_nhomalt_male=0;non_topmed_AC_male=5;non_topmed_AN_male=3004;non_topmed_AF_male=0.00166445;non_topmed_nhomalt_male=0;controls_AC_nfe_female=0;controls_AN_nfe_female=740;controls_AF_nfe_female=0;controls_nhomalt_nfe_female=0;non_neuro_AC_amr=0;non_neuro_AN_amr=114;non_neuro_AF_amr=0;non_neuro_nhomalt_amr=0;non_neuro_AC_eas_female=0;non_neuro_AN_eas_female=12;non_neuro_AF_eas_female=0;non_neuro_nhomalt_eas_female=0;AC_asj_male=1;AN_asj_male=50;AF_asj_male=0.02;nhomalt_asj_male=0;controls_AC_nfe_male=1;controls_AN_nfe_male=908;controls_AF_nfe_male=0.00110132;controls_nhomalt_nfe_male=0;non_neuro_AC_fin=0;non_neuro_AN_fin=378;non_neuro_AF_fin=0;non_neuro_nhomalt_fin=0;AC_oth_female=2;AN_oth_female=112;AF_oth_female=0.0178571;nhomalt_oth_female=0;controls_AC_nfe=1;controls_AN_nfe=1648;controls_AF_nfe=0.000606796;controls_nhomalt_nfe=0;controls_AC_oth_female=0;controls_AN_oth_female=48;controls_AF_oth_female=0;controls_nhomalt_oth_female=0;controls_AC_asj=0;controls_AN_asj=8;controls_AF_asj=0;controls_nhomalt_asj=0;non_neuro_AC_amr_male=0;non_neuro_AN_amr_male=58;non_neuro_AF_amr_male=0;non_neuro_nhomalt_amr_male=0;controls_AC_nfe_nwe=1;controls_AN_nfe_nwe=308;controls_AF_nfe_nwe=0.00324675;controls_nhomalt_nfe_nwe=0;AC_nfe_nwe=2;AN_nfe_nwe=2906;AF_nfe_nwe=0.000688231;nhomalt_nfe_nwe=0;controls_AC_nfe_seu=0;controls_AN_nfe_seu=16;controls_AF_nfe_seu=0;controls_nhomalt_nfe_seu=0;non_neuro_AC_amr_female=0;non_neuro_AN_amr_female=56;non_neuro_AF_amr_female=0;non_neuro_nhomalt_amr_female=0;non_neuro_AC_nfe_onf=1;non_neuro_AN_nfe_onf=464;non_neuro_AF_nfe_onf=0.00215517;non_neuro_nhomalt_nfe_onf=0;non_topmed_AC_eas_male=0;non_topmed_AN_eas_male=34;non_topmed_AF_eas_male=0;non_topmed_nhomalt_eas_male=0;controls_AC_amr_female=0;controls_AN_amr_female=16;controls_AF_amr_female=0;controls_nhomalt_amr_female=0;non_neuro_AC_fin_male=0;non_neuro_AN_fin_male=200;non_neuro_AF_fin_male=0;non_neuro_nhomalt_fin_male=0;AC_female=4;AN_female=3236;AF_female=0.00123609;nhomalt_female=0;non_neuro_AC_oth_male=0;non_neuro_AN_oth_male=84;non_neuro_AF_oth_male=0;non_neuro_nhomalt_oth_male=0;non_topmed_AC_nfe_est=0;non_topmed_AN_nfe_est=1352;non_topmed_AF_nfe_est=0;non_topmed_nhomalt_nfe_est=0;non_topmed_AC_nfe_nwe=2;non_topmed_AN_nfe_nwe=1632;non_topmed_AF_nfe_nwe=0.00122549;non_topmed_nhomalt_nfe_nwe=0;non_topmed_AC_amr_male=0;non_topmed_AN_amr_male=96;non_topmed_AF_amr_male=0;non_topmed_nhomalt_amr_male=0;non_topmed_AC_nfe_onf=1;non_topmed_AN_nfe_onf=448;non_topmed_AF_nfe_onf=0.00223214;non_topmed_nhomalt_nfe_onf=0;controls_AC_eas_male=0;controls_AN_eas_male=16;controls_AF_eas_male=0;controls_nhomalt_eas_male=0;controls_AC_oth_male=0;controls_AN_oth_male=52;controls_AF_oth_male=0;controls_nhomalt_oth_male=0;non_topmed_AC=9;non_topmed_AN=5806;non_topmed_AF=0.00155012;non_topmed_nhomalt=0;controls_AC_fin=0;controls_AN_fin=378;controls_AF_fin=0;controls_nhomalt_fin=0;non_neuro_AC_nfe=3;non_neuro_AN_nfe=4272;non_neuro_AF_nfe=0.000702247;non_neuro_nhomalt_nfe=0;non_neuro_AC_fin_female=0;non_neuro_AN_fin_female=178;non_neuro_AF_fin_female=0;non_neuro_nhomalt_fin_female=0;non_topmed_AC_nfe_seu=0;non_topmed_AN_nfe_seu=38;non_topmed_AF_nfe_seu=0;non_topmed_nhomalt_nfe_seu=0;controls_AC_eas_female=0;controls_AN_eas_female=12;controls_AF_eas_female=0;controls_nhomalt_eas_female=0;non_topmed_AC_asj=1;non_topmed_AN_asj=38;non_topmed_AF_asj=0.0263158;non_topmed_nhomalt_asj=0;controls_AC_nfe_onf=0;controls_AN_nfe_onf=124;controls_AF_nfe_onf=0;controls_nhomalt_nfe_onf=0;non_neuro_AC=7;non_neuro_AN=5332;non_neuro_AF=0.00131283;non_neuro_nhomalt=0;non_topmed_AC_nfe=3;non_topmed_AN_nfe=3470;non_topmed_AF_nfe=0.000864553;non_topmed_nhomalt_nfe=0;non_topmed_AC_raw=9;non_topmed_AN_raw=24832;non_topmed_AF_raw=0.000362436;non_topmed_nhomalt_raw=0;non_neuro_AC_nfe_est=0;non_neuro_AN_nfe_est=1212;non_neuro_AF_nfe_est=0;non_neuro_nhomalt_nfe_est=0;non_topmed_AC_oth_male=0;non_topmed_AN_oth_male=114;non_topmed_AF_oth_male=0;non_topmed_nhomalt_oth_male=0;AC_nfe_est=0;AN_nfe_est=1356;AF_nfe_est=0;nhomalt_nfe_est=0;non_topmed_AC_afr_male=1;non_topmed_AN_afr_male=434;non_topmed_AF_afr_male=0.00230415;non_topmed_nhomalt_afr_male=0;AC_eas_male=0;AN_eas_male=36;AF_eas_male=0;nhomalt_eas_male=0;controls_AC_eas=0;controls_AN_eas=28;controls_AF_eas=0;controls_nhomalt_eas=0;non_neuro_AC_eas_male=0;non_neuro_AN_eas_male=36;non_neuro_AF_eas_male=0;non_neuro_nhomalt_eas_male=0;non_neuro_AC_asj_male=1;non_neuro_AN_asj_male=44;non_neuro_AF_asj_male=0.0227273;non_neuro_nhomalt_asj_male=0;controls_AC_oth=0;controls_AN_oth=100;controls_AF_oth=0;controls_nhomalt_oth=0;AC_nfe=3;AN_nfe=4928;AF_nfe=0.000608766;nhomalt_nfe=0;non_topmed_AC_female=4;non_topmed_AN_female=2802;non_topmed_AF_female=0.00142755;non_topmed_nhomalt_female=0;non_neuro_AC_asj=1;non_neuro_AN_asj=56;non_neuro_AF_asj=0.0178571;non_neuro_nhomalt_asj=0;non_topmed_AC_eas_female=0;non_topmed_AN_eas_female=10;non_topmed_AF_eas_female=0;non_topmed_nhomalt_eas_female=0;non_neuro_AC_raw=7;non_neuro_AN_raw=20066;non_neuro_AF_raw=0.000348849;non_neuro_nhomalt_raw=0;non_topmed_AC_eas=0;non_topmed_AN_eas=44;non_topmed_AF_eas=0;non_topmed_nhomalt_eas=0;non_topmed_AC_fin_male=0;non_topmed_AN_fin_male=526;non_topmed_AF_fin_male=0;non_topmed_nhomalt_fin_male=0;AC_fin=0;AN_fin=1124;AF_fin=0;nhomalt_fin=0;AC_nfe_male=3;AN_nfe_male=2824;AF_nfe_male=0.00106232;nhomalt_nfe_male=0;controls_AC_amr_male=0;controls_AN_amr_male=30;controls_AF_amr_male=0;controls_nhomalt_amr_male=0;controls_AC_afr_female=0;controls_AN_afr_female=96;controls_AF_afr_female=0;controls_nhomalt_afr_female=0;controls_AC_amr=0;controls_AN_amr=46;controls_AF_amr=0;controls_nhomalt_amr=0;AC_asj_female=0;AN_asj_female=22;AF_asj_female=0;nhomalt_asj_female=0;non_neuro_AC_eas=0;non_neuro_AN_eas=48;non_neuro_AF_eas=0;non_neuro_nhomalt_eas=0;non_neuro_AC_male=5;non_neuro_AN_male=3008;non_neuro_AF_male=0.00166223;non_neuro_nhomalt_male=0;AC_asj=1;AN_asj=72;AF_asj=0.0138889;nhomalt_asj=0;controls_AC_nfe_est=0;controls_AN_nfe_est=1200;controls_AF_nfe_est=0;controls_nhomalt_nfe_est=0;non_topmed_AC_asj_female=0;non_topmed_AN_asj_female=16;non_topmed_AF_asj_female=0;non_topmed_nhomalt_asj_female=0;non_topmed_AC_oth=2;non_topmed_AN_oth=224;non_topmed_AF_oth=0.00892857;non_topmed_nhomalt_oth=0;non_topmed_AC_fin_female=0;non_topmed_AN_fin_female=598;non_topmed_AF_fin_female=0;non_topmed_nhomalt_fin_female=0;AC_oth=2;AN_oth=236;AF_oth=0.00847458;nhomalt_oth=0;non_neuro_AC_nfe_male=3;non_neuro_AN_nfe_male=2432;non_neuro_AF_nfe_male=0.00123355;non_neuro_nhomalt_nfe_male=0;controls_AC_female=0;controls_AN_female=1096;controls_AF_female=0;controls_nhomalt_female=0;non_topmed_AC_fin=0;non_topmed_AN_fin=1124;non_topmed_AF_fin=0;non_topmed_nhomalt_fin=0;non_topmed_AC_nfe_female=0;non_topmed_AN_nfe_female=1692;non_topmed_AF_nfe_female=0;non_topmed_nhomalt_nfe_female=0;controls_AC_asj_male=0;controls_AN_asj_male=2;controls_AF_asj_male=0;controls_nhomalt_asj_male=0;non_topmed_AC_asj_male=1;non_topmed_AN_asj_male=22;non_topmed_AF_asj_male=0.0454545;non_topmed_nhomalt_asj_male=0;non_neuro_AC_oth=1;non_neuro_AN_oth=146;non_neuro_AF_oth=0.00684932;non_neuro_nhomalt_oth=0;AC_male=5;AN_male=4106;AF_male=0.00121773;nhomalt_male=0;controls_AC_fin_female=0;controls_AN_fin_female=178;controls_AF_fin_female=0;controls_nhomalt_fin_female=0;controls_AC_asj_female=0;controls_AN_asj_female=6;controls_AF_asj_female=0;controls_nhomalt_asj_female=0;AC_amr_male=0;AN_amr_male=100;AF_amr_male=0;nhomalt_amr_male=0;AC_amr_female=1;AN_amr_female=78;AF_amr_female=0.0128205;nhomalt_amr_female=0;AC_oth_male=0;AN_oth_male=124;AF_oth_male=0;nhomalt_oth_male=0;non_neuro_AC_nfe_seu=0;non_neuro_AN_nfe_seu=14;non_neuro_AF_nfe_seu=0;non_neuro_nhomalt_nfe_seu=0;non_topmed_AC_afr_female=1;non_topmed_AN_afr_female=304;non_topmed_AF_afr_female=0.00328947;non_topmed_nhomalt_afr_female=0;non_topmed_AC_afr=2;non_topmed_AN_afr=738;non_topmed_AF_afr=0.00271003;non_topmed_nhomalt_afr=0;controls_AC=2;controls_AN=2436;controls_AF=0.000821018;controls_nhomalt=0;non_neuro_AC_oth_female=1;non_neuro_AN_oth_female=62;non_neuro_AF_oth_female=0.016129;non_neuro_nhomalt_oth_female=0;non_topmed_faf95_amr=0.000305;non_topmed_faf99_amr=0.000305;faf95_afr=0.00047001;faf99_afr=0.00046996;controls_faf95_afr=0.000224;controls_faf99_afr=0.000224;faf95_amr=0.000288;faf99_amr=0.000288;faf95_eas=0;faf99_eas=0;faf95=0.00063865;faf99=0.0006395;non_neuro_faf95_afr=0.00111728;non_neuro_faf99_afr=0.00111671;non_neuro_faf95_amr=0;non_neuro_faf99_amr=0;controls_faf95_nfe=3.1e-05;controls_faf99_nfe=3.1e-05;non_topmed_faf95=0.00080814;non_topmed_faf99=0.00080791;non_neuro_faf95_nfe=0.000191;non_neuro_faf99_nfe=0.00019047;non_neuro_faf95=0.00061599;non_neuro_faf99=0.00061588;non_topmed_faf95_nfe=0.0002353;non_topmed_faf99_nfe=0.00023558;controls_faf95_eas=0;controls_faf99_eas=0;faf95_nfe=0.0001658;faf99_nfe=0.00016511;non_topmed_faf95_eas=0;non_topmed_faf99_eas=0;controls_faf95_amr=0;controls_faf99_amr=0;non_neuro_faf95_eas=0;non_neuro_faf99_eas=0;non_topmed_faf95_afr=0.00048118;non_topmed_faf99_afr=0.00048064;controls_faf95=0.00014568;controls_faf99=0.00014565;controls_popmax=afr;controls_AC_popmax=1;controls_AN_popmax=228;controls_AF_popmax=0.00438596;controls_nhomalt_popmax=0;popmax=amr;AC_popmax=1;AN_popmax=178;AF_popmax=0.00561798;nhomalt_popmax=0;age_hist_het_bin_freq=1|0|1|1|0|2|0|0|0|0;age_hist_het_n_smaller=1;age_hist_het_n_larger=0;age_hist_hom_bin_freq=0|0|0|0|0|0|0|0|0|0;age_hist_hom_n_smaller=0;age_hist_hom_n_larger=0;non_neuro_popmax=afr;non_neuro_AC_popmax=2;non_neuro_AN_popmax=318;non_neuro_AF_popmax=0.00628931;non_neuro_nhomalt_popmax=0;non_topmed_popmax=amr;non_topmed_AC_popmax=1;non_topmed_AN_popmax=168;non_topmed_AF_popmax=0.00595238;non_topmed_nhomalt_popmax=0");
            writer.WriteLine(
                "22\t17467793\trs200526150\tAAGAA\tA\t2.96178e+06\tPASS\tAC=25;AN=13820;AF=0.00180897;rf_tp_probability=0.6944;FS=0;InbreedingCoeff=-0.0226;MQ=61.07;MQRankSum=0.061;QD=19.6;ReadPosRankSum=0.177;SOR=0.694;BaseQRankSum=-0.031;ClippingRankSum=-0.053;DP=657153;VQSLOD=5.11;VQSR_culprit=FS;variant_type=multi-indel;allele_type=del;n_alt_alleles=2;pab_max=1;gq_hist_alt_bin_freq=0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|1|0|36;gq_hist_all_bin_freq=2892|4902|1140|827|277|141|343|478|268|556|481|207|525|87|178|89|169|40|119|5100;dp_hist_alt_bin_freq=0|0|0|1|5|8|10|5|4|1|0|0|1|1|0|0|0|0|1|0;dp_hist_alt_n_larger=0;dp_hist_all_bin_freq=3|25|286|1366|3137|4439|4355|3211|1821|851|331|175|79|53|32|42|22|27|18|12;dp_hist_all_n_larger=25;ab_hist_alt_bin_freq=0|0|0|0|0|0|2|2|6|8|3|6|7|2|0|0|0|0|0|0;AC_nfe_seu=0;AN_nfe_seu=60;AF_nfe_seu=0;nhomalt_nfe_seu=0;controls_AC_afr_male=0;controls_AN_afr_male=654;controls_AF_afr_male=0;controls_nhomalt_afr_male=0;non_topmed_AC_amr=17;non_topmed_AN_amr=272;non_topmed_AF_amr=0.0625;non_topmed_nhomalt_amr=1;AC_raw=25;AN_raw=28996;AF_raw=0.000862188;nhomalt_raw=1;AC_fin_female=0;AN_fin_female=834;AF_fin_female=0;nhomalt_fin_female=0;non_neuro_AC_asj_female=0;non_neuro_AN_asj_female=38;non_neuro_AF_asj_female=0;non_neuro_nhomalt_asj_female=0;non_neuro_AC_afr_male=0;non_neuro_AN_afr_male=730;non_neuro_AF_afr_male=0;non_neuro_nhomalt_afr_male=0;AC_afr_male=2;AN_afr_male=2172;AF_afr_male=0.00092081;nhomalt_afr_male=0;AC_afr=2;AN_afr=3678;AF_afr=0.000543774;nhomalt_afr=0;non_neuro_AC_afr_female=0;non_neuro_AN_afr_female=754;non_neuro_AF_afr_female=0;non_neuro_nhomalt_afr_female=0;non_topmed_AC_amr_female=9;non_topmed_AN_amr_female=132;non_topmed_AF_amr_female=0.0681818;non_topmed_nhomalt_amr_female=1;non_topmed_AC_oth_female=2;non_topmed_AN_oth_female=190;non_topmed_AF_oth_female=0.0105263;non_topmed_nhomalt_oth_female=0;AC_eas_female=0;AN_eas_female=248;AF_eas_female=0;nhomalt_eas_female=0;AC_afr_female=0;AN_afr_female=1506;AF_afr_female=0;nhomalt_afr_female=0;non_neuro_AC_female=7;non_neuro_AN_female=4262;non_neuro_AF_female=0.00164242;non_neuro_nhomalt_female=0;controls_AC_afr=0;controls_AN_afr=1120;controls_AF_afr=0;controls_nhomalt_afr=0;AC_nfe_onf=0;AN_nfe_onf=904;AF_nfe_onf=0;nhomalt_nfe_onf=0;controls_AC_fin_male=0;controls_AN_fin_male=276;controls_AF_fin_male=0;controls_nhomalt_fin_male=0;non_neuro_AC_nfe_nwe=1;non_neuro_AN_nfe_nwe=3534;non_neuro_AF_nfe_nwe=0.000282965;non_neuro_nhomalt_nfe_nwe=0;AC_fin_male=0;AN_fin_male=708;AF_fin_male=0;nhomalt_fin_male=0;AC_nfe_female=1;AN_nfe_female=3128;AF_nfe_female=0.000319693;nhomalt_nfe_female=0;AC_amr=18;AN_amr=286;AF_amr=0.0629371;nhomalt_amr=1;non_topmed_AC_nfe_male=1;non_topmed_AN_nfe_male=2566;non_topmed_AF_nfe_male=0.000389712;non_topmed_nhomalt_nfe_male=0;AC_eas=0;AN_eas=656;AF_eas=0;nhomalt_eas=0;nhomalt=1;non_neuro_AC_nfe_female=1;non_neuro_AN_nfe_female=2732;non_neuro_AF_nfe_female=0.000366032;non_neuro_nhomalt_nfe_female=0;non_neuro_AC_afr=0;non_neuro_AN_afr=1484;non_neuro_AF_afr=0;non_neuro_nhomalt_afr=0;controls_AC_raw=4;controls_AN_raw=9932;controls_AF_raw=0.000402739;controls_nhomalt_raw=0;controls_AC_male=3;controls_AN_male=2680;controls_AF_male=0.0011194;controls_nhomalt_male=0;non_topmed_AC_male=11;non_topmed_AN_male=6164;non_topmed_AF_male=0.00178456;non_topmed_nhomalt_male=0;controls_AC_nfe_female=0;controls_AN_nfe_female=1186;controls_AF_nfe_female=0;controls_nhomalt_nfe_female=0;non_neuro_AC_amr=9;non_neuro_AN_amr=184;non_neuro_AF_amr=0.048913;non_neuro_nhomalt_amr=0;non_neuro_AC_eas_female=0;non_neuro_AN_eas_female=248;non_neuro_AF_eas_female=0;non_neuro_nhomalt_eas_female=0;AC_asj_male=0;AN_asj_male=92;AF_asj_male=0;nhomalt_asj_male=0;controls_AC_nfe_male=0;controls_AN_nfe_male=1378;controls_AF_nfe_male=0;controls_nhomalt_nfe_male=0;non_neuro_AC_fin=0;non_neuro_AN_fin=532;non_neuro_AF_fin=0;non_neuro_nhomalt_fin=0;AC_oth_female=2;AN_oth_female=194;AF_oth_female=0.0103093;nhomalt_oth_female=0;controls_AC_nfe=0;controls_AN_nfe=2564;controls_AF_nfe=0;controls_nhomalt_nfe=0;controls_AC_oth_female=0;controls_AN_oth_female=76;controls_AF_oth_female=0;controls_nhomalt_oth_female=0;controls_AC_asj=0;controls_AN_asj=20;controls_AF_asj=0;controls_nhomalt_asj=0;non_neuro_AC_amr_male=4;non_neuro_AN_amr_male=74;non_neuro_AF_amr_male=0.0540541;non_neuro_nhomalt_amr_male=0;controls_AC_nfe_nwe=0;controls_AN_nfe_nwe=426;controls_AF_nfe_nwe=0;controls_nhomalt_nfe_nwe=0;AC_nfe_nwe=2;AN_nfe_nwe=3958;AF_nfe_nwe=0.000505306;nhomalt_nfe_nwe=0;controls_AC_nfe_seu=0;controls_AN_nfe_seu=26;controls_AF_nfe_seu=0;controls_nhomalt_nfe_seu=0;non_neuro_AC_amr_female=5;non_neuro_AN_amr_female=110;non_neuro_AF_amr_female=0.0454545;non_neuro_nhomalt_amr_female=0;non_neuro_AC_nfe_onf=0;non_neuro_AN_nfe_onf=704;non_neuro_AF_nfe_onf=0;non_neuro_nhomalt_nfe_onf=0;non_topmed_AC_eas_male=0;non_topmed_AN_eas_male=400;non_topmed_AF_eas_male=0;non_topmed_nhomalt_eas_male=0;controls_AC_amr_female=1;controls_AN_amr_female=46;controls_AF_amr_female=0.0217391;controls_nhomalt_amr_female=0;non_neuro_AC_fin_male=0;non_neuro_AN_fin_male=276;non_neuro_AF_fin_male=0;non_neuro_nhomalt_fin_male=0;AC_female=13;AN_female=6098;AF_female=0.00213185;nhomalt_female=1;non_neuro_AC_oth_male=1;non_neuro_AN_oth_male=156;non_neuro_AF_oth_male=0.00641026;non_neuro_nhomalt_oth_male=0;non_topmed_AC_nfe_est=0;non_topmed_AN_nfe_est=2184;non_topmed_AF_nfe_est=0;non_topmed_nhomalt_nfe_est=0;non_topmed_AC_nfe_nwe=2;non_topmed_AN_nfe_nwe=2250;non_topmed_AF_nfe_nwe=0.000888889;non_topmed_nhomalt_nfe_nwe=0;non_topmed_AC_amr_male=8;non_topmed_AN_amr_male=140;non_topmed_AF_amr_male=0.0571429;non_topmed_nhomalt_amr_male=0;non_topmed_AC_nfe_onf=0;non_topmed_AN_nfe_onf=646;non_topmed_AF_nfe_onf=0;non_topmed_nhomalt_nfe_onf=0;controls_AC_eas_male=0;controls_AN_eas_male=244;controls_AF_eas_male=0;controls_nhomalt_eas_male=0;controls_AC_oth_male=0;controls_AN_oth_male=84;controls_AF_oth_male=0;controls_nhomalt_oth_male=0;non_topmed_AC=23;non_topmed_AN=11642;non_topmed_AF=0.00197561;non_topmed_nhomalt=1;controls_AC_fin=0;controls_AN_fin=532;controls_AF_fin=0;controls_nhomalt_fin=0;non_neuro_AC_nfe=1;non_neuro_AN_nfe=6226;non_neuro_AF_nfe=0.000160617;non_neuro_nhomalt_nfe=0;non_neuro_AC_fin_female=0;non_neuro_AN_fin_female=256;non_neuro_AF_fin_female=0;non_neuro_nhomalt_fin_female=0;non_topmed_AC_nfe_seu=0;non_topmed_AN_nfe_seu=60;non_topmed_AF_nfe_seu=0;non_topmed_nhomalt_nfe_seu=0;controls_AC_eas_female=0;controls_AN_eas_female=172;controls_AF_eas_female=0;controls_nhomalt_eas_female=0;non_topmed_AC_asj=0;non_topmed_AN_asj=68;non_topmed_AF_asj=0;non_topmed_nhomalt_asj=0;controls_AC_nfe_onf=0;controls_AN_nfe_onf=168;controls_AF_nfe_onf=0;controls_nhomalt_nfe_onf=0;non_neuro_AC=12;non_neuro_AN=9480;non_neuro_AF=0.00126582;non_neuro_nhomalt=0;non_topmed_AC_nfe=2;non_topmed_AN_nfe=5140;non_topmed_AF_nfe=0.000389105;non_topmed_nhomalt_nfe=0;non_topmed_AC_raw=23;non_topmed_AN_raw=24482;non_topmed_AF_raw=0.000939466;non_topmed_nhomalt_raw=1;non_neuro_AC_nfe_est=0;non_neuro_AN_nfe_est=1962;non_neuro_AF_nfe_est=0;non_neuro_nhomalt_nfe_est=0;non_topmed_AC_oth_male=1;non_topmed_AN_oth_male=184;non_topmed_AF_oth_male=0.00543478;non_topmed_nhomalt_oth_male=0;AC_nfe_est=0;AN_nfe_est=2192;AF_nfe_est=0;nhomalt_nfe_est=0;non_topmed_AC_afr_male=1;non_topmed_AN_afr_male=2132;non_topmed_AF_afr_male=0.000469043;non_topmed_nhomalt_afr_male=0;AC_eas_male=0;AN_eas_male=408;AF_eas_male=0;nhomalt_eas_male=0;controls_AC_eas=0;controls_AN_eas=416;controls_AF_eas=0;controls_nhomalt_eas=0;non_neuro_AC_eas_male=0;non_neuro_AN_eas_male=408;non_neuro_AF_eas_male=0;non_neuro_nhomalt_eas_male=0;non_neuro_AC_asj_male=0;non_neuro_AN_asj_male=80;non_neuro_AF_asj_male=0;non_neuro_nhomalt_asj_male=0;controls_AC_oth=0;controls_AN_oth=160;controls_AF_oth=0;controls_nhomalt_oth=0;AC_nfe=2;AN_nfe=7114;AF_nfe=0.000281136;nhomalt_nfe=0;non_topmed_AC_female=12;non_topmed_AN_female=5478;non_topmed_AF_female=0.00219058;non_topmed_nhomalt_female=1;non_neuro_AC_asj=0;non_neuro_AN_asj=118;non_neuro_AF_asj=0;non_neuro_nhomalt_asj=0;non_topmed_AC_eas_female=0;non_topmed_AN_eas_female=240;non_topmed_AF_eas_female=0;non_topmed_nhomalt_eas_female=0;non_neuro_AC_raw=12;non_neuro_AN_raw=19660;non_neuro_AF_raw=0.000610376;non_neuro_nhomalt_raw=0;non_topmed_AC_eas=0;non_topmed_AN_eas=640;non_topmed_AF_eas=0;non_topmed_nhomalt_eas=0;non_topmed_AC_fin_male=0;non_topmed_AN_fin_male=708;non_topmed_AF_fin_male=0;non_topmed_nhomalt_fin_male=0;AC_fin=0;AN_fin=1542;AF_fin=0;nhomalt_fin=0;AC_nfe_male=1;AN_nfe_male=3986;AF_nfe_male=0.000250878;nhomalt_nfe_male=0;controls_AC_amr_male=3;controls_AN_amr_male=38;controls_AF_amr_male=0.0789474;controls_nhomalt_amr_male=0;controls_AC_afr_female=0;controls_AN_afr_female=466;controls_AF_afr_female=0;controls_nhomalt_afr_female=0;controls_AC_amr=4;controls_AN_amr=84;controls_AF_amr=0.047619;controls_nhomalt_amr=0;AC_asj_female=0;AN_asj_female=46;AF_asj_female=0;nhomalt_asj_female=0;non_neuro_AC_eas=0;non_neuro_AN_eas=656;non_neuro_AF_eas=0;non_neuro_nhomalt_eas=0;non_neuro_AC_male=5;non_neuro_AN_male=5218;non_neuro_AF_male=0.000958222;non_neuro_nhomalt_male=0;AC_asj=0;AN_asj=138;AF_asj=0;nhomalt_asj=0;controls_AC_nfe_est=0;controls_AN_nfe_est=1944;controls_AF_nfe_est=0;controls_nhomalt_nfe_est=0;non_topmed_AC_asj_female=0;non_topmed_AN_asj_female=34;non_topmed_AF_asj_female=0;non_topmed_nhomalt_asj_female=0;non_topmed_AC_oth=3;non_topmed_AN_oth=374;non_topmed_AF_oth=0.00802139;non_topmed_nhomalt_oth=0;non_topmed_AC_fin_female=0;non_topmed_AN_fin_female=834;non_topmed_AF_fin_female=0;non_topmed_nhomalt_fin_female=0;AC_oth=3;AN_oth=406;AF_oth=0.00738916;nhomalt_oth=0;non_neuro_AC_nfe_male=0;non_neuro_AN_nfe_male=3494;non_neuro_AF_nfe_male=0;non_neuro_nhomalt_nfe_male=0;controls_AC_female=1;controls_AN_female=2216;controls_AF_female=0.000451264;controls_nhomalt_female=0;non_topmed_AC_fin=0;non_topmed_AN_fin=1542;non_topmed_AF_fin=0;non_topmed_nhomalt_fin=0;non_topmed_AC_nfe_female=1;non_topmed_AN_nfe_female=2574;non_topmed_AF_nfe_female=0.0003885;non_topmed_nhomalt_nfe_female=0;controls_AC_asj_male=0;controls_AN_asj_male=6;controls_AF_asj_male=0;controls_nhomalt_asj_male=0;non_topmed_AC_asj_male=0;non_topmed_AN_asj_male=34;non_topmed_AF_asj_male=0;non_topmed_nhomalt_asj_male=0;non_neuro_AC_oth=2;non_neuro_AN_oth=280;non_neuro_AF_oth=0.00714286;non_neuro_nhomalt_oth=0;AC_male=12;AN_male=7722;AF_male=0.001554;nhomalt_male=0;controls_AC_fin_female=0;controls_AN_fin_female=256;controls_AF_fin_female=0;controls_nhomalt_fin_female=0;controls_AC_asj_female=0;controls_AN_asj_female=14;controls_AF_asj_female=0;controls_nhomalt_asj_female=0;AC_amr_male=8;AN_amr_male=144;AF_amr_male=0.0555556;nhomalt_amr_male=0;AC_amr_female=10;AN_amr_female=142;AF_amr_female=0.0704225;nhomalt_amr_female=1;AC_oth_male=1;AN_oth_male=212;AF_oth_male=0.00471698;nhomalt_oth_male=0;non_neuro_AC_nfe_seu=0;non_neuro_AN_nfe_seu=26;non_neuro_AF_nfe_seu=0;non_neuro_nhomalt_nfe_seu=0;non_topmed_AC_afr_female=0;non_topmed_AN_afr_female=1474;non_topmed_AF_afr_female=0;non_topmed_nhomalt_afr_female=0;non_topmed_AC_afr=1;non_topmed_AN_afr=3606;non_topmed_AF_afr=0.000277316;non_topmed_nhomalt_afr=0;controls_AC=4;controls_AN=4896;controls_AF=0.000816993;controls_nhomalt=0;non_neuro_AC_oth_female=1;non_neuro_AN_oth_female=124;non_neuro_AF_oth_female=0.00806452;non_neuro_nhomalt_oth_female=0;non_topmed_faf95_amr=0.0398231;non_topmed_faf99_amr=0.0398236;faf95_afr=9.592e-05;faf99_afr=9.609e-05;controls_faf95_afr=0;controls_faf99_afr=0;faf95_amr=0.0406793;faf99_amr=0.0406792;faf95_eas=0;faf99_eas=0;faf95=0.00125772;faf99=0.00125736;non_neuro_faf95_afr=0;non_neuro_faf99_afr=0;non_neuro_faf95_amr=0.0255171;non_neuro_faf99_amr=0.0255167;controls_faf95_nfe=0;controls_faf99_nfe=0;non_topmed_faf95=0.00134988;non_topmed_faf99=0.00134945;non_neuro_faf95_nfe=8e-06;non_neuro_faf99_nfe=8e-06;non_neuro_faf95=0.00072973;non_neuro_faf99=0.00073008;non_topmed_faf95_nfe=6.881e-05;non_topmed_faf99_nfe=6.877e-05;controls_faf95_eas=0;controls_faf99_eas=0;faf95_nfe=4.922e-05;faf99_nfe=4.923e-05;non_topmed_faf95_eas=0;non_topmed_faf99_eas=0;controls_faf95_amr=0.0162655;controls_faf99_amr=0.0162653;non_neuro_faf95_eas=0;non_neuro_faf99_eas=0;non_topmed_faf95_afr=1.4e-05;non_topmed_faf99_afr=1.4e-05;controls_faf95=0.00027835;controls_faf99=0.00027827;controls_popmax=amr;controls_AC_popmax=4;controls_AN_popmax=84;controls_AF_popmax=0.047619;controls_nhomalt_popmax=0;popmax=amr;AC_popmax=18;AN_popmax=286;AF_popmax=0.0629371;nhomalt_popmax=1;age_hist_het_bin_freq=0|0|2|1|1|1|0|0|0|0;age_hist_het_n_smaller=4;age_hist_het_n_larger=0;age_hist_hom_bin_freq=0|0|0|0|0|0|0|0|0|0;age_hist_hom_n_smaller=0;age_hist_hom_n_larger=0;non_neuro_popmax=amr;non_neuro_AC_popmax=9;non_neuro_AN_popmax=184;non_neuro_AF_popmax=0.048913;non_neuro_nhomalt_popmax=0;non_topmed_popmax=amr;non_topmed_AC_popmax=17;non_topmed_AN_popmax=272;non_topmed_AF_popmax=0.0625;non_topmed_nhomalt_popmax=1");
            writer.WriteLine(
                "22\t17467793\trs200526150\tAAGAA\tA\t2.96178e+06\tPASS\tAC=4501;AN=13820;AF=0.325687;rf_tp_probability=0.6944;FS=0;InbreedingCoeff=-0.0226;MQ=61.07;MQRankSum=0.061;QD=19.6;ReadPosRankSum=0.177;SOR=0.694;BaseQRankSum=-0.031;ClippingRankSum=-0.053;DP=657153;VQSLOD=5.11;VQSR_culprit=FS;variant_type=multi-indel;allele_type=del;n_alt_alleles=2;pab_max=1;gq_hist_alt_bin_freq=3|3|4|4|5|3|4|6|8|10|21|14|36|33|27|47|34|35|43|4884;gq_hist_all_bin_freq=2897|4907|1144|830|282|143|344|482|273|559|484|208|528|92|176|87|149|45|119|5070;dp_hist_alt_bin_freq=0|6|126|551|1133|1285|1033|600|260|102|40|27|13|13|3|11|1|6|7|2;dp_hist_alt_n_larger=5;dp_hist_all_bin_freq=3|25|286|1366|3137|4439|4355|3211|1821|851|331|175|79|53|32|42|22|27|18|12;dp_hist_all_n_larger=25;ab_hist_alt_bin_freq=0|7|1|7|36|124|277|456|835|741|1055|616|404|155|42|25|5|6|5|0;AC_nfe_seu=19;AN_nfe_seu=60;AF_nfe_seu=0.316667;nhomalt_nfe_seu=1;controls_AC_afr_male=325;controls_AN_afr_male=654;controls_AF_afr_male=0.496942;controls_nhomalt_afr_male=35;non_topmed_AC_amr=77;non_topmed_AN_amr=272;non_topmed_AF_amr=0.283088;non_topmed_nhomalt_amr=2;AC_raw=4527;AN_raw=28996;AF_raw=0.156125;nhomalt_raw=356;AC_fin_female=187;AN_fin_female=834;AF_fin_female=0.224221;nhomalt_fin_female=6;non_neuro_AC_asj_female=15;non_neuro_AN_asj_female=38;non_neuro_AF_asj_female=0.394737;non_neuro_nhomalt_asj_female=0;non_neuro_AC_afr_male=358;non_neuro_AN_afr_male=730;non_neuro_AF_afr_male=0.490411;non_neuro_nhomalt_afr_male=37;AC_afr_male=1071;AN_afr_male=2172;AF_afr_male=0.493094;nhomalt_afr_male=113;AC_afr=1825;AN_afr=3678;AF_afr=0.496194;nhomalt_afr=196;non_neuro_AC_afr_female=376;non_neuro_AN_afr_female=754;non_neuro_AF_afr_female=0.498674;non_neuro_nhomalt_afr_female=42;non_topmed_AC_amr_female=35;non_topmed_AN_amr_female=132;non_topmed_AF_amr_female=0.265152;non_topmed_nhomalt_amr_female=0;non_topmed_AC_oth_female=58;non_topmed_AN_oth_female=190;non_topmed_AF_oth_female=0.305263;non_topmed_nhomalt_oth_female=6;AC_eas_female=135;AN_eas_female=248;AF_eas_female=0.544355;nhomalt_eas_female=14;AC_afr_female=754;AN_afr_female=1506;AF_afr_female=0.500664;nhomalt_afr_female=83;non_neuro_AC_female=1325;non_neuro_AN_female=4262;non_neuro_AF_female=0.310887;non_neuro_nhomalt_female=93;controls_AC_afr=566;controls_AN_afr=1120;controls_AF_afr=0.505357;controls_nhomalt_afr=67;AC_nfe_onf=233;AN_nfe_onf=904;AF_nfe_onf=0.257743;nhomalt_nfe_onf=13;controls_AC_fin_male=58;controls_AN_fin_male=276;controls_AF_fin_male=0.210145;controls_nhomalt_fin_male=2;non_neuro_AC_nfe_nwe=797;non_neuro_AN_nfe_nwe=3534;non_neuro_AF_nfe_nwe=0.225523;non_neuro_nhomalt_nfe_nwe=38;AC_fin_male=146;AN_fin_male=708;AF_fin_male=0.206215;nhomalt_fin_male=4;AC_nfe_female=774;AN_nfe_female=3128;AF_nfe_female=0.247442;nhomalt_nfe_female=42;AC_amr=79;AN_amr=286;AF_amr=0.276224;nhomalt_amr=2;non_topmed_AC_nfe_male=636;non_topmed_AN_nfe_male=2566;non_topmed_AF_nfe_male=0.247857;non_topmed_nhomalt_nfe_male=33;AC_eas=359;AN_eas=656;AF_eas=0.547256;nhomalt_eas=35;nhomalt=352;non_neuro_AC_nfe_female=666;non_neuro_AN_nfe_female=2732;non_neuro_AF_nfe_female=0.243777;non_neuro_nhomalt_nfe_female=30;non_neuro_AC_afr=734;non_neuro_AN_afr=1484;non_neuro_AF_afr=0.494609;non_neuro_nhomalt_afr=79;controls_AC_raw=1673;controls_AN_raw=9932;controls_AF_raw=0.168445;controls_nhomalt_raw=138;controls_AC_male=920;controls_AN_male=2680;controls_AF_male=0.343284;controls_nhomalt_male=78;non_topmed_AC_male=2163;non_topmed_AN_male=6164;non_topmed_AF_male=0.350909;non_topmed_nhomalt_male=179;controls_AC_nfe_female=300;controls_AN_nfe_female=1186;controls_AF_nfe_female=0.252951;controls_nhomalt_nfe_female=11;non_neuro_AC_amr=55;non_neuro_AN_amr=184;non_neuro_AF_amr=0.298913;non_neuro_nhomalt_amr=1;non_neuro_AC_eas_female=135;non_neuro_AN_eas_female=248;non_neuro_AF_eas_female=0.544355;non_neuro_nhomalt_eas_female=14;AC_asj_male=34;AN_asj_male=92;AF_asj_male=0.369565;nhomalt_asj_male=5;controls_AC_nfe_male=360;controls_AN_nfe_male=1378;controls_AF_nfe_male=0.261248;controls_nhomalt_nfe_male=21;non_neuro_AC_fin=118;non_neuro_AN_fin=532;non_neuro_AF_fin=0.221805;non_neuro_nhomalt_fin=3;AC_oth_female=60;AN_oth_female=194;AF_oth_female=0.309278;nhomalt_oth_female=7;controls_AC_nfe=660;controls_AN_nfe=2564;controls_AF_nfe=0.25741;controls_nhomalt_nfe=32;controls_AC_oth_female=19;controls_AN_oth_female=76;controls_AF_oth_female=0.25;controls_nhomalt_oth_female=1;controls_AC_asj=9;controls_AN_asj=20;controls_AF_asj=0.45;controls_nhomalt_asj=1;non_neuro_AC_amr_male=24;non_neuro_AN_amr_male=74;non_neuro_AF_amr_male=0.324324;non_neuro_nhomalt_amr_male=1;controls_AC_nfe_nwe=99;controls_AN_nfe_nwe=426;controls_AF_nfe_nwe=0.232394;controls_nhomalt_nfe_nwe=5;AC_nfe_nwe=894;AN_nfe_nwe=3958;AF_nfe_nwe=0.225872;nhomalt_nfe_nwe=44;controls_AC_nfe_seu=10;controls_AN_nfe_seu=26;controls_AF_nfe_seu=0.384615;controls_nhomalt_nfe_seu=0;non_neuro_AC_amr_female=31;non_neuro_AN_amr_female=110;non_neuro_AF_amr_female=0.281818;non_neuro_nhomalt_amr_female=0;non_neuro_AC_nfe_onf=190;non_neuro_AN_nfe_onf=704;non_neuro_AF_nfe_onf=0.269886;non_neuro_nhomalt_nfe_onf=12;non_topmed_AC_eas_male=219;non_topmed_AN_eas_male=400;non_topmed_AF_eas_male=0.5475;non_topmed_nhomalt_eas_male=20;controls_AC_amr_female=18;controls_AN_amr_female=46;controls_AF_amr_female=0.391304;controls_nhomalt_amr_female=0;non_neuro_AC_fin_male=58;non_neuro_AN_fin_male=276;non_neuro_AF_fin_male=0.210145;non_neuro_nhomalt_fin_male=2;AC_female=1965;AN_female=6098;AF_female=0.322237;nhomalt_female=152;non_neuro_AC_oth_male=49;non_neuro_AN_oth_male=156;non_neuro_AF_oth_male=0.314103;non_neuro_nhomalt_oth_male=5;non_topmed_AC_nfe_est=577;non_topmed_AN_nfe_est=2184;non_topmed_AF_nfe_est=0.264194;non_topmed_nhomalt_nfe_est=32;non_topmed_AC_nfe_nwe=515;non_topmed_AN_nfe_nwe=2250;non_topmed_AF_nfe_nwe=0.228889;non_topmed_nhomalt_nfe_nwe=28;non_topmed_AC_amr_male=42;non_topmed_AN_amr_male=140;non_topmed_AF_amr_male=0.3;non_topmed_nhomalt_amr_male=2;non_topmed_AC_nfe_onf=169;non_topmed_AN_nfe_onf=646;non_topmed_AF_nfe_onf=0.26161;non_topmed_nhomalt_nfe_onf=8;controls_AC_eas_male=136;controls_AN_eas_male=244;controls_AF_eas_male=0.557377;controls_nhomalt_eas_male=15;controls_AC_oth_male=25;controls_AN_oth_male=84;controls_AF_oth_male=0.297619;controls_nhomalt_oth_male=4;non_topmed_AC=3972;non_topmed_AN=11642;non_topmed_AF=0.341178;non_topmed_nhomalt=324;controls_AC_fin=118;controls_AN_fin=532;controls_AF_fin=0.221805;controls_nhomalt_fin=3;non_neuro_AC_nfe=1506;non_neuro_AN_nfe=6226;non_neuro_AF_nfe=0.241889;non_neuro_nhomalt_nfe=73;non_neuro_AC_fin_female=60;non_neuro_AN_fin_female=256;non_neuro_AF_fin_female=0.234375;non_neuro_nhomalt_fin_female=1;non_topmed_AC_nfe_seu=19;non_topmed_AN_nfe_seu=60;non_topmed_AF_nfe_seu=0.316667;non_topmed_nhomalt_nfe_seu=1;controls_AC_eas_female=95;controls_AN_eas_female=172;controls_AF_eas_female=0.552326;controls_nhomalt_eas_female=12;non_topmed_AC_asj=24;non_topmed_AN_asj=68;non_topmed_AF_asj=0.352941;non_topmed_nhomalt_asj=1;controls_AC_nfe_onf=46;controls_AN_nfe_onf=168;controls_AF_nfe_onf=0.27381;controls_nhomalt_nfe_onf=4;non_neuro_AC=2909;non_neuro_AN=9480;non_neuro_AF=0.306857;non_neuro_nhomalt=207;non_topmed_AC_nfe=1280;non_topmed_AN_nfe=5140;non_topmed_AF_nfe=0.249027;non_topmed_nhomalt_nfe=69;non_topmed_AC_raw=3996;non_topmed_AN_raw=24482;non_topmed_AF_raw=0.163222;non_topmed_nhomalt_raw=327;non_neuro_AC_nfe_est=509;non_neuro_AN_nfe_est=1962;non_neuro_AF_nfe_est=0.259429;non_neuro_nhomalt_nfe_est=23;non_topmed_AC_oth_male=56;non_topmed_AN_oth_male=184;non_topmed_AF_oth_male=0.304348;non_topmed_nhomalt_oth_male=6;AC_nfe_est=579;AN_nfe_est=2192;AF_nfe_est=0.264142;nhomalt_nfe_est=32;non_topmed_AC_afr_male=1054;non_topmed_AN_afr_male=2132;non_topmed_AF_afr_male=0.494371;non_topmed_nhomalt_afr_male=113;AC_eas_male=224;AN_eas_male=408;AF_eas_male=0.54902;nhomalt_eas_male=21;controls_AC_eas=231;controls_AN_eas=416;controls_AF_eas=0.555288;controls_nhomalt_eas=27;non_neuro_AC_eas_male=224;non_neuro_AN_eas_male=408;non_neuro_AF_eas_male=0.54902;non_neuro_nhomalt_eas_male=21;non_neuro_AC_asj_male=31;non_neuro_AN_asj_male=80;non_neuro_AF_asj_male=0.3875;non_neuro_nhomalt_asj_male=5;controls_AC_oth=44;controls_AN_oth=160;controls_AF_oth=0.275;controls_nhomalt_oth=5;AC_nfe=1725;AN_nfe=7114;AF_nfe=0.24248;nhomalt_nfe=90;non_topmed_AC_female=1809;non_topmed_AN_female=5478;non_topmed_AF_female=0.33023;non_topmed_nhomalt_female=145;non_neuro_AC_asj=46;non_neuro_AN_asj=118;non_neuro_AF_asj=0.389831;non_neuro_nhomalt_asj=5;non_topmed_AC_eas_female=132;non_topmed_AN_eas_female=240;non_topmed_AF_eas_female=0.55;non_topmed_nhomalt_eas_female=14;non_neuro_AC_raw=2928;non_neuro_AN_raw=19660;non_neuro_AF_raw=0.148932;non_neuro_nhomalt_raw=211;non_topmed_AC_eas=351;non_topmed_AN_eas=640;non_topmed_AF_eas=0.548438;non_topmed_nhomalt_eas=34;non_topmed_AC_fin_male=146;non_topmed_AN_fin_male=708;non_topmed_AF_fin_male=0.206215;non_topmed_nhomalt_fin_male=4;AC_fin=333;AN_fin=1542;AF_fin=0.215953;nhomalt_fin=10;AC_nfe_male=951;AN_nfe_male=3986;AF_nfe_male=0.238585;nhomalt_nfe_male=48;controls_AC_amr_male=12;controls_AN_amr_male=38;controls_AF_amr_male=0.315789;controls_nhomalt_amr_male=0;controls_AC_afr_female=241;controls_AN_afr_female=466;controls_AF_afr_female=0.517167;controls_nhomalt_afr_female=32;controls_AC_amr=30;controls_AN_amr=84;controls_AF_amr=0.357143;controls_nhomalt_amr=0;AC_asj_female=18;AN_asj_female=46;AF_asj_female=0.391304;nhomalt_asj_female=0;non_neuro_AC_eas=359;non_neuro_AN_eas=656;non_neuro_AF_eas=0.547256;non_neuro_nhomalt_eas=35;non_neuro_AC_male=1584;non_neuro_AN_male=5218;non_neuro_AF_male=0.303565;non_neuro_nhomalt_male=114;AC_asj=52;AN_asj=138;AF_asj=0.376812;nhomalt_asj=5;controls_AC_nfe_est=505;controls_AN_nfe_est=1944;controls_AF_nfe_est=0.259774;controls_nhomalt_nfe_est=23;non_topmed_AC_asj_female=14;non_topmed_AN_asj_female=34;non_topmed_AF_asj_female=0.411765;non_topmed_nhomalt_asj_female=0;non_topmed_AC_oth=114;non_topmed_AN_oth=374;non_topmed_AF_oth=0.304813;non_topmed_nhomalt_oth=12;non_topmed_AC_fin_female=187;non_topmed_AN_fin_female=834;non_topmed_AF_fin_female=0.224221;non_topmed_nhomalt_fin_female=6;AC_oth=128;AN_oth=406;AF_oth=0.315271;nhomalt_oth=14;non_neuro_AC_nfe_male=840;non_neuro_AN_nfe_male=3494;non_neuro_AF_nfe_male=0.240412;non_neuro_nhomalt_nfe_male=43;controls_AC_female=738;controls_AN_female=2216;controls_AF_female=0.333032;controls_nhomalt_female=57;non_topmed_AC_fin=333;non_topmed_AN_fin=1542;non_topmed_AF_fin=0.215953;non_topmed_nhomalt_fin=10;non_topmed_AC_nfe_female=644;non_topmed_AN_nfe_female=2574;non_topmed_AF_nfe_female=0.250194;non_topmed_nhomalt_nfe_female=36;controls_AC_asj_male=4;controls_AN_asj_male=6;controls_AF_asj_male=0.666667;controls_nhomalt_asj_male=1;non_topmed_AC_asj_male=10;non_topmed_AN_asj_male=34;non_topmed_AF_asj_male=0.294118;non_topmed_nhomalt_asj_male=1;non_neuro_AC_oth=91;non_neuro_AN_oth=280;non_neuro_AF_oth=0.325;non_neuro_nhomalt_oth=11;AC_male=2536;AN_male=7722;AF_male=0.328412;nhomalt_male=200;controls_AC_fin_female=60;controls_AN_fin_female=256;controls_AF_fin_female=0.234375;controls_nhomalt_fin_female=1;controls_AC_asj_female=5;controls_AN_asj_female=14;controls_AF_asj_female=0.357143;controls_nhomalt_asj_female=0;AC_amr_male=42;AN_amr_male=144;AF_amr_male=0.291667;nhomalt_amr_male=2;AC_amr_female=37;AN_amr_female=142;AF_amr_female=0.260563;nhomalt_amr_female=0;AC_oth_male=68;AN_oth_male=212;AF_oth_male=0.320755;nhomalt_oth_male=7;non_neuro_AC_nfe_seu=10;non_neuro_AN_nfe_seu=26;non_neuro_AF_nfe_seu=0.384615;non_neuro_nhomalt_nfe_seu=0;non_topmed_AC_afr_female=739;non_topmed_AN_afr_female=1474;non_topmed_AF_afr_female=0.501357;non_topmed_nhomalt_afr_female=83;non_topmed_AC_afr=1793;non_topmed_AN_afr=3606;non_topmed_AF_afr=0.497227;non_topmed_nhomalt_afr=196;controls_AC=1658;controls_AN=4896;controls_AF=0.338644;controls_nhomalt=135;non_neuro_AC_oth_female=42;non_neuro_AN_oth_female=124;non_neuro_AF_oth_female=0.33871;non_neuro_nhomalt_oth_female=6;non_topmed_faf95_amr=0.232194;non_topmed_faf99_amr=0.232194;faf95_afr=0.477244;faf99_afr=0.477244;controls_faf95_afr=0.470932;controls_faf99_afr=0.470932;faf95_amr=0.227168;faf99_amr=0.227169;faf95_eas=0.500629;faf99_eas=0.500629;faf95=0.317744;faf99=0.317744;non_neuro_faf95_afr=0.464967;non_neuro_faf99_afr=0.464967;non_neuro_faf95_amr=0.235846;non_neuro_faf99_amr=0.235846;controls_faf95_nfe=0.241154;controls_faf99_nfe=0.241154;non_topmed_faf95=0.332322;non_topmed_faf99=0.332323;non_neuro_faf95_nfe=0.231727;non_neuro_faf99_nfe=0.231728;non_neuro_faf95=0.297558;non_neuro_faf99=0.297559;non_topmed_faf95_nfe=0.237689;non_topmed_faf99_nfe=0.23769;controls_faf95_eas=0.49659;controls_faf99_eas=0.49659;faf95_nfe=0.232957;faf99_nfe=0.232956;non_topmed_faf95_eas=0.501191;non_topmed_faf99_eas=0.501191;controls_faf95_amr=0.257071;controls_faf99_amr=0.257071;non_neuro_faf95_eas=0.500629;non_neuro_faf99_eas=0.500629;non_topmed_faf95_afr=0.47807;non_topmed_faf99_afr=0.47807;controls_faf95=0.32508;controls_faf99=0.325081;controls_popmax=eas;controls_AC_popmax=231;controls_AN_popmax=416;controls_AF_popmax=0.555288;controls_nhomalt_popmax=27;popmax=eas;AC_popmax=359;AN_popmax=656;AF_popmax=0.547256;nhomalt_popmax=35;age_hist_het_bin_freq=128|162|214|283|349|260|234|152|93|46;age_hist_het_n_smaller=717;age_hist_het_n_larger=23;age_hist_hom_bin_freq=9|11|18|24|26|15|20|8|12|6;age_hist_hom_n_smaller=82;age_hist_hom_n_larger=4;non_neuro_popmax=eas;non_neuro_AC_popmax=359;non_neuro_AN_popmax=656;non_neuro_AF_popmax=0.547256;non_neuro_nhomalt_popmax=35;non_topmed_popmax=eas;non_topmed_AC_popmax=351;non_topmed_AN_popmax=640;non_topmed_AF_popmax=0.548438;non_topmed_nhomalt_popmax=34");

            writer.Flush();

            stream.Position = 0;
            return stream;
        }

        private static IEnumerable<GenericScoreItem> GetConflictingGnomadItems()
        {
            var sequence = new SimpleSequence(
                new string('T', VariantUtils.MaxUpstreamLength) + "AAAGAAAGAAAG",
                17467787                                        - 1 - VariantUtils.MaxUpstreamLength
            );
            var sequenceProvider = new SimpleSequenceProvider(GenomeAssembly.GRCh38, sequence, ChromosomeUtilities.RefNameToChromosome);

            var parserSettings = new ParserSettings(
                new ColumnIndex(0, 2, 3, 4, 5, null),
                new[] {"A", "C", "G", "T"},
                GenericScoreParser.MaxRepresentativeScores
            );

            var gnomadReader = new GenericScoreParser(parserSettings, new StreamReader(GetChr22_17467787_17467799_genome()), null);

            return gnomadReader.GetItems();
        }

        public static ISequenceProvider GetAllASequenceProvider(GenomeAssembly assembly = GenomeAssembly.GRCh37)
        {
            var seqProvider = new Mock<ISequenceProvider>();
            seqProvider.SetupGet(x => x.Assembly).Returns(assembly);
            seqProvider.Setup(x => x.Sequence.Substring(It.IsAny<int>(), 1)).Returns("A");

            return seqProvider.Object;
        }
    }
}