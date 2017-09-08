using VariantAnnotation.Interface.AnnotatedPositions;
using VariantAnnotation.Interface.Intervals;
using VariantAnnotation.Interface.Positions;
using VariantAnnotation.Interface.Sequence;
using VariantAnnotation.Utilities;

namespace VariantAnnotation.AnnotatedPositions
{
	// TODO: why do we need a class here? shouldn't this just be a utility function?
	public static class HgvsCodingNomenclature
	{
		/// <summary>
		/// constructor
		/// </summary>
		public static string GetHgvscAnnotation(ITranscript transcript, ISimpleVariant variant, ISequence refSequence)
		{
			// sanity check: don't try to handle odd characters, make sure this is not a reference allele, 
			//               and make sure that we have protein coordinates
			if (variant.Type == VariantType.reference || SequenceUtilities.HasNonCanonicalBase(variant.AltAllele)) return null;

			var onReverseStrand = transcript.Gene.OnReverseStrand;

			var refAllele = onReverseStrand ? SequenceUtilities.GetReverseComplement(variant.RefAllele) : variant.RefAllele;
			var altAllele = onReverseStrand ? SequenceUtilities.GetReverseComplement(variant.AltAllele) : variant.AltAllele;

			// decide event type from HGVS nomenclature
			var genomicChange = GetGenomicChange(transcript, onReverseStrand, refSequence, variant);

			// calculate the reference start and end
			var hgvsInterval = HgvsUtilities.GetReletiveCoordinates(variant, transcript, onReverseStrand);

			if (genomicChange == GenomicChange.Duplication)
			{
				// for duplication, the hgvs positions are deceremented by alt allele length
				var incrementLength = altAllele.Length;
				var dupStart        = hgvsInterval.Start - incrementLength;
				var dupEnd          = dupStart + incrementLength - 1;

				hgvsInterval = new Interval(dupStart, dupEnd);

				refAllele = altAllele;
			}

			var startPositionOffset = HgvsUtilities.GetCdnaPositionOffset(transcript, hgvsInterval.Start);
			var endPositionOffset = startPositionOffset;
			if (hgvsInterval.Start != hgvsInterval.End)
				endPositionOffset = HgvsUtilities.GetCdnaPositionOffset(transcript, hgvsInterval.End);

			// sanity check: make sure we have coordinates
			if (startPositionOffset == null || endPositionOffset == null) return null;

			var transcriptLen = transcript.End - transcript.Start + 1;

			//_hgvs notation past the transcript
			if (startPositionOffset.Position > transcriptLen || endPositionOffset.Position > transcriptLen) return null;

			var hgvsNotation = new HgvscNotation(refAllele, altAllele,
				FormatUtilities.CombineIdAndVersion(transcript.Id, transcript.Version), genomicChange,
				startPositionOffset,
				endPositionOffset,
				transcript.Translation != null);

			// generic formatting
			return hgvsNotation.ToString();
		}

		/// <summary>
		/// get the genomic change that resulted from this variation [Sequence.pm:482 hgvsvariant_notation]
		/// </summary>
		public static GenomicChange GetGenomicChange(IInterval interval, bool onReverseStrand, ISequence refSequence, ISimpleVariant variant)
		{
			
			// length of the reference allele. Negative lengths make no sense
			var refLength = variant.End - variant.Start + 1;
			if (refLength < 0) refLength = 0;

			// length of alternative allele
			var altLength = variant.AltAllele.Length;

			// sanity check: make sure that the alleles are different
			if (variant.RefAllele == variant.AltAllele) return GenomicChange.Unknown;

			// deletion
			if (altLength == 0) return GenomicChange.Deletion;


			if (refLength == altLength)
			{
				// substitution
				if (refLength == 1) return GenomicChange.Substitution;

				// inversion
				var rcRefAllele = SequenceUtilities.GetReverseComplement(variant.RefAllele);
				return variant.AltAllele == rcRefAllele ? GenomicChange.Inversion : GenomicChange.DelIns;
			}

			// deletion/insertion
			if (refLength != 0) return GenomicChange.DelIns;

			// If this is an insertion, we should check if the preceeding reference nucleotides
			// match the insertion. In that case it should be annotated as a multiplication.
			var isGenomicDuplicate = Transcript.TranscriptUtilities.IsDuplicateWithinInterval(refSequence, variant, interval, onReverseStrand);

			return isGenomicDuplicate ? GenomicChange.Duplication : GenomicChange.Insertion;

			
		}
	}

	public enum GenomicChange
	{
		Unknown,
		Deletion,
		Duplication,
		DelIns,
		Insertion,
		Inversion,
		Multiple,
		Substitution
	}
}
