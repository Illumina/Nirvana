using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SAUtils.DataStructures;
using SAUtils.InputFileParsers.IntermediateAnnotation;
using SAUtils.Interface;
using VariantAnnotation.Interface.Providers;
using VariantAnnotation.Interface.SA;
using VariantAnnotation.SA;

namespace SAUtils.MergeInterimTsvs
{
    public static class MergeUtilities
    {
        public static List<T> GetMinItems<T>(List<IEnumerator<T>> iSaEnumerators) where T : IComparable<T>
        {
            if (iSaEnumerators.Count == 0) return null;

            var firstItems = iSaEnumerators.Select(x => x.Current);
            var minItem = firstItems.Min();

            var minItems = new List<T>();
            List<IEnumerator<T>> emptyEnumerators=null;

            foreach (var saEnumerator in iSaEnumerators)
            {
                if (minItem.CompareTo(saEnumerator.Current) < 0) continue;

                while (minItem.CompareTo(saEnumerator.Current) == 0)
                {
                    minItems.Add(saEnumerator.Current);
                    if (saEnumerator.MoveNext()) continue;
                    if (emptyEnumerators == null)
                        emptyEnumerators = new List<IEnumerator<T>>();
                    emptyEnumerators.Add(saEnumerator);
                    break;
                }
            }

            RemoveEmptyEnumerators(emptyEnumerators, iSaEnumerators);
            return minItems.Count == 0 ? null : minItems;
        }

        public static IEnumerable<IDataSourceVersion> GetDataSourceVersions(IEnumerable<SaHeader> saHeaders)
        {
            return saHeaders.Select(header => header.GetDataSourceVersion());
        }

        public static void CheckAssemblyConsistancy(IEnumerable<SaHeader> saHeaders)
        {
            var uniqueAssemblies = saHeaders.Select(x => x.GenomeAssembly)
                .Where(x => !InterimTsvsMerger.AssembliesIgnoredInConsistancyCheck.Contains(x))
                .Distinct()
                .ToList();

            if (uniqueAssemblies.Count > 1)
                throw new InvalidDataException($"ERROR: The genome assembly for all data sources should be the same. Found {string.Join(", ", uniqueAssemblies.ToArray())}");
        }


        private static void RemoveEmptyEnumerators<T>(List<IEnumerator<T>> emptyEnumerators, List<IEnumerator<T>> enumerators)
        {
            if (emptyEnumerators == null || emptyEnumerators.Count == 0) return;

            foreach (var enumerator in emptyEnumerators)
            {
                enumerators.Remove(enumerator);
            }
        }

        public static List<ISupplementaryInterval> GetIntervals(IEnumerable<ParallelIntervalTsvReader> intervalReaders, string refName)
        {
            var intervals = new List<ISupplementaryInterval>();
            if (intervalReaders == null) return intervals;

            foreach (var intervalReader in intervalReaders)
            {
                intervals.AddRange(intervalReader.GetItems(refName));
            }

            return intervals;
        }

        public static List<ISupplementaryInterval> GetSpecificIntervals(ReportFor reportFor, IEnumerable<ISupplementaryInterval> intervals)
        {
            return intervals.Where(interval => interval.ReportingFor == reportFor).ToList();
        }

        public static (int, ISaPosition) GetSaPosition(List<IInterimSaItem> saItems)
        {
            if (saItems == null || saItems.Count == 0) return (0,null);

            var position = saItems[0].Position;
            var dataSources = new List<ISaDataSource>();
            string globalMajorAllele = null;
            foreach (var intermediateSaItem in saItems)
            {
                if (intermediateSaItem.KeyName == InterimSaCommon.RefMinorTag)
                {
                    if (intermediateSaItem is SaMiscellanies miscItem) globalMajorAllele = miscItem.GlobalMajorAllele;
                }
                else
                {
                    dataSources.Add(intermediateSaItem as ISaDataSource);
                }
            }

            return (position, new SaPosition(dataSources.ToArray(), globalMajorAllele));
        }
    }
}