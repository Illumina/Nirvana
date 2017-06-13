using System;
using System.Collections.Generic;
using VariantAnnotation.Algorithms;
using VariantAnnotation.DataStructures.SupplementaryAnnotations;
using VariantAnnotation.Interface;

namespace SAUtils.DataStructures
{
	public abstract class SupplementaryDataItem: IComparable<SupplementaryDataItem>, IEquatable<SupplementaryDataItem>
	{
		public string Chromosome { get; protected set; }
		public int Start { get; protected set; }
		public string ReferenceAllele { get; protected set; }
		public string AlternateAllele { get; protected set; }
	    internal bool IsInterval { get; set; }

		public abstract SupplementaryInterval GetSupplementaryInterval(IChromosomeRenamer renamer);

        public int CompareTo(SupplementaryDataItem otherItem)
        {
            if (otherItem == null) return -1;
            if (Chromosome.Equals(otherItem.Chromosome)) return Start.CompareTo(otherItem.Start);
            return string.CompareOrdinal(Chromosome, otherItem.Chromosome);
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

        public static  void RemoveConflictedAlleles<T>(List<T> saItems) where T : SupplementaryDataItem
        {
            var allelesSet = new HashSet<Tuple<string, string>>();
            var conflictSet = new HashSet<Tuple<string, string>>();
            foreach (var saItem in saItems)
            {
                var alleleTuple = Tuple.Create(saItem.ReferenceAllele, saItem.AlternateAllele);

                if (allelesSet.Contains(alleleTuple))
                    conflictSet.Add(alleleTuple);

                allelesSet.Add(alleleTuple);
            }

            saItems.RemoveAll(x => conflictSet.Contains(Tuple.Create(x.ReferenceAllele, x.AlternateAllele)));

        }

    }
}
