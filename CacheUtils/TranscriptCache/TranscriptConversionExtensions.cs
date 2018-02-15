using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CacheUtils.DataDumperImport.DataStructures.Mutable;
using VariantAnnotation.AnnotatedPositions.Transcript;
using VariantAnnotation.Caches.DataStructures;
using VariantAnnotation.Interface.AnnotatedPositions;
using VariantAnnotation.Interface.Intervals;

namespace CacheUtils.TranscriptCache
{
    public static class TranscriptConversionExtensions
    {
        public static IEnumerable<ITranscript> ToTranscripts(this MutableTranscript[] mutableTranscripts)
        {
            var transcripts = new List<ITranscript>(mutableTranscripts.Length);
            transcripts.AddRange(mutableTranscripts.Select(mt => mt.ToTranscript()));
            return transcripts;
        }

        private static ITranscript ToTranscript(this MutableTranscript mt)
        {
            var translation = mt.CodingRegion == null ? null : new Translation(mt.CodingRegion, CompactId.Convert(mt.ProteinId, mt.ProteinVersion),
                mt.PeptideSequence);

            var transcriptRegions = TranscriptRegionMerger.GetTranscriptRegions(mt.CdnaMaps, mt.Exons, mt.Introns, mt.Gene.OnReverseStrand);
            CheckTranscriptRegions(mt.Id, mt.CdnaMaps, mt.Exons, mt.Introns, transcriptRegions);

            var startExonPhase  = mt.StartExonPhase < 0 ? (byte)0 : (byte)mt.StartExonPhase;
            var sortedMicroRnas = mt.MicroRnas?.OrderBy(x => x.Start).ToArray();

            return new Transcript(mt.Chromosome, mt.Start, mt.End, CompactId.Convert(mt.Id, mt.Version), translation,
                mt.BioType, mt.UpdatedGene, mt.TotalExonLength, startExonPhase, mt.IsCanonical, transcriptRegions,
                (ushort)mt.Exons.Length, sortedMicroRnas, mt.SiftIndex, mt.PolyPhenIndex, mt.Source,
                mt.CdsStartNotFound, mt.CdsEndNotFound, mt.SelenocysteinePositions, mt.RnaEdits);
        }

        private static void CheckTranscriptRegions(string transcriptId, MutableTranscriptRegion[] cdnaMaps,
            MutableExon[] exons, IInterval[] introns, ITranscriptRegion[] regions)
        {
            try
            {
                foreach (var region in regions)
                {
                    if (region.Id == 0)       throw new InvalidDataException($"Expected transcript ({transcriptId}) to have regions with non-zero IDs.");
                    if (region.CdnaStart < 1) throw new InvalidDataException($"Expected transcript ({transcriptId}) to have regions with true cDNA start positions.");
                    if (region.CdnaEnd < 1)   throw new InvalidDataException($"Expected transcript ({transcriptId}) to have regions with true cDNA end positions.");

                    if (region.Type != TranscriptRegionType.Exon && region.Type != TranscriptRegionType.Intron &&
                        region.Type != TranscriptRegionType.Gap)
                        throw new InvalidDataException($"Found unexpected transcript region type ({region.Type}) in transcript ({transcriptId}).");
                }

                // check contiguity of genomic coordinates
                if (regions.Length > 1)
                {
                    for (int i = 1; i < regions.Length; i++)
                    {
                        var prevRegion = regions[i - 1];
                        var region = regions[i];

                        var delta = region.Start - prevRegion.End;
                        if (delta != 1) throw new InvalidDataException($"Found non-contiguous genomic coordinates in transcript regions in transcript ({transcriptId}).");
                    }
                }
            }
            catch (Exception)
            {
                DumpTranscriptRegions(regions);
                DumpExons(exons);
                DumpIntrons(introns);
                DumpCdnaMaps(cdnaMaps);
                throw;
            }
        }

        private static void DumpTranscriptRegions(ITranscriptRegion[] regions)
        {
            Console.WriteLine("\ntranscript regions:");
            foreach(var region in regions) DumpTranscriptRegion(region);
        }

        private static void DumpTranscriptRegion(ITranscriptRegion region)
        {
            Console.WriteLine($"{region.Type}\t{region.Id}\t{region.Start}\t{region.End}\t{region.CdnaStart}\t{region.CdnaEnd}");
        }

        private static void DumpCdnaMaps(MutableTranscriptRegion[] cdnaMaps)
        {
            Console.WriteLine("\ncDNA maps:");
            foreach (var cdnaMap in cdnaMaps.OrderBy(x => x.Start).ThenBy(x => x.End)) DumpCdnaMap(cdnaMap);
        }

        private static void DumpCdnaMap(MutableTranscriptRegion cdnaMap)
        {
            Console.WriteLine($"{cdnaMap.Start}\t{cdnaMap.End}\t{cdnaMap.CdnaStart}\t{cdnaMap.CdnaEnd}");
        }

        private static void DumpIntrons(IInterval[] introns)
        {
            Console.WriteLine("\nIntrons:");
            foreach (var intron in introns.OrderBy(x => x.Start).ThenBy(x => x.End)) DumpIntron(intron);
        }

        private static void DumpIntron(IInterval intron)
        {
            Console.WriteLine($"{intron.Start}\t{intron.End}");
        }

        private static void DumpExons(MutableExon[] exons)
        {
            Console.WriteLine("\nExons:");
            foreach (var exon in exons.OrderBy(x => x.Start).ThenBy(x => x.End)) DumpExon(exon);
        }

        private static void DumpExon(MutableExon exon)
        {
            Console.WriteLine($"{exon.Start}\t{exon.End}");
        }
    }
}
