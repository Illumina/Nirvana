using System.Linq;
using ErrorHandling.Exceptions;
using VariantAnnotation.Interface.Providers;

namespace SAUtils.GenericScore.GenericScoreParser
{
    public sealed class SaItemValidator
    {
        public bool Validate(GenericScoreItem saItem, bool skipIncorrectRefEntries, ISequenceProvider refProvider)
        {
            bool   hasParRegions = CheckParRegion(saItem, refProvider);
            string refSequence   = refProvider.Sequence.Substring(saItem.Position - 1, saItem.RefAllele.Length);

            if (string.IsNullOrEmpty(saItem.RefAllele) || saItem.RefAllele == refSequence || hasParRegions) return true;
            if (skipIncorrectRefEntries) return false;
            throw new UserErrorException(
                $"The provided reference allele {saItem.RefAllele} at {saItem.Chromosome.UcscName}:{saItem.Position} is different from {refSequence} in the reference genome sequence." +
                $"\nInput Line:\n {saItem.InputLine}");
        }

        private bool CheckParRegion(GenericScoreItem saItem, ISequenceProvider refProvider)
        {
            return RegionUtilities.OverlapsParRegion(saItem, refProvider.Assembly)
                   && !string.IsNullOrEmpty(saItem.RefAllele)
                   && saItem.RefAllele.All(x => x is 'N' or 'n');
        }
    }
}