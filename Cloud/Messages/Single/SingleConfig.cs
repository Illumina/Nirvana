using System.Collections.Generic;

namespace Cloud.Messages.Single
{
    public sealed class SingleConfig
    {
        // ReSharper disable InconsistentNaming
        public string id;
        public string genomeAssembly;
        public SingleVariant variant;
        public int vepVersion;
        public string supplementaryAnnotations;
        public List<SaUrls> customAnnotations;
        // ReSharper restore InconsistentNaming
       
    }
}
