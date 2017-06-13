using System;
using System.Collections.Generic;
using System.Linq;
using CacheUtils.CreateCache.FileHandling;
using ErrorHandling.Exceptions;
using VariantAnnotation.DataStructures;
using VariantAnnotation.DataStructures.Transcript;
using VariantAnnotation.Utilities;

namespace CacheUtils.CreateCache.Algorithms
{
    public sealed class CanonicalTranscriptMarker
    {
        #region members

        private readonly HashSet<string> _lrgEntries;

        #endregion

        /// <summary>
        /// constructor
        /// </summary>
        public CanonicalTranscriptMarker(string lrgPath)
        {
            _lrgEntries = LrgReader.GetTranscriptIds(lrgPath);
        }

        /// <summary>
        /// marks the canonical transcripts
        /// </summary>
        public int MarkTranscripts(List<Transcript> transcripts)
        {
            var genes = AggregateGenes(transcripts);
            var canonicalTranscripts = GetCanonicalTranscripts(genes);
            return SetCanonicalFlags(canonicalTranscripts, transcripts);
        }

        /// <summary>
        /// returns a mapping of gene ID to canonical transcript ID
        /// </summary>
        private static SortedDictionary<int, string> GetCanonicalTranscripts(SortedDictionary<int, HashSet<TranscriptMetadata>> genes)
        {
            // - Order all of the overlapping transcripts by cds length
            // - Pick the longest transcript that has an associated Locus Reference Genome (LRG) sequence
            // - If no LRGs exist for the set of transcripts, pick the longest transcript that is coding
            // - If there is a tie, pick the transcript with the smaller accession id number
            var canonicalTranscripts = new SortedDictionary<int, string>();

            foreach (var kvp in genes)
            {
                // ====================================
                // examine normal transcripts (NM & NR)
                // ====================================

                var sortedTranscripts = GetSortedNmNrTranscripts(kvp.Value);

                // pick the transcript with the smallest accession
                if (sortedTranscripts.Count > 0)
                {
                    canonicalTranscripts[kvp.Key] = sortedTranscripts[0].TranscriptId;
                }
            }

            return canonicalTranscripts;
        }

        /// <summary>
        /// returns a dictionary that aggregates the transcripts by gene
        /// </summary>
        private SortedDictionary<int, HashSet<TranscriptMetadata>> AggregateGenes(List<Transcript> transcripts)
        {
            var genes = new SortedDictionary<int, HashSet<TranscriptMetadata>>();

            foreach (var transcript in transcripts)
            {
                int geneId           = GetGeneId(transcript);
                int cdsLength        = GetCdsLength(transcript);
                int transcriptLength = transcript.End - transcript.Start + 1;
                var isLrg            = _lrgEntries.Contains(transcript.Id.ToString());

                var metadata = new TranscriptMetadata(transcript.Id, transcriptLength, cdsLength, isLrg);

                HashSet<TranscriptMetadata> observedMetadata;
                if (genes.TryGetValue(geneId, out observedMetadata))
                {
                    observedMetadata.Add(metadata);
                }
                else
                {
                    observedMetadata = new HashSet<TranscriptMetadata> { metadata };
                    genes[geneId] = observedMetadata;
                }
            }

            return genes;
        }

        private static int GetCdsLength(Transcript transcript)
        {
            if (transcript.Translation == null) return 0;

            var codingRegion = transcript.Translation.CodingRegion;
            return codingRegion.CdnaEnd - codingRegion.CdnaStart + 1;
        }

        private static int GetGeneId(Transcript transcript)
        {
            var entrezGeneId = transcript.Gene.EntrezGeneId.ToString();

            int geneId;           
            if (string.IsNullOrEmpty(entrezGeneId)) throw new UserErrorException($"Expected a non-empty Entrez gene ID during canonical aggregation: {transcript.Gene}");

            if (!int.TryParse(entrezGeneId, out geneId))
            {
                throw new UserErrorException($"Unable to convert Entrez gene ID ({entrezGeneId}) to an integer: {transcript.Gene}");
            }

            return geneId;
        }

        /// <summary>
        /// clears the canonical flags from all transcripts and adds the new canonical transcripts
        /// </summary>
        private static int SetCanonicalFlags(SortedDictionary<int, string> canonicalTranscripts, List<Transcript> transcripts)
        {
            int numCanonicalFlagsChanged = 0;

            for (int i = 0; i < transcripts.Count; i++)
            {
                var transcript = transcripts[i];
                int geneId     = GetGeneId(transcript);

                // no canonical transcript
                string canonicalTranscriptId;
                if (!canonicalTranscripts.TryGetValue(geneId, out canonicalTranscriptId))
                {
                    if (transcript.IsCanonical)
                    {
                        transcripts[i] = UpdateCanonical(transcript, false);
                        numCanonicalFlagsChanged++;
                    }
                }

                // change the transcripts as needed
                if (transcript.Id.ToString() == canonicalTranscriptId)
                {
                    if (!transcript.IsCanonical)
                    {
                        transcripts[i] = UpdateCanonical(transcript, true);
                        numCanonicalFlagsChanged++;
                    }
                }
                else
                {
                    if (transcript.IsCanonical)
                    {
                        transcripts[i] = UpdateCanonical(transcript, false);
                        numCanonicalFlagsChanged++;
                    }
                }
            }

            return numCanonicalFlagsChanged;
        }

        private static Transcript UpdateCanonical(Transcript transcript, bool isCanonical)
        {
            return new Transcript(transcript.ReferenceIndex, transcript.Start, transcript.End, transcript.Id,
                transcript.Version, transcript.Translation, transcript.BioType, transcript.Gene,
                transcript.TotalExonLength, transcript.StartExonPhase, isCanonical, transcript.Introns,
                transcript.MicroRnas, transcript.CdnaMaps, transcript.SiftIndex, transcript.PolyPhenIndex,
                transcript.TranscriptSource);
        }

        /// <summary>
        /// returns a sorted list of all the transcripts that have an NM_ or NR_ prefix
        /// </summary>
        private static List<TranscriptMetadata> GetSortedNmNrTranscripts(HashSet<TranscriptMetadata> transcripts)
        {
            var selectedTranscripts =
                transcripts.Where(
                    transcript => transcript.TranscriptId.StartsWith("NM_") ||
                    transcript.TranscriptId.StartsWith("NR_")).ToList();

            return selectedTranscripts.OrderByDescending(x => x.IsLrg)
                    .ThenByDescending(x => x.CdsLength)
                    .ThenByDescending(x => x.TranscriptLength)
                    .ThenBy(x => x.AccessionNumber)
                    .ToList();
        }

        public sealed class TranscriptMetadata : IEquatable<TranscriptMetadata>
        {
            public readonly string TranscriptId;
            public readonly int CdsLength;
            public readonly int TranscriptLength;
            public readonly bool IsLrg;
            public readonly int AccessionNumber;            

            #region IEquatable Overrides

            public override int GetHashCode()
            {
                return TranscriptId.GetHashCode() ^ CdsLength.GetHashCode() ^ TranscriptLength.GetHashCode();
            }

            /// <summary>
            /// Indicates whether the current object is equal to another object of the same type.
            /// </summary>
            public bool Equals(TranscriptMetadata value)
            {
                if (this == null) throw new NullReferenceException();
                if (value == null) return false;
                if (this == value) return true;
                return TranscriptId == value.TranscriptId && CdsLength == value.CdsLength &&
                       TranscriptLength == value.TranscriptLength;
            }

            #endregion

            /// <summary>
            /// constructor
            /// </summary>
            public TranscriptMetadata(CompactId transcriptId, int transcriptLength, int cdsLength, bool isLrg)
            {
                var id = transcriptId.ToString();

                TranscriptId     = id;
                TranscriptLength = transcriptLength;
                CdsLength        = cdsLength;
                IsLrg            = isLrg;
                AccessionNumber  = GetAccessionNumber(id);
            }

            private static int GetAccessionNumber(string transcriptId)
            {
                int accession;

                int firstUnderLine = transcriptId.IndexOf('_');
                if (firstUnderLine != -1) transcriptId = transcriptId.Substring(firstUnderLine + 1);
                var tuple = FormatUtilities.SplitVersion(transcriptId);

                return int.TryParse(tuple.Item1, out accession) ? accession : 0;
            }
        }
    }
}
