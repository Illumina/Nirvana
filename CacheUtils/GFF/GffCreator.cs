using System;
using System.Collections.Generic;
using System.IO;
using ErrorHandling.Exceptions;
using VariantAnnotation.Algorithms;
using VariantAnnotation.DataStructures;
using VariantAnnotation.DataStructures.CompressedSequence;
using VariantAnnotation.FileHandling;
using VariantAnnotation.FileHandling.TranscriptCache;
using VariantAnnotation.Utilities;

namespace CacheUtils.GFF
{
    public sealed class GffCreator
    {
        private readonly string _cachePrefix;
        private readonly string[] _referenceNames;
        private readonly HashSet<Gene> _observedGenes;

        private readonly Dictionary<Gene, int> _internalGeneId;

        public GffCreator(string cachePrefix, string referencePath)
        {
            _cachePrefix    = cachePrefix;
            _referenceNames = GetUcscReferenceNames(referencePath);
            _observedGenes  = new HashSet<Gene>();
            _internalGeneId = new Dictionary<Gene, int>();
        }

        public void Create(string outputPath)
        {
            using (var writer = GZipUtilities.GetStreamWriter(outputPath))
            {
                Console.Write("- reading {0}... ", Path.GetFileName(_cachePrefix));
                var cache = GetCache(CacheConstants.TranscriptPath(_cachePrefix));
                Console.WriteLine("found {0:N0} transcripts.", cache.Transcripts.Length);

                AddGenesToDictionary(cache.Genes);

                Console.Write("- writing GFF entries... ");
                foreach (var transcript in cache.Transcripts) Write(writer, _referenceNames[transcript.ReferenceIndex], transcript);
                Console.WriteLine("finished.");
            }
        }

        private void AddGenesToDictionary(Gene[] genes)
        {
            for (int geneIndex = 0; geneIndex < genes.Length; geneIndex++)
            {
                var gene = genes[geneIndex];

                int oldGeneIndex;
                if (_internalGeneId.TryGetValue(gene, out oldGeneIndex))
                {
                    throw new UserErrorException($"Found a duplicate gene in the dictionary: {genes[geneIndex]} ({geneIndex} vs {oldGeneIndex})");
                }

                _internalGeneId[gene] = geneIndex;
            }
        }

        private static string[] GetUcscReferenceNames(string compressedReferencePath)
        {
            string[] refNames;
            var compressedSequence = new CompressedSequence();

            using (var reader = new CompressedSequenceReader(FileUtilities.GetReadStream(compressedReferencePath), compressedSequence))
            {
                refNames = new string[reader.Metadata.Count];
                for (int refIndex = 0; refIndex < reader.Metadata.Count; refIndex++)
                {
                    refNames[refIndex] = reader.Metadata[refIndex].UcscName;
                }
            }

            return refNames;
        }

        /// <summary>
        /// returns a datastore specified by the filepath
        /// </summary>
        private static GlobalCache GetCache(string cachePath)
        {
            if (!File.Exists(cachePath)) throw new FileNotFoundException($"Could not find {cachePath}");

            GlobalCache cache;
            using (var reader = new GlobalCacheReader(cachePath)) cache = reader.Read();
            return cache;
        }

        private void Write(TextWriter writer, string ucscReferenceName, Transcript transcript)
        {
            // write the gene
            WriteGene(writer, ucscReferenceName, transcript.TranscriptSource, transcript.Gene);

            // write the transcript
            WriteTranscript(writer, ucscReferenceName, transcript);

            // calculate the exon phases
            var exonPhases = GetExonPhases(transcript.StartExonPhase, transcript.Gene.OnReverseStrand,
                transcript.CdnaMaps);

            // write all of the exons
            for (int exonIndex = 0; exonIndex < transcript.CdnaMaps.Length; exonIndex++)
                WriteExon(writer, ucscReferenceName, transcript, transcript.CdnaMaps[exonIndex], exonIndex, exonPhases[exonIndex]);
        }

        private void WriteGene(TextWriter writer, string ucscReferenceName,
            TranscriptDataSource transcriptDataSource, Gene gene)
        {
            if (_observedGenes.Contains(gene)) return;
            _observedGenes.Add(gene);

            var strand = gene.OnReverseStrand ? '-' : '+';
            writer.Write($"{ucscReferenceName}\t{transcriptDataSource}\tgene\t{gene.Start}\t{gene.End}\t.\t{strand}\t.\t");

            var geneId = transcriptDataSource == TranscriptDataSource.Ensembl
                ? gene.EnsemblId.ToString()
                : gene.EntrezGeneId.ToString();

            if (!string.IsNullOrEmpty(geneId)) writer.Write($"gene_id \"{geneId}\"; ");
            if (!gene.EntrezGeneId.IsEmpty) writer.Write($"entrez_gene_id \"{gene.EntrezGeneId}\"; ");
            if (!gene.EnsemblId.IsEmpty) writer.Write($"ensembl_gene_id \"{gene.EnsemblId}\"; ");
            if (!string.IsNullOrEmpty(gene.Symbol)) writer.Write($"gene_name \"{gene.Symbol}\"; ");
            writer.WriteLine($"internal_gene_id \"{_internalGeneId[gene]}\"; ");
        }

        private static byte[] GetExonPhases(byte currentPhase, bool onReverseStrand, CdnaCoordinateMap[] cdnaMaps)
        {
            var exonPhases = new byte[cdnaMaps.Length];

            if (onReverseStrand)
            {
                for (int index = cdnaMaps.Length - 1; index >= 0; index--)
                {
                    exonPhases[index] = currentPhase;
                    currentPhase = (byte)((currentPhase + GetExonLength(cdnaMaps[index]) % 3) % 3);
                }
                return exonPhases;
            }

            for (int index = 0; index < cdnaMaps.Length; index++)
            {
                exonPhases[index] = currentPhase;
                currentPhase = (byte)((currentPhase + GetExonLength(cdnaMaps[index]) % 3) % 3);
            }

            return exonPhases;
        }

        private static int GetExonLength(CdnaCoordinateMap cdnaMap)
        {
            return cdnaMap.GenomicEnd - cdnaMap.GenomicStart + 1;
        }

        private void WriteTranscript(TextWriter writer, string ucscReferenceName, Transcript transcript)
        {
            // write the general data
            var strand = transcript.Gene.OnReverseStrand ? '-' : '+';
            writer.Write($"{ucscReferenceName}\t{transcript.TranscriptSource}\ttranscript\t{transcript.Start}\t{transcript.End}\t.\t{strand}\t.\t");

            WriteGeneralAttributes(writer, transcript);
            writer.WriteLine($"internal_gene_id \"{_internalGeneId[transcript.Gene]}\"; ");
        }

        private static void WriteGeneralAttributes(TextWriter writer, Transcript transcript)
        {
            var geneId = transcript.TranscriptSource == TranscriptDataSource.Ensembl
                ? transcript.Gene.EnsemblId.ToString()
                : transcript.Gene.EntrezGeneId.ToString();

            if (!string.IsNullOrEmpty(geneId)) writer.Write($"gene_id \"{geneId}\"; ");
            if (!string.IsNullOrEmpty(transcript.Gene.Symbol)) writer.Write($"gene_name \"{transcript.Gene.Symbol}\"; ");

            if (!transcript.Id.IsEmpty) writer.Write($"transcript_id \"{FormatUtilities.CombineIdAndVersion(transcript.Id, transcript.Version)}\"; ");
            writer.Write($"transcript_type \"{BioTypeUtilities.GetBiotypeDescription(transcript.BioType)}\"; ");

            if (transcript.IsCanonical) writer.Write("tag \"canonical\"; ");

            if (!string.IsNullOrEmpty(transcript.Translation?.ProteinId.ToString()))
                writer.Write($"protein_id \"{FormatUtilities.CombineIdAndVersion(transcript.Translation.ProteinId, transcript.Translation.ProteinVersion)}\"; ");
        }

        private void WriteExon(TextWriter writer, string ucscReferenceName, Transcript transcript,
            CdnaCoordinateMap exon, int exonIndex, byte exonPhase)
        {
            var strand = transcript.Gene.OnReverseStrand ? '-' : '+';
            
            // write the exon entry
            WriteExonEntry(writer, ucscReferenceName, transcript, "exon", exon.GenomicStart, exon.GenomicEnd, strand, exonIndex, exonPhase);
            if (transcript.Translation == null) return;

            var codingRegion = transcript.Translation.CodingRegion;

            // write the CDS entry
            if (HasCds(exon, codingRegion.GenomicStart, codingRegion.GenomicEnd))
            {
                int cdsStart, cdsEnd;
                GetCdsCoordinates(exon, codingRegion.GenomicStart, codingRegion.GenomicEnd, out cdsStart,
                    out cdsEnd);
                WriteExonEntry(writer, ucscReferenceName, transcript, "CDS", cdsStart, cdsEnd, strand, exonIndex, exonPhase);
            }

            // write the UTR entry
            if (HasUtr(exon, codingRegion.GenomicStart, codingRegion.GenomicEnd))
            {
                // check before CDS
                if (exon.GenomicStart < codingRegion.GenomicStart)
                {
                    int utrEnd = codingRegion.GenomicStart - 1;
                    if (utrEnd > exon.GenomicEnd) utrEnd = exon.GenomicEnd;
                    WriteExonEntry(writer, ucscReferenceName, transcript, "UTR", exon.GenomicStart, utrEnd, strand,
                        exonIndex, exonPhase);
                }

                // check after CDS
                if (exon.GenomicEnd > codingRegion.GenomicEnd)
                {
                    int utrStart = codingRegion.GenomicEnd + 1;
                    if (utrStart < exon.GenomicStart) utrStart = exon.GenomicStart;
                    WriteExonEntry(writer, ucscReferenceName, transcript, "UTR", utrStart, exon.GenomicEnd, strand,
                        exonIndex, exonPhase);
                }
            }
        }

        private static void GetCdsCoordinates(CdnaCoordinateMap exon, int codingRegionStart, int codingRegionEnd, out int cdsStart,
            out int cdsEnd)
        {
            cdsStart = exon.GenomicStart;
            cdsEnd   = exon.GenomicEnd;

            if (cdsStart < codingRegionStart) cdsStart = codingRegionStart;
            if (cdsEnd > codingRegionEnd) cdsEnd = codingRegionEnd;
        }

        private static bool HasCds(CdnaCoordinateMap exon, int codingRegionStart, int codingRegionEnd)
        {
            if (codingRegionStart == -1 || codingRegionEnd == -1) return false;
            return Overlap.Partial(exon.GenomicStart, exon.GenomicEnd, codingRegionStart, codingRegionEnd);
        }

        private static bool HasUtr(CdnaCoordinateMap exon, int codingRegionStart, int codingRegionEnd)
        {
            if (codingRegionStart == -1 || codingRegionEnd == -1) return false;
            return exon.GenomicStart < codingRegionStart || exon.GenomicEnd > codingRegionEnd;
        }

        private void WriteExonEntry(TextWriter writer, string ucscReferenceName, Transcript transcript,
            string featureType, int start, int end, char strand, int exonIndex, byte exonPhase)
        {
            writer.Write($"{ucscReferenceName}\t{transcript.TranscriptSource}\t{featureType}\t{start}\t{end}\t.\t{strand}\t{exonPhase}\t");

            WriteGeneralAttributes(writer, transcript);

            var exonNumber = transcript.Gene.OnReverseStrand ? transcript.CdnaMaps.Length - exonIndex : exonIndex + 1;
            writer.Write($"exon_number {exonNumber}; ");
            writer.WriteLine($"internal_gene_id \"{_internalGeneId[transcript.Gene]}\"; ");
        }
    }
}
