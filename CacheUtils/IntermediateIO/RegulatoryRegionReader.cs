using System;
using System.Collections.Generic;
using System.IO;
using Genome;
using IO;
using OptimizedCore;
using VariantAnnotation.AnnotatedPositions.Transcript;
using VariantAnnotation.Caches.DataStructures;
using VariantAnnotation.Interface.AnnotatedPositions;
using VariantAnnotation.Interface.Caches;

namespace CacheUtils.IntermediateIO
{
    internal sealed class RegulatoryRegionReader : IDisposable
    {
        private readonly IDictionary<ushort, IChromosome> _refIndexToChromosome;
        private readonly StreamReader _reader;

        internal RegulatoryRegionReader(Stream stream, IDictionary<ushort, IChromosome> refIndexToChromosome)
        {
            _refIndexToChromosome = refIndexToChromosome;
            _reader = FileUtilities.GetStreamReader(stream);
            IntermediateIoCommon.ReadHeader(_reader, IntermediateIoCommon.FileType.Regulatory);
        }

        public IRegulatoryRegion[] GetRegulatoryRegions()
        {
            var regulatoryRegions = new List<IRegulatoryRegion>();

            while (true)
            {
                var regulatoryRegion = GetNextRegulatoryRegion();
                if (regulatoryRegion == null) break;
                regulatoryRegions.Add(regulatoryRegion);
            }

            return regulatoryRegions.ToArray();
        }

        private IRegulatoryRegion GetNextRegulatoryRegion()
        {
            string line = _reader.ReadLine();
            if (line == null) return null;

            var cols              = line.OptimizedSplit('\t');
            ushort referenceIndex = ushort.Parse(cols[1]);
            int start             = int.Parse(cols[2]);
            int end               = int.Parse(cols[3]);
            var id                = CompactId.Convert(cols[4]);
            var type              = (RegulatoryRegionType)byte.Parse(cols[6]);

            var chromosome = ReferenceNameUtilities.GetChromosome(_refIndexToChromosome, referenceIndex);
            return new RegulatoryRegion(chromosome, start, end, id, type);
        }

        public void Dispose() => _reader.Dispose();
    }
}
