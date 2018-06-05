using System.Collections.Generic;
using System.IO;
using CacheUtils.DataDumperImport.DataStructures.Import;
using CacheUtils.DataDumperImport.DataStructures.Mutable;
using CacheUtils.DataDumperImport.Utilities;
using Intervals;

namespace CacheUtils.DataDumperImport.Import
{
    internal static class ImportVariantEffectFeatureCache
    {
        private static readonly HashSet<string> KnownKeys;

        static ImportVariantEffectFeatureCache()
        {
            KnownKeys = new HashSet<string>
            {
                ImportKeys.CodonTable,
                ImportKeys.FivePrimeUtr,
                ImportKeys.Introns,
                ImportKeys.Mapper,
                ImportKeys.Peptide,
                ImportKeys.ProteinFeatures,
                ImportKeys.ProteinFunctionPredictions,
                ImportKeys.Selenocysteines,
                ImportKeys.SeqEdits,
                ImportKeys.SplicedSequence,
                ImportKeys.SortedExons,
                ImportKeys.ThreePrimeUtr,
                ImportKeys.TranslateableSeq
            };
        }

        /// <summary>
        /// parses the relevant data from each variant effect feature cache
        /// </summary>
        public static (MutableTranscriptRegion[] CdnaMaps, IInterval[] Introns, string PeptideSequence, string
            TranslateableSequence, string SiftData, string PolyPhenData, int[] SelenocysteinePositions) Parse(IImportNode importNode)
        {
            var objectValue = importNode.GetObjectValueNode();
            if (objectValue == null) throw new InvalidDataException("Encountered a variant effect feature cache node that could not be converted to an object value node.");

            MutableTranscriptRegion[] cdnaMaps = null;
            IInterval[] introns                = null;
            string peptideSequence             = null;
            string translateableSequence       = null;
            string siftData                    = null;
            string polyphenData                = null;
            int[] selenocysteinePositions      = null;

            foreach (var node in objectValue.Values)
            {
                // sanity check: make sure we know about the keys are used for
                if (!KnownKeys.Contains(node.Key))
                {
                    throw new InvalidDataException($"Encountered an unknown key in the dumper variant effect feature cache object: {node.Key}");
                }

                switch (node.Key)
                {
                    case ImportKeys.CodonTable:
                    case ImportKeys.FivePrimeUtr:
                    case ImportKeys.ProteinFeatures:
                    case ImportKeys.Selenocysteines:                    
                    case ImportKeys.SortedExons:
                    case ImportKeys.SplicedSequence:
                    case ImportKeys.ThreePrimeUtr:
                        // not used
                        break;
                    case ImportKeys.Introns:
                        introns = node.ParseListObjectKeyValueNode(ImportIntron.ParseList);
                        break;
                    case ImportKeys.Mapper:
                        cdnaMaps = node.ParseObjectKeyValueNode(ImportTranscriptMapper.Parse);
                        break;
                    case ImportKeys.Peptide:
                        peptideSequence = node.GetString();
                        break;
                    case ImportKeys.ProteinFunctionPredictions:
                        if (node is ObjectKeyValueNode predictionsNode)
                        {
                            (siftData, polyphenData) = ImportProteinFunctionPredictions.Parse(predictionsNode.Value);
                        }
                        else
                        {
                            throw new InvalidDataException($"Could not transform the AbstractData object into an ObjectKeyValue: [{node.GetType()}]");
                        }
                        break;
                    case ImportKeys.SeqEdits:
                        selenocysteinePositions = node.ParseListObjectKeyValueNode(ImportSeqEdits.Parse);
                        break;
                    case ImportKeys.TranslateableSeq:
                        translateableSequence = node.GetString();
                        break;
                    default:
                        throw new InvalidDataException($"Unknown key found: {node.Key}");
                }
            }

            return (cdnaMaps, introns, peptideSequence, translateableSequence, siftData, polyphenData, selenocysteinePositions);
        }
    }
}
