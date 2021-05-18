using VariantAnnotation.AnnotatedPositions;
using VariantAnnotation.GeneFusions.Calling;
using VariantAnnotation.Interface.AnnotatedPositions;

namespace VariantAnnotation.GeneFusions.HGVS
{
    public static class HgvsRnaNomenclature
    {
        public static string GetHgvs(BreakPointTranscript first, BreakPointTranscript second)
        {
            string firstCoordinate  = GetHgvsRnaCoordinate(first);
            string secondCoordinate = GetHgvsRnaCoordinate(second);

            return
                $"{first.Transcript.Id.WithVersion}({first.Transcript.Gene.Symbol}):r.?_{firstCoordinate}::{second.Transcript.Id.WithVersion}({second.Transcript.Gene.Symbol}):r.{secondCoordinate}_?";
        }

        // ReSharper disable once UseDeconstructionOnParameter
        private static string GetHgvsRnaCoordinate(BreakPointTranscript first)
        {
            ITranscript    transcript     = first.Transcript;
            PositionOffset positionOffset = HgvsUtilities.GetPositionOffset(transcript, first.GenomicPosition, first.RegionIndex, true);
            return positionOffset.Value;
        }
    }
}