using System;
using System.Collections.Generic;
using System.IO;
using CacheUtils.DataDumperImport.DataStructures.Mutable;
using VariantAnnotation.Interface.AnnotatedPositions;
using VariantAnnotation.Interface.Intervals;

namespace CacheUtils.IntermediateIO
{
    internal sealed class MutableTranscriptWriter : IDisposable
    {
        private readonly StreamWriter _writer;

        internal MutableTranscriptWriter(StreamWriter writer, IntermediateIoHeader header)
        {
            _writer         = writer;
            _writer.NewLine = "\n";
            header.Write(_writer, IntermediateIoCommon.FileType.Transcript);
        }

        internal void Write(MutableTranscript transcript)
        {
            WriteTranscriptInfo(transcript);
            WriteGene(_writer, transcript.Gene);
            WriteTranslation(transcript.CodingRegion, transcript.ProteinId, transcript.ProteinVersion, transcript.PeptideSequence);
            WriteExons(transcript.Exons);
            WriteIntervals(transcript.Introns, "Introns");
            WriteCdnaMaps(transcript.CdnaMaps);
            WriteIntervals(transcript.MicroRnas, "miRNAs");
            WriteSelenocysteines(transcript.SelenocysteinePositions);
            WriteRnaEdits(transcript.RnaEdits);
        }

        private void WriteRnaEdits(IReadOnlyCollection<IRnaEdit> rnaEdits)
        {
            if (rnaEdits == null)
            {
                _writer.WriteLine("RnaEdits\t0");
                return;
            }

            _writer.Write($"RnaEdits\t{rnaEdits.Count}");
            foreach (var rnaEdit in rnaEdits) _writer.Write($"\t{rnaEdit.Start}\t{rnaEdit.End}\t{rnaEdit.Bases}");
            _writer.WriteLine();
        }

        private void WriteSelenocysteines(IReadOnlyCollection<int> positions)
        {
            if (positions == null)
            {
                _writer.WriteLine("Sec\t0");
                return;
            }

            _writer.Write($"Sec\t{positions.Count}");
            foreach (var pos in positions) _writer.Write($"\t{pos}");
            _writer.WriteLine();
        }

        private void WriteCdnaMaps(IReadOnlyCollection<ITranscriptRegion> cdnaMaps)
        {
            _writer.Write($"cDNA\t{cdnaMaps.Count}");
            foreach (var cdnaMap in cdnaMaps) _writer.Write($"\t{cdnaMap.Start}\t{cdnaMap.End}\t{cdnaMap.CdnaStart}\t{cdnaMap.CdnaEnd}");
            _writer.WriteLine();
        }

        private void WriteIntervals(IReadOnlyCollection<IInterval> intervals, string description)
        {
            if (intervals == null)
            {
                _writer.WriteLine($"{description}\t0");
                return;
            }

            _writer.Write($"{description}\t{intervals.Count}");
            foreach (var interval in intervals) _writer.Write($"\t{interval.Start}\t{interval.End}");
            _writer.WriteLine();
        }

        private void WriteExons(IReadOnlyCollection<MutableExon> exons)
        {
            _writer.Write($"Exons\t{exons.Count}");
            foreach (var exon in exons) _writer.Write($"\t{exon.Start}\t{exon.End}\t{exon.Phase}");
            _writer.WriteLine();
        }

        private void WriteTranslation(ICodingRegion codingRegion, string proteinId, byte proteinVersion, string peptideSequence) =>
            _writer.WriteLine($"Translation\t{proteinId}\t{proteinVersion}\t{codingRegion.Start}\t{codingRegion.End}\t{codingRegion.CdnaStart}\t{codingRegion.CdnaEnd}\t{peptideSequence}");

        private static void WriteGene(TextWriter writer, MutableGene gene)
        {
            var strand = gene.OnReverseStrand ? 'R' : 'F';
            writer.WriteLine($"Gene\t{gene.GeneId}\t{gene.Chromosome.UcscName}\t{gene.Chromosome.Index}\t{gene.Start}\t{gene.End}\t{strand}\t{gene.Symbol}\t{(int)gene.SymbolSource}\t{gene.HgncId}");
        }

        private void WriteTranscriptInfo(MutableTranscript transcript)
        {
            _writer.WriteLine($"Transcript\t{transcript.Id}\t{transcript.Version}\t{transcript.Chromosome.UcscName}\t{transcript.Chromosome.Index}\t{transcript.Start}\t{transcript.End}\t{transcript.BioType}\t{(byte)transcript.BioType}\t{BoolToChar(transcript.IsCanonical)}\t{transcript.TotalExonLength}\t{transcript.CcdsId}\t{transcript.RefSeqId}\t{(byte)transcript.Source}\t{BoolToChar(transcript.CdsStartNotFound)}\t{BoolToChar(transcript.CdsEndNotFound)}\t{transcript.StartExonPhase}\t{transcript.BamEditStatus}");
            _writer.WriteLine(transcript.TranslateableSequence);
        }

        private static char BoolToChar(bool b) => b ? 'Y' : 'N';

        public void Dispose() => _writer.Dispose();
    }
}
