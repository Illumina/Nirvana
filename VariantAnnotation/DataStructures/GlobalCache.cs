using System.Collections.Generic;
using VariantAnnotation.DataStructures.Intervals;
using VariantAnnotation.FileHandling;
using VariantAnnotation.FileHandling.SupplementaryAnnotations;
using VariantAnnotation.FileHandling.TranscriptCache;
using VariantAnnotation.Interface;

namespace VariantAnnotation.DataStructures
{
    public sealed class GlobalCache : IDataSource
    {
        public readonly IFileHeader Header;
        public readonly Transcript.Transcript[] Transcripts;
        public readonly RegulatoryElement[] RegulatoryElements;
        public readonly Gene[] Genes;
        public readonly SimpleInterval[] Introns;
        public readonly SimpleInterval[] MicroRnas;
        public readonly string[] PeptideSeqs;

        private ushort VepVersion => (Header.Custom as GlobalCustomHeader)?.VepVersion ?? 0;
        public GenomeAssembly GenomeAssembly => Header.GenomeAssembly;
        public IEnumerable<IDataSourceVersion> DataSourceVersions => new List<IDataSourceVersion>
        {
            new DataSourceVersion("VEP", VepVersion.ToString(), Header.CreationTimeTicks, Header.TranscriptSource.ToString())
        };

        /// <summary>
        /// constructor
        /// </summary>
        public GlobalCache(IFileHeader header, Transcript.Transcript[] transcripts, RegulatoryElement[] regulatoryElements,
            Gene[] genes, SimpleInterval[] introns, SimpleInterval[] microRnas, string[] peptideSeqs)
        {
            Header             = header;
            Transcripts        = transcripts;
            RegulatoryElements = regulatoryElements;
            Genes              = genes;
            Introns            = introns;
            MicroRnas          = microRnas;
            PeptideSeqs        = peptideSeqs;
        }

        public void Clear() { }
    }
}
