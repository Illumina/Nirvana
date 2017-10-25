using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SAUtils.DataStructures;
using VariantAnnotation.Interface.Providers;

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

        public static IEnumerable<IDataSourceVersion> GetDataSourceVersions(List<SmallAnnotationsHeader> interimSaHeaders,
            List<IntervalAnnotationHeader> intervalHeaders)
        {
            return interimSaHeaders.Select(header => header.GetDataSourceVersion()).Concat(intervalHeaders.Select(header => header.GetDataSourceVersion()));
        }

        public static void CheckAssemblyConsistancy(IEnumerable<SmallAnnotationsHeader> iSaHeaders, IEnumerable<IntervalAnnotationHeader> iIntervalHeaders)
        {
            var uniqueAssemblies = iSaHeaders.Select(x => x.GenomeAssembly)
                .Concat(iIntervalHeaders.Select(x => x.GenomeAssembly))
                .Where(x => !MergeInterimTsvs.AssembliesIgnoredInConsistancyCheck.Contains(x))
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

        
    }
}