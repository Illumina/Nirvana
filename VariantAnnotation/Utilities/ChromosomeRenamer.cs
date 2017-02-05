using System;
using System.Collections.Generic;
using System.IO;
using VariantAnnotation.DataStructures.CompressedSequence;
using VariantAnnotation.FileHandling;
using VariantAnnotation.Interface;

namespace VariantAnnotation.Utilities
{
    /// <summary>
    /// converts between UCSC-style and Ensembl-style chromosome names
    /// </summary>
    public class ChromosomeRenamer : IChromosomeRenamer
    {
        private readonly Dictionary<string, string> _ensemblToUcscReferenceNames = new Dictionary<string, string>();
        private readonly Dictionary<string, string> _ucscToEnsemblReferenceNames = new Dictionary<string, string>();
        private readonly HashSet<string> _inVepReferenceNames                    = new HashSet<string>();
        private readonly Dictionary<string, ushort> _referenceIndex              = new Dictionary<string, ushort>();

        public int NumRefSeqs { get; private set; }

        public string[] EnsemblReferenceNames { get; private set; }
        public string[] UcscReferenceNames { get; private set; }

        public const ushort UnknownReferenceIndex = ushort.MaxValue;

        /// <summary>
        /// returns true if the specified reference sequence is in the standard reference sequences and in VEP
        /// </summary>
        public bool InReferenceAndVep(string referenceName) => _inVepReferenceNames.Contains(referenceName);

        /// <summary>
        /// adds reference metadata from the compressed sequence reader
        /// </summary>
        public void AddReferenceMetadata(List<ReferenceMetadata> refMetadataList)
        {
            ushort index = 0;
            NumRefSeqs = refMetadataList.Count;

            _ensemblToUcscReferenceNames.Clear();
            _ucscToEnsemblReferenceNames.Clear();
            _inVepReferenceNames.Clear();
            _referenceIndex.Clear();

            var ensemblRefNames = new List<string>();
            var ucscRefNames    = new List<string>();

            foreach (var refMetadata in refMetadataList)
            {
                AddReferenceName(refMetadata.EnsemblName, refMetadata.UcscName, index, ensemblRefNames, ucscRefNames);

                if (refMetadata.InVep)
                {
                    _inVepReferenceNames.Add(refMetadata.EnsemblName);
                    _inVepReferenceNames.Add(refMetadata.UcscName);
                }

                index++;
            }

            EnsemblReferenceNames = ensemblRefNames.ToArray();
            UcscReferenceNames    = ucscRefNames.ToArray();
        }

        private void AddReferenceSequenceIndexEntry(string refName, ushort refIndex)
        {
            if (!_referenceIndex.ContainsKey(refName)) _referenceIndex[refName] = refIndex;
        }

        /// <summary>
        /// adds a Ensembl/UCSC reference name pair to the current dictionary
        /// </summary>
        private void AddReferenceName(string ensemblReferenceName, string ucscReferenceName, ushort refIndex,
            List<string> ensemblRefNames, List<string> ucscRefNames)
        {
            var isUcscEmpty    = string.IsNullOrEmpty(ucscReferenceName);
            var isEnsemblEmpty = string.IsNullOrEmpty(ensemblReferenceName);

            // sanity check: make sure we have at least one reference name
            if (isUcscEmpty && isEnsemblEmpty)
            {
                ensemblRefNames.Add(ensemblReferenceName);
                ucscRefNames.Add(ucscReferenceName);
                return;
            }

            if (isUcscEmpty) ucscReferenceName       = ensemblReferenceName;
            if (isEnsemblEmpty) ensemblReferenceName = ucscReferenceName;

            _ucscToEnsemblReferenceNames[ucscReferenceName]    = ensemblReferenceName;
            _ensemblToUcscReferenceNames[ensemblReferenceName] = ucscReferenceName;

            AddReferenceSequenceIndexEntry(ensemblReferenceName, refIndex);
            AddReferenceSequenceIndexEntry(ucscReferenceName, refIndex);
            ensemblRefNames.Add(ensemblReferenceName);
            ucscRefNames.Add(ucscReferenceName);
        }

        /// <summary>
        /// returns the Ensembl reference name if an UCSC reference name is encountered.
        /// </summary>
        public string GetEnsemblReferenceName(string ucscReferenceName, bool useOriginalOnFailedLookup = true)
        {
            if (NumRefSeqs == 0) throw new InvalidOperationException("Tried to use the chromosome renamer before it was initialized.");
            if (ucscReferenceName == null) return null;
            return GetReferenceName(_ucscToEnsemblReferenceNames, ucscReferenceName, useOriginalOnFailedLookup);
        }

        /// <summary>
        /// returns the UCSC reference name if an Ensembl reference name is encountered.
        /// </summary>
        public string GetUcscReferenceName(string ensemblReferenceName, bool useOriginalOnFailedLookup = true)
        {
            if (NumRefSeqs == 0) throw new InvalidOperationException("Tried to use the chromosome renamer before it was initialized.");
            if (ensemblReferenceName == null) return null;
            return GetReferenceName(_ensemblToUcscReferenceNames, ensemblReferenceName, useOriginalOnFailedLookup);
        }

        private string GetReferenceName(Dictionary<string, string> dict, string referenceName, bool useOriginalOnFailedLookup)
        {
            string newReferenceName;
            if (dict.TryGetValue(referenceName, out newReferenceName)) return newReferenceName;
            return useOriginalOnFailedLookup ? referenceName : null;
        }

        public ushort GetReferenceIndex(string referenceName)
        {
            ushort refIndex;
            return _referenceIndex.TryGetValue(referenceName, out refIndex) ? refIndex : UnknownReferenceIndex;
        }

        public static ChromosomeRenamer GetChromosomeRenamer(Stream stream)
        {
            var sequence = new CompressedSequence();
            // ReSharper disable once UnusedVariable
            var reader = new CompressedSequenceReader(stream, sequence);
            return sequence.Renamer;
        }
    }
}
