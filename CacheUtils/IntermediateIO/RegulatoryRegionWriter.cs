using System;
using System.IO;
using VariantAnnotation.Interface.AnnotatedPositions;

namespace CacheUtils.IntermediateIO
{
    internal sealed class RegulatoryRegionWriter : IDisposable
    {
        private readonly StreamWriter _writer;

        internal RegulatoryRegionWriter(StreamWriter writer, IntermediateIoHeader header)
        {
            _writer = writer;
            _writer.NewLine = "\n";
            header.Write(_writer, IntermediateIoCommon.FileType.Regulatory);
        }

        internal void Write(IRegulatoryRegion region) => _writer.WriteLine(
            $"{region.Chromosome.UcscName}\t{region.Chromosome.Index}\t{region.Start}\t{region.End}\t{region.Id}\t{region.Type}\t{(byte) region.Type}");

        public void Dispose() => _writer.Dispose();
    }
}
