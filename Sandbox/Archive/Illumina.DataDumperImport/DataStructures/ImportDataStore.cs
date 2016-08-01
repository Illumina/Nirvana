using Illumina.DataDumperImport.DataStructures.VEP;
using System.Collections.Generic;
using System.Linq;

namespace Illumina.DataDumperImport.DataStructures
{
    public class ImportDataStore
    {
        #region members

        // store our transcripts
        private readonly HashSet<string> _transcriptIds = new HashSet<string>();
        public readonly List<Transcript> Transcripts  = new List<Transcript>();

        // store our regulatory features
        private readonly HashSet<string> _regulatoryFeatureIds       = new HashSet<string>();
        public readonly List<RegulatoryFeature> RegulatoryFeatures = new List<RegulatoryFeature>();

        // blacklist and whitelist management
        private bool _removeUnwantedTranscripts;
        private bool _useWhiteList;
        private List<string> _whiteListPrefixes;

        private int _numFilteredWhiteList;
        private int _numFilteredBlackList;
        private int _numFilteredNonUnique;
        private int _numFilteredNoId;

        public ushort CurrentReferenceIndex;

        #endregion

        /// <summary>
        /// activates the whitelist filter
        /// </summary>
        public void EnableWhiteList(List<string> whiteListPrefixes)
        {
            _removeUnwantedTranscripts = true;
            _useWhiteList              = true;
            _whiteListPrefixes         = whiteListPrefixes;
        }

        /// <summary>
        /// clears the temporary data
        /// </summary>
        public void Clear()
        {
            _transcriptIds.Clear();
            Transcripts.Clear();
            _regulatoryFeatureIds.Clear();
            RegulatoryFeatures.Clear();

            _numFilteredWhiteList = 0;
            _numFilteredBlackList = 0;
            _numFilteredNonUnique = 0;
            _numFilteredNoId      = 0;
        }

        /// <summary>
        /// copies the genes, exons, slices, transcripts, and mapper pairs to another set of data structures
        /// </summary>
        public void CopyDataFrom(ImportDataStore other)
        {
            // copy transcripts
            foreach (var transcript in other.Transcripts)
            {
                // skip transcripts without identifiers (e.g. tRNA biotype in RefSeq VEP79)
                if (string.IsNullOrEmpty(transcript.StableId))
                {
                    _numFilteredNoId++;
                    continue;
                }

                // make sure we only keep transcripts
                if (_removeUnwantedTranscripts)
                {
                    // handle transcripts like NM_, XM_, NR_, etc.
                    if (_useWhiteList && !FoundPrefix(transcript.StableId, _whiteListPrefixes))
                    {
                        _numFilteredWhiteList++;
                        continue;
                    }
                }

                // only allow unique transcripts
                var transcriptKey = $"{transcript.StableId}.{transcript.Start}.{transcript.End}";

                if (!_transcriptIds.Contains(transcriptKey))
                {
                    Transcripts.Add(transcript);
                    _transcriptIds.Add(transcriptKey);
                }
                else
                {
                    _numFilteredNonUnique++;
                }
            }

            // copy regulatory features
            foreach (var regulatoryFeature in other.RegulatoryFeatures)
            {
                // skip regulatory features without identifiers
                if (string.IsNullOrEmpty(regulatoryFeature.StableId)) continue;

                // only allow unique regulatory features
                if (!_regulatoryFeatureIds.Contains(regulatoryFeature.StableId))
                {
                    RegulatoryFeatures.Add(regulatoryFeature);
                    _regulatoryFeatureIds.Add(regulatoryFeature.StableId);
                }
            }
        }

        /// <summary>
        /// returns true if the transcript contains a prefix from the specified list
        /// </summary>
        private static bool FoundPrefix(string transcriptId, List<string> prefixes)
        {
            return prefixes.Any(transcriptId.StartsWith);
        }

        /// <summary>
        /// returns a string representation of the import data store
        /// </summary>
        public override string ToString()
        {
            int numFiltered = _numFilteredNonUnique + _numFilteredBlackList + _numFilteredWhiteList + _numFilteredNoId;

            return
                $"{Transcripts.Count} transcripts: {numFiltered} filt.: {_numFilteredNonUnique} !unique, {_numFilteredWhiteList} !white, {_numFilteredBlackList} black, {_numFilteredNoId} w/o ID, {RegulatoryFeatures.Count} regulatory";
        }
    }
}
