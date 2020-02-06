using System.Collections.Generic;
using System.IO;
using System.Linq;
using Compression.Utilities;

namespace AminoAcidAligner
{
    public static class Utilities
    {
        public static (string TranscriptId, string Species, string Chromosome) ParseSequenceName(string name)
        {
            //>ENST00000641515.2_hg38_1_2 3 0 0 chr1:65565-65573+
            var terms = name.Split('_', ' ', '\t');
            var transcriptId = terms[0].TrimStart('>'); 
            //remove versions for Ensembl transcripts
            if (transcriptId.StartsWith("ENST")) transcriptId = transcriptId.Split('.')[0];

            var species = terms[1];
            string chrom = null;
            chrom = terms.Length > 7 && string.IsNullOrEmpty(terms[7])? null: terms[7].Split(':')[0];
            
            return (transcriptId, species, chrom);
        }

        /// <summary>
        /// using the CCDS file to find equivalence between Ensembl 
        /// </summary>
        /// <param name="fileName">input file name</param>
        /// <returns>ensembl to RefSeq transcript mapping</returns>
        //#ccds   original_member current_member  source  nucleotide_ID   protein_ID      status_in_CCDS  sequence_status
        // CCDS2.2 1       0       NCBI    NM_152486.2     NP_689699.2     Updated 0
        // CCDS2.2 0       1       NCBI    NM_152486.3     NP_689699.2     Accepted        1
        // CCDS2.2 1       1       EBI,WTSI        ENST00000342066.7       ENSP00000342313.3       Accepted 
        public static List<HashSet<string>> GroupTranscripts(string fileName)
        {
            var ccdsToTranscriptIds = new Dictionary<string, HashSet<string>>();
            
            const int ccdsIndex = 0;
            const int transcriptIndex = 4;
            using (var reader = GZipUtilities.GetAppropriateStreamReader(fileName))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    if(line.StartsWith('#')) continue;
                    var terms = line.Split('\t');
                    var ccds = terms[ccdsIndex];
                    var transcriptId = terms[transcriptIndex];
                    //remove versions for Ensembl transcripts
                    if (transcriptId.StartsWith("ENST")) transcriptId = transcriptId.Split('.')[0];

                    if (ccdsToTranscriptIds.TryGetValue(ccds, out var transcriptIds))
                    {
                        transcriptIds.Add(transcriptId);
                    }
                    else ccdsToTranscriptIds.Add(ccds, new HashSet<string>(){transcriptId});
                    
                }
            }

            return ccdsToTranscriptIds.Values.ToList();
        }

        public static Dictionary<string, HashSet<string>> GetEquivalentIds(List<HashSet<string>> transcriptGroups)
        {
            var idToGroup = new Dictionary<string, HashSet<string>>();
            foreach (var transcriptGroup in transcriptGroups)
            {
                foreach (var transcript in transcriptGroup)
                {
                    idToGroup.Add(transcript, transcriptGroup);
                }
            }

            return idToGroup;
        }

    }
}