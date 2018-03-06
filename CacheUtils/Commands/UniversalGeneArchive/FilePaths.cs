namespace CacheUtils.Commands.UniversalGeneArchive
{
    // ReSharper disable UnusedAutoPropertyAccessor.Global
    public sealed class FilePaths
    {        
        public AssemblySpecificPaths GRCh37 { get; set; }
        public AssemblySpecificPaths GRCh38 { get; set; }

        // ReSharper disable once ClassNeverInstantiated.Global
        public class AssemblySpecificPaths
        {
            public string ReferencePath { get; set; }
            public string EnsemblCachePath { get; set; }
            public string RefSeqCachePath { get; set; }
        }
    }
    // ReSharper restore UnusedAutoPropertyAccessor.Global
}
