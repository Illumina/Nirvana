using System.Collections.Generic;
using System.IO;
using CacheUtils.DataDumperImport.DataStructures.Import;
using VariantAnnotation.Interface.AnnotatedPositions;
using VariantAnnotation.Interface.Intervals;

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
        public static (ICdnaCoordinateMap[] CdnaMaps, IInterval[] Introns, string PeptideSequence, string
            TranslateableSequence, string SiftData, string PolyPhenData, int[] SelenocysteinePositions) Parse(ObjectValueNode objectValue)
        {
            ICdnaCoordinateMap[] cdnaMaps = null;
            IInterval[] introns           = null;
            string peptideSequence        = null;
            string translateableSequence  = null;
            string siftData               = null;
            string polyphenData           = null;
            int[] selenocysteinePositions = null;

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
                        if (node is ListObjectKeyValueNode intronsList)
                        {
                            introns = ImportIntron.ParseList(intronsList.Values);
                        }
                        else if (!node.IsUndefined())
                        {
                            throw new InvalidDataException($"Could not transform the AbstractData object into a ListObjectKeyValue: [{node.GetType()}]");
                        }
                        break;
                    case ImportKeys.Mapper:
                        if (node is ObjectKeyValueNode mapperNode)
                        {
                            cdnaMaps = ImportTranscriptMapper.Parse(mapperNode.Value);
                        }
                        else
                        {
                            throw new InvalidDataException($"Could not transform the AbstractData object into an ObjectKeyValue: [{node.GetType()}]");
                        }
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
                        if (node is ListObjectKeyValueNode seqEditsNodes)
                        {
                            selenocysteinePositions = ImportSeqEdits.Parse(seqEditsNodes.Values);
                        }
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
