using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CacheUtils.DataDumperImport.DataStructures.Mutable;
using Intervals;
using VariantAnnotation.Interface.AnnotatedPositions;

namespace CacheUtils.TranscriptCache
{
    public static class TranscriptRegionValidater
    {
        public static void Validate(string transcriptId, IEnumerable<MutableTranscriptRegion> cdnaMaps,
            IEnumerable<MutableExon> exons, IEnumerable<IInterval> introns, ITranscriptRegion[] regions)
        {
            try
            {
                ValidateRegions(transcriptId, regions);
                if (regions.Length <= 1) return;
                CheckGenomicCoordinateContiguity(transcriptId, regions);
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

        private static void CheckGenomicCoordinateContiguity(string transcriptId, IReadOnlyList<ITranscriptRegion> regions)
        {
            for (var i = 1; i < regions.Count; i++)
            {
                var prevRegion = regions[i - 1];
                var region     = regions[i];

                int delta = region.Start - prevRegion.End;
                if (delta != 1) throw new InvalidDataException($"Found non-contiguous genomic coordinates in transcript regions in transcript ({transcriptId}).");
            }
        }

        private static void ValidateRegions(string transcriptId, IEnumerable<ITranscriptRegion> regions)
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
        }

        private static void DumpTranscriptRegions(IEnumerable<ITranscriptRegion> regions)
        {
            Console.WriteLine("\ntranscript regions:");
            foreach (var region in regions) DumpTranscriptRegion(region);
        }

        private static void DumpTranscriptRegion(ITranscriptRegion region) => Console.WriteLine($"{region.Type}\t{region.Id}\t{region.Start}\t{region.End}\t{region.CdnaStart}\t{region.CdnaEnd}");

        private static void DumpCdnaMaps(IEnumerable<MutableTranscriptRegion> cdnaMaps)
        {
            Console.WriteLine("\ncDNA maps:");
            foreach (var cdnaMap in cdnaMaps.OrderBy(x => x.Start).ThenBy(x => x.End)) DumpCdnaMap(cdnaMap);
        }

        private static void DumpCdnaMap(ITranscriptRegion cdnaMap) => Console.WriteLine($"{cdnaMap.Start}\t{cdnaMap.End}\t{cdnaMap.CdnaStart}\t{cdnaMap.CdnaEnd}");

        private static void DumpIntrons(IEnumerable<IInterval> introns)
        {
            Console.WriteLine("\nIntrons:");
            foreach (var intron in introns.OrderBy(x => x.Start).ThenBy(x => x.End)) DumpIntron(intron);
        }

        private static void DumpIntron(IInterval intron) => Console.WriteLine($"{intron.Start}\t{intron.End}");

        private static void DumpExons(IEnumerable<MutableExon> exons)
        {
            Console.WriteLine("\nExons:");
            foreach (var exon in exons.OrderBy(x => x.Start).ThenBy(x => x.End)) DumpExon(exon);
        }

        private static void DumpExon(IInterval exon) => Console.WriteLine($"{exon.Start}\t{exon.End}");
    }
}
