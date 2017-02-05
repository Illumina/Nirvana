using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using VariantAnnotation.DataStructures;
using VariantAnnotation.DataStructures.CompressedSequence;
using VariantAnnotation.FileHandling;
using VariantAnnotation.FileHandling.TranscriptCache;
using VariantAnnotation.Utilities;

namespace GenerateVcfFromOmimTests
{
    public sealed class OmimVcfCreator
    {
        private readonly string _inputPrefix;
        private readonly string _outPath;
        private readonly HashSet<string> _processedGeneSet;
        private readonly ICompressedSequence _compressedSequence;
        private readonly List<string> _nucleotides = new List<string> { "A", "T", "C", "G" };
        private readonly DataFileManager _dataFileManager;
        private readonly ChromosomeRenamer _renamer;

        public OmimVcfCreator(string inputPrefix, string refSeqPath, string outPath)
        {
            _inputPrefix = inputPrefix;
            _outPath     = outPath;

            _compressedSequence = new CompressedSequence();
            var reader = new CompressedSequenceReader(FileUtilities.GetReadStream(refSeqPath), _compressedSequence);
            _renamer = _compressedSequence.Renamer;
            _dataFileManager = new DataFileManager(reader, _compressedSequence);
            _processedGeneSet = new HashSet<string>();
        }

        public void Create()
        {
            using (var reader = new GlobalCacheReader(CacheConstants.TranscriptPath(_inputPrefix)))
            using (var writer = GZipUtilities.GetStreamWriter(_outPath))
            {
                WriteVcfHeader(writer);

                var cache = reader.Read();
                Console.Write("- found {0} transcripts... ", cache.Transcripts.Length);
                foreach (var transcript in cache.Transcripts) CreateVcf(writer, transcript);
                Console.WriteLine("finished.");
            }
        }

        private void WriteVcfHeader(StreamWriter writer)
        {
            writer.WriteLine("##fileformat=VCFv4.1");
            writer.WriteLine("##fileDate=20161013");
            writer.WriteLine($"##GenomeAssembly={_compressedSequence.GenomeAssembly}");
            writer.WriteLine("##Comments=Files for test OMIM");
            writer.WriteLine("#CHROM\tPOS\tID\tREF\tALT\tQUAL\tFILTER\tINFO");
        }

        private void CreateVcf(StreamWriter writer, Transcript transcript)
        {
            var geneSymbol = transcript.Gene.Symbol;
            if (!transcript.IsCanonical && _processedGeneSet.Contains(geneSymbol)) return;
            if (transcript.Translation == null) return;
            _processedGeneSet.Add(geneSymbol);

            _dataFileManager.LoadReference(transcript.ReferenceIndex, () => {});

            var position  = (transcript.Translation.CodingRegion.GenomicStart + transcript.Translation.CodingRegion.GenomicEnd) / 2;
            var refAllele = _compressedSequence.Substring(position - 1, 1);
            var altAllele = _nucleotides.First(nuceleotide => nuceleotide != refAllele);

            writer.WriteLine($"{_renamer.UcscReferenceNames[transcript.ReferenceIndex]}\t{position}\t.\t{refAllele}\t{altAllele}\t.\t.\t.");
        }
    }
}