using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Genome;
using IO;
using Moq;
using SAUtils.DataStructures;
using SAUtils.InputFileParsers.ClinVar;
using UnitTests.TestDataStructures;
using UnitTests.TestUtilities;
using VariantAnnotation.Interface.Providers;
using Xunit;

namespace UnitTests.SAUtils.InputFileParsers
{
    public sealed class ClinVarXmlReaderTests
    {
        private static ISequenceProvider GetSequenceProvider(GenomeAssembly assembly, int start, string refSequence)
        {
            var seqProvider = new Mock<ISequenceProvider>();
            seqProvider.Setup(x => x.RefNameToChromosome).Returns(ChromosomeUtilities.RefNameToChromosome);
            seqProvider.Setup(x => x.Assembly).Returns(assembly);
            seqProvider.Setup(x => x.Sequence).Returns(new SimpleSequence(refSequence, start - 1));
            return seqProvider.Object;
        }

        [Fact]
        public void BasicReadTest()
        {
            var sequenceProvider = GetSequenceProvider(GenomeAssembly.GRCh37, 41234419, "A");
            var reader = new ClinVarReader(FileUtilities.GetReadStream(Resources.ClinvarXmlFiles("RCV000077146.xml")),Stream.Null, sequenceProvider);

            var items = reader.GetRcvItems();
            Assert.True(items.Any());

            
            var clinVarItem = items.First();
            Assert.Equal("RCV000077146.3", clinVarItem.Id);
            Assert.Equal("17", clinVarItem.Chromosome.EnsemblName);
            Assert.Equal(41234419, clinVarItem.Position);
            Assert.Equal("A", clinVarItem.RefAllele);
            Assert.Equal("C", clinVarItem.AltAllele);
            Assert.Equal("2019-12-15", new DateTime(clinVarItem.LastUpdatedDate).ToString("yyyy-MM-dd"));
            Assert.Equal(clinVarItem.AlleleOrigins, new List<string> { "germline" });
            Assert.Equal("C2676676", clinVarItem.MedGenIds.First());
            Assert.Equal("145", clinVarItem.OrphanetIds.First());
            Assert.Equal("604370", clinVarItem.OmimIds.First());
            Assert.Equal("Breast-ovarian cancer, familial 1", clinVarItem.Phenotypes.First());
        }

        [Fact]
        public void RCV000001373_NoExtraOmimId()
        {
            var sequenceProvider = GetSequenceProvider(GenomeAssembly.GRCh37, 3209662, "AGCAGACGGGCA");
            var reader = new ClinVarReader(FileUtilities.GetReadStream(Resources.ClinvarXmlFiles("RCV000001373.xml")), Stream.Null, sequenceProvider);
            var clinVarItems = reader.GetRcvItems().ToArray();
            Assert.Single(clinVarItems);

            var clinVarItem = clinVarItems[0];
            Assert.Equal("RCV000001373.3", clinVarItem.Id);

            var omimIds = clinVarItem.OmimIds;
            Assert.Single(omimIds);
            Assert.Equal("610206.0007", omimIds.First());
        }

        [Fact]
        public void RCV000435546_NotMissing()
        {
            var sequenceProvider = GetSequenceProvider(GenomeAssembly.GRCh37, 110221557, "CGCGG");
            var reader = new ClinVarReader(FileUtilities.GetReadStream(Resources.ClinvarXmlFiles("RCV000435546.xml")), Stream.Null, sequenceProvider);
            var clinVarItems = reader.GetRcvItems();
            Assert.True(clinVarItems.Any());
        }


        [Fact]
        public void MissingAltAllele()
        {
            var sequenceProvider = GetSequenceProvider(GenomeAssembly.GRCh37, 118165691, "C");
            var reader = new ClinVarReader(FileUtilities.GetReadStream(Resources.ClinvarXmlFiles("RCV000120902.xml")), Stream.Null, sequenceProvider);

            var items = reader.GetRcvItems();
            Assert.True(items.Any());

            foreach (var clinVarItem in items)
            {
                Assert.Equal("C", clinVarItem.RefAllele);
                Assert.Equal("G", clinVarItem.AltAllele);
            }
        }

        
        [Fact]
        public void NonEnglishChars()
        {
            var sequenceProvider = GetSequenceProvider(GenomeAssembly.GRCh37, 225592188, "TAGAAGA");
            var reader = new ClinVarReader(FileUtilities.GetReadStream(Resources.ClinvarXmlFiles("RCV000087262.xml")), Stream.Null, sequenceProvider);

            var items = reader.GetRcvItems();
            Assert.True(items.Any());

            foreach (var clinVarItem in items)
            {
                Assert.Equal("Pelger-Huët anomaly", clinVarItem.Phenotypes.First());
            }
        }

        [Fact]
        public void WrongPosition()
        {
            var sequenceProvider = GetSequenceProvider(GenomeAssembly.GRCh37, 112064826, "G");

            var reader = new ClinVarReader(FileUtilities.GetReadStream(Resources.ClinvarXmlFiles("RCV000073701.xml")), Stream.Null, sequenceProvider);

            var items = reader.GetRcvItems();
            Assert.True(items.Any());

            foreach (var clinVarItem in items)
            {
                switch (clinVarItem.Position)
                {
                    case 112064826:
                        Assert.Equal("G", clinVarItem.RefAllele);
                        Assert.Equal("C", clinVarItem.AltAllele);
                        break;
                    default:
                        throw new InvalidDataException($"Unexpected clinvar item start point : {clinVarItem.Position}");
                }
            }
        }

        [Fact]
        public void PubmedTest1()
        {
            var sequenceProvider = GetSequenceProvider(GenomeAssembly.GRCh37, 10183453, "AGCGCGCACGCAGCTCCGCCCC");

            var reader = new ClinVarReader(FileUtilities.GetReadStream(Resources.ClinvarXmlFiles("RCV000152657.xml")),Stream.Null, sequenceProvider);

            var items = reader.GetRcvItems();
            Assert.True(items.Any());

            foreach (var clinVarItem in items)
            {
                Assert.Equal( new List<long> { 12114475, 18836774, 22357542, 24033266 }, clinVarItem.PubmedIds);
            }
        }

        [Fact]
        public void PubmedTest2()
        {
            var sequenceProvider = GetSequenceProvider(GenomeAssembly.GRCh37, 5247992, "CAAAG");

            var reader = new ClinVarReader(FileUtilities.GetReadStream(Resources.ClinvarXmlFiles("RCV000016673.xml")), Stream.Null, sequenceProvider);

            var items = reader.GetRcvItems();
            Assert.True(items.Any());

            foreach (var clinVarItem in items)
            {
                Assert.Equal(new List<long> { 6826539, 9113933, 9845707, 12000828, 12383672 }, clinVarItem.PubmedIds);
            }
        }

        [Fact]
        public void PubmedTest3()
        {
            var sequenceProvider = GetSequenceProvider(GenomeAssembly.GRCh37, 55259485, "C");

            var reader = new ClinVarReader(FileUtilities.GetReadStream(Resources.ClinvarXmlFiles("RCV000038438.xml")), Stream.Null, sequenceProvider);

            var items = reader.GetRcvItems();
            Assert.True(items.Any());

            foreach (var clinVarItem in items)
            {
                Assert.Equal( new List<long> { 17285735, 17877814, 22848293, 24033266 }, clinVarItem.PubmedIds);
            }
        }

        [Fact]
        public void PubmedTest4()
        {
            var sequenceProvider = GetSequenceProvider(GenomeAssembly.GRCh37, 43609944, "GCTGT");

            var reader = new ClinVarReader(FileUtilities.GetReadStream(Resources.ClinvarXmlFiles("RCV000021819.xml")), Stream.Null, sequenceProvider);

            var items = reader.GetRcvItems();
            Assert.True(items.Any());

            foreach (var clinVarItem in items)
            {
                Assert.Equal( new List<long> { 7595167, 8099202, 8612479 }, clinVarItem.PubmedIds);
            }
        }

        [Fact]
        public void PubmedTest5()
        {
            var sequenceProvider = GetSequenceProvider(GenomeAssembly.GRCh37, 88907409, "A");

            var reader = new ClinVarReader(FileUtilities.GetReadStream(Resources.ClinvarXmlFiles("RCV000000734.xml")), Stream.Null, sequenceProvider);

            var items = reader.GetRcvItems();
            Assert.True(items.Any());

            foreach (var clinVarItem in items)
            {
                Assert.Equal(699, clinVarItem.VariationId);
                Assert.Null(clinVarItem.PubmedIds);
                Assert.Contains("\"variationId\":699", clinVarItem.GetJsonString());
            }
        }

        [Fact]
        public void PubmedTest6()
        {
            var sequenceProvider = GetSequenceProvider(GenomeAssembly.GRCh37, 118165691, "C");

            //extracting from SCV record
            var reader = new ClinVarReader(FileUtilities.GetReadStream(Resources.ClinvarXmlFiles("RCV000120902.xml")), Stream.Null, sequenceProvider);

            var items = reader.GetRcvItems();
            Assert.True(items.Any());

            foreach (var clinVarItem in items)
            {
                Assert.Equal(clinVarItem.PubmedIds, new List<long> { 24728327 });
            }
        }

        [Fact]
        public void PubmedTest7_comma_trimming()
        {
            var sequenceProvider = GetSequenceProvider(GenomeAssembly.GRCh37, 41258568, "A");

            //extracting from SCV record
            var reader = new ClinVarReader(FileUtilities.GetReadStream(Resources.ClinvarXmlFiles("RCV000167792.xml")), Stream.Null, sequenceProvider);

            var items = reader.GetRcvItems();
            Assert.True(items.Any());

            foreach (var clinVarItem in items)
            {
                Assert.Equal(clinVarItem.PubmedIds, new List<long> { 23239986, 28492532, 30472649 });
            }
        }

        [Fact]
        public void MultiScvPubmed()
        {
            var sequenceProvider = GetSequenceProvider(GenomeAssembly.GRCh37, 15589551, "AG");

            //extracting from SCV record
            var reader = new ClinVarReader(FileUtilities.GetReadStream(Resources.ClinvarXmlFiles("RCV000194003.xml")) , Stream.Null, sequenceProvider);

            var items = reader.GetRcvItems();
            Assert.True(items.Any());

            foreach (var clinVarItem in items)
            {
                Assert.Equal(clinVarItem.PubmedIds, new List<long> {25741868, 26092869});
            }
        }

        [Fact]
        public void NoClinVarItem_due_to_ref_mismatch()
        {
            var sequenceProvider =
                GetSequenceProvider(GenomeAssembly.GRCh37, 90982267, "A");

            var reader = new ClinVarReader(FileUtilities.GetReadStream(Resources.ClinvarXmlFiles("RCV000000101.xml")), Stream.Null, sequenceProvider);

            Assert.False(reader.GetRcvItems().Any());
        }

        [Fact]
        public void ClinVarForRef()
        {
            var sequenceProvider = GetSequenceProvider(GenomeAssembly.GRCh37, 31496350, "C");

            var reader = new ClinVarReader(FileUtilities.GetReadStream(Resources.ClinvarXmlFiles("RCV000124712.xml")), Stream.Null, sequenceProvider);

            var clinVarList = new List<ClinVarItem>();
            foreach (var clinVarItem in reader.GetRcvItems())
            {
                clinVarList.Add(clinVarItem);
                Assert.Equal(clinVarItem.RefAllele, clinVarItem.AltAllele);
            }

            Assert.Single(clinVarList);
        }

        [Fact]
        public void MultiplePhenotypes()
        {
            var sequenceProvider = GetSequenceProvider(GenomeAssembly.GRCh37, 172659738, "C");

            //no citations show up for this RCV in the website. But the XML has these pubmed ids under fields that we parse pubmed ids from
            var reader = new ClinVarReader(FileUtilities.GetReadStream(Resources.ClinvarXmlFiles("RCV000144179.xml")), Stream.Null, sequenceProvider);

            var items = reader.GetRcvItems();
            Assert.True(items.Any());

            foreach (var clinVarItem in items)
            {
                var expectedPhenotypes = new List<string> { "Single ventricle", "small Atrial septal defect" };
                Assert.Equal(expectedPhenotypes, clinVarItem.Phenotypes);
            }
        }

        [Fact]
        public void MultipleOrigins()
        {
            var sequenceProvider = GetSequenceProvider(GenomeAssembly.GRCh37, 18671566, "G");
            //no citations show up for this RCV in the website. But the XML has these pubmed ids under fields that we parse pubmed ids from
            var reader = new ClinVarReader(FileUtilities.GetReadStream(Resources.ClinvarXmlFiles("RCV000080071.xml")), Stream.Null, sequenceProvider);

            var items = reader.GetRcvItems();
            Assert.True(items.Any());

            foreach (var clinVarItem in items)
            {
                var expectedOrigins = new List<string> { "germline", "maternal", "unknown" };
                Assert.Equal(expectedOrigins, clinVarItem.AlleleOrigins);
            }
        }


        [Fact]
        public void SkipGeneralCitations()
        {
            var sequenceProvider = GetSequenceProvider(GenomeAssembly.GRCh37, 67705958, "G");
            //no citations show up for this RCV in the website. But the XML has these pubmed ids under fields that we parse pubmed ids from
            var reader = new ClinVarReader(FileUtilities.GetReadStream(Resources.ClinvarXmlFiles("RCV000003254.xml")), Stream.Null, sequenceProvider);

            foreach (var clinVarItem in reader.GetRcvItems())
            {
                Assert.Equal(clinVarItem.PubmedIds, new List<long>
                {
                    12023369,
                    17068223,
                    17447842,
                    17587057,
                    17786191,
                    17804789,
                    18438406,
                    19122664,
                    20228799
                });
            }
        }

        [Fact]
        public void IndelTest()
        {
            var sequenceProvider = GetSequenceProvider(GenomeAssembly.GRCh37, 187122303, "TCATACAGGTCATCGCT");

            var reader = new ClinVarReader(FileUtilities.GetReadStream(Resources.ClinvarXmlFiles("RCV000032548.xml")), Stream.Null, sequenceProvider);

            var items = reader.GetRcvItems();
            Assert.True(items.Any());

            foreach (var clinVarItem in items)
            {
                Assert.Equal("RCV000032548.8", clinVarItem.Id);

                switch (clinVarItem.Id)
                {
                    case "RCV000032548.8":
                        Assert.Equal("4", clinVarItem.Chromosome.EnsemblName);
                        Assert.Equal(187122303, clinVarItem.Position);
                        Assert.Equal(17, clinVarItem.RefAllele.Length);
                        Assert.Equal("GC", clinVarItem.AltAllele);
                        break;
                }
            }
        }


        [Fact]
        [Trait("jira", "NIR-2034")]
        public void MultiScvPubmeds()
        {
            var sequenceProvider = GetSequenceProvider(GenomeAssembly.GRCh37, 116411990, "C");

            //extracting from SCV record
            var reader = new ClinVarReader(FileUtilities.GetReadStream(Resources.ClinvarXmlFiles("RCV000203290.xml")), Stream.Null, sequenceProvider);

            var items = reader.GetRcvItems();
            Assert.True(items.Any());

            foreach (var clinVarItem in items)
            {
                Assert.Equal(clinVarItem.PubmedIds, new List<long> { 23806086, 24088041, 25736269 });
            }
        }

        [Fact]
        [Trait("jira", "NIR-2034")]
        public void MultipleAlleleOrigins()
        {
            var sequenceProvider = GetSequenceProvider(GenomeAssembly.GRCh37, 32890572, "G");

            var reader = new ClinVarReader(FileUtilities.GetReadStream(Resources.ClinvarXmlFiles("RCV000112977.xml")), Stream.Null, sequenceProvider);

            var items = reader.GetRcvItems();
            Assert.True(items.Any());

            foreach (var clinVarItem in items)
            {
                Assert.Equal(2, clinVarItem.AlleleOrigins.Count());
                Assert.NotEqual(clinVarItem.AlleleOrigins.First(), clinVarItem.AlleleOrigins.Last());

                foreach (var origin in clinVarItem.AlleleOrigins)
                {
                    Assert.True(origin == "unknown" || origin == "germline");
                }
            }
        }

        [Fact]
        [Trait("jira", "NIR-2748")]
        public void Discard_entries_with_unknown_variant_type()
        {
            var sequenceProvider = GetSequenceProvider(GenomeAssembly.GRCh37, 66765160, "CAG");

            var reader = new ClinVarReader(FileUtilities.GetReadStream(Resources.ClinvarXmlFiles("RCV000485802.xml")), Stream.Null, sequenceProvider);

            Assert.False(reader.GetRcvItems().Any());
        }

        [Fact]
        [Trait("jira", "NIR-2035")]
        public void EmptyRefAndAlt()
        {
            var sequenceProvider = GetSequenceProvider(GenomeAssembly.GRCh37, 31805881, "G");

            var reader = new ClinVarReader(FileUtilities.GetReadStream(Resources.ClinvarXmlFiles("RCV000083638.xml")), Stream.Null, sequenceProvider);

            Assert.Empty(reader.GetRcvItems());
        }

        [Fact]
        [Trait("jira", "NIR-2036")]
        public void SkipMicrosattelite()
        {
            var sequenceProvider = GetSequenceProvider(GenomeAssembly.GRCh37, 87637894, "CTG");

            var reader = new ClinVarReader(FileUtilities.GetReadStream(Resources.ClinvarXmlFiles("RCV000005426.xml")), Stream.Null, sequenceProvider);

            Assert.Empty(reader.GetRcvItems());
        }

        [Fact]
        public void SkipAlus()
        {
            var sequenceProvider = GetSequenceProvider(GenomeAssembly.GRCh37, 32893302, "TAAA");

            var reader = new ClinVarReader(FileUtilities.GetReadStream(Resources.ClinvarXmlFiles("RCV000724338.xml")), Stream.Null, sequenceProvider);

            Assert.Empty(reader.GetRcvItems());
        }

        [Fact]
        [Trait("jira", "NIR-2072")]
        public void MissingClinvarInsertion()
        {
            var sequenceProvider = GetSequenceProvider(GenomeAssembly.GRCh37, 2337967, "G");

            var reader = new ClinVarReader(FileUtilities.GetReadStream(Resources.ClinvarXmlFiles("RCV000179026.xml")), Stream.Null, sequenceProvider);

            var items = reader.GetRcvItems();
            Assert.True(items.Any());

            foreach (var clinVarItem in items)
            {
                Assert.Equal(2337968, clinVarItem.Position);
            }
        }

        [Fact]
        [Trait("jira", "NIR-2072")]
        public void MissingClinvarInsertionShift()
        {
            var sequenceProvider = GetSequenceProvider(GenomeAssembly.GRCh37, 3751645, "G");

            var reader = new ClinVarReader(FileUtilities.GetReadStream(Resources.ClinvarXmlFiles("RCV000207071.xml")), Stream.Null, sequenceProvider);

            var items = reader.GetRcvItems();
            Assert.True(items.Any());

            foreach (var clinVarItem in items)
            {
                Assert.Equal(3751646, clinVarItem.Position);
            }
        }

        [Fact]
        [Trait("jira", "NIR-2072")]
        public void MissingClinvarInsertionShift2()
        {
            var sequenceProvider = GetSequenceProvider(GenomeAssembly.GRCh37, 9324412, "C");
            var reader = new ClinVarReader(FileUtilities.GetReadStream(Resources.ClinvarXmlFiles("RCV000017510.xml")), Stream.Null, sequenceProvider);

            var items = reader.GetRcvItems();
            Assert.True(items.Any());

            foreach (var clinVarItem in items)
            {
                Assert.Equal(9324413, clinVarItem.Position);
            }
        }

        [Fact]
        [Trait("jira", "NIR-2045")]
        public void AlternatePhenotype()
        {
            var sequenceProvider = GetSequenceProvider(GenomeAssembly.GRCh37, 42018227, "GTC");
            var reader = new ClinVarReader(FileUtilities.GetReadStream(Resources.ClinvarXmlFiles("RCV000032707.xml")), Stream.Null, sequenceProvider);

            var items = reader.GetRcvItems();
            Assert.True(items.Any());

            foreach (var clinVarItem in items)
            {
                Assert.NotNull(clinVarItem.Phenotypes);
            }
        }

        [Fact]
        [Trait("jira", "NIR-2072")]
        public void IupacBases()
        {
            var sequenceProvider = GetSequenceProvider(GenomeAssembly.GRCh38, 32339320, "C");

            var reader = new ClinVarReader(FileUtilities.GetReadStream(Resources.ClinvarXmlFiles("RCV000113363.xml")), Stream.Null, sequenceProvider);

            var items = reader.GetRcvItems();
            Assert.True(items.Any());

            var altAlleles = new List<string>();
            foreach (var clinVarItem in items)
            {
                altAlleles.Add(clinVarItem.AltAllele);
                Assert.Equal(new[] {"pathogenic"}, clinVarItem.Significances);
            }
            
            Assert.Equal(2, altAlleles.Count);
        }

        [Fact]
        [Trait("jira", "NIR-2072")]
        public void OmitOmimFromAltPhenotypes()
        {
            var sequenceProvider = GetSequenceProvider(GenomeAssembly.GRCh37, 55529187, "G");
            var reader = new ClinVarReader(FileUtilities.GetReadStream(Resources.ClinvarXmlFiles("RCV000030349.xml")), Stream.Null, sequenceProvider);

            var items = reader.GetRcvItems();
            Assert.True(items.Any());

            foreach (var clinVarItem in items)
            {
                Assert.Single(clinVarItem.OmimIds);
            }
        }

        [Fact]
        [Trait("jira", "NIR-2072")]
        public void TrimSpaceFromOmimIds()
        {
            var sequenceProvider = GetSequenceProvider(GenomeAssembly.GRCh37, 129283520, "A");
            var reader = new ClinVarReader(FileUtilities.GetReadStream(Resources.ClinvarXmlFiles("RCV000373191.xml")), Stream.Null, sequenceProvider);

            var items = reader.GetRcvItems();
            Assert.True(items.Any());

            foreach (var clinVarItem in items)
            {
                Assert.Single(clinVarItem.OmimIds);
                Assert.Equal("609060", clinVarItem.OmimIds.FirstOrDefault());
            }
        }

        [Fact]
        [Trait("jira", "NIR-2099")]
        public void ClinvarInsertion()
        {
            var sequenceProvider = GetSequenceProvider(GenomeAssembly.GRCh37, 122318386, "A");

            var reader = new ClinVarReader(FileUtilities.GetReadStream(Resources.ClinvarXmlFiles("RCV000153339.xml")), Stream.Null, sequenceProvider);
            var items = reader.GetRcvItems();
            Assert.True(items.Any());

            foreach (var clinVarItem in items)
            {
                Assert.Equal(122318387, clinVarItem.Position);
            }
        }


        [Fact]
        public void Remove9DigitsPubmedId()
        {
            var sequenceProvider = GetSequenceProvider(GenomeAssembly.GRCh37, 534286, "C");

            var reader = new ClinVarReader(FileUtilities.GetReadStream(Resources.ClinvarXmlFiles("RCV000207504.xml")), Stream.Null, sequenceProvider);
            var items = reader.GetRcvItems();
            Assert.True(items.Any());

            foreach (var clinVarItem in items)
            {
                Assert.Equal(clinVarItem.PubmedIds, new List<long> { 16329078, 16372351, 19213030, 21438134, 25741868 });
            }
        }

        [Fact]
        public void CaptureGeneOmimId()
        {
            var sequenceProvider = GetSequenceProvider(GenomeAssembly.GRCh37, 3494833, "A");

            var reader = new ClinVarReader(FileUtilities.GetReadStream(Resources.ClinvarXmlFiles("RCV000235027.xml")), Stream.Null, sequenceProvider);
            var items = reader.GetRcvItems();
            Assert.True(items.Any());

            foreach (var clinVarItem in items)
            {
                Assert.Equal(clinVarItem.OmimIds, new List<string> { "601462", "610285.0001" });
            }
        }

        [Fact]
        public void CapturePhenotypicSeriesOmimIDandUniq()
        {
            var sequenceProvider = GetSequenceProvider(GenomeAssembly.GRCh37, 122746325, "A");

            var reader = new ClinVarReader(FileUtilities.GetReadStream(Resources.ClinvarXmlFiles("RCV000401212.xml")), Stream.Null, sequenceProvider);
            var items = reader.GetRcvItems();
            Assert.True(items.Any());

            foreach (var clinVarItem in items)
            {
                Assert.Equal(clinVarItem.OmimIds, new List<string> { "209900" });
            }
        }

        [Fact]
        public void CapturePhenotypeSeriesOmimId()
        {
            var sequenceProvider = GetSequenceProvider(GenomeAssembly.GRCh37, 15513004, "GGAA");

            var reader = new ClinVarReader(FileUtilities.GetReadStream(Resources.ClinvarXmlFiles("RCV000406351.xml")), Stream.Null, sequenceProvider);
            var items = reader.GetRcvItems();
            Assert.True(items.Any());

            foreach (var clinVarItem in items)
            {
                Assert.Equal(clinVarItem.OmimIds, new List<string> { "213300" });
            }
        }

        [Fact]
        public void RemoveDuplicationWithWrongRefSequence()
        {
            var sequenceProvider = GetSequenceProvider(GenomeAssembly.GRCh37, 10183702, "GCGGCCGCGGCCCG");

            var reader = new ClinVarReader(FileUtilities.GetReadStream(Resources.ClinvarXmlFiles("RCV000267121.xml")), Stream.Null, sequenceProvider);
            Assert.False(reader.GetRcvItems().Any());
        }

        [Fact]
        [Trait("jira", "NIR-2372")]
        public void AllelicOmimIdsForSnvs()
        {
            var sequenceProvider = GetSequenceProvider(GenomeAssembly.GRCh37, 111329354, "G");

            var reader = new ClinVarReader(FileUtilities.GetReadStream(Resources.ClinvarXmlFiles("RCV000170338.xml")), Stream.Null, sequenceProvider);

            var clinvarItems = reader.GetRcvItems().ToList();
            Assert.Single(clinvarItems);

            var clinvarItem = clinvarItems[0];
            Assert.Single(clinvarItem.OmimIds);
            Assert.Equal("612800.0003", clinvarItem.OmimIds.First());
        }

        [Fact]
        [Trait("jira", "NIR-2372")]
        public void AllelicOmimIdsForDeletions()
        {
            var sequenceProvider = GetSequenceProvider(GenomeAssembly.GRCh37, 111335401, "GCTC");

            var reader = new ClinVarReader(FileUtilities.GetReadStream(Resources.ClinvarXmlFiles("RCV000170338.xml")), Stream.Null, sequenceProvider);

            var clinvarItems = reader.GetRcvItems().ToList();
            Assert.Single(clinvarItems);

            var clinvarItem = clinvarItems[0];
            Assert.Single(clinvarItem.OmimIds);
            Assert.Equal("612800.0002", clinvarItem.OmimIds.First());
        }

        [Fact]
        [Trait("jira", "NIR-2372")]
        public void ExcludeAllelicOmimIdsFromTraits()
        {
            var sequenceProvider = GetSequenceProvider(GenomeAssembly.GRCh37, 100887648, "AGAT");

            var reader = new ClinVarReader(FileUtilities.GetReadStream(Resources.ClinvarXmlFiles("RCV000050055.xml")), Stream.Null, sequenceProvider);

            var clinvarItems = reader.GetRcvItems().ToList();
            Assert.Single(clinvarItems);

            var clinvarItem = clinvarItems[0];
            Assert.Single(clinvarItem.OmimIds);
            Assert.Equal("216550", clinvarItem.OmimIds.First());
        }

        [Fact]
        [Trait("jira", "NIR-2372")]
        public void AllelicOmimIdsFromAttributeSetChrX()
        {
            var sequenceProvider = GetSequenceProvider(GenomeAssembly.GRCh37, 595469, "C");

            var reader = new ClinVarReader(FileUtilities.GetReadStream(Resources.ClinvarXmlFiles("RCV000010551.xml")), Stream.Null, sequenceProvider);

            var clinvarItems = reader.GetRcvItems().ToList();

            Assert.Single(clinvarItems);

            foreach (var clinVarItem in clinvarItems)
            {
                Assert.Equal(2, clinVarItem.OmimIds.Count());
            }
        }

        [Fact]
        [Trait("jira", "NIR-2372")]
        public void AllelicOmimIdsFromAttributeSetChrY()
        {
            var sequenceProvider = GetSequenceProvider(GenomeAssembly.GRCh37, 545469, "C");

            var reader = new ClinVarReader(FileUtilities.GetReadStream(Resources.ClinvarXmlFiles("RCV000010551.xml")), Stream.Null, sequenceProvider);

            var clinvarItems = reader.GetRcvItems().ToList();

            Assert.Single(clinvarItems);

            foreach (var clinVarItem in clinvarItems)
            {
                Assert.Equal(2, clinVarItem.OmimIds.Count());
            }
        }

        [Fact]
        [Trait("jira", "NIR-2372")]
        public void MultipleEntryRecordVariant1()
        {
            var sequenceProvider = GetSequenceProvider(GenomeAssembly.GRCh37, 8045031, "G");

            var reader = new ClinVarReader(FileUtilities.GetReadStream(Resources.ClinvarXmlFiles("RCV000007484.xml")), Stream.Null, sequenceProvider);

            var clinvarItems = reader.GetRcvItems().ToList();

            Assert.Single(clinvarItems);
        }

        [Fact]
        [Trait("jira", "NIR-2372")]
        public void MultipleEntryRecordVariant2()
        {
            var sequenceProvider = GetSequenceProvider(GenomeAssembly.GRCh37, 8021910, "G");

            var reader = new ClinVarReader(FileUtilities.GetReadStream(Resources.ClinvarXmlFiles("RCV000007484.xml")), Stream.Null, sequenceProvider);

            var clinvarItems = reader.GetRcvItems().ToList();

            Assert.Single(clinvarItems);
        }

        [Fact]
        [Trait("jira", "NIR-2372")]
        public void SkipMicrosatellitesWithoutAltAllele()
        {
            var sequenceProvider = GetSequenceProvider(GenomeAssembly.GRCh37, 46191240, "ATTCT");

            var reader = new ClinVarReader(FileUtilities.GetReadStream(Resources.ClinvarXmlFiles("RCV000001054.xml")), Stream.Null, sequenceProvider);

            Assert.False(reader.GetRcvItems().Any());
        }

        [Fact]
        [Trait("jira", "NIR-2029")]
        public void MissingClinvarInsertion2()
        {
            var sequenceProvider = GetSequenceProvider(GenomeAssembly.GRCh38, 132903738, "A");
            var reader = new ClinVarReader(FileUtilities.GetReadStream(Resources.ClinvarXmlFiles("RCV000342164.xml")), Stream.Null, sequenceProvider);

            var clinvarItems = reader.GetRcvItems().ToList();
            Assert.Single(clinvarItems);
        }

        [Fact]
        public void Skip_entries_with_inconsistant_start_end()
        {
            var sequenceProvider = GetSequenceProvider(GenomeAssembly.GRCh37, 132903739, "AAACGCTCATAGAGTAACTGGTTGTGCAGTAAAAGCAACTGGTCTCAAACGCTCATAGAGTAACTGGTTGTGCAGTAAAAGCAACTGGTCTC");
            var reader = new ClinVarReader(FileUtilities.GetReadStream(Resources.ClinvarXmlFiles("RCV000342164.xml")), Stream.Null, sequenceProvider);

            Assert.False(reader.GetRcvItems().Any());
        }

        [Fact]
        public void Alternate_phenotypes()
        {
            var sequenceProvider = GetSequenceProvider(GenomeAssembly.GRCh37, 204732740, "G");
            var reader = new ClinVarReader(FileUtilities.GetReadStream(Resources.ClinvarXmlFiles("RCV000537563.xml")), Stream.Null, sequenceProvider);

            var clinvarItems = reader.GetRcvItems().ToList();

            Assert.Single(clinvarItems[0].Phenotypes);
        }

        [Fact]
        public void Mising_entry()
        {
            var sequenceProvider = GetSequenceProvider(GenomeAssembly.GRCh37, 36888396, "C");
            var reader = new ClinVarReader(FileUtilities.GetReadStream(Resources.ClinvarXmlFiles("RCV000171474.xml")), Stream.Null, sequenceProvider);

            var clinvarItems = reader.GetRcvItems().ToList();

            Assert.Equal("",clinvarItems[0].RefAllele);
        }

        [Fact]
        public void Multiple_significance()
        {
            var sequenceProvider = GetSequenceProvider(GenomeAssembly.GRCh38, 72349076, "T");
            var reader = new ClinVarReader(FileUtilities.GetReadStream(Resources.ClinvarXmlFiles("RCV000169296.xml")), Stream.Null, sequenceProvider);

            var clinvarItems = reader.GetRcvItems().ToList();

            Assert.Equal(new[]{ "pathogenic", "likely pathogenic" }, clinvarItems[0].Significances);
        }

        [Fact]
        public void Multiple_significance_from_explanation()
        {
            var sequenceProvider = GetSequenceProvider(GenomeAssembly.GRCh38, 12665750, "T");
            var reader = new ClinVarReader(FileUtilities.GetReadStream(Resources.ClinvarXmlFiles("RCV000001752.xml")), Stream.Null, sequenceProvider);

            var clinvarItems = reader.GetRcvItems().ToList();

            Assert.Equal(new[] { "pathogenic", "uncertain significance" }, clinvarItems[0].Significances);
        }

        [Fact]
        public void Override_microsatellite_type()
        {
            var sequenceProvider = GetSequenceProvider(GenomeAssembly.GRCh37, 88929173, "CGAG");
            var reader = new ClinVarReader(FileUtilities.GetReadStream(Resources.ClinvarXmlFiles("RCV000205418.xml")), Stream.Null, sequenceProvider);

            var clinvarItems = reader.GetRcvItems();

            Assert.Single(clinvarItems);
        }

        [Fact]
        public void OneRcv_oneVcv()
        {
            var sequenceProvider = GetSequenceProvider(GenomeAssembly.GRCh37, 31496350, "C");

            var reader = new ClinVarReader(FileUtilities.GetReadStream(Resources.ClinvarXmlFiles("RCV000124712.xml")),
                FileUtilities.GetReadStream(Resources.VcvXmlFiles("VCV000137106.xml")), sequenceProvider);
            
            var items = reader.GetItems().ToArray();
            Assert.Equal(2,items.Length);
        }
    }
}
