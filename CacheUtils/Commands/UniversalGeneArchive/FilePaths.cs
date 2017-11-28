namespace CacheUtils.Commands.UniversalGeneArchive
{
    // ReSharper disable UnusedAutoPropertyAccessor.Global
    public sealed class FilePaths
    {        
        public string HgncPath { get; set; }        
        public string GeneInfoPath { get; set; }
        public AssemblySpecificPaths GRCh37 { get; set; }
        public AssemblySpecificPaths GRCh38 { get; set; }

        // ReSharper disable once ClassNeverInstantiated.Global
        public class AssemblySpecificPaths
        {
            public string ReferencePath { get; set; }
            public string AssemblyInfoPath { get; set; }
            public string EnsemblCachePath { get; set; }
            public string RefSeqCachePath { get; set; }
            public string EnsemblGtfPath { get; set; }
            public string RefSeqGenomeGffPath { get; set; }
            public string RefSeqGffPath { get; set; }
        }
    }
    // ReSharper restore UnusedAutoPropertyAccessor.Global
}
