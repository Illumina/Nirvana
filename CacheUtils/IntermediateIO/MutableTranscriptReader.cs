using System;
using System.Collections.Generic;
using System.IO;
using CacheUtils.DataDumperImport.DataStructures;
using CacheUtils.DataDumperImport.DataStructures.Mutable;
using CacheUtils.TranscriptCache;
using Genome;
using Intervals;
using IO;
using OptimizedCore;
using VariantAnnotation.AnnotatedPositions.Transcript;
using VariantAnnotation.Caches.DataStructures;
using VariantAnnotation.Interface.AnnotatedPositions;

namespace CacheUtils.IntermediateIO
{
    internal sealed class MutableTranscriptReader : IDisposable
    {
        private readonly IDictionary<ushort, IChromosome> _refIndexToChromosome;
        private readonly StreamReader _reader;
        public readonly IntermediateIoHeader Header;

        private readonly ISequence _sequence = new NSequence();

        internal MutableTranscriptReader(Stream stream, IDictionary<ushort, IChromosome> refIndexToChromosome)
        {
            _refIndexToChromosome = refIndexToChromosome;
            _reader = FileUtilities.GetStreamReader(stream);
            Header  = IntermediateIoCommon.ReadHeader(_reader, IntermediateIoCommon.FileType.Transcript);
        }

        public MutableTranscript[] GetTranscripts()
        {
            var transcripts = new List<MutableTranscript>();

            while (true)
            {
                var transcript = GetNextTranscript();
                if (transcript == null) break;
                transcripts.Add(transcript);
            }

            return transcripts.ToArray();
        }

        private MutableTranscript GetNextTranscript()
        {
            string line = _reader.ReadLine();
            if (line == null) return null;

            var transcriptInfo  = ReadTranscriptInfo(line);
            var gene            = ReadGene(transcriptInfo.Chromosome);
            var translation     = ReadTranslation();
            var exons           = ReadExons(transcriptInfo.Chromosome);
            var introns         = ReadIntervals("Introns");
            var cdnaMaps        = ReadCdnaMaps();
            var mirnas          = ReadIntervals("miRNAs");
            var selenocysteines = ReadSelenocysteines();
            var rnaEdits        = ReadRnaEdits();

            var transcript = new MutableTranscript(transcriptInfo.Chromosome, transcriptInfo.Start, transcriptInfo.End,
                transcriptInfo.Id, transcriptInfo.Version, transcriptInfo.CcdsId, transcriptInfo.RefSeqId,
                transcriptInfo.BioType, transcriptInfo.IsCanonical, translation.CodingRegion, translation.Id,
                translation.Version, translation.PeptideSeq, transcriptInfo.Source, gene, exons,
                transcriptInfo.StartExonPhase, transcriptInfo.TotalExonLength, introns, cdnaMaps, null, null,
                transcriptInfo.TranslateableSequence, mirnas, transcriptInfo.CdsStartNotFound,
                transcriptInfo.CdsEndNotFound, selenocysteines, rnaEdits, transcriptInfo.BamEditStatus);

            AddMutableContents(transcript);

            return transcript;
        }

        private void AddMutableContents(MutableTranscript mt)
        {
            mt.TranscriptRegions = TranscriptRegionMerger.GetTranscriptRegions(mt.CdnaMaps, mt.Exons, mt.Introns, mt.Gene.OnReverseStrand);
            TranscriptRegionValidater.Validate(mt.Id, mt.CdnaMaps, mt.Exons, mt.Introns, mt.TranscriptRegions);

            mt.NewStartExonPhase = mt.StartExonPhase < 0 ? (byte)0 : (byte)mt.StartExonPhase;

            if (mt.CodingRegion == null) return;

            var codingSequence = new CodingSequence(_sequence, mt.CodingRegion, mt.TranscriptRegions,
                mt.Gene.OnReverseStrand, mt.NewStartExonPhase, mt.RnaEdits);

            mt.CdsLength = codingSequence.GetCodingSequence().Length;

            mt.CodingRegion = new CodingRegion(mt.CodingRegion.Start, mt.CodingRegion.End,
                mt.CodingRegion.CdnaStart, mt.CodingRegion.CdnaEnd, mt.CdsLength);
        }

        private int[] ReadSelenocysteines()
        {
            var cols = GetColumns("Sec");

            int numPositions = int.Parse(cols[1]);
            if (numPositions == 0) return null;

            var positions = new int[numPositions];
            var colIndex = 2;

            for (var i = 0; i < numPositions; i++) positions[i] = int.Parse(cols[colIndex++]);
            return positions;
        }

        private IRnaEdit[] ReadRnaEdits()
        {
            var cols = GetColumns("RnaEdits");

            int numRnaEdits = int.Parse(cols[1]);
            if (numRnaEdits == 0) return null;

            var rnaEdits = new IRnaEdit[numRnaEdits];
            var colIndex = 2;

            for (var i = 0; i < numRnaEdits; i++)
            {
                int start    = int.Parse(cols[colIndex++]);
                int end      = int.Parse(cols[colIndex++]);
                string bases = cols[colIndex++];
                rnaEdits[i] = new RnaEdit(start, end, bases);
            }

            return rnaEdits;
        }

        private MutableTranscriptRegion[] ReadCdnaMaps()
        {
            var cols = GetColumns("cDNA");

            int numCdnaMaps = int.Parse(cols[1]);
            if (numCdnaMaps == 0) return null;

            var cdnaMaps = new MutableTranscriptRegion[numCdnaMaps];
            var colIndex = 2;

            for (var i = 0; i < numCdnaMaps; i++)
            {
                int start     = int.Parse(cols[colIndex++]);
                int end       = int.Parse(cols[colIndex++]);
                int cdnaStart = int.Parse(cols[colIndex++]);
                int cdnaEnd   = int.Parse(cols[colIndex++]);
                cdnaMaps[i]   = new MutableTranscriptRegion(TranscriptRegionType.Exon, 0, start, end, cdnaStart, cdnaEnd);
            }

            return cdnaMaps;
        }

        private IInterval[] ReadIntervals(string description)
        {
            var cols = GetColumns(description);

            int numIntervals = int.Parse(cols[1]);
            if (numIntervals == 0) return null;

            var intervals = new IInterval[numIntervals];
            var colIndex = 2;

            for (var i = 0; i < numIntervals; i++)
            {
                int start    = int.Parse(cols[colIndex++]);
                int end      = int.Parse(cols[colIndex++]);
                intervals[i] = new Interval(start, end);
            }

            return intervals;
        }

        private MutableExon[] ReadExons(IChromosome chromosome)
        {
            var cols = GetColumns("Exons");

            int numExons = int.Parse(cols[1]);
            if (numExons == 0) return null;

            var exons = new MutableExon[numExons];
            var colIndex = 2;

            for (var i = 0; i < numExons; i++)
            {
                int start = int.Parse(cols[colIndex++]);
                int end   = int.Parse(cols[colIndex++]);
                var phase = (byte)(int.Parse(cols[colIndex++]) + 1);
                exons[i]  = new MutableExon(chromosome, start, end, phase);
            }

            return exons;
        }

        private (string Id, byte Version, ICodingRegion CodingRegion, string PeptideSeq) ReadTranslation()
        {
            var cols = GetColumns("Translation");

            string id         = cols[1];
            byte version      = byte.Parse(cols[2]);
            int start         = int.Parse(cols[3]);
            int end           = int.Parse(cols[4]);
            int cdnaStart     = int.Parse(cols[5]);
            int cdnaEnd       = int.Parse(cols[6]);
            string peptideSeq = cols[7];

            var codingRegion = start == -1 && end == -1
                ? null
                : new CodingRegion(start, end, cdnaStart, cdnaEnd, 0);

            return (id, version, codingRegion, peptideSeq);
        }

        private MutableGene ReadGene(IChromosome chromosome)
        {
            var cols = GetColumns("Gene");

            string id            = cols[1];
            int start            = int.Parse(cols[4]);
            int end              = int.Parse(cols[5]);
            bool onReverseStrand = cols[6] == "R";
            string symbol        = cols[7];
            var symbolSource     = (GeneSymbolSource)int.Parse(cols[8]);
            int hgncId           = int.Parse(cols[9]);

            return new MutableGene(chromosome, start, end, onReverseStrand, symbol, symbolSource, id, hgncId);
        }

        private (string Id, byte Version, IChromosome Chromosome, int Start, int End, BioType BioType, bool IsCanonical,
            int TotalExonLength, string CcdsId, string RefSeqId, Source Source, bool CdsStartNotFound, bool
            CdsEndNotFound, string TranslateableSequence, int StartExonPhase, string BamEditStatus) ReadTranscriptInfo(
                string line)
        {
            var cols = GetColumns("Transcript", line);

            string id             = cols[1];
            byte version          = byte.Parse(cols[2]);
            ushort referenceIndex = ushort.Parse(cols[4]);
            int start             = int.Parse(cols[5]);
            int end               = int.Parse(cols[6]);
            var biotype           = (BioType)byte.Parse(cols[8]);
            bool isCanonical      = cols[9] == "Y";
            int totalExonLength   = int.Parse(cols[10]);
            string ccdsId         = cols[11];
            string refSeqId       = cols[12];
            var source            = (Source)byte.Parse(cols[13]);
            bool cdsStartNotFound = cols[14] == "Y";
            bool cdsEndNotFound   = cols[15] == "Y";
            int startExonPhase    = int.Parse(cols[16]);
            string bamEditStatus  = cols[17];

            string translateableSequence = _reader.ReadLine();
            var chromosome = ReferenceNameUtilities.GetChromosome(_refIndexToChromosome, referenceIndex);

            return (id, version, chromosome, start, end, biotype, isCanonical, totalExonLength, ccdsId, refSeqId, source
                , cdsStartNotFound, cdsEndNotFound, translateableSequence, startExonPhase, bamEditStatus);
        }

        private string[] GetColumns(string keyword, string line = null)
        {
            if (line == null) line = _reader.ReadLine();
            var cols = line?.OptimizedSplit('\t');
            if (cols == null) throw new InvalidDataException("Found an unexpected null when parsing the columns in the transcript reader.");
            if (cols[0] != keyword) throw new InvalidDataException($"Could not find the {keyword} keyword in the transcripts file.");
            return cols;
        }

        public void Dispose() => _reader.Dispose();
    }
}
