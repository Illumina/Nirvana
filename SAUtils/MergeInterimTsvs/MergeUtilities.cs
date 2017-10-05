using System;
using System.Collections.Generic;
using System.Linq;

namespace SAUtils.MergeInterimTsvs
{
    public static class MergeUtilities
    {
        public static List<T> GetMinItems<T>(List<IEnumerator<T>> interimSaItemsList) where T : IComparable<T>
        {
            if (interimSaItemsList.Count == 0) return null;

            //var minItem = GetMinItem(interimSaItemsList);
            var firstItems = interimSaItemsList.Select(x => x.Current);
            var minItem = firstItems.Min();

            var minItems = new List<T>();
            var emptyEnumerators = new List<IEnumerator<T>>();

            foreach (var saEnumerator in interimSaItemsList)
            {
                if (minItem.CompareTo(saEnumerator.Current) < 0) continue;

                while (minItem.CompareTo(saEnumerator.Current) == 0)
                {
                    minItems.Add(saEnumerator.Current);
                    if (saEnumerator.MoveNext()) continue;
                    emptyEnumerators.Add(saEnumerator);
                    break;
                }
            }

            RemoveEmptyEnumerators(emptyEnumerators, interimSaItemsList);
            return minItems.Count == 0 ? null : minItems;
        }


        //private static T GetMinItem<T>(List<IEnumerator<T>> interimSaItemsList) where T : IComparable<T>
        //{
        //    var minItem = interimSaItemsList[0].Current;
        //    foreach (var saEnumerator in interimSaItemsList)
        //    {
        //        if (minItem.CompareTo(saEnumerator.Current) > 0)
        //            minItem = saEnumerator.Current;
        //    }
        //    return minItem;
        //}

        private static void RemoveEmptyEnumerators<T>(List<IEnumerator<T>> emptyEnumerators, List<IEnumerator<T>> enumerators)
        {
            if (emptyEnumerators.Count == 0) return;

            foreach (var enumerator in emptyEnumerators)
            {
                enumerators.Remove(enumerator);
            }
        }

        public static void RemoveEmptyEnumerators<T>(List<IEnumerator<T>> enumerators)
        {
            var emptyEnumerators = enumerators.Where(x => x.MoveNext() == false);
            
            foreach (var enumerator in emptyEnumerators)
            {
                enumerators.Remove(enumerator);
            }
        }

    }
}