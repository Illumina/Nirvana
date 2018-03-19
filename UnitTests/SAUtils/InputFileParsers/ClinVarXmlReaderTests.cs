using System.Collections.Generic;
using System.IO;
using System.Linq;
using Moq;
using SAUtils.DataStructures;
using SAUtils.InputFileParsers.ClinVar;
using UnitTests.TestDataStructures;
using UnitTests.TestUtilities;
using VariantAnnotation.Interface.Providers;
using VariantAnnotation.Interface.Sequence;
using VariantAnnotation.Sequence;
using Xunit;

namespace UnitTests.SAUtils.InputFileParsers
{
    [Collection("ChromosomeRenamer")]
    public sealed class ClinVarXmlReaderTests
    {
        private readonly Dictionary<string, IChromosome> _refNameToChromosome;

        public ClinVarXmlReaderTests()
        {
            _refNameToChromosome = new Dictionary<string, IChromosome>
            {
                {"17", new Chromosome("chr17", "17", 16)},
                {"1", new Chromosome("chr1", "1", 0)},
                {"2", new Chromosome("chr2", "2", 1)},
                {"22", new Chromosome("chr22", "2", 21)}
            };
        }

        private static ISequenceProvider GetSequenceProvider(GenomeAssembly assembly, IChromosome chromosome, int start, string refSequence)
        {
            var seqProvider = new Mock<ISequenceProvider>();
            if (chromosome.EnsemblName == "X" || chromosome.EnsemblName == "Y")
                seqProvider.Setup(x => x.RefNameToChromosome).Returns(new Dictionary<string, IChromosome> { { "X", new Chromosome("chrX", "X", 1) }, { "Y", new Chromosome("chrY", "Y", 2) } });
            else
                seqProvider.Setup(x => x.RefNameToChromosome).Returns(new Dictionary<string, IChromosome> { { chromosome.EnsemblName, chromosome } });
            seqProvider.Setup(x => x.GenomeAssembly).Returns(assembly);
            seqProvider.Setup(x => x.Sequence).Returns(new SimpleSequence(refSequence, start - 1));
            return seqProvider.Object;
        }

        [Fact]
        public void BasicReadTest()
        {
            var sequenceProvider = GetSequenceProvider(GenomeAssembly.GRCh37, new Chromosome("chr17", "17", 16), 41234419, "A");
            var reader = new ClinVarXmlReader(new FileInfo(Resources.ClinvarXmlFiles("RCV000077146.xml")), sequenceProvider);

            Assert.True(reader.GetItems().Any());

            foreach (var clinVarItem in reader.GetItems())
            {
                Assert.Equal("RCV000077146.3", clinVarItem.Id);

                switch (clinVarItem.Id)
                {
                    case "RCV000077146.3":
                        Assert.Equal("17", clinVarItem.Chromosome.EnsemblName);
                        Assert.Equal(41234419, clinVarItem.Start);
                        Assert.Equal("A", clinVarItem.ReferenceAllele);
                        Assert.Equal("C", clinVarItem.AlternateAllele);
                        Assert.Equal(ClinVarXmlReader.ParseDate("2016-07-31"), clinVarItem.LastUpdatedDate);
                        Assert.True(clinVarItem.AlleleOrigins.SequenceEqual(new List<string> { "germline" }));
                        Assert.Equal("C2676676", clinVarItem.MedGenIDs.First());
                        Assert.Equal("145", clinVarItem.OrphanetIDs.First());
                        Assert.Equal("604370", clinVarItem.OmimIDs.First());
                        Assert.Equal("Breast-ovarian cancer, familial 1", clinVarItem.Phenotypes.First());
                        Assert.Null(clinVarItem.PubmedIds);
                        break;
                }
            }
        }

        [Fact]
        public void MissingAltAllele()
        {
            var sequenceProvider = GetSequenceProvider(GenomeAssembly.GRCh37, new Chromosome("chr1", "1", 0), 118165691, "C");
            var reader = new ClinVarXmlReader(new FileInfo(Resources.ClinvarXmlFiles("RCV000120902.xml")), sequenceProvider);

            Assert.True(reader.GetItems().Any());

            foreach (var clinVarItem in reader.GetItems())
            {
                Assert.Equal("C", clinVarItem.ReferenceAllele);
                Assert.Equal("G", clinVarItem.AlternateAllele);
            }
        }

        //[Fact(Skip = "need different compressed sequence")]
        //public void MultiEntryXmlParsing()
        //{
        //    var mockProvider = new Mock<ISequenceProvider>();
        //    mockProvider.Setup(x => x.GetChromosomeDictionary()).Returns(_refNameToChromosome);

        //    var sequenceProvider = mockProvider.Object;

        //    var reader = new ClinVarXmlReader(new FileInfo(Resources.ClinvarXmlFiles("MultiClinvar.xml")), sequenceProvider);

        //    var clinvarList = new List<ClinVarItem>();
        //    foreach (var clinVarItem in reader.GetItems())
        //    {
        //        switch (clinVarItem.ID)
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
        //    //var reader = new ClinVarXmlReader(new FileInfo(Resources.ClinvarXmlFiles("RCV000007484.xml")),sequenceProvider);

        //    //foreach (var clinVarItem in reader.GetItems())
        //    //{
        //    //    switch (clinVarItem.Start)
        //    //    {
        //    //        case 8045031:
        //    //            Assert.Equal("G", clinVarItem.ReferenceAllele);
        //    //            Assert.Equal("A", clinVarItem.AltAllele);
        //    //            break;
        //    //        case 8021911:
        //    //            Assert.Equal("GTGCTGGACGGTGTCCCT", clinVarItem.AltAllele);
        //    //            var sa = new SupplementaryAnnotationPosition(clinVarItem.Start);
        //    //            var saCreator = new SupplementaryPositionCreator(sa);

        //    //            clinVarItem.SetSupplementaryAnnotations(saCreator);
        //    //            Assert.Equal("iGTGCTGGACGGTGTCCCT", clinVarItem.SaAltAllele);
        //    //            break;
        //    //        default:
        //    //            throw new InvalidDataException($"Unexpected clinvar item start point : {clinVarItem.Start}");
        //    //    }
        //    //}
        //}

        [Fact]
        public void NonEnglishChars()
        {
            var sequenceProvider = GetSequenceProvider(GenomeAssembly.GRCh37, new Chromosome("chr1", "1", 0), 225592188, "TAGAAGA");
            var reader = new ClinVarXmlReader(new FileInfo(Resources.ClinvarXmlFiles("RCV000087262.xml")), sequenceProvider);

            Assert.True(reader.GetItems().Any());
            foreach (var clinVarItem in reader.GetItems())
            {
                Assert.Equal("Pelger-Huët anomaly", clinVarItem.Phenotypes.First());
            }
        }

        [Fact]
        public void WrongPosition()
        {
            var sequenceProvider = GetSequenceProvider(GenomeAssembly.GRCh37, new Chromosome("chr5", "5", 4), 112064826, "G");

            var reader = new ClinVarXmlReader(new FileInfo(Resources.ClinvarXmlFiles("RCV000073701.xml")), sequenceProvider);

            Assert.True(reader.GetItems().Any());

            foreach (var clinVarItem in reader.GetItems())
            {
                switch (clinVarItem.Start)
                {
                    case 112064826:
                        Assert.Equal("G", clinVarItem.ReferenceAllele);
                        Assert.Equal("C", clinVarItem.AlternateAllele);
                        break;
                    default:
                        throw new InvalidDataException($"Unexpected clinvar item start point : {clinVarItem.Start}");
                }
            }
        }

        [Fact]
        public void PubmedTest1()
        {
            var sequenceProvider = GetSequenceProvider(GenomeAssembly.GRCh37, new Chromosome("chr3", "3", 2), 10183457, "CGCACGCAGCTCCGCCCCGCG");

            var reader = new ClinVarXmlReader(new FileInfo(Resources.ClinvarXmlFiles("RCV000152657.xml")), sequenceProvider);

            Assert.True(reader.GetItems().Any());

            foreach (var clinVarItem in reader.GetItems())
            {
                Assert.True(
                    clinVarItem.PubmedIds.SequenceEqual(new List<long> { 12114475, 18836774, 22357542, 24033266 }));
            }
        }

        [Fact]
        public void PubmedTest2()
        {
            var sequenceProvider = GetSequenceProvider(GenomeAssembly.GRCh37, new Chromosome("chr11", "11", 10), 5247993, "AAAG");

            var reader = new ClinVarXmlReader(new FileInfo(Resources.ClinvarXmlFiles("RCV000016673.xml")), sequenceProvider);

            Assert.True(reader.GetItems().Any());
            foreach (var clinVarItem in reader.GetItems())
            {
                Assert.True(
                    clinVarItem.PubmedIds.SequenceEqual(
                        new List<long> { 6714226, 6826539, 9113933, 9845707, 12000828, 12383672 }));
            }
        }

        [Fact]
        public void PubmedTest3()
        {
            var sequenceProvider = GetSequenceProvider(GenomeAssembly.GRCh37, new Chromosome("chr7", "7", 6), 55259485, "C");

            var reader = new ClinVarXmlReader(new FileInfo(Resources.ClinvarXmlFiles("RCV000038438.xml")), sequenceProvider);

            Assert.True(reader.GetItems().Any());
            foreach (var clinVarItem in reader.GetItems())
            {
                Assert.True(
                    clinVarItem.PubmedIds.SequenceEqual(new List<long> { 17285735, 17877814, 22848293, 24033266 }));
            }
        }

        [Fact]
        public void PubmedTest4()
        {
            var sequenceProvider = GetSequenceProvider(GenomeAssembly.GRCh37, new Chromosome("chr10", "10", 6), 43609944, "GCTGT");

            var reader = new ClinVarXmlReader(new FileInfo(Resources.ClinvarXmlFiles("RCV000021819.xml")), sequenceProvider);

            Assert.True(reader.GetItems().Any());
            foreach (var clinVarItem in reader.GetItems())
            {
                Assert.True(clinVarItem.PubmedIds.SequenceEqual(new List<long> { 8099202 }));
            }
        }

        [Fact]
        public void PubmedTest5()
        {
            var sequenceProvider = GetSequenceProvider(GenomeAssembly.GRCh37, new Chromosome("chr16", "16", 6), 88907409, "A");

            var reader = new ClinVarXmlReader(new FileInfo(Resources.ClinvarXmlFiles("RCV000000734.xml")), sequenceProvider);

            Assert.True(reader.GetItems().Any());
            foreach (var clinVarItem in reader.GetItems())
            {
                Assert.Null(clinVarItem.PubmedIds);
            }
        }

        [Fact]
        public void PubmedTest6()
        {
            var sequenceProvider = GetSequenceProvider(GenomeAssembly.GRCh37, new Chromosome("chr1", "1", 6), 118165691, "C");

            //extracting from SCV record
            var reader = new ClinVarXmlReader(new FileInfo(Resources.ClinvarXmlFiles("RCV000120902.xml")), sequenceProvider);

            Assert.True(reader.GetItems().Any());
            foreach (var clinVarItem in reader.GetItems())
            {
                Assert.True(clinVarItem.PubmedIds.SequenceEqual(new List<long> { 24728327 }));
            }
        }

        [Fact]
        public void MultiScvPubmed()
        {
            var sequenceProvider = GetSequenceProvider(GenomeAssembly.GRCh37, new Chromosome("chr4", "4", 6), 15589553, "G");

            //extracting from SCV record
            var reader = new ClinVarXmlReader(new FileInfo(Resources.ClinvarXmlFiles("RCV000194003.xml")), sequenceProvider);

            Assert.True(reader.GetItems().Any());

            foreach (var clinVarItem in reader.GetItems())
            {
                Assert.True(clinVarItem.PubmedIds.SequenceEqual(new List<long> { 25741868, 26092869 }));
            }
        }

        [Fact]
        public void NoClinVarItem()
        {
            var mockProvider = new Mock<ISequenceProvider>();
            mockProvider.Setup(x => x.RefNameToChromosome).Returns(_refNameToChromosome);

            var sequenceProvider = mockProvider.Object;

            var reader = new ClinVarXmlReader(new FileInfo(Resources.ClinvarXmlFiles("RCV000000101.xml")), sequenceProvider);

            Assert.False(reader.GetItems().Any());
        }

        [Fact]
        public void ClinVarForRef()
        {
            var sequenceProvider = GetSequenceProvider(GenomeAssembly.GRCh37, new Chromosome("chrX", "X", 6), 31496350, "C");

            var reader = new ClinVarXmlReader(new FileInfo(Resources.ClinvarXmlFiles("RCV000124712.xml")), sequenceProvider);

            var clinVarList = new List<ClinVarItem>();
            foreach (var clinVarItem in reader.GetItems())
            {
                clinVarList.Add(clinVarItem);
                Assert.Equal(clinVarItem.ReferenceAllele, clinVarItem.AlternateAllele);
            }

            Assert.Single(clinVarList);
        }

        [Fact]
        public void MultiplePhenotypes()
        {
            var sequenceProvider = GetSequenceProvider(GenomeAssembly.GRCh37, new Chromosome("chr5", "5", 6), 172659738, "C");

            //no citations show up for this RCV in the website. But the XML has these pubmed ids under fields that we parse pubmed ids from
            var reader = new ClinVarXmlReader(new FileInfo(Resources.ClinvarXmlFiles("RCV000144179.xml")), sequenceProvider);

            Assert.True(reader.GetItems().Any());

            foreach (var clinVarItem in reader.GetItems())
            {
                var expectedPhenotypes = new List<string> { "Single ventricle", "small Atrial septal defect" };
                Assert.True(expectedPhenotypes.SequenceEqual(clinVarItem.Phenotypes));
            }
        }

        [Fact]
        public void MultipleOrigins()
        {
            var sequenceProvider = GetSequenceProvider(GenomeAssembly.GRCh37, new Chromosome("chrX", "X", 23), 18671566, "G");
            //no citations show up for this RCV in the website. But the XML has these pubmed ids under fields that we parse pubmed ids from
            var reader = new ClinVarXmlReader(new FileInfo(Resources.ClinvarXmlFiles("RCV000080071.xml")), sequenceProvider);

            Assert.True(reader.GetItems().Any());
            foreach (var clinVarItem in reader.GetItems())
            {
                var expectedOrigins = new List<string> { "germline", "maternal", "unknown" };
                Assert.True(expectedOrigins.SequenceEqual(clinVarItem.AlleleOrigins));
            }
        }


        [Fact]
        public void SkipGeneralCitations()
        {
            var sequenceProvider = GetSequenceProvider(GenomeAssembly.GRCh37, new Chromosome("chr1", "1", 0), 67705958, "G");
            //no citations show up for this RCV in the website. But the XML has these pubmed ids under fields that we parse pubmed ids from
            var reader = new ClinVarXmlReader(new FileInfo(Resources.ClinvarXmlFiles("RCV000003254.xml")), sequenceProvider);

            foreach (var clinVarItem in reader.GetItems())
            {
                Assert.True(clinVarItem.PubmedIds.SequenceEqual(new List<long>
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
                }));
            }
        }

        [Fact]
        public void IndelTest()
        {
            var sequenceProvider = GetSequenceProvider(GenomeAssembly.GRCh37, new Chromosome("chr4", "4", 3), 187122303, "ACGTACGTACGTACGTA");

            var reader = new ClinVarXmlReader(new FileInfo(Resources.ClinvarXmlFiles("RCV000032548.xml")), sequenceProvider);

            Assert.True(reader.GetItems().Any());

            foreach (var clinVarItem in reader.GetItems())
            {
                Assert.Equal("RCV000032548.5", clinVarItem.Id);

                switch (clinVarItem.Id)
                {
                    case "RCV000032548.5":
                        Assert.Equal("4", clinVarItem.Chromosome.EnsemblName);
                        Assert.Equal(187122303, clinVarItem.Start);
                        Assert.Equal(17, clinVarItem.ReferenceAllele.Length);
                        Assert.Equal("GC", clinVarItem.AlternateAllele);
                        Assert.Equal(ClinVarXmlReader.ParseDate("2016-08-29"), clinVarItem.LastUpdatedDate);
                        break;
                }
            }
        }


        [Fact]
        [Trait("jira", "NIR-2034")]
        public void MultiScvPubmeds()
        {
            var sequenceProvider = GetSequenceProvider(GenomeAssembly.GRCh37, new Chromosome("chr7", "7", 3), 116411990, "C");

            //extracting from SCV record
            var reader = new ClinVarXmlReader(new FileInfo(Resources.ClinvarXmlFiles("RCV000203290.xml")), sequenceProvider);

            Assert.True(reader.GetItems().Any());

            foreach (var clinVarItem in reader.GetItems())
            {
                Assert.True(clinVarItem.PubmedIds.SequenceEqual(new List<long> { 23806086, 24088041, 25736269 }));
            }
        }

        [Fact]
        [Trait("jira", "NIR-2034")]
        public void MultipleAlleleOrigins()
        {
            var sequenceProvider = GetSequenceProvider(GenomeAssembly.GRCh37, new Chromosome("chr13", "13", 3), 32890572, "G");

            var reader = new ClinVarXmlReader(new FileInfo(Resources.ClinvarXmlFiles("RCV000112977.xml")), sequenceProvider);

            Assert.True(reader.GetItems().Any());

            foreach (var clinVarItem in reader.GetItems())
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
            var sequenceProvider = GetSequenceProvider(GenomeAssembly.GRCh37, new Chromosome("chrX", "X", 0), 66765160, "CAG");

            var reader = new ClinVarXmlReader(new FileInfo(Resources.ClinvarXmlFiles("RCV000485802.xml")), sequenceProvider);

            Assert.False(reader.GetItems().Any());
        }

        [Fact]
        [Trait("jira", "NIR-2035")]
        public void EmptyRefAndAlt()
        {
            var sequenceProvider = GetSequenceProvider(GenomeAssembly.GRCh37, new Chromosome("chr2", "2", 3), 31805881, "G");

            var reader = new ClinVarXmlReader(new FileInfo(Resources.ClinvarXmlFiles("RCV000083638.xml")), sequenceProvider);

            Assert.False(reader.GetItems().Any());
        }

        [Fact]
        [Trait("jira", "NIR-2036")]
        public void SkipMicrosattelite()
        {
            var sequenceProvider = GetSequenceProvider(GenomeAssembly.GRCh37, new Chromosome("chr16", "16", 15), 87637894, "CTG");

            var reader = new ClinVarXmlReader(new FileInfo(Resources.ClinvarXmlFiles("RCV000005426.xml")), sequenceProvider);

            Assert.False(reader.GetItems().Any());
        }

        [Fact]
        [Trait("jira", "NIR-2072")]
        public void MissingClinvarInsertion()
        {
            var sequenceProvider = GetSequenceProvider(GenomeAssembly.GRCh37, new Chromosome("chr1", "1", 1), 2337967, "G");

            var reader = new ClinVarXmlReader(new FileInfo(Resources.ClinvarXmlFiles("RCV000179026.xml")), sequenceProvider);

            Assert.True(reader.GetItems().Any());
            foreach (var clinVarItem in reader.GetItems())
            {
                Assert.Equal(2337968, clinVarItem.Start);
            }
        }

        [Fact]
        [Trait("jira", "NIR-2072")]
        public void MissingClinvarInsertionShift()
        {
            var sequenceProvider = GetSequenceProvider(GenomeAssembly.GRCh37, new Chromosome("chr1", "1", 1), 3751645, "G");

            var reader = new ClinVarXmlReader(new FileInfo(Resources.ClinvarXmlFiles("RCV000207071.xml")), sequenceProvider);

            Assert.True(reader.GetItems().Any());

            foreach (var clinVarItem in reader.GetItems())
            {
                Assert.Equal(3751646, clinVarItem.Start);
            }
        }

        [Fact]
        [Trait("jira", "NIR-2072")]
        public void MissingClinvarInsertionShift2()
        {
            var sequenceProvider = GetSequenceProvider(GenomeAssembly.GRCh37, new Chromosome("chr1", "1", 1), 9324412, "C");
            var reader = new ClinVarXmlReader(new FileInfo(Resources.ClinvarXmlFiles("RCV000017510.xml")), sequenceProvider);

            Assert.True(reader.GetItems().Any());

            foreach (var clinVarItem in reader.GetItems())
            {
                Assert.Equal(9324413, clinVarItem.Start);
            }
        }

        [Fact]
        [Trait("jira", "NIR-2045")]
        public void AlternatePhenotype()
        {
            var sequenceProvider = GetSequenceProvider(GenomeAssembly.GRCh37, new Chromosome("chr7", "7", 1), 42018228, "TC");
            var reader = new ClinVarXmlReader(new FileInfo(Resources.ClinvarXmlFiles("RCV000032707.xml")), sequenceProvider);

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
            var sequenceProvider = GetSequenceProvider(GenomeAssembly.GRCh37, new Chromosome("chr13", "13", 1), 32913457, "C");

            var reader = new ClinVarXmlReader(new FileInfo(Resources.ClinvarXmlFiles("RCV000113363.xml")), sequenceProvider);

            Assert.True(reader.GetItems().Any());
            var altAlleles = new List<string>();
            foreach (var clinVarItem in reader.GetItems())
            {
                altAlleles.Add(clinVarItem.AlternateAllele);
            }

            Assert.Equal(2, altAlleles.Count);
        }

        [Fact]
        [Trait("jira", "NIR-2072")]
        public void OmitOmimFromAltPhenotypes()
        {
            var sequenceProvider = GetSequenceProvider(GenomeAssembly.GRCh37, new Chromosome("chr1", "1", 1), 55529187, "G");
            var reader = new ClinVarXmlReader(new FileInfo(Resources.ClinvarXmlFiles("RCV000030349.xml")), sequenceProvider);

            Assert.True(reader.GetItems().Any());

            foreach (var clinVarItem in reader.GetItems())
            {
                Assert.Single(clinVarItem.OmimIDs);
            }
        }

        [Fact]
        [Trait("jira", "NIR-2072")]
        public void TrimSpaceFromOmimIds()
        {
            var sequenceProvider = GetSequenceProvider(GenomeAssembly.GRCh37, new Chromosome("chrX", "X", 1), 129283520, "A");
            var reader = new ClinVarXmlReader(new FileInfo(Resources.ClinvarXmlFiles("RCV000373191.xml")), sequenceProvider);

            Assert.True(reader.GetItems().Any());

            foreach (var clinVarItem in reader.GetItems())
            {
                Assert.Single(clinVarItem.OmimIDs);
                Assert.Equal("609060", clinVarItem.OmimIDs.FirstOrDefault());
            }
        }

        [Fact]
        [Trait("jira", "NIR-2099")]
        public void ClinvarInsertion()
        {
            var sequenceProvider = GetSequenceProvider(GenomeAssembly.GRCh37, new Chromosome("chrX", "X", 1), 122318386, "A");

            var reader = new ClinVarXmlReader(new FileInfo(Resources.ClinvarXmlFiles("RCV000153339.xml")), sequenceProvider);
            Assert.True(reader.GetItems().Any());
            foreach (var clinVarItem in reader.GetItems())
            {
                Assert.Equal(122318387, clinVarItem.Start);
            }
        }


        [Fact]
        public void Remove9DigitsPubmedId()
        {
            var sequenceProvider = GetSequenceProvider(GenomeAssembly.GRCh37, new Chromosome("chr11", "11", 1), 534286, "C");

            var reader = new ClinVarXmlReader(new FileInfo(Resources.ClinvarXmlFiles("RCV000207504.xml")), sequenceProvider);
            Assert.True(reader.GetItems().Any());

            foreach (var clinVarItem in reader.GetItems())
            {
                Assert.True(
                    clinVarItem.PubmedIds.SequenceEqual(
                        new List<long> { 16329078, 16372351, 19213030, 21438134, 25741868 }));
            }
        }

        [Fact]
        public void CaptureGeneOmimId()
        {
            var sequenceProvider = GetSequenceProvider(GenomeAssembly.GRCh37, new Chromosome("chr4", "4", 1), 3494837, "TGCC");

            var reader = new ClinVarXmlReader(new FileInfo(Resources.ClinvarXmlFiles("RCV000235027.xml")), sequenceProvider);
            Assert.True(reader.GetItems().Any());

            foreach (var clinVarItem in reader.GetItems())
            {
                Assert.True(clinVarItem.OmimIDs.SequenceEqual(new List<string> { "601462", "610285.0001" }));
            }
        }

        [Fact]
        public void CapturePhenotypicSeriesOmimIDandUniq()
        {
            var sequenceProvider = GetSequenceProvider(GenomeAssembly.GRCh37, new Chromosome("chr4", "4", 1), 122746325, "A");

            var reader = new ClinVarXmlReader(new FileInfo(Resources.ClinvarXmlFiles("RCV000401212.xml")), sequenceProvider);
            Assert.True(reader.GetItems().Any());

            foreach (var clinVarItem in reader.GetItems())
            {
                Assert.True(clinVarItem.OmimIDs.SequenceEqual(new List<string> { "209900" }));
            }
        }

        [Fact]
        public void CapturePhenotypeSeriesOmimId()
        {
            var sequenceProvider = GetSequenceProvider(GenomeAssembly.GRCh37, new Chromosome("chr4", "4", 1), 15513014, "GAA");

            var reader = new ClinVarXmlReader(new FileInfo(Resources.ClinvarXmlFiles("RCV000406351.xml")), sequenceProvider);
            Assert.True(reader.GetItems().Any());

            foreach (var clinVarItem in reader.GetItems())
            {
                Assert.True(clinVarItem.OmimIDs.SequenceEqual(new List<string> { "213300" }));
            }
        }

        [Fact]
        public void RemoveDuplicationWithWrongRefSequence()
        {
            var sequenceProvider = GetSequenceProvider(GenomeAssembly.GRCh37, new Chromosome("chr3", "3", 1), 10183702, "GCGGCCGCGGCCCG");

            var reader = new ClinVarXmlReader(new FileInfo(Resources.ClinvarXmlFiles("RCV000267121.xml")), sequenceProvider);
            Assert.False(reader.GetItems().Any());
        }

        [Fact]
        [Trait("jira", "NIR-2372")]
        public void AllelicOmimIdsForSnvs()
        {
            var sequenceProvider = GetSequenceProvider(GenomeAssembly.GRCh37, new Chromosome("chr13", "13", 1), 111329354, "G");

            var reader = new ClinVarXmlReader(new FileInfo(Resources.ClinvarXmlFiles("RCV000170338.xml")), sequenceProvider);

            var clinvarItems = reader.GetItems().ToList();
            Assert.Single(clinvarItems);

            var clinvarItem = clinvarItems[0];
            Assert.Single(clinvarItem.OmimIDs);
            Assert.Equal("612800.0003", clinvarItem.OmimIDs.First());
        }

        [Fact]
        [Trait("jira", "NIR-2372")]
        public void AllelicOmimIdsForDeletions()
        {
            var sequenceProvider = GetSequenceProvider(GenomeAssembly.GRCh37, new Chromosome("chr13", "13", 1), 111335402, "CTC");

            var reader = new ClinVarXmlReader(new FileInfo(Resources.ClinvarXmlFiles("RCV000170338.xml")), sequenceProvider);

            var clinvarItems = reader.GetItems().ToList();
            Assert.Single(clinvarItems);

            var clinvarItem = clinvarItems[0];
            Assert.Single(clinvarItem.OmimIDs);
            Assert.Equal("612800.0002", clinvarItem.OmimIDs.First());
        }

        [Fact]
        [Trait("jira", "NIR-2372")]
        public void ExcludeAllelicOmimIdsFromTraits()
        {
            var sequenceProvider = GetSequenceProvider(GenomeAssembly.GRCh37, new Chromosome("chr8", "8", 1), 100887650, "ATG");

            var reader = new ClinVarXmlReader(new FileInfo(Resources.ClinvarXmlFiles("RCV000050055.xml")), sequenceProvider);

            var clinvarItems = reader.GetItems().ToList();
            Assert.Single(clinvarItems);

            var clinvarItem = clinvarItems[0];
            Assert.Single(clinvarItem.OmimIDs);
            Assert.Equal("216550", clinvarItem.OmimIDs.First());
        }

        [Fact]
        [Trait("jira", "NIR-2372")]
        public void AllelicOmimIdsFromAttributeSetChrX()
        {
            var sequenceProvider = GetSequenceProvider(GenomeAssembly.GRCh37, new Chromosome("chrX", "X", 1), 595469, "C");

            var reader = new ClinVarXmlReader(new FileInfo(Resources.ClinvarXmlFiles("RCV000010551.xml")), sequenceProvider);

            var clinvarItems = reader.GetItems().ToList();

            Assert.Single(clinvarItems);

            foreach (var clinVarItem in clinvarItems)
            {
                Assert.Equal(2, clinVarItem.OmimIDs.Count());
            }
        }

        [Fact]
        [Trait("jira", "NIR-2372")]
        public void AllelicOmimIdsFromAttributeSetChrY()
        {
            var sequenceProvider = GetSequenceProvider(GenomeAssembly.GRCh37, new Chromosome("chrY", "Y", 1), 545469, "C");

            var reader = new ClinVarXmlReader(new FileInfo(Resources.ClinvarXmlFiles("RCV000010551.xml")), sequenceProvider);

            var clinvarItems = reader.GetItems().ToList();

            Assert.Single(clinvarItems);

            foreach (var clinVarItem in clinvarItems)
            {
                Assert.Equal(2, clinVarItem.OmimIDs.Count());
            }
        }

        [Fact]
        [Trait("jira", "NIR-2372")]
        public void MultipleEntryRecordVariant1()
        {
            var sequenceProvider = GetSequenceProvider(GenomeAssembly.GRCh37, new Chromosome("chr1", "1", 1), 8045031, "G");

            var reader = new ClinVarXmlReader(new FileInfo(Resources.ClinvarXmlFiles("RCV000007484.xml")), sequenceProvider);

            var clinvarItems = reader.GetItems().ToList();

            Assert.Single(clinvarItems);
        }

        [Fact]
        [Trait("jira", "NIR-2372")]
        public void MultipleEntryRecordVariant2()
        {
            var sequenceProvider = GetSequenceProvider(GenomeAssembly.GRCh37, new Chromosome("chr1", "1", 1), 8021910, "ACGTACGTACGTACGTACGTACGTACGTACGTACGTACGT");

            var reader = new ClinVarXmlReader(new FileInfo(Resources.ClinvarXmlFiles("RCV000007484.xml")), sequenceProvider);

            var clinvarItems = reader.GetItems().ToList();

            Assert.Single(clinvarItems);
        }

        [Fact]
        [Trait("jira", "NIR-2372")]
        public void SkipMicrosatellitesWithoutAltAllele()
        {
            var sequenceProvider = GetSequenceProvider(GenomeAssembly.GRCh37, new Chromosome("chr22", "22", 1), 46191240, "ATTCT");

            var reader = new ClinVarXmlReader(new FileInfo(Resources.ClinvarXmlFiles("RCV000001054.xml")), sequenceProvider);

            Assert.False(reader.GetItems().Any());
        }

        [Fact]
        [Trait("jira", "NIR-2029")]
        public void MissingClinvarInsertion2()
        {
            var sequenceProvider = GetSequenceProvider(GenomeAssembly.GRCh38, new Chromosome("chr9", "9", 1), 132903739, "AAACGCTCATAGAGTAACTGGTTGTGCAGTAAAAGCAACTGGTCTCAAACGCTCATAGAGTAACTGGTTGTGCAGTAAAAGCAACTGGTCTC");
            var reader = new ClinVarXmlReader(new FileInfo(Resources.ClinvarXmlFiles("RCV000342164.xml")), sequenceProvider);

            var clinvarItems = reader.GetItems().ToList();
            Assert.Single(clinvarItems);
        }

        [Fact]
        public void Skip_entries_with_inconsistant_start_end()
        {
            var sequenceProvider = GetSequenceProvider(GenomeAssembly.GRCh37, new Chromosome("chr9", "9", 1), 132903739, "AAACGCTCATAGAGTAACTGGTTGTGCAGTAAAAGCAACTGGTCTCAAACGCTCATAGAGTAACTGGTTGTGCAGTAAAAGCAACTGGTCTC");
            var reader = new ClinVarXmlReader(new FileInfo(Resources.ClinvarXmlFiles("RCV000342164.xml")), sequenceProvider);

            Assert.False(reader.GetItems().Any());
        }

        [Fact]
        public void Alternate_phenotypes()
        {
            var sequenceProvider = GetSequenceProvider(GenomeAssembly.GRCh37, new Chromosome("chr2", "2", 1), 204732740, "G");
            var reader = new ClinVarXmlReader(new FileInfo(Resources.ClinvarXmlFiles("RCV000537563.xml")), sequenceProvider);

            var clinvarItems = reader.GetItems().ToList();

            Assert.Equal(2, clinvarItems[0].Phenotypes.Count());
        }
    }
}
