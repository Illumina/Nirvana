using System.IO;
using System.Linq;
using VariantAnnotation.Interface.Providers;

namespace SAUtils.GenericScore.GenericScoreParser
{
    public sealed class SaItemValidator
    {
        private readonly bool? _strictSnvCheck;
        private readonly bool? _strictReferenceCheck;
        
        
        /// <summary>
        /// Performs checks on each saItem, will throw exception if strict checking is enabled, otherwise returns true/false
        /// Setting strict checking to null disables all checks and true is always returned
        /// </summary>
        /// <param name="strictSnvCheck"></param> Set to null to disable, if true, then exception will be thrown
        /// <param name="strictReferenceCheck"></param> Set to null to disable, if true, then exception will be thrown
        public SaItemValidator(bool? strictSnvCheck, bool? strictReferenceCheck)
        {
            _strictSnvCheck       = strictSnvCheck;
            _strictReferenceCheck = strictReferenceCheck;
        }

        public bool Validate(GenericScoreItem saItem, ISequenceProvider refProvider)
        {
            return CheckSnv(saItem) && CheckReference(saItem, refProvider);
        }

        private bool CheckReference(GenericScoreItem saItem, ISequenceProvider refProvider)
        {
            if (_strictReferenceCheck == null)
                return true;

            bool   hasParRegions = CheckParRegion(saItem, refProvider);
            string refSequence   = refProvider.Sequence.Substring(saItem.Position - 1, saItem.RefAllele.Length);

            if (string.IsNullOrEmpty(saItem.RefAllele) || saItem.RefAllele == refSequence || hasParRegions)
                return true;

            if (_strictReferenceCheck == false)
                return false;

            throw new InvalidDataException(
                $"The provided reference allele {saItem.RefAllele} at {saItem.Chromosome.UcscName}:{saItem.Position} is different from {refSequence} in the reference genome sequence." +
                $"\nInput Line:\n {saItem.InputLine}");
        }

        private bool CheckParRegion(GenericScoreItem saItem, ISequenceProvider refProvider)
        {
            return RegionUtilities.OverlapsParRegion(saItem, refProvider.Assembly)
                   && !string.IsNullOrEmpty(saItem.RefAllele)
                   && saItem.RefAllele.All(x => x is 'N' or 'n');
        }

        private bool CheckSnv(GenericScoreItem saItem)
        {
            if (_strictSnvCheck == null)
                return true;

            if (saItem.RefAllele.Length == 1 && saItem.AltAllele.Length == 1)
                return true;

            if (_strictSnvCheck == false)
                return false;

            throw new InvalidDataException($"Only SNV is expected in the input file. Exception found: {saItem.Chromosome}:{saItem.Position}");
        }
    }
}