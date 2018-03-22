using System;
using System.Collections.Generic;
using CommonUtilities;
using VariantAnnotation.Interface.Sequence;

namespace SAUtils.DataStructures
{
	public abstract class SupplementaryDataItem: IComparable<SupplementaryDataItem>, IEquatable<SupplementaryDataItem>
	{
		public IChromosome Chromosome { get; protected set; }
		public int Start { get; protected set; }
		public string ReferenceAllele { get; protected set; }
		public string AlternateAllele { get; protected set; }
	    internal bool IsInterval { get; set; }

		public abstract SupplementaryIntervalItem GetSupplementaryInterval();

        public int CompareTo(SupplementaryDataItem otherItem)
        {
            if (otherItem == null) return -1;
            return Chromosome.Equals(otherItem.Chromosome) ? Start.CompareTo(otherItem.Start) : string.CompareOrdinal(Chromosome.UcscName, otherItem.Chromosome.UcscName);
        }

        public bool Equals(SupplementaryDataItem other)
        {
            if (other == null) return false;
            return Start.Equals(other.Start) && Chromosome.Equals(other.Chromosome);
        }

		public void Trim()
		{
			if (ReferenceAllele==null || AlternateAllele==null || Start < 0)
				return;

		    var newAlleles = BiDirectionalTrimmer.Trim(Start, ReferenceAllele, AlternateAllele);
            //SupplementaryAnnotationUtilities.GetReducedAlleles(Start, ReferenceAllele, AlternateAllele);

		    Start           = newAlleles.Item1;
		    ReferenceAllele = newAlleles.Item2;
		    AlternateAllele = newAlleles.Item3;
			
		}

        public static  void RemoveConflictingAlleles<T>(List<T> saItems) where T : SupplementaryDataItem
        {
            var allelesSet = new HashSet<(string, string)>();
            var conflictSet = new HashSet<(string, string)>();
            foreach (var saItem in saItems)
            {
                var alleleTuple = (saItem.ReferenceAllele, saItem.AlternateAllele);

                if (allelesSet.Contains(alleleTuple))
                    conflictSet.Add(alleleTuple);

                allelesSet.Add(alleleTuple);
            }

            saItems.RemoveAll(x => conflictSet.Contains((x.ReferenceAllele, x.AlternateAllele)));

        }

    }
}
