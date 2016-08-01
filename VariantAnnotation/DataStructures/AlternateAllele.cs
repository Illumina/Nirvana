using System;
using System.Collections.Generic;
using System.Text;
using VariantAnnotation.DataStructures.SupplementaryAnnotations;
using VariantAnnotation.FileHandling;
using VariantAnnotation.Interface;

namespace VariantAnnotation.DataStructures
{
    public class VariantAlternateAllele : IEquatable<VariantAlternateAllele>
    {
        #region members

        public int ReferenceBegin;
        public int ReferenceEnd;

        public string ReferenceAllele;
        public string AlternateAllele;

        public bool IsStructuralVariant;

        public string CopyNumber;
        public BreakEnd BreakEnd;

        // will be used for consequence reporting
        public VariantType VepVariantType     = VariantType.unknown;

        // this is the real variant type that will be output into the VCF and JSON files
        public VariantType NirvanaVariantType = VariantType.unknown;

        public readonly int GenotypeIndex;

        // used to determine if this alternate allele is a genomic duplicate
        private bool _isForwardTranscriptDuplicate;
        private bool _isReverseTranscriptDuplicate;

        public string ConservationScore;
        public string VariantId;

        public SupplementaryAnnotation SupplementaryAnnotation;
	    public readonly List<CustomInterval> CustomIntervals;
	    

        // This is the SA's alternate allele representation of this variant's alternate allele . We need this for extracting the appropriate allele specific annotation.
        public string SuppAltAllele;

        public readonly bool IsSymbolicAllele;
		  
        #endregion

        // constructor
        public VariantAlternateAllele(int begin, int end, string refAllele, string altAllele, int genotypeIndex = 1)
        {
            ReferenceBegin  = begin;
            ReferenceEnd    = end;
            AlternateAllele = altAllele.ToUpperInvariant();
            ReferenceAllele = refAllele.ToUpperInvariant();
            GenotypeIndex   = genotypeIndex;

            int dummyInt = ReferenceBegin;
            SuppAltAllele = SupplementaryAnnotation.GetReducedAlleles(ReferenceAllele, AlternateAllele, ref dummyInt).Item2;

            IsSymbolicAllele = StructuralVariant.IsSymbolicAllele(altAllele);
			CustomIntervals= new List<CustomInterval>();
        }

        /// <summary>
        /// copy constructor
        /// </summary>
        public VariantAlternateAllele(VariantAlternateAllele altAllele)
        {
            AlternateAllele              = altAllele.AlternateAllele;
            BreakEnd                     = altAllele.BreakEnd;
            ConservationScore            = altAllele.ConservationScore;
            CopyNumber                   = altAllele.CopyNumber;
            GenotypeIndex                = altAllele.GenotypeIndex;
            _isForwardTranscriptDuplicate = altAllele._isForwardTranscriptDuplicate;
            _isReverseTranscriptDuplicate = altAllele._isReverseTranscriptDuplicate;
            IsStructuralVariant          = altAllele.IsStructuralVariant;
            IsSymbolicAllele             = altAllele.IsSymbolicAllele;
            NirvanaVariantType           = altAllele.NirvanaVariantType;
            ReferenceAllele              = altAllele.ReferenceAllele;
            ReferenceBegin               = altAllele.ReferenceBegin;
            ReferenceEnd                 = altAllele.ReferenceEnd;
            SuppAltAllele                = altAllele.SuppAltAllele;
            SupplementaryAnnotation      = altAllele.SupplementaryAnnotation;
            VariantId                    = altAllele.VariantId;
            VepVariantType               = altAllele.VepVariantType;
        }

        public void CheckForDuplicationForAltAllele()
	    {
			if (VepVariantType != VariantType.insertion) return;
			int altAlleleLen = AlternateAllele.Length;

	        var compressedSequence = AnnotationLoader.Instance.CompressedSequence;
			var forwardRegion = compressedSequence.Substring(ReferenceBegin - 1, altAlleleLen);
			var reverseRegion = compressedSequence.Substring(ReferenceEnd - altAlleleLen, altAlleleLen);

			_isForwardTranscriptDuplicate = forwardRegion == AlternateAllele;
			_isReverseTranscriptDuplicate = reverseRegion == AlternateAllele;
		}

	    public bool CheckForDuplicationForAltAlleleWithinTranscript(Transcript transcript)
	    {
			if (VepVariantType != VariantType.insertion) return false;
			int altAlleleLen = AlternateAllele.Length;
			var compressedSequence = AnnotationLoader.Instance.CompressedSequence;
		    string compareRegion;

            if (transcript.OnReverseStrand)
		    {
				if( ReferenceEnd+altAlleleLen > transcript.End) return false;
			    compareRegion = compressedSequence.Substring(ReferenceBegin - 1, altAlleleLen);
		    }
		    else
		    {
				if(ReferenceBegin - altAlleleLen < transcript.Start) return false;
			    compareRegion = compressedSequence.Substring(ReferenceEnd - altAlleleLen, altAlleleLen);

		    }

			if (compareRegion == AlternateAllele) return true;
			return false;
		}

		public void AddCustomAnnotation(SupplementaryAnnotation sa)
		{
			// sanity check: SVs don't use supplementary annotations for now
			if (IsStructuralVariant) return;

			sa.SetIsAlleleSpecific(SuppAltAllele);

			if (SupplementaryAnnotation == null)
			{
				SupplementaryAnnotation = sa;
				return;
			}
			
			if (SupplementaryAnnotation.CustomItems != null)
				SupplementaryAnnotation.CustomItems.AddRange(sa.CustomItems);
			else SupplementaryAnnotation.CustomItems = sa.CustomItems;

		}
        /// <summary>
        /// sets the supplementary annotation allele
        /// </summary>
        public void SetSupplementaryAnnotation(SupplementaryAnnotation sa)
        {
            // sanity check: SVs don't use supplementary annotations for now
            if (IsStructuralVariant) return;

            sa.SetIsAlleleSpecific(SuppAltAllele);

            SupplementaryAnnotation = sa;
        }

        /// <summary>
        /// returns true if this object is equal to the other object
        /// </summary>
        public bool Equals(VariantAlternateAllele other)
        {
            if (ReferenceEquals(this, other)) return true;
            if (other == null) return false;

            return (ReferenceBegin  == other.ReferenceBegin)  &&
                   (ReferenceEnd    == other.ReferenceEnd)    &&
                   (ReferenceAllele == other.ReferenceAllele) &&
                   (AlternateAllele == other.AlternateAllele);
        }

        /// <summary>
        /// returns a string representation of this alternate allele
        /// </summary>
        public override string ToString()
        {
            var sb = new StringBuilder();

            sb.AppendLine(new string('-', 42));

            sb.AppendFormat("reference allele: {0}\n", ReferenceAllele);
            sb.AppendFormat("variant allele:   {0}\n", AlternateAllele);
            sb.AppendFormat("reference range:  {0} - {1}\n", ReferenceBegin, ReferenceEnd);
            sb.AppendFormat("variant type:     {0}\n", VepVariantType);

            sb.AppendLine(new string('-', 42));

            return sb.ToString();
		}

	    public void AddCustomIntervals(List<CustomInterval> overlappingCustomIntervals)
	    {
		    if (overlappingCustomIntervals == null) return;

		    foreach (var customInterval in overlappingCustomIntervals)
		    {
			    if (customInterval.Overlaps(ReferenceBegin,ReferenceEnd)) CustomIntervals.Add(customInterval);
		    }
	    }

	    
    }
}
