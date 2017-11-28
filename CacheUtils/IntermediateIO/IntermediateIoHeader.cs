using System.IO;
using VariantAnnotation.Interface.AnnotatedPositions;
using VariantAnnotation.Interface.Sequence;

namespace CacheUtils.IntermediateIO
{
    public sealed class IntermediateIoHeader
    {
        private readonly ushort _vepVersion;
        public readonly long VepReleaseTicks;
        public readonly Source Source;
        public readonly GenomeAssembly GenomeAssembly;
        private readonly int _numRefSeqs;

        public IntermediateIoHeader(ushort vepVersion, long vepReleaseTicks, Source transcriptSource,
            GenomeAssembly genomeAssembly, int numRefSeqs)
        {
            _vepVersion      = vepVersion;
            VepReleaseTicks = vepReleaseTicks;
            Source          = transcriptSource;
            GenomeAssembly  = genomeAssembly;
            _numRefSeqs      = numRefSeqs;
        }

        internal void Write(StreamWriter writer, IntermediateIoCommon.FileType fileType)
        {
            writer.WriteLine($"{IntermediateIoCommon.Header}\t{(byte)fileType}");
            writer.WriteLine($"{_vepVersion}\t{VepReleaseTicks}\t{(byte)Source}\t{(byte)GenomeAssembly}\t{_numRefSeqs}");
        }

        internal static (string Id, IntermediateIoCommon.FileType Type, IntermediateIoHeader Header) Read(StreamReader reader)
        {
            var cols  = reader.ReadLine().Split('\t');
            var cols2 = reader.ReadLine().Split('\t');

            var id              = cols[0];
            var type            = (IntermediateIoCommon.FileType)byte.Parse(cols[1]);

            var vepVersion      = ushort.Parse(cols2[0]);
            var vepReleaseTicks = long.Parse(cols2[1]);
            var source          = (Source)byte.Parse(cols2[2]);
            var genomeAssembly  = (GenomeAssembly)byte.Parse(cols2[3]);
            var numRefSeqs      = int.Parse(cols2[4]);

            var header = new IntermediateIoHeader(vepVersion, vepReleaseTicks, source, genomeAssembly, numRefSeqs);
            return (id, type, header);
        }
    }
}
