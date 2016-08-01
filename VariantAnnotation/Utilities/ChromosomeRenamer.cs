using System;
using System.Collections.Generic;
using VariantAnnotation.FileHandling;

namespace VariantAnnotation.Utilities
{
    /// <summary>
    /// converts between UCSC-style and Ensembl-style chromosome names
    /// </summary>
    public class ChromosomeRenamer
    {
        #region members

        private readonly Dictionary<string, string> _ensemblToUcscReferenceSequenceNames;
        private readonly Dictionary<string, string> _ucscToEnsemblReferenceSequenceNames;
        private readonly HashSet<string> _inVepReferenceSequenceNames; 
        private bool _hasReferenceMetadata;

        public readonly Dictionary<string, int> EnsemblReferenceSequenceIndex;  

        #endregion

        /// <summary>
        /// constructor
        /// </summary>
        public ChromosomeRenamer()
        {
            EnsemblReferenceSequenceIndex        = new Dictionary<string, int>();
            _ensemblToUcscReferenceSequenceNames = new Dictionary<string, string>();
            _ucscToEnsemblReferenceSequenceNames = new Dictionary<string, string>();
            _inVepReferenceSequenceNames         = new HashSet<string>();
        }

        /// <summary>
        /// adds reference metadata from the 
        /// </summary>
        public void AddReferenceMetadata(List<ReferenceMetadata> refMetadataList)
        {
            _hasReferenceMetadata = true;
            var ensemblReferenceNames = new List<string>();
            int index = 0;

            foreach (var refMetadata in refMetadataList)
            {
                AddReferenceName(refMetadata.EnsemblName, refMetadata.UcscName);

                if (refMetadata.InVep)
                {
                    _inVepReferenceSequenceNames.Add(refMetadata.EnsemblName);
                    _inVepReferenceSequenceNames.Add(refMetadata.UcscName);
                }

                var ensemblReferenceName = refMetadata.EnsemblName;
                if (string.IsNullOrEmpty(ensemblReferenceName)) ensemblReferenceName = refMetadata.UcscName;
                if (string.IsNullOrEmpty(ensemblReferenceName)) continue;

                ensemblReferenceNames.Add(ensemblReferenceName);
                EnsemblReferenceSequenceIndex[ensemblReferenceName] = index++;
            }

            ensemblReferenceNames.ToArray();
        }

        /// <summary>
        /// adds a Ensembl/UCSC reference name pair to the current dictionary
        /// </summary>
        private void AddReferenceName(string ensemblReferenceName, string ucscReferenceName)
        {
            bool isUcscEmpty    = string.IsNullOrEmpty(ucscReferenceName);
            bool isEnsemblEmpty = string.IsNullOrEmpty(ensemblReferenceName);

            // sanity check: make sure we have at least one reference name
            if (isUcscEmpty && isEnsemblEmpty) return;

            if (isUcscEmpty)
            {
                _ucscToEnsemblReferenceSequenceNames[ensemblReferenceName] = ensemblReferenceName;
                _ensemblToUcscReferenceSequenceNames[ensemblReferenceName] = ensemblReferenceName;
                return;
            }

            if (isEnsemblEmpty)
            {
                _ucscToEnsemblReferenceSequenceNames[ucscReferenceName] = ucscReferenceName;
                _ensemblToUcscReferenceSequenceNames[ucscReferenceName] = ucscReferenceName;
                return;
            }

            // normal situation
            _ucscToEnsemblReferenceSequenceNames[ucscReferenceName]    = ensemblReferenceName;
            _ensemblToUcscReferenceSequenceNames[ensemblReferenceName] = ucscReferenceName;
        }

        /// <summary>
        /// returns the Ensembl reference name if an UCSC reference name is encountered.
        /// </summary>
        public string GetEnsemblReferenceName(string ucscReferenceName, bool useOriginalOnFailedLookup = true)
        {
            if (!_hasReferenceMetadata)
            {
                throw new InvalidOperationException("Tried to use the chromosome renamer before it was initialized.");
            }

            if (ucscReferenceName == null) return null;

            string ensemblReferenceName;

            if (_ucscToEnsemblReferenceSequenceNames.TryGetValue(ucscReferenceName, out ensemblReferenceName))
            {
                return ensemblReferenceName;
            }

            return useOriginalOnFailedLookup ? ucscReferenceName : null;
        }

        /// <summary>
        /// returns the UCSC reference name if an Ensembl reference name is encountered.
        /// </summary>
        public string GetUcscReferenceName(string ensemblReferenceName, bool useOriginalOnFailedLookup = true)
        {
            if (!_hasReferenceMetadata)
            {
                throw new InvalidOperationException("Tried to use the chromosome renamer before it was initialized.");
            }

            if (ensemblReferenceName == null) return null;

            string ucscReferenceName;
            if (_ensemblToUcscReferenceSequenceNames.TryGetValue(ensemblReferenceName, out ucscReferenceName))
            {
                return ucscReferenceName;
            }

            return useOriginalOnFailedLookup ? ensemblReferenceName : null;
        }

        /// <summary>
        /// returns true if the specified reference sequence is in the standard reference sequences and in VEP
        /// </summary>
        public bool InReferenceAndVep(string referenceName)
        {
            return _inVepReferenceSequenceNames.Contains(referenceName);
        }
    }
}
