using System.Collections.Generic;

namespace SAUtils.AAConservation
{
    public class ProteinGroup
    {
        public readonly Dictionary<string, HashSet<string>> GeneToTranscripts;
        public ProteinGroup(string transcriptId, string gene)
        {
            GeneToTranscripts = new Dictionary<string, HashSet<string>>();
            GeneToTranscripts.Add(gene, new HashSet<string>(){transcriptId});
        }

        public void AddTranscript(string transcriptId, string geneId)
        {
            if (GeneToTranscripts.TryGetValue(geneId, out var transcriptIds)) transcriptIds.Add(transcriptId);
            else GeneToTranscripts.Add(geneId, new HashSet<string>(){transcriptId});
        }

    }
}