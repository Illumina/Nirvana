using VariantAnnotation.DataStructures.Transcript;
using VariantAnnotation.Interface;

namespace CacheUtils.DataDumperImport.FileHandling
{
    public sealed class GlobalImportHeader
    {
        public ushort VepVersion { get; }
        public long VepReleaseTicks { get; }
        public TranscriptDataSource TranscriptSource { get; }
        public GenomeAssembly GenomeAssembly { get; }

        /// <summary>
        /// constructor
        /// </summary>
        public GlobalImportHeader(ushort vepVersion, long vepReleaseTicks, TranscriptDataSource transcriptSource,
            GenomeAssembly genomeAssembly)
        {
            VepVersion       = vepVersion;
            VepReleaseTicks  = vepReleaseTicks;
            TranscriptSource = transcriptSource;
            GenomeAssembly   = genomeAssembly;
        }
    }
}
