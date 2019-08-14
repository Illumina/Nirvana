using System;
using System.Collections.Generic;
using System.Linq;
using ErrorHandling.Exceptions;
using VariantAnnotation.Interface.SA;
using Variants;

namespace SAUtils.DataStructures
{
    public static class SuppDataUtilities
    {
        public static int CompareTo(ISupplementaryDataItem item, ISupplementaryDataItem other)
        {
            if (other == null) return -1;
            return item.Chromosome.Index == other.Chromosome.Index ? item.Position.CompareTo(other.Position) : item.Chromosome.Index.CompareTo(other.Chromosome.Index);
        }

        public static void Trim(this ISupplementaryDataItem saItem)
        {
            if (saItem.RefAllele == null || saItem.AltAllele == null || saItem.Position < 0)
                return;

            var newAlleles = BiDirectionalTrimmer.Trim(saItem.Position, saItem.RefAllele, saItem.AltAllele);

            saItem.Position  = newAlleles.Start;
            saItem.RefAllele = newAlleles.RefAllele;
            saItem.AltAllele = newAlleles.AltAllele;

        }

        public static List<ISupplementaryDataItem> RemoveConflictingAlleles(List<ISupplementaryDataItem> saItems, bool throwErrorOnConflicts)
        {
            var nonDuplicateSet  = new Dictionary<string, ISupplementaryDataItem>();
            var conflictSet = new List<string>();

            foreach (var saItem in saItems)
            {
                var refAlt = saItem.RefAllele+'>'+saItem.AltAllele;

                if (nonDuplicateSet.TryGetValue(refAlt, out var dupItem))
                {
                    if (saItem.GetJsonString() != dupItem.GetJsonString())
                    {
                        if(throwErrorOnConflicts)
                            throw new UserErrorException($"Conflicting entries for items at {saItem.Chromosome.UcscName}:{saItem.Position} for alleles {saItem.RefAllele} > {saItem.AltAllele}");
                        conflictSet.Add(refAlt);
                    }
                }
                else nonDuplicateSet.Add(refAlt, saItem);
            }

            var values = nonDuplicateSet.Values.ToList();

            if (conflictSet.Count > 0)
            {
                values.RemoveAll(x => conflictSet.Contains(x.RefAllele + '>' + x.AltAllele));
            }

            return values;
        }

        public static ISupplementaryDataItem GetPositionalAnnotation(IList<ISupplementaryDataItem> saItems)
        {
            // all items in the list are assumed to be objects of the same implementation
            var firstItem = saItems[0];
            switch (firstItem)
            {
                case AlleleFrequencyItem _:
                    return GetGlobalMinor(saItems);
                // if onekgen return Ancestral allele 
                case AncestralAlleleItem _:
                    return GetConsensus(saItems);
            }

            return null;
        }

        private static ISupplementaryDataItem GetConsensus(IList<ISupplementaryDataItem> saItems)
        {
            //check consistancy
            string ancestralAllele = null;
            foreach (var supplementaryDataItem in saItems)
            {
                var aaItem = (AncestralAlleleItem) supplementaryDataItem;
                //note: aaItem.AncestralAllele cannot be null at this point
                if (ancestralAllele == null) ancestralAllele = aaItem.AncestralAllele;

                if (ancestralAllele != aaItem.AncestralAllele) return null;
                
            }

            return ancestralAllele==null? null : saItems[0];
        }

        
        private static ISupplementaryDataItem GetGlobalMinor(IList<ISupplementaryDataItem> saItems)
        {
            var alleleFreqDict = new Dictionary<string, double>();

            foreach (var supplementaryDataItem in saItems)
            {
                var frequencyItem = (AlleleFrequencyItem) supplementaryDataItem;
                if (!double.MinValue.Equals(frequencyItem.AltFrequency))
                    alleleFreqDict[frequencyItem.AltAllele] = frequencyItem.AltFrequency;
            }

            if (alleleFreqDict.Count == 0) return null;

            var firstItem = saItems[0];
            
            string refAllele = firstItem.RefAllele;

            string globalMajorAllele = GetMostFrequentAllele(alleleFreqDict, refAllele);
            if (globalMajorAllele == null) return null;

            alleleFreqDict.Remove(globalMajorAllele);

            string globalMinorAllele = GetMostFrequentAllele(alleleFreqDict, refAllele, false);

            if (globalMinorAllele == null) return null;
            double frequency = alleleFreqDict[globalMinorAllele];
            return new GlobalMinorItem(firstItem.Chromosome, firstItem.Position, globalMinorAllele, frequency);

        }

        public static string GetMostFrequentAllele(Dictionary<string, double> alleleFreqDict, string refAllele, bool isRefPreferred = true)
        {
            if (alleleFreqDict.Count == 0) return null;

            // find all alleles that have max frequency.
            double maxFreq = alleleFreqDict.Values.Max();
            if (Math.Abs(maxFreq - double.MinValue) < double.Epsilon) return null;

            var maxFreqAlleles = (from pair in alleleFreqDict where Math.Abs(pair.Value - maxFreq) < double.Epsilon select pair.Key).ToList();


            // if there is only one with max frequency, return it
            if (maxFreqAlleles.Count == 1)
                return maxFreqAlleles[0];

            // if ref is preferred (as in global major) it is returned
            if (isRefPreferred && maxFreqAlleles.Contains(refAllele))
                return refAllele;

            // else refAllele is removed and the first of the remaining allele is returned (arbitrary selection)
            maxFreqAlleles.Remove(refAllele);
            return maxFreqAlleles[0];

        }
        
    }
}