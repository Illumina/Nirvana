using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CacheUtils.DataDumperImport.DataStructures.Mutable;
using CacheUtils.Utilities;

namespace CacheUtils.TranscriptCache
{
    public sealed class CanonicalTranscriptMarker
    {
        private readonly HashSet<string> _lrgTranscriptIds;

        public CanonicalTranscriptMarker(HashSet<string> lrgTranscriptIds)
        {
            _lrgTranscriptIds = lrgTranscriptIds;
        }

        public int MarkTranscripts(MutableTranscript[] transcripts)
        {
            var transcriptsByGeneId          = GetTranscriptsByEntrezGeneId(transcripts);
            var canonicalTranscriptsByGeneId = GetCanonicalTranscriptsByGeneId(transcriptsByGeneId);
            return SetCanonicalFlags(canonicalTranscriptsByGeneId, transcripts);
        }

        private SortedDictionary<int, HashSet<TranscriptMetadata>> GetTranscriptsByEntrezGeneId(IEnumerable<MutableTranscript> transcripts)
        {
            var genes = new SortedDictionary<int, HashSet<TranscriptMetadata>>();

            foreach (var transcript in transcripts)
            {
                string idWithVersion = transcript.Id + '.' + transcript.Version;

                int cdsLength        = transcript.CodingRegion?.Length ?? 0;
                int transcriptLength = transcript.End - transcript.Start + 1;
                bool isLrg           = _lrgTranscriptIds.Contains(transcript.Id);
                int accession        = AccessionUtilities.GetAccessionNumber(transcript.Id);

                var metadata = new TranscriptMetadata(idWithVersion, accession, transcriptLength, cdsLength, isLrg);
                int geneId   = ConvertGeneIdToInt(transcript.Gene.GeneId);

                if (genes.TryGetValue(geneId, out var observedMetadata)) observedMetadata.Add(metadata);
                else genes[geneId] = new HashSet<TranscriptMetadata> { metadata };
            }

            return genes;
        }

        private static SortedDictionary<int, string> GetCanonicalTranscriptsByGeneId(SortedDictionary<int, HashSet<TranscriptMetadata>> genes)
        {
            // - Order all of the overlapping transcripts by cds length
            // - Pick the longest transcript that has an associated Locus Reference Genome (LRG) sequence
            // - If no LRGs exist for the set of transcripts, pick the longest transcript that is coding
            // - If there is a tie, pick the transcript with the smaller accession id number
            var canonicalTranscripts = new SortedDictionary<int, string>();

            foreach (var kvp in genes)
            {
                var sortedTranscripts = GetSortedTrustedTranscripts(kvp.Value);

                // pick the transcript with the smallest accession
                if (sortedTranscripts.Count > 0) canonicalTranscripts[kvp.Key] = sortedTranscripts[0].TranscriptId;
            }

            return canonicalTranscripts;
        }

        private static int ConvertGeneIdToInt(string geneId)
        {
            if (string.IsNullOrEmpty(geneId)) throw new InvalidDataException("Expected a non-empty Entrez gene ID during canonical aggregation.");
            if (geneId.StartsWith("ENSG")) geneId = geneId.Substring(4);
            if (!int.TryParse(geneId, out int geneIdNumber)) throw new InvalidDataException($"Unable to convert Entrez gene ID ({geneId}) to an integer.");
            return geneIdNumber;
        }

        private static int SetCanonicalFlags(IReadOnlyDictionary<int, string> canonicalTranscriptsByGeneId, IEnumerable<MutableTranscript> transcripts)
        {
            var numCanonicalTranscripts = 0;

            foreach (var transcript in transcripts)
            {
                int geneId = ConvertGeneIdToInt(transcript.Gene.GeneId);
                transcript.IsCanonical = false;

                // no canonical transcript
                if (!canonicalTranscriptsByGeneId.TryGetValue(geneId, out string canonicalTranscriptId)) continue;
                string idWithVersion = transcript.Id + '.' + transcript.Version;
                if (idWithVersion != canonicalTranscriptId) continue;

                // mark the transcript canonical
                transcript.IsCanonical = true;
                numCanonicalTranscripts++;
            }

            return numCanonicalTranscripts;
        }

        /// <summary>
        /// returns a sorted list of all the transcripts that have an ENST, NM_, or NR_ prefix
        /// </summary>
        private static List<TranscriptMetadata> GetSortedTrustedTranscripts(IEnumerable<TranscriptMetadata> transcripts)
        {
            var selectedTranscripts =
                transcripts.Where(
                    transcript => transcript.TranscriptId.StartsWith("ENST") ||
                    transcript.TranscriptId.StartsWith("NM_") ||
                    transcript.TranscriptId.StartsWith("NR_")).ToList();

            return selectedTranscripts.OrderByDescending(x => x.IsLrg)
                    .ThenByDescending(x => x.CdsLength)
                    .ThenByDescending(x => x.TranscriptLength)
                    .ThenBy(x => x.Accession)
                    .ToList();
        }

        public sealed class TranscriptMetadata : IEquatable<TranscriptMetadata>
        {
            public readonly string TranscriptId;
            public readonly int CdsLength;
            public readonly int TranscriptLength;
            public readonly bool IsLrg;
            public readonly int Accession;

            public TranscriptMetadata(string transcriptId, int accession, int transcriptLength, int cdsLength, bool isLrg)
            {
                TranscriptId     = transcriptId;
                TranscriptLength = transcriptLength;
                CdsLength        = cdsLength;
                IsLrg            = isLrg;
                Accession        = accession;
            }

            public bool Equals(TranscriptMetadata other)
            {
                if (ReferenceEquals(null, other)) return false;
                if (ReferenceEquals(this, other)) return true;
                return string.Equals(TranscriptId, other.TranscriptId) && CdsLength == other.CdsLength &&
                       TranscriptLength == other.TranscriptLength && IsLrg == other.IsLrg &&
                       Accession == other.Accession;
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    int hashCode = TranscriptId != null ? TranscriptId.GetHashCode() : 0;
                    hashCode = (hashCode * 397) ^ CdsLength;
                    hashCode = (hashCode * 397) ^ TranscriptLength;
                    hashCode = (hashCode * 397) ^ IsLrg.GetHashCode();
                    hashCode = (hashCode * 397) ^ Accession;
                    return hashCode;
                }
            }
        }
    }
}
