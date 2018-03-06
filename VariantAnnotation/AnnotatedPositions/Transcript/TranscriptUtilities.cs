using CommonUtilities;
using VariantAnnotation.Interface.AnnotatedPositions;
using VariantAnnotation.Interface.Sequence;
using VariantAnnotation.Utilities;

namespace VariantAnnotation.AnnotatedPositions.Transcript
{
    public static class TranscriptUtilities
    {
	    /// <summary>
	    /// returns the alternate CDS given the reference sequence, the cds coordinates, and the alternate allele.
	    /// </summary>
	    public static string GetAlternateCds(ISequence refSequence, int cdsBegin, int cdsEnd, string alternateAllele,
		    ITranscriptRegion[] regions, bool onReverseStrand, byte startExonPhase, int cdnaCodingStart)
	    {
		    var splicedSeq     = GetSplicedSequence(refSequence, regions, onReverseStrand);
		    int numPaddedBases = startExonPhase;

            int shift           = cdnaCodingStart - 1;
            int upstreamLength  = GetUpstreamLength(shift, cdsBegin - numPaddedBases - 1, splicedSeq.Length);
            int downstreamStart = cdsEnd - numPaddedBases + shift;

            string upstreamSeq   = splicedSeq.Substring(shift, upstreamLength);
	        string downstreamSeq = downstreamStart < splicedSeq.Length ? splicedSeq.Substring(downstreamStart) : "";

		    if (alternateAllele == null) alternateAllele = string.Empty;
		    var paddedBases = numPaddedBases > 0 ? new string('N', numPaddedBases) : "";

		    return paddedBases + upstreamSeq + alternateAllele + downstreamSeq;
	    }

        private static int GetUpstreamLength(int start, int length, int seqLength)
        {
            int desiredLength = start + length;
            int maxLength     = seqLength - start;
            return desiredLength <= seqLength ? length : maxLength;
        }

        /// <summary>
	    /// Retrieves all Exon sequences and concats them together. 
	    /// This includes 5' UTR + cDNA + 3' UTR [Transcript.pm:862 spliced_seq]
	    /// </summary>
	    private static string GetSplicedSequence(ISequence refSequence, ITranscriptRegion[] regions, bool onReverseStrand)
	    {
		    var sb = StringBuilderCache.Acquire();

		    foreach (var region in regions)
		    {
		        if (region.Type != TranscriptRegionType.Exon) continue;
			    var exonLength = region.End - region.Start + 1;

			    // sanity check: handle the situation where no reference has been provided
			    if (refSequence == null)
			    {
				    sb.Append(new string('N', exonLength));
				    continue;
			    }

			    sb.Append(refSequence.Substring(region.Start - 1, exonLength));
		    }

	        var results = StringBuilderCache.GetStringAndRelease(sb);
		    return onReverseStrand ? SequenceUtilities.GetReverseComplement(results) : results;
	    }
    }
}