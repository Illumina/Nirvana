using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using CommonUtilities;
using SAUtils.DataStructures;
using VariantAnnotation.IO;

namespace SAUtils.TsvWriters
{
    public sealed class DbsnpGaTsvWriter : ISaItemTsvWriter
    {

        #region members
        private readonly SaTsvWriter _dbsnpWriter;
        private readonly SaTsvWriter _globalAlleleWriter;
        #endregion

        #region IDisposable
        public void Dispose()
        {
            _dbsnpWriter.Dispose();
            _globalAlleleWriter.Dispose();
        }
        #endregion

        public DbsnpGaTsvWriter(SaTsvWriter dbsnpWriter, SaTsvWriter globalAlleleWriter)
        {
            _dbsnpWriter = dbsnpWriter;
            _globalAlleleWriter = globalAlleleWriter;
        }


        public void WritePosition(IEnumerable<SupplementaryDataItem> saItems)
        {
            var itemsByAllele = GetItemsByAllele(saItems);
            WriteDbsnpTsv(itemsByAllele);
            WriteGlobalAlleleTsv(itemsByAllele);
        }

        private void WriteGlobalAlleleTsv(Dictionary<(string, string), List<DbSnpItem>> itemsByAllele)
        {
            var alleleFreqDict = GetAlleleFrequencies(itemsByAllele);
            if (alleleFreqDict.Count == 0) return;

            var firstItem = itemsByAllele.First().Value[0];
            var refAllele = firstItem.ReferenceAllele;
            var chromosome = firstItem.Chromosome;
            var position = firstItem.Start;

            string globalMinorAlleleFrequency = null;

            var globalMajorAllele = GetMostFrequentAllele(alleleFreqDict, refAllele);
            if (globalMajorAllele == null) return;

            alleleFreqDict.Remove(globalMajorAllele);

            var globalMinorAllele = GetMostFrequentAllele(alleleFreqDict, refAllele, false);
            if (globalMinorAllele != null)
                globalMinorAlleleFrequency = alleleFreqDict[globalMinorAllele].ToString(CultureInfo.InvariantCulture);

            string vcfString = null;
            if (globalMinorAllele != null)
            {
                vcfString = globalMinorAllele + '|' + globalMinorAlleleFrequency;
            }

            var sb = StringBuilderCache.Acquire();
            var jsonObject = new JsonObject(sb);
            jsonObject.AddStringValue("globalMinorAllele", globalMinorAllele);
            jsonObject.AddStringValue("globalMinorAlleleFrequency", globalMinorAlleleFrequency, false);

            _globalAlleleWriter.AddEntry(chromosome.EnsemblName, position, refAllele, "N", vcfString, new List<string> { StringBuilderCache.GetStringAndRelease(sb) });
        }

        public static string GetMostFrequentAllele(Dictionary<string, double> alleleFreqDict, string refAllele, bool isRefPreferred = true)
        {
            if (alleleFreqDict.Count == 0) return null;

            // find all alleles that have max frequency.
            var maxFreq = alleleFreqDict.Values.Max();
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
        private static Dictionary<string, double> GetAlleleFrequencies(Dictionary<(string ReferenceAllele, string AlternateAllele), List<DbSnpItem>> itemsByAllele)
        {
            var alleleFreqDict = new Dictionary<string, double>();

            foreach (var kvp in itemsByAllele)
            {
                var refAllele = kvp.Key.ReferenceAllele;
                var altAllele = kvp.Key.AlternateAllele;

                foreach (var dbSnpItem in kvp.Value)
                {
                    if (!dbSnpItem.RefAlleleFreq.Equals(double.MinValue))
                        alleleFreqDict[refAllele] = dbSnpItem.RefAlleleFreq;
                    if (!dbSnpItem.AltAlleleFreq.Equals(double.MinValue))
                        alleleFreqDict[altAllele] = dbSnpItem.AltAlleleFreq;
                }
            }
            return alleleFreqDict;
        }

        private void WriteDbsnpTsv(Dictionary<(string RefAllele, string AltAllele), List<DbSnpItem>> itemsByAllele)
        {
            foreach (var kvp in itemsByAllele)
            {
                var refAllele = kvp.Key.RefAllele;
                var altAllele = kvp.Key.AltAllele;
                var itemsGroup = kvp.Value;

                var uniqueIds = new HashSet<long>(itemsGroup.Select(x => x.RsId).ToList());
                var vcfString = string.Join(",", uniqueIds.Select(x => $"rs{x}").ToArray());
                var jsonString = "\"ids\":[" + string.Join(",", uniqueIds.OrderBy(x => x).Select(x => $"\"rs{x}\"").ToArray()) + "]";

                var chromosome = itemsGroup[0].Chromosome;
                var position = itemsGroup[0].Start;

                _dbsnpWriter.AddEntry(chromosome.EnsemblName, position, refAllele, altAllele, vcfString, new List<string> { jsonString });
            }
        }

        private static Dictionary<(string ReferenceAllele, string AlternateAllele), List<DbSnpItem>> GetItemsByAllele(IEnumerable<SupplementaryDataItem> saItems)
        {
            var itemsForPosition = new List<DbSnpItem>();
            foreach (var item in saItems)
            {
                if (!(item is DbSnpItem dbSnpItem))
                    throw new InvalidDataException("Expecting enumerable of DbSnpItems!!");
                itemsForPosition.Add(dbSnpItem);
            }

            var itemsByAllele = new Dictionary<(string, string), List<DbSnpItem>>();
            foreach (var item in itemsForPosition)
            {
                var alleleTuple = (item.ReferenceAllele, item.AlternateAllele);

                if (itemsByAllele.ContainsKey(alleleTuple))
                    itemsByAllele[alleleTuple].Add(item);
                else itemsByAllele[alleleTuple] = new List<DbSnpItem> { item };
            }
            return itemsByAllele;
        }


    }
}