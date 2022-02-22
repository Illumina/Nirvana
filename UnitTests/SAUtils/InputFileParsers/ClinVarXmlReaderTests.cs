using System.Collections.Generic;
using System.IO;
using System.Linq;
using Genome;
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
            var reader = new ClinVarXmlReader(Resources.ClinvarXmlFiles("RCV000077146.xml"), sequenceProvider);

            Assert.True(reader.GetItems().Any());

            var clinVarItem = reader.GetItems().First();
            Assert.Equal("RCV000077146.3", clinVarItem.Id);
            Assert.Equal("17", clinVarItem.Chromosome.EnsemblName);
            Assert.Equal(41234419, clinVarItem.Position);
            Assert.Equal("A", clinVarItem.RefAllele);
            Assert.Equal("C", clinVarItem.AltAllele);
            Assert.Equal("2019-01-25", clinVarItem.LastUpdateDate);
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
            var reader = new ClinVarXmlReader(Resources.ClinvarXmlFiles("RCV000001373.xml"), sequenceProvider);
            var clinVarItems = reader.GetItems().ToArray();
            Assert.Single(clinVarItems);

            var clinVarItem = clinVarItems[0];
            Assert.Equal("RCV000001373.3", clinVarItem.Id);

            var omimIds = clinVarItem.OmimIds;
            Assert.Single(omimIds);
            Assert.Equal("610206.0007", omimIds[0]);
        }

        [Fact]
        public void RCV000435546_NotMissing()
        {
            var sequenceProvider = GetSequenceProvider(GenomeAssembly.GRCh37, 110221557, "CGCGG");
            var reader = new ClinVarXmlReader(Resources.ClinvarXmlFiles("RCV000435546.xml"), sequenceProvider);
            var clinVarItems = reader.GetItems();
            Assert.True(clinVarItems.Any());
        }


        [Fact]
        public void MissingAltAllele()
        {
            var sequenceProvider = GetSequenceProvider(GenomeAssembly.GRCh37, 118165691, "C");
            var reader = new ClinVarXmlReader(Resources.ClinvarXmlFiles("RCV000120902.xml"), sequenceProvider);

            Assert.True(reader.GetItems().Any());

            foreach (var clinVarItem in reader.GetItems())
            {
                Assert.Equal("C", clinVarItem.RefAllele);
                Assert.Equal("G", clinVarItem.AltAllele);
            }
        }

        //[Fact(Skip = "need different compressed sequence")]
        //[Fact]
        //public void MultiEntryXmlParsing()
        //{
        //    var mockProvider = new Mock<ISequenceProvider>();
        //    mockProvider.Setup(x => x.RefNameToChromosome).Returns(_refNameToChromosome);

        //    var sequenceProvider = mockProvider.Object;

        //    var reader = new ClinVarXmlReader(Resources.ClinvarXmlFiles("MultiClinvar.xml"), sequenceProvider);

        //    var clinvarList = new List<ClinVarItem>();
        //    foreach (var clinVarItem in reader.GetItems())
        //    {
        //        switch (clinVarItem.Id)
        //        {
        //            case "RCV000000064.5":
        //                Assert.Equal(ClinVarXmlReader.ParseDate("2016-02-17"), clinVarItem.LastUpdatedDate);
        //                Assert.Equal("risk factor", clinVarItem.Significance);
        //                break;
        //            case "RCV000000068.3":
        //                Assert.Equal(ClinVarXmlReader.ParseDate("2016-02-17"), clinVarItem.LastUpdatedDate);
        //                Assert.Equal("pathogenic", clinVarItem.Significance);
        //                Assert.Equal("C3150419", clinVarItem.MedGenIDs.First());
        //                break;
        //            case "RCV000000069.3":
        //                Assert.Equal(ClinVarXmlReader.ParseDate("2016-02-17"), clinVarItem.LastUpdatedDate);
        //                Assert.Equal("pathogenic", clinVarItem.Significance);
        //                Assert.Equal("C3150419", clinVarItem.MedGenIDs.First());
        //                Assert.Equal(20179356, clinVarItem.PubmedIds.First());
        //                break;
        //            default:
        //                throw new InvalidDataException("Unexpected clinvar id encountered");
        //        }
        //        clinvarList.Add(clinVarItem);
        //    }

        //    clinvarList.Sort();
        //    Assert.Equal(2, clinvarList.Count);
        //    Assert.Equal("2", clinvarList[0].Chromosome.EnsemblName);
        //    Assert.Equal("22", clinvarList[1].Chromosome.EnsemblName);
        //}

        //[Fact(Skip = "new SA")]
        //public void MultiVariantEntry()
        //{
        //    //var reader = new ClinVarXmlReader(Resources.ClinvarXmlFiles("RCV000007484.xml"),sequenceProvider);

        //    //foreach (var clinVarItem in reader.GetItems())
        //    //{
        //    //    switch (clinVarItem.Position)
        //    //    {
        //    //        case 8045031:
        //    //            Assert.Equal("G", clinVarItem.ReferenceAllele);
        //    //            Assert.Equal("A", clinVarItem.AltAllele);
        //    //            break;
        //    //        case 8021911:
        //    //            Assert.Equal("GTGCTGGACGGTGTCCCT", clinVarItem.AltAllele);
        //    //            var sa = new SupplementaryAnnotationPosition(clinVarItem.Position);
        //    //            var saCreator = new SupplementaryPositionCreator(sa);

        //    //            clinVarItem.SetSupplementaryAnnotations(saCreator);
        //    //            Assert.Equal("iGTGCTGGACGGTGTCCCT", clinVarItem.SaAltAllele);
        //    //            break;
        //    //        default:
        //    //            throw new InvalidDataException($"Unexpected clinvar item start point : {clinVarItem.Position}");
        //    //    }
        //    //}
        //}

        [Fact]
        public void NonEnglishChars()
        {
            var sequenceProvider = GetSequenceProvider(GenomeAssembly.GRCh37, 225592188, "TAGAAGA");
            var reader = new ClinVarXmlReader(Resources.ClinvarXmlFiles("RCV000087262.xml"), sequenceProvider);

            Assert.True(reader.GetItems().Any());
            foreach (var clinVarItem in reader.GetItems())
            {
                Assert.Equal("Pelger-Huët anomaly", clinVarItem.Phenotypes.First());
            }
        }

        [Fact]
        public void WrongPosition()
        {
            var sequenceProvider = GetSequenceProvider(GenomeAssembly.GRCh37, 112064826, "G");

            var reader = new ClinVarXmlReader(Resources.ClinvarXmlFiles("RCV000073701.xml"), sequenceProvider);

            Assert.True(reader.GetItems().Any());

            foreach (var clinVarItem in reader.GetItems())
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
            var sequenceProvider = GetSequenceProvider(GenomeAssembly.GRCh37, 10183457, "CGCACGCAGCTCCGCCCCGCG");

            var reader = new ClinVarXmlReader(Resources.ClinvarXmlFiles("RCV000152657.xml"), sequenceProvider);

            Assert.True(reader.GetItems().Any());

            foreach (var clinVarItem in reader.GetItems())
            {
                Assert.Equal( new List<long> { 12114475, 18836774, 22357542, 24033266 }, clinVarItem.PubMedIds.Select(long.Parse));
            }
        }

        [Fact]
        public void PubmedTest2()
        {
            var sequenceProvider = GetSequenceProvider(GenomeAssembly.GRCh37, 5247993, "AAAG");

            var reader = new ClinVarXmlReader(Resources.ClinvarXmlFiles("RCV000016673.xml"), sequenceProvider);

            Assert.True(reader.GetItems().Any());
            foreach (var clinVarItem in reader.GetItems())
            {
                Assert.Equal(new List<long> { 6826539, 9113933, 9845707, 12000828, 12383672 }, clinVarItem.PubMedIds.Select(long.Parse));
            }
        }

        [Fact]
        public void PubmedTest3()
        {
            var sequenceProvider = GetSequenceProvider(GenomeAssembly.GRCh37, 55259485, "C");

            var reader = new ClinVarXmlReader(Resources.ClinvarXmlFiles("RCV000038438.xml"), sequenceProvider);

            Assert.True(reader.GetItems().Any());
            foreach (var clinVarItem in reader.GetItems())
            {
                Assert.Equal( new List<long> { 17285735, 17877814, 22848293, 24033266 }, clinVarItem.PubMedIds.Select(long.Parse));
            }
        }

        [Fact]
        public void PubmedTest4()
        {
            var sequenceProvider = GetSequenceProvider(GenomeAssembly.GRCh37, 43609944, "GCTGT");

            var reader = new ClinVarXmlReader(Resources.ClinvarXmlFiles("RCV000021819.xml"), sequenceProvider);

            Assert.True(reader.GetItems().Any());
            foreach (var clinVarItem in reader.GetItems())
            {
                Assert.Equal( new List<long> { 7595167, 8099202, 8612479 }, clinVarItem.PubMedIds.Select(long.Parse));
            }
        }

        [Fact]
        public void PubmedTest5()
        {
            var sequenceProvider = GetSequenceProvider(GenomeAssembly.GRCh37, 88907409, "A");

            var reader = new ClinVarXmlReader(Resources.ClinvarXmlFiles("RCV000000734.xml"), sequenceProvider);

            Assert.True(reader.GetItems().Any());
            foreach (var clinVarItem in reader.GetItems())
            {
                Assert.Null(clinVarItem.PubMedIds);
            }
        }

        [Fact]
        public void PubmedTest6()
        {
            var sequenceProvider = GetSequenceProvider(GenomeAssembly.GRCh37, 118165691, "C");

            //extracting from SCV record
            var reader = new ClinVarXmlReader(Resources.ClinvarXmlFiles("RCV000120902.xml"), sequenceProvider);

            Assert.True(reader.GetItems().Any());
            foreach (var clinVarItem in reader.GetItems())
            {
                Assert.Equal(clinVarItem.PubMedIds.Select(long.Parse), new List<long> { 24728327 });
            }
        }

        [Fact]
        public void MultiScvPubmed()
        {
            var sequenceProvider =
                GetSequenceProvider(GenomeAssembly.GRCh37, 15589553, "G");

            //extracting from SCV record
            var reader = new ClinVarXmlReader(Resources.ClinvarXmlFiles("RCV000194003.xml"), sequenceProvider);

            Assert.True(reader.GetItems().Any());

            foreach (var clinVarItem in reader.GetItems())
            {
                Assert.Equal(clinVarItem.PubMedIds.Select(long.Parse), new List<long> {25741868, 26092869});
            }
        }

        [Fact]
        public void NoClinVarItem_due_to_ref_mismatch()
        {
            var sequenceProvider =
                GetSequenceProvider(GenomeAssembly.GRCh37, 90982267, "A");

            var reader = new ClinVarXmlReader(Resources.ClinvarXmlFiles("RCV000000101.xml"), sequenceProvider);

            Assert.False(reader.GetItems().Any());
        }

        [Fact]
        public void ClinVarForRef()
        {
            var sequenceProvider = GetSequenceProvider(GenomeAssembly.GRCh37, 31496350, "C");

            var reader = new ClinVarXmlReader(Resources.ClinvarXmlFiles("RCV000124712.xml"), sequenceProvider);

            var clinVarList = new List<ClinVarItem>();
            foreach (var clinVarItem in reader.GetItems())
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
            var reader = new ClinVarXmlReader(Resources.ClinvarXmlFiles("RCV000144179.xml"), sequenceProvider);

            Assert.True(reader.GetItems().Any());

            foreach (var clinVarItem in reader.GetItems())
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
            var reader = new ClinVarXmlReader(Resources.ClinvarXmlFiles("RCV000080071.xml"), sequenceProvider);

            Assert.True(reader.GetItems().Any());
            foreach (var clinVarItem in reader.GetItems())
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
            var reader = new ClinVarXmlReader(Resources.ClinvarXmlFiles("RCV000003254.xml"), sequenceProvider);

            foreach (var clinVarItem in reader.GetItems())
            {
                Assert.Equal(clinVarItem.PubMedIds.Select(long.Parse), new List<long>
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
            var sequenceProvider = GetSequenceProvider(GenomeAssembly.GRCh37, 187122303, "ACGTACGTACGTACGTA");

            var reader = new ClinVarXmlReader(Resources.ClinvarXmlFiles("RCV000032548.xml"), sequenceProvider);

            Assert.True(reader.GetItems().Any());

            foreach (var clinVarItem in reader.GetItems())
            {
                Assert.Equal("RCV000032548.8", clinVarItem.Id);

                switch (clinVarItem.Id)
                {
                    case "RCV000032548.8":
                        Assert.Equal("4", clinVarItem.Chromosome.EnsemblName);
                        Assert.Equal(187122303, clinVarItem.Position);
                        Assert.Equal(17, clinVarItem.RefAllele.Length);
                        Assert.Equal("GC", clinVarItem.AltAllele);
                        Assert.Equal("2018-12-28", clinVarItem.LastUpdateDate);
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
            var reader = new ClinVarXmlReader(Resources.ClinvarXmlFiles("RCV000203290.xml"), sequenceProvider);

            Assert.True(reader.GetItems().Any());

            foreach (var clinVarItem in reader.GetItems())
            {
                Assert.Equal(clinVarItem.PubMedIds.Select(long.Parse), new List<long> { 23806086, 24088041, 25736269 });
            }
        }

        [Fact]
        [Trait("jira", "NIR-2034")]
        public void MultipleAlleleOrigins()
        {
            var sequenceProvider = GetSequenceProvider(GenomeAssembly.GRCh37, 32890572, "G");

            var reader = new ClinVarXmlReader(Resources.ClinvarXmlFiles("RCV000112977.xml"), sequenceProvider);

            Assert.True(reader.GetItems().Any());

            foreach (var clinVarItem in reader.GetItems())
            {
                Assert.Equal(2, clinVarItem.AlleleOrigins.Length);
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

            var reader = new ClinVarXmlReader(Resources.ClinvarXmlFiles("RCV000485802.xml"), sequenceProvider);

            Assert.False(reader.GetItems().Any());
        }

        [Fact]
        [Trait("jira", "NIR-2035")]
        public void EmptyRefAndAlt()
        {
            var sequenceProvider = GetSequenceProvider(GenomeAssembly.GRCh37, 31805881, "G");

            var reader = new ClinVarXmlReader(Resources.ClinvarXmlFiles("RCV000083638.xml"), sequenceProvider);

            Assert.False(reader.GetItems().Any());
        }

        [Fact]
        [Trait("jira", "NIR-2036")]
        public void SkipMicrosattelite()
        {
            var sequenceProvider = GetSequenceProvider(GenomeAssembly.GRCh37, 87637894, "CTG");

            var reader = new ClinVarXmlReader(Resources.ClinvarXmlFiles("RCV000005426.xml"), sequenceProvider);

            Assert.False(reader.GetItems().Any());
        }

        [Fact]
        [Trait("jira", "NIR-2072")]
        public void MissingClinvarInsertion()
        {
            var sequenceProvider = GetSequenceProvider(GenomeAssembly.GRCh37, 2337967, "G");

            var reader = new ClinVarXmlReader(Resources.ClinvarXmlFiles("RCV000179026.xml"), sequenceProvider);

            Assert.True(reader.GetItems().Any());
            foreach (var clinVarItem in reader.GetItems())
            {
                Assert.Equal(2337968, clinVarItem.Position);
            }
        }

        [Fact]
        [Trait("jira", "NIR-2072")]
        public void MissingClinvarInsertionShift()
        {
            var sequenceProvider = GetSequenceProvider(GenomeAssembly.GRCh37, 3751645, "G");

            var reader = new ClinVarXmlReader(Resources.ClinvarXmlFiles("RCV000207071.xml"), sequenceProvider);

            Assert.True(reader.GetItems().Any());

            foreach (var clinVarItem in reader.GetItems())
            {
                Assert.Equal(3751646, clinVarItem.Position);
            }
        }

        [Fact]
        [Trait("jira", "NIR-2072")]
        public void MissingClinvarInsertionShift2()
        {
            var sequenceProvider = GetSequenceProvider(GenomeAssembly.GRCh37, 9324412, "C");
            var reader = new ClinVarXmlReader(Resources.ClinvarXmlFiles("RCV000017510.xml"), sequenceProvider);

            Assert.True(reader.GetItems().Any());

            foreach (var clinVarItem in reader.GetItems())
            {
                Assert.Equal(9324413, clinVarItem.Position);
            }
        }

        [Fact]
        [Trait("jira", "NIR-2045")]
        public void AlternatePhenotype()
        {
            var sequenceProvider = GetSequenceProvider(GenomeAssembly.GRCh37, 42018228, "TC");
            var reader = new ClinVarXmlReader(Resources.ClinvarXmlFiles("RCV000032707.xml"), sequenceProvider);

            Assert.True(reader.GetItems().Any());

            foreach (var clinVarItem in reader.GetItems())
            {
                Assert.NotNull(clinVarItem.Phenotypes);
            }
        }

        [Fact]
        [Trait("jira", "NIR-2072")]
        public void IupacBases()
        {
            var sequenceProvider = GetSequenceProvider(GenomeAssembly.GRCh38, 32339320, "C");

            var reader = new ClinVarXmlReader(Resources.ClinvarXmlFiles("RCV000113363.xml"), sequenceProvider);

            Assert.True(reader.GetItems().Any());
            var altAlleles = new List<string>();
            foreach (var clinVarItem in reader.GetItems())
            {
                altAlleles.Add(clinVarItem.AltAllele);
                Assert.Equal(new[] {"pathogenic"}, clinVarItem.Significance);
            }
            
            Assert.Equal(2, altAlleles.Count);
        }

        [Fact]
        [Trait("jira", "NIR-2072")]
        public void OmitOmimFromAltPhenotypes()
        {
            var sequenceProvider = GetSequenceProvider(GenomeAssembly.GRCh37, 55529187, "G");
            var reader = new ClinVarXmlReader(Resources.ClinvarXmlFiles("RCV000030349.xml"), sequenceProvider);

            Assert.True(reader.GetItems().Any());

            foreach (var clinVarItem in reader.GetItems())
            {
                Assert.Single(clinVarItem.OmimIds);
            }
        }

        [Fact]
        [Trait("jira", "NIR-2072")]
        public void TrimSpaceFromOmimIds()
        {
            var sequenceProvider = GetSequenceProvider(GenomeAssembly.GRCh37, 129283520, "A");
            var reader = new ClinVarXmlReader(Resources.ClinvarXmlFiles("RCV000373191.xml"), sequenceProvider);

            Assert.True(reader.GetItems().Any());

            foreach (var clinVarItem in reader.GetItems())
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

            var reader = new ClinVarXmlReader(Resources.ClinvarXmlFiles("RCV000153339.xml"), sequenceProvider);
            Assert.True(reader.GetItems().Any());
            foreach (var clinVarItem in reader.GetItems())
            {
                Assert.Equal(122318387, clinVarItem.Position);
            }
        }


        [Fact]
        public void Remove9DigitsPubmedId()
        {
            var sequenceProvider = GetSequenceProvider(GenomeAssembly.GRCh37, 534286, "C");

            var reader = new ClinVarXmlReader(Resources.ClinvarXmlFiles("RCV000207504.xml"), sequenceProvider);
            Assert.True(reader.GetItems().Any());

            foreach (var clinVarItem in reader.GetItems())
            {
                Assert.Equal(clinVarItem.PubMedIds.Select(long.Parse), new List<long> { 16329078, 16372351, 19213030, 21438134, 25741868 });
            }
        }

        [Fact]
        public void CaptureGeneOmimId()
        {
            var sequenceProvider = GetSequenceProvider(GenomeAssembly.GRCh37, 3494837, "TGCC");

            var reader = new ClinVarXmlReader(Resources.ClinvarXmlFiles("RCV000235027.xml"), sequenceProvider);
            Assert.True(reader.GetItems().Any());

            foreach (var clinVarItem in reader.GetItems())
            {
                Assert.Equal(clinVarItem.OmimIds, new List<string> { "601462", "610285.0001" });
            }
        }

        [Fact]
        public void CapturePhenotypicSeriesOmimIDandUniq()
        {
            var sequenceProvider = GetSequenceProvider(GenomeAssembly.GRCh37, 122746325, "A");

            var reader = new ClinVarXmlReader(Resources.ClinvarXmlFiles("RCV000401212.xml"), sequenceProvider);
            Assert.True(reader.GetItems().Any());

            foreach (var clinVarItem in reader.GetItems())
            {
                Assert.Equal(clinVarItem.OmimIds, new List<string> { "209900" });
            }
        }

        [Fact]
        public void CapturePhenotypeSeriesOmimId()
        {
            var sequenceProvider = GetSequenceProvider(GenomeAssembly.GRCh37, 15513014, "GAA");

            var reader = new ClinVarXmlReader(Resources.ClinvarXmlFiles("RCV000406351.xml"), sequenceProvider);
            Assert.True(reader.GetItems().Any());

            foreach (var clinVarItem in reader.GetItems())
            {
                Assert.Equal(clinVarItem.OmimIds, new List<string> { "213300" });
            }
        }

        [Fact]
        public void RemoveDuplicationWithWrongRefSequence()
        {
            var sequenceProvider = GetSequenceProvider(GenomeAssembly.GRCh37, 10183702, "GCGGCCGCGGCCCG");

            var reader = new ClinVarXmlReader(Resources.ClinvarXmlFiles("RCV000267121.xml"), sequenceProvider);
            Assert.False(reader.GetItems().Any());
        }

        [Fact]
        [Trait("jira", "NIR-2372")]
        public void AllelicOmimIdsForSnvs()
        {
            var sequenceProvider = GetSequenceProvider(GenomeAssembly.GRCh37, 111329354, "G");

            var reader = new ClinVarXmlReader(Resources.ClinvarXmlFiles("RCV000170338.xml"), sequenceProvider);

            var clinvarItems = reader.GetItems().ToList();
            Assert.Single(clinvarItems);

            var clinvarItem = clinvarItems[0];
            Assert.Single(clinvarItem.OmimIds);
            Assert.Equal("612800.0003", clinvarItem.OmimIds.First());
        }

        [Fact]
        [Trait("jira", "NIR-2372")]
        public void AllelicOmimIdsForDeletions()
        {
            var sequenceProvider = GetSequenceProvider(GenomeAssembly.GRCh37, 111335402, "CTC");

            var reader = new ClinVarXmlReader(Resources.ClinvarXmlFiles("RCV000170338.xml"), sequenceProvider);

            var clinvarItems = reader.GetItems().ToList();
            Assert.Single(clinvarItems);

            var clinvarItem = clinvarItems[0];
            Assert.Single(clinvarItem.OmimIds);
            Assert.Equal("612800.0002", clinvarItem.OmimIds.First());
        }

        [Fact]
        [Trait("jira", "NIR-2372")]
        public void ExcludeAllelicOmimIdsFromTraits()
        {
            var sequenceProvider = GetSequenceProvider(GenomeAssembly.GRCh37, 100887650, "ATG");

            var reader = new ClinVarXmlReader(Resources.ClinvarXmlFiles("RCV000050055.xml"), sequenceProvider);

            var clinvarItems = reader.GetItems().ToList();
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

            var reader = new ClinVarXmlReader(Resources.ClinvarXmlFiles("RCV000010551.xml"), sequenceProvider);

            var clinvarItems = reader.GetItems().ToList();

            Assert.Single(clinvarItems);

            foreach (var clinVarItem in clinvarItems)
            {
                Assert.Equal(2, clinVarItem.OmimIds.Length);
            }
        }

        [Fact]
        [Trait("jira", "NIR-2372")]
        public void AllelicOmimIdsFromAttributeSetChrY()
        {
            var sequenceProvider = GetSequenceProvider(GenomeAssembly.GRCh37, 545469, "C");

            var reader = new ClinVarXmlReader(Resources.ClinvarXmlFiles("RCV000010551.xml"), sequenceProvider);

            var clinvarItems = reader.GetItems().ToList();

            Assert.Single(clinvarItems);

            foreach (var clinVarItem in clinvarItems)
            {
                Assert.Equal(2, clinVarItem.OmimIds.Length);
            }
        }

        [Fact]
        [Trait("jira", "NIR-2372")]
        public void MultipleEntryRecordVariant1()
        {
            var sequenceProvider = GetSequenceProvider(GenomeAssembly.GRCh37, 8045031, "G");

            var reader = new ClinVarXmlReader(Resources.ClinvarXmlFiles("RCV000007484.xml"), sequenceProvider);

            var clinvarItems = reader.GetItems().ToList();

            Assert.Single(clinvarItems);
        }

        [Fact]
        [Trait("jira", "NIR-2372")]
        public void MultipleEntryRecordVariant2()
        {
            var sequenceProvider = GetSequenceProvider(GenomeAssembly.GRCh37, 8021910, "ACGTACGTACGTACGTACGTACGTACGTACGTACGTACGT");

            var reader = new ClinVarXmlReader(Resources.ClinvarXmlFiles("RCV000007484.xml"), sequenceProvider);

            var clinvarItems = reader.GetItems().ToList();

            Assert.Single(clinvarItems);
        }

        [Fact]
        [Trait("jira", "NIR-2372")]
        public void SkipMicrosatellitesWithoutAltAllele()
        {
            var sequenceProvider = GetSequenceProvider(GenomeAssembly.GRCh37, 46191240, "ATTCT");

            var reader = new ClinVarXmlReader(Resources.ClinvarXmlFiles("RCV000001054.xml"), sequenceProvider);

            Assert.False(reader.GetItems().Any());
        }

        [Fact]
        [Trait("jira", "NIR-2029")]
        public void MissingClinvarInsertion2()
        {
            var sequenceProvider = GetSequenceProvider(GenomeAssembly.GRCh38, 132903739, "AAACGCTCATAGAGTAACTGGTTGTGCAGTAAAAGCAACTGGTCTCAAACGCTCATAGAGTAACTGGTTGTGCAGTAAAAGCAACTGGTCTC");
            var reader = new ClinVarXmlReader(Resources.ClinvarXmlFiles("RCV000342164.xml"), sequenceProvider);

            var clinvarItems = reader.GetItems().ToList();
            Assert.Single(clinvarItems);
        }

        [Fact]
        public void Skip_entries_with_inconsistant_start_end()
        {
            var sequenceProvider = GetSequenceProvider(GenomeAssembly.GRCh37, 132903739, "AAACGCTCATAGAGTAACTGGTTGTGCAGTAAAAGCAACTGGTCTCAAACGCTCATAGAGTAACTGGTTGTGCAGTAAAAGCAACTGGTCTC");
            var reader = new ClinVarXmlReader(Resources.ClinvarXmlFiles("RCV000342164.xml"), sequenceProvider);

            Assert.False(reader.GetItems().Any());
        }

        [Fact]
        public void Alternate_phenotypes()
        {
            var sequenceProvider = GetSequenceProvider(GenomeAssembly.GRCh37, 204732740, "G");
            var reader = new ClinVarXmlReader(Resources.ClinvarXmlFiles("RCV000537563.xml"), sequenceProvider);

            var clinvarItems = reader.GetItems().ToList();

            Assert.Single(clinvarItems[0].Phenotypes);
        }

        [Fact]
        public void Mising_entry()
        {
            var sequenceProvider = GetSequenceProvider(GenomeAssembly.GRCh37, 36888396, "C");
            var reader = new ClinVarXmlReader(Resources.ClinvarXmlFiles("RCV000171474.xml"), sequenceProvider);

            var clinvarItems = reader.GetItems().ToList();

            Assert.Equal("",clinvarItems[0].RefAllele);
        }

        [Fact]
        public void Multiple_significance()
        {
            var sequenceProvider = GetSequenceProvider(GenomeAssembly.GRCh38, 72349076, "T");
            var reader = new ClinVarXmlReader(Resources.ClinvarXmlFiles("RCV000169296.xml"), sequenceProvider);

            var clinvarItems = reader.GetItems().ToList();

            Assert.Equal(new[]{ "pathogenic", "likely pathogenic" }, clinvarItems[0].Significance);
        }

        [Fact]
        public void Multiple_significance_from_explanation()
        {
            var sequenceProvider = GetSequenceProvider(GenomeAssembly.GRCh38, 12665750, "T");
            var reader = new ClinVarXmlReader(Resources.ClinvarXmlFiles("RCV000001752.xml"), sequenceProvider);

            var clinvarItems = reader.GetItems().ToList();

            Assert.Equal(new[] { "pathogenic", "uncertain significance" }, clinvarItems[0].Significance);
        }
    }
}
