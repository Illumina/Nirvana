using System;
using System.Collections.Generic;
using System.IO;
using CacheUtils.Helpers;
using CacheUtils.TranscriptCache.Comparers;
using Compression.Utilities;
using ErrorHandling.Exceptions;
using VariantAnnotation.Algorithms;
using VariantAnnotation.AnnotatedPositions.Transcript;
using VariantAnnotation.Interface.AnnotatedPositions;
using VariantAnnotation.Interface.Intervals;
using VariantAnnotation.IO.Caches;

namespace CacheUtils.GFF
{
    public sealed class GffCreator
    {
        private readonly string _cachePrefix;
        private readonly string _referencePath;
        private readonly HashSet<IGene> _observedGenes;
        private readonly Dictionary<IGene, int> _internalGeneId;

        public GffCreator(string cachePrefix, string referencePath)
        {
            _cachePrefix   = cachePrefix;
            _referencePath = referencePath;

            var geneComparer = new GeneComparer();
            _observedGenes   = new HashSet<IGene>(geneComparer);
            _internalGeneId  = new Dictionary<IGene, int>(geneComparer);
        }

        public void Create(string outputPath)
        {
            using (var writer = GZipUtilities.GetStreamWriter(outputPath))
            {
                var cachePath    = CacheConstants.TranscriptPath(_cachePrefix);
                var sequenceData = SequenceHelper.GetDictionaries(_referencePath);

                Console.Write("- reading {0}... ", Path.GetFileName(cachePath));
                var cache = TranscriptCacheHelper.GetCache(cachePath, sequenceData.refIndexToChromosome);
                Console.WriteLine("found {0:N0} reference sequences.", cache.TranscriptIntervalArrays.Length);

                AddGenesToDictionary(cache.Genes);

                Console.Write("- writing GFF entries... ");
                foreach (var transcriptArray in cache.TranscriptIntervalArrays)
                {
                    if (transcriptArray == null) continue;
                    foreach (var interval in transcriptArray.Array) Write(writer, interval.Value);                    
                }
                Console.WriteLine("finished.");
            }
        }

        private void AddGenesToDictionary(IReadOnlyList<IGene> genes)
        {
            for (int geneIndex = 0; geneIndex < genes.Count; geneIndex++)
            {
                var gene = genes[geneIndex];

                if (_internalGeneId.TryGetValue(gene, out var oldGeneIndex))
                {
                    throw new UserErrorException($"Found a duplicate gene in the dictionary: {genes[geneIndex]} ({geneIndex} vs {oldGeneIndex})");
                }

                _internalGeneId[gene] = geneIndex;
            }
        }

        private void Write(TextWriter writer, ITranscript transcript)
        {
            WriteGene(writer, transcript.Source, transcript.Gene);
            WriteTranscript(writer, transcript);

            var exonPhases = GetExonPhases(transcript.StartExonPhase, transcript.Gene.OnReverseStrand, transcript.CdnaMaps);

            for (int exonIndex = 0; exonIndex < transcript.CdnaMaps.Length; exonIndex++)
                WriteExon(writer, transcript, transcript.CdnaMaps[exonIndex], exonIndex, exonPhases[exonIndex]);
        }

        private void WriteGene(TextWriter writer, Source source, IGene gene)
        {
            if (_observedGenes.Contains(gene)) return;
            _observedGenes.Add(gene);

            var strand = gene.OnReverseStrand ? '-' : '+';
            writer.Write($"{gene.Chromosome.UcscName}\t{source}\tgene\t{gene.Start}\t{gene.End}\t.\t{strand}\t.\t");

            var geneId = source == Source.Ensembl
                ? gene.EnsemblId.ToString()
                : gene.EntrezGeneId.ToString();

            if (!string.IsNullOrEmpty(geneId)) writer.Write($"gene_id \"{geneId}\"; ");
            if (!gene.EntrezGeneId.IsEmpty()) writer.Write($"entrez_gene_id \"{gene.EntrezGeneId}\"; ");
            if (!gene.EnsemblId.IsEmpty()) writer.Write($"ensembl_gene_id \"{gene.EnsemblId}\"; ");
            if (!string.IsNullOrEmpty(gene.Symbol)) writer.Write($"gene_name \"{gene.Symbol}\"; ");
            writer.WriteLine($"internal_gene_id \"{_internalGeneId[gene]}\"; ");
        }

        private static byte[] GetExonPhases(byte currentPhase, bool onReverseStrand, IReadOnlyList<ICdnaCoordinateMap> cdnaMaps)
        {
            var exonPhases = new byte[cdnaMaps.Count];

            if (onReverseStrand)
            {
                for (int index = cdnaMaps.Count - 1; index >= 0; index--)
                {
                    exonPhases[index] = currentPhase;
                    currentPhase = (byte)((currentPhase + GetExonLength(cdnaMaps[index]) % 3) % 3);
                }
                return exonPhases;
            }

            for (int index = 0; index < cdnaMaps.Count; index++)
            {
                exonPhases[index] = currentPhase;
                currentPhase = (byte)((currentPhase + GetExonLength(cdnaMaps[index]) % 3) % 3);
            }

            return exonPhases;
        }

        private static int GetExonLength(IInterval cdnaMap) => cdnaMap.End - cdnaMap.Start + 1;

        private void WriteTranscript(TextWriter writer, ITranscript transcript)
        {
            var strand = transcript.Gene.OnReverseStrand ? '-' : '+';
            writer.Write($"{transcript.Chromosome.UcscName}\t{transcript.Source}\ttranscript\t{transcript.Start}\t{transcript.End}\t.\t{strand}\t.\t");

            WriteGeneralAttributes(writer, transcript);
            writer.WriteLine($"internal_gene_id \"{_internalGeneId[transcript.Gene]}\"; ");
        }

        private static void WriteGeneralAttributes(TextWriter writer, ITranscript transcript)
        {
            var geneId = transcript.Source == Source.Ensembl
                ? transcript.Gene.EnsemblId.ToString()
                : transcript.Gene.EntrezGeneId.ToString();

            if (!string.IsNullOrEmpty(geneId)) writer.Write($"gene_id \"{geneId}\"; ");
            if (!string.IsNullOrEmpty(transcript.Gene.Symbol)) writer.Write($"gene_name \"{transcript.Gene.Symbol}\"; ");

            if (!transcript.Id.IsEmpty()) writer.Write($"transcript_id \"{transcript.Id.WithVersion}\"; ");
            writer.Write($"transcript_type \"{AnnotatedTranscript.GetBioType(transcript.BioType)}\"; ");

            if (transcript.IsCanonical) writer.Write("tag \"canonical\"; ");

            var proteinId = transcript.Translation?.ProteinId.WithVersion;
            if (!string.IsNullOrEmpty(proteinId)) writer.Write($"protein_id \"{proteinId}\"; ");
        }

        private void WriteExon(TextWriter writer, ITranscript transcript, IInterval exon, int exonIndex,
            byte exonPhase)
        {
            var strand = transcript.Gene.OnReverseStrand ? '-' : '+';

            // write the exon entry
            WriteExonEntry(writer, transcript, "exon", exon.Start, exon.End, strand, exonIndex, exonPhase);
            if (transcript.Translation == null) return;

            var codingRegion = transcript.Translation.CodingRegion;

            // write the CDS entry
            if (HasCds(exon, codingRegion.Start, codingRegion.End))
            {
                GetCdsCoordinates(exon, codingRegion.Start, codingRegion.End, out var cdsStart, out var cdsEnd);
                WriteExonEntry(writer, transcript, "CDS", cdsStart, cdsEnd, strand, exonIndex, exonPhase);
            }

            // write the UTR entry
            // ReSharper disable once InvertIf
            if (HasUtr(exon, codingRegion.Start, codingRegion.End))
            {
                // check before CDS
                if (exon.Start < codingRegion.Start)
                {
                    int utrEnd = codingRegion.Start - 1;
                    if (utrEnd > exon.End) utrEnd = exon.End;
                    WriteExonEntry(writer, transcript, "UTR", exon.Start, utrEnd, strand, exonIndex, exonPhase);
                }

                // check after CDS
                // ReSharper disable once InvertIf
                if (exon.End > codingRegion.End)
                {
                    int utrStart = codingRegion.End + 1;
                    if (utrStart < exon.Start) utrStart = exon.Start;
                    WriteExonEntry(writer, transcript, "UTR", utrStart, exon.End, strand, exonIndex, exonPhase);
                }
            }
        }

        private static void GetCdsCoordinates(IInterval exon, int codingRegionStart, int codingRegionEnd,
            out int cdsStart, out int cdsEnd)
        {
            cdsStart = exon.Start;
            cdsEnd = exon.End;

            if (cdsStart < codingRegionStart) cdsStart = codingRegionStart;
            if (cdsEnd > codingRegionEnd) cdsEnd = codingRegionEnd;
        }

        private static bool HasCds(IInterval exon, int codingRegionStart, int codingRegionEnd)
        {
            if (codingRegionStart == -1 || codingRegionEnd == -1) return false;
            return exon.Overlaps(codingRegionStart, codingRegionEnd);
        }

        private static bool HasUtr(IInterval exon, int codingRegionStart, int codingRegionEnd)
        {
            if (codingRegionStart == -1 || codingRegionEnd == -1) return false;
            return exon.Start < codingRegionStart || exon.End > codingRegionEnd;
        }

        private void WriteExonEntry(TextWriter writer, ITranscript transcript, string featureType, int start, int end,
            char strand, int exonIndex, byte exonPhase)
        {
            writer.Write($"{transcript.Chromosome.UcscName}\t{transcript.Source}\t{featureType}\t{start}\t{end}\t.\t{strand}\t{exonPhase}\t");

            WriteGeneralAttributes(writer, transcript);

            var exonNumber = transcript.Gene.OnReverseStrand ? transcript.CdnaMaps.Length - exonIndex : exonIndex + 1;
            writer.Write($"exon_number {exonNumber}; ");
            writer.WriteLine($"internal_gene_id \"{_internalGeneId[transcript.Gene]}\"; ");
        }
    }
}
