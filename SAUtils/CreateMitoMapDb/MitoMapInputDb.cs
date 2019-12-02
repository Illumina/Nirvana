using System.Collections.Generic;

namespace SAUtils.CreateMitoMapDb
{
    public struct MitoMapInputDb
    {
        public Dictionary<string, string> InternalReferenceIdToPubmedId { get; }

        public MitoMapInputDb(Dictionary<string, string> internalReferenceIdToPubmedId)
        {
            InternalReferenceIdToPubmedId = internalReferenceIdToPubmedId;
        }
    }
}