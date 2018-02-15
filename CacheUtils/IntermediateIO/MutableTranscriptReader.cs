using System;
using System.Collections.Generic;
using System.IO;
using CacheUtils.DataDumperImport.DataStructures;
using CacheUtils.DataDumperImport.DataStructures.Mutable;
using CommonUtilities;
using VariantAnnotation.Caches.DataStructures;
using VariantAnnotation.Interface.AnnotatedPositions;
using VariantAnnotation.Interface.Intervals;
using VariantAnnotation.Interface.Sequence;

namespace CacheUtils.IntermediateIO
{
    internal sealed class MutableTranscriptReader : IDisposable
    {
        private readonly IDictionary<ushort, IChromosome> _refIndexToChromosome;
        private readonly StreamReader _reader;
        public readonly IntermediateIoHeader Header;

        internal MutableTranscriptReader(Stream stream, IDictionary<ushort, IChromosome> refIndexToChromosome)
        {
            _refIndexToChromosome = refIndexToChromosome;
            _reader = new StreamReader(stream);
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
            var line = _reader.ReadLine();
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

            return new MutableTranscript(transcriptInfo.Chromosome, transcriptInfo.Start, transcriptInfo.End,
                transcriptInfo.Id, transcriptInfo.Version, transcriptInfo.CcdsId, transcriptInfo.RefSeqId,
                transcriptInfo.BioType, transcriptInfo.IsCanonical, translation.CodingRegion, translation.Id,
                translation.Version, translation.PeptideSeq, transcriptInfo.Source, gene, exons,
                transcriptInfo.StartExonPhase, transcriptInfo.TotalExonLength, introns, cdnaMaps, null, null,
                transcriptInfo.TranslateableSequence, mirnas, transcriptInfo.CdsStartNotFound,
                transcriptInfo.CdsEndNotFound, selenocysteines, rnaEdits, transcriptInfo.BamEditStatus);
        }

        private int[] ReadSelenocysteines()
        {
            var cols = GetColumns("Sec");

            int numPositions = int.Parse(cols[1]);
            if (numPositions == 0) return null;

            var positions = new int[numPositions];
            int colIndex = 2;

            for (int i = 0; i < numPositions; i++) positions[i] = int.Parse(cols[colIndex++]);
            return positions;
        }

        private IRnaEdit[] ReadRnaEdits()
        {
            var cols = GetColumns("RnaEdits");

            int numRnaEdits = int.Parse(cols[1]);
            if (numRnaEdits == 0) return null;

            var rnaEdits = new IRnaEdit[numRnaEdits];
            int colIndex = 2;

            for (int i = 0; i < numRnaEdits; i++)
            {
                var start   = int.Parse(cols[colIndex++]);
                var end     = int.Parse(cols[colIndex++]);
                var bases   = cols[colIndex++];
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
            int colIndex = 2;

            for (int i = 0; i < numCdnaMaps; i++)
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
            int colIndex  = 2;

            for (int i = 0; i < numIntervals; i++)
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

            var exons    = new MutableExon[numExons];
            int colIndex = 2;

            for (int i = 0; i < numExons; i++)
            {
                int start  = int.Parse(cols[colIndex++]);
                int end    = int.Parse(cols[colIndex++]);
                byte phase = (byte)(int.Parse(cols[colIndex++]) + 1);
                exons[i]   = new MutableExon(chromosome, start, end, phase);
            }

            return exons;
        }

        private (string Id, byte Version, ITranscriptRegion CodingRegion, string PeptideSeq) ReadTranslation()
        {
            var cols = GetColumns("Translation");

            var id         = cols[1];
            var version    = byte.Parse(cols[2]);
            var start      = int.Parse(cols[3]);
            var end        = int.Parse(cols[4]);
            var cdnaStart  = int.Parse(cols[5]);
            var cdnaEnd    = int.Parse(cols[6]);
            var peptideSeq = cols[7];

            var codingRegion = start == -1 && end == -1
                ? null
                : new TranscriptRegion(TranscriptRegionType.CodingRegion, 0, start, end, cdnaStart, cdnaEnd);

            return (id, version, codingRegion, peptideSeq);
        }

        private MutableGene ReadGene(IChromosome chromosome)
        {
            var cols = GetColumns("Gene");

            var id              = cols[1];
            var start           = int.Parse(cols[4]);
            var end             = int.Parse(cols[5]);
            var onReverseStrand = cols[6] == "R";
            var symbol          = cols[7];
            var symbolSource    = (GeneSymbolSource)int.Parse(cols[8]);
            var hgncId          = int.Parse(cols[9]);

            return new MutableGene(chromosome, start, end, onReverseStrand, symbol, symbolSource, id, hgncId);
        }

        private (string Id, byte Version, IChromosome Chromosome, int Start, int End, BioType BioType, bool IsCanonical,
            int TotalExonLength, string CcdsId, string RefSeqId, Source Source, bool CdsStartNotFound, bool
            CdsEndNotFound, string TranslateableSequence, int StartExonPhase, string BamEditStatus) ReadTranscriptInfo(
                string line)
        {
            var cols = GetColumns("Transcript", line);

            var id                = cols[1];
            var version           = byte.Parse(cols[2]);
            var referenceIndex    = ushort.Parse(cols[4]);
            var start             = int.Parse(cols[5]);
            var end               = int.Parse(cols[6]);
            var biotype           = (BioType)byte.Parse(cols[8]);
            var isCanonical       = cols[9] == "Y";
            var totalExonLength   = int.Parse(cols[10]);
            var ccdsId            = cols[11];
            var refSeqId          = cols[12];
            var source            = (Source)byte.Parse(cols[13]);
            var cdsStartNotFound  = cols[14] == "Y";
            var cdsEndNotFound    = cols[15] == "Y";
            var startExonPhase    = int.Parse(cols[16]);
            var bamEditStatus     = cols[17];

            var translateableSequence = _reader.ReadLine();
            var chromosome = ReferenceNameUtilities.GetChromosome(_refIndexToChromosome, referenceIndex);

            return (id, version, chromosome, start, end, biotype, isCanonical, totalExonLength, ccdsId, refSeqId, source
                , cdsStartNotFound, cdsEndNotFound, translateableSequence, startExonPhase, bamEditStatus);
        }

        private string[] GetColumns(string keyword, string line = null)
        {
            if (line == null) line = _reader.ReadLine();
            var cols = line?.Split('\t');
            if (cols == null) throw new InvalidDataException("Found an unexpected null when parsing the columns in the transcript reader.");
            if (cols[0] != keyword) throw new InvalidDataException($"Could not find the {keyword} keyword in the transcripts file.");
            return cols;
        }

        public void Dispose() => _reader.Dispose();
    }
}
