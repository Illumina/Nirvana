using System;
using System.Collections.Generic;
using System.Text;
using VariantAnnotation.DataStructures.CompressedSequence;
using VariantAnnotation.DataStructures.SupplementaryAnnotations;
using VariantAnnotation.FileHandling.VCF;
using VariantAnnotation.Interface;

namespace VariantAnnotation.DataStructures.Variants
{
    public sealed class VariantAlternateAllele : IEquatable<VariantAlternateAllele>, IInterval, IAllele
    {
        #region members

        public int Start { get; set; }
        public int End { get; set; }

        public string ReferenceAllele { get; internal set; }
        public string AlternateAllele { get; internal set; }

        public bool IsStructuralVariant { get; set; }

        public string CopyNumber;
        public List<BreakEnd> BreakEnds;

        // will be used for consequence reporting
        public VariantType VepVariantType = VariantType.unknown;

        // this is the real variant type that will be output into the VCF and JSON files
        public VariantType NirvanaVariantType { get; set; } = VariantType.unknown;

        public int GenotypeIndex { get; }

        // used to determine if this alternate allele is a genomic duplicate
        private bool _isForwardTranscriptDuplicate;
        private bool _isReverseTranscriptDuplicate;

        public string ConservationScore { get; internal set; }
        public string VariantId { get; internal set; }

        public List<ISaDataSource> SupplementaryAnnotationPosition = new List<ISaDataSource>();

        // This is the SA's alternate allele representation of this variant's alternate allele . We need this for extracting the appropriate allele specific annotation.
        public string SuppAltAllele { get; internal set; }
		public string RepeatUnit { get; private set; }
		public int RepeatCount { get; }
		public int RefRepeatCount { get; private set; }
        
        public readonly bool IsSymbolicAllele;
	    public bool IsRepeatExpansion { get; }
	    public bool IsRecomposedVariant { get;  }

	    #endregion

		// constructor
		public VariantAlternateAllele(int begin, int end, string refAllele, string altAllele, int genotypeIndex = 1,bool isRecomposed = false)
        {
            Start             = begin;
            End               = end;
	        IsRepeatExpansion = altAllele.Contains(VcfCommon.StrTag);
	        AlternateAllele   = altAllele.ToUpperInvariant();
            ReferenceAllele   = refAllele.ToUpperInvariant();
            GenotypeIndex     = genotypeIndex;

	        if (IsRepeatExpansion)
	        {
		        AlternateAllele = AlternateAllele.Trim('<', '>') ;
				RepeatCount = Convert.ToInt32(AlternateAllele.Substring(3));
	        }

	        int dummyInt  = Start;
	        
			SuppAltAllele = SupplementaryAnnotationUtilities.GetReducedAlleles(dummyInt, ReferenceAllele, AlternateAllele).Item3;

            IsSymbolicAllele = StructuralVariant.IsSymbolicAllele(altAllele)|| IsRepeatExpansion;
            IsRecomposedVariant = isRecomposed;
        }

	    public void SetRepeatUnit(string repeatUnit)
	    {
		    RepeatUnit = repeatUnit;
	    }

	    public void SetRefRepeatCount(int count)
	    {
		    RefRepeatCount = count;
	    }

	    /// <summary>
        /// copy constructor
        /// </summary>
        public VariantAlternateAllele(VariantAlternateAllele altAllele)
        {
            AlternateAllele                 = altAllele.AlternateAllele;
            BreakEnds                       = altAllele.BreakEnds;
            ConservationScore               = altAllele.ConservationScore;
            CopyNumber                      = altAllele.CopyNumber;
            GenotypeIndex                   = altAllele.GenotypeIndex;
            _isForwardTranscriptDuplicate   = altAllele._isForwardTranscriptDuplicate;
            _isReverseTranscriptDuplicate   = altAllele._isReverseTranscriptDuplicate;
            IsStructuralVariant             = altAllele.IsStructuralVariant;
            IsSymbolicAllele                = altAllele.IsSymbolicAllele;
            NirvanaVariantType              = altAllele.NirvanaVariantType;
            ReferenceAllele                 = altAllele.ReferenceAllele;
            Start                           = altAllele.Start;
            End                             = altAllele.End;
            SuppAltAllele                   = altAllele.SuppAltAllele;
            SupplementaryAnnotationPosition = altAllele.SupplementaryAnnotationPosition;
            VariantId                       = altAllele.VariantId;
            VepVariantType                  = altAllele.VepVariantType;
        }

        public void CheckForDuplicationForAltAllele(ICompressedSequence compressedSequence)
        {
            if (VepVariantType != VariantType.insertion) return;
            int altAlleleLen = AlternateAllele.Length;

            var forwardRegion = compressedSequence.Substring(Start - 1, altAlleleLen);
            var reverseRegion = compressedSequence.Substring(End - altAlleleLen, altAlleleLen);

            _isForwardTranscriptDuplicate = forwardRegion == AlternateAllele;
            _isReverseTranscriptDuplicate = reverseRegion == AlternateAllele;
        }

        public bool CheckForDuplicationForAltAlleleWithinTranscript(ICompressedSequence compressedSequence, Transcript.Transcript transcript)
        {
            if (VepVariantType != VariantType.insertion) return false;
            int altAlleleLen = AlternateAllele.Length;
            string compareRegion;

            if (transcript.Gene.OnReverseStrand)
            {
                if (End + altAlleleLen > transcript.End) return false;
                compareRegion = compressedSequence.Substring(Start - 1, altAlleleLen);
            }
            else
            {
                if (Start - altAlleleLen < transcript.Start) return false;
                compareRegion = compressedSequence.Substring(End - altAlleleLen, altAlleleLen);

            }

            if (compareRegion == AlternateAllele) return true;
            return false;
        }


		/// <summary>
		/// sets the supplementary annotation allele
		/// </summary>
		public void SetSupplementaryAnnotation(ISaPosition sa)
        {
			// sanity check: SVs don't use supplementary annotations for now
			if (IsStructuralVariant) return;

            SupplementaryAnnotationPosition.AddRange(sa.DataSources);
		}

        /// <summary>
        /// returns true if this object is equal to the other object
        /// </summary>
        public bool Equals(VariantAlternateAllele other)
        {
            if (ReferenceEquals(this, other)) return true;
            if (other == null) return false;

            return Start           == other.Start           &&
                   End             == other.End             &&
                   ReferenceAllele == other.ReferenceAllele &&
                   AlternateAllele == other.AlternateAllele &&
                   IsRecomposedVariant == other.IsRecomposedVariant;
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
            sb.AppendFormat("reference range:  {0} - {1}\n", Start, End);
            sb.AppendFormat("variant type:     {0}\n", VepVariantType);

            sb.AppendLine(new string('-', 42));

            return sb.ToString();
        }


    }
}
