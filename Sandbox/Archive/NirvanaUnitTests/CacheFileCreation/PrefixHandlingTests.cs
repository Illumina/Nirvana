using System.Collections.Generic;
using System.Linq;
using Illumina.DataDumperImport.Utilities;
using Illumina.VariantAnnotation.DataStructures;
using Xunit;
using DS  = Illumina.DataDumperImport.DataStructures;
using VEP = Illumina.DataDumperImport.DataStructures.VEP;

namespace NirvanaUnitTests.CacheFileCreation
{
    public class PrefixHandlingTests
    {
        #region members

        private readonly VEP.Transcript _ensemblTranscript;
        private readonly VEP.Transcript _refSeqTranscript;
        private readonly VEP.Transcript _unknownTranscript;
        private readonly VEP.Transcript _ccdsTranscript;
        private readonly VEP.Transcript _ensemblEstTranscript;

        private readonly VEP.Exon _commonExon;
        private readonly VEP.Exon _ensemblExon;
        private readonly VEP.Exon _refSeqExon;
        private readonly VEP.Exon _unknownExon;
        private readonly VEP.Exon _ccdsExon;
        private readonly VEP.Exon _ensemblEstExon;

        private readonly DS.ImportDataStore _tempDataStore;

        #endregion

        // constructor
        public PrefixHandlingTests()
        {
            // keep some null objects around
            MicroRna[] microRnas = null;
            VEP.Gene gene        = new VEP.Gene(0,1,8,"GENE",false);
            VEP.Slice slice      = null;

            // define some exons
            _commonExon     = new VEP.Exon(0, 1, 2, "BOB", false, 1, 2);
            _ensemblExon    = new VEP.Exon(0, 2, 3, "TIM", true, 2, 3);
            _refSeqExon     = new VEP.Exon(0, 3, 4, "PAM", false, 3, 4);
            _unknownExon    = new VEP.Exon(0, 4, 5, "JIM", true, 4, 5);
            _ccdsExon       = new VEP.Exon(0, 5, 6, "JON", true, 5, 6);
            _ensemblEstExon = new VEP.Exon(0, 6, 7, "KIM", true, 6, 7);

            var ensemblExons    = new[] { _commonExon, _ensemblExon };
            var refSeqExons     = new[] { _commonExon, _refSeqExon };
            var unknownExons    = new[] { _commonExon, _unknownExon };
            var ccdsExons       = new[] { _commonExon, _ccdsExon };
            var ensemblEstExons = new[] { _commonExon, _ensemblEstExon };

            // define some introns
            var intron = new VEP.Intron
            {
                Start = 10,
                End   = 20,
                Slice = null
            };

            // define our variant cache
            var variantCache = new VEP.VariantEffectFeatureCache
            {
                Introns = new[] {intron},
                Mapper  = new VEP.TranscriptMapper
                {
                    ExonCoordinateMapper = new VEP.Mapper
                    {
                        PairGenomic = new VEP.PairGenomic
                        {
                            Genomic = new List<VEP.MapperPair>()
                            {
                                new VEP.MapperPair(new VEP.MapperUnit(0, 10, 20, VEP.MapperUnitType.Genomic),
                                    new VEP.MapperUnit(0, 20, 30, VEP.MapperUnitType.CodingDna))
                            }
                        }
                    }
                }
            };

            // define our translation
            var translation = new VEP.Translation { StartExon = _commonExon, EndExon = _commonExon };

            // ReSharper disable ExpressionIsAlwaysNull
            _ensemblTranscript = new VEP.Transcript(BioType.Unknown, ensemblExons, gene, translation, variantCache, slice, false, false, 0, 0, 0,

                100, 200, "CCDS", "DB", "PROTEIN", "REFSEQ", "GENE", "ENST00000343572", "NRAS", GeneSymbolSource.HGNC,
                "1234", 0, microRnas);

            _refSeqTranscript = new VEP.Transcript(BioType.Unknown, refSeqExons, gene, translation, variantCache, slice, false, false, 0, 0, 0,
                100, 200, "CCDS", "DB", "PROTEIN", "REFSEQ", "GENE", "NM_003585", "NRAS", GeneSymbolSource.HGNC, "1234",
                3, microRnas);

            _unknownTranscript = new VEP.Transcript(BioType.Unknown, unknownExons, gene, translation, variantCache, slice, false, false, 0, 0, 0,
                100, 200, "CCDS", "DB", "PROTEIN", "REFSEQ", "GENE", "100126512", "NRAS", GeneSymbolSource.HGNC,
                "1234", 0, microRnas);

            _ccdsTranscript = new VEP.Transcript(BioType.Unknown, ccdsExons, gene, translation, variantCache, slice, false, false, 0, 0, 0,
                100, 200, "CCDS", "DB", "PROTEIN", "REFSEQ", "GENE", "CCDS10994", "NRAS", GeneSymbolSource.HGNC,
                "1234", 1, microRnas);

            _ensemblEstTranscript = new VEP.Transcript(BioType.Unknown, ensemblEstExons, gene, translation, variantCache, slice, false, false, 0, 0,
                0, 100, 200, "CCDS", "DB", "PROTEIN", "REFSEQ", "GENE", "ENSESTT00000018742", "NRAS", GeneSymbolSource.HGNC,
                "1234", 0, microRnas);
            // ReSharper restore ExpressionIsAlwaysNull

            _tempDataStore = new DS.ImportDataStore();

            // add our exons to the data store
            // _tempDataStore.Exons[_commonExon]     = _commonExon;
            // _tempDataStore.Exons[_ensemblExon]    = _ensemblExon;
            // _tempDataStore.Exons[_refSeqExon]     = _refSeqExon;
            // _tempDataStore.Exons[_unknownExon]    = _unknownExon;
            // _tempDataStore.Exons[_ccdsExon]       = _ccdsExon;
            // _tempDataStore.Exons[_ensemblEstExon] = _ensemblEstExon;

            // create a set of transcripts
            _tempDataStore.Transcripts.Add(_ensemblTranscript);
            _tempDataStore.Transcripts.Add(_refSeqTranscript);
            _tempDataStore.Transcripts.Add(_unknownTranscript);
            _tempDataStore.Transcripts.Add(_ccdsTranscript);
            _tempDataStore.Transcripts.Add(_ensemblEstTranscript);
        }

        [Fact]
        public void WhitelistTest()
        {
            var dataStore = new DS.ImportDataStore();

            // create our whitelist
            var whiteList = new List<string> { "NM_" };
            dataStore.EnableWhiteList(whiteList);

            dataStore.CopyDataFrom(_tempDataStore);

            NirvanaDataStore nirvanaDataStore = DataStoreUtilities.ConvertData(dataStore, TranscriptDataSource.RefSeq);

            // make sure that our Ensembl EST transcript is not in the data store, but everything else is
            bool foundEnsembl    = nirvanaDataStore.Transcripts.Any(transcript => transcript.StableId == _ensemblTranscript.StableId);
            bool foundRefSeq     = nirvanaDataStore.Transcripts.Any(transcript => transcript.StableId == _refSeqTranscript.StableId);
            bool foundUnknown    = nirvanaDataStore.Transcripts.Any(transcript => transcript.StableId == _unknownTranscript.StableId);
            bool foundCcds       = nirvanaDataStore.Transcripts.Any(transcript => transcript.StableId == _ccdsTranscript.StableId);
            bool foundEnsemblEst = nirvanaDataStore.Transcripts.Any(transcript => transcript.StableId == _ensemblEstTranscript.StableId);

            Assert.False(foundEnsembl);
            Assert.True(foundRefSeq);
            Assert.False(foundUnknown);
            Assert.False(foundCcds);
            Assert.False(foundEnsemblEst);

            // make sure that _ensemblEstExon is not in the data store, but everything else is
            bool foundCommonExon     = nirvanaDataStore.Exons.Any(exon => exon.Start == _commonExon.Start);
            bool foundEnsemblExon    = nirvanaDataStore.Exons.Any(exon => exon.Start == _ensemblExon.Start);
            bool foundRefSeqExon     = nirvanaDataStore.Exons.Any(exon => exon.Start == _refSeqExon.Start);
            bool foundUnknownExon    = nirvanaDataStore.Exons.Any(exon => exon.Start == _unknownExon.Start);
            bool foundCcdsExon       = nirvanaDataStore.Exons.Any(exon => exon.Start == _ccdsExon.Start);
            bool foundEnsemblEstExon = nirvanaDataStore.Exons.Any(exon => exon.Start == _ensemblEstExon.Start);

            Assert.True(foundCommonExon);
            Assert.False(foundEnsemblExon);
            Assert.True(foundRefSeqExon);
            Assert.False(foundUnknownExon);
            Assert.False(foundCcdsExon);
            Assert.False(foundEnsemblEstExon);
        }
    }
}
