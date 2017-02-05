using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using VariantAnnotation.DataStructures.CompressedSequence;
using VariantAnnotation.FileHandling;
using VariantAnnotation.Utilities;
using ErrorHandling.Exceptions;

namespace CacheUtils.ParseVepCacheDirectory
{
    public sealed class ReferenceIndex
    {
        private readonly Dictionary<string, ushort> _referenceSequenceIndices;
        public readonly ushort NumReferenceSeqs;
        private readonly ChromosomeRenamer _renamer;

        public ReferenceIndex(string compressedReferencePath)
        {
            var compressedSequence = new CompressedSequence();
            var compressedSequenceReader = new CompressedSequenceReader(FileUtilities.GetReadStream(compressedReferencePath), compressedSequence);
            _renamer = compressedSequence.Renamer;

            var referenceMetadataList    = compressedSequenceReader.Metadata;
            _referenceSequenceIndices    = new Dictionary<string, ushort>();

            NumReferenceSeqs = (ushort)referenceMetadataList.Count;

            for (ushort refIndex = 0; refIndex < NumReferenceSeqs; refIndex++)
            {
                var refMetadata = referenceMetadataList[refIndex];
                AddReferenceSequence(refMetadata.UcscName, refIndex);
                AddReferenceSequence(refMetadata.EnsemblName, refIndex);
            }
        }

        private void AddReferenceSequence(string referenceName, ushort refIndex)
        {
            if (!string.IsNullOrEmpty(referenceName)) _referenceSequenceIndices[referenceName] = refIndex;
        }

        public ushort GetIndex(string referenceName)
        {
            ushort referenceIndex;
            if (!_referenceSequenceIndices.TryGetValue(referenceName, out referenceIndex))
            {
                throw new GeneralException($"Unable to find the reference index for the specified reference: {referenceName}");
            }

            return referenceIndex;
        }

        public List<Tuple<string, string>> GetUcscKaryotypeOrder(string dirPath)
        {
            var vepDirectories = Directory.GetDirectories(dirPath);
            var referenceDict  = new SortedDictionary<ushort, Tuple<string, string>>();

            foreach (var dir in vepDirectories)
            {
                string referenceName = Path.GetFileName(dir);
                if (!_renamer.InReferenceAndVep(referenceName)) continue;

                string ucscReferenceName = _renamer.GetUcscReferenceName(referenceName, false);

                var refIndex = GetIndex(ucscReferenceName);
                referenceDict[refIndex] = new Tuple<string, string>(ucscReferenceName, dir);
            }

            return referenceDict.Values.ToList();
        }
    }
}
