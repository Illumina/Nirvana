using System.Text.RegularExpressions;
using ErrorHandling.Exceptions;
using VariantAnnotation.FileHandling;
using VariantAnnotation.Utilities;

namespace VariantAnnotation.DataStructures
{
    public class BreakEnd
    {
        #region members

        private readonly string _referenceName;
        private string _referenceName2;

        private readonly string _position;
        private string _position2;

        private char _orientation;
        private char _orientation2;

        private const string ForwardBreakEnd = "[";

        private readonly ChromosomeRenamer _chromosomeRenamer;

        #endregion

        // constructor
        public BreakEnd(string referenceName, string position, string refAllele, string altAllele)
        {
            _chromosomeRenamer = AnnotationLoader.Instance.ChromosomeRenamer;
            _referenceName = _chromosomeRenamer.GetEnsemblReferenceName(referenceName);
            _position      = position;

            ParseAltAllele(refAllele, altAllele);
        }

        /// <summary>
        /// parses the alternate allele
        /// </summary>
        private void ParseAltAllele(string refAllele, string altAllele)
        {
            bool regexSuccess = false;

            // (\w+)([\[\]])([^:]+):(\d+)([\[\]])
            // ([\[\]])([^:]+):(\d+)([\[\]])(\w+)
            if (altAllele.StartsWith(refAllele))
            {
                var forwardRegex = new Regex(@"\w+([\[\]])([^:]+):(\d+)([\[\]])", RegexOptions.Compiled);
                var match        = forwardRegex.Match(altAllele);

                if (match.Success)
                {
                    _orientation    = match.Groups[1].Value == ForwardBreakEnd ? '+' : '-';
					_referenceName2 = _chromosomeRenamer.GetEnsemblReferenceName(match.Groups[2].Value);
                    _position2      = match.Groups[3].Value;
                    _orientation2   = match.Groups[4].Value == ForwardBreakEnd ? '+' : '-';
                    regexSuccess   = true;
                }
            }
            else
            {
                var reverseRegex = new Regex(@"([\[\]])([^:]+):(\d+)([\[\]])\w+", RegexOptions.Compiled);
                var match        = reverseRegex.Match(altAllele);

                if (match.Success)
                {
                    _orientation2   = match.Groups[1].Value == ForwardBreakEnd ? '+' : '-';
                    _referenceName2 = _chromosomeRenamer.GetEnsemblReferenceName(match.Groups[2].Value);
                    _position2      = match.Groups[3].Value;
                    _orientation    = match.Groups[4].Value == ForwardBreakEnd ? '+' : '-';
                    regexSuccess   = true;
                }
            }

            if (!regexSuccess)
            {
                throw new GeneralException(
                    "Unable to successfully parse the complex rearrangements for the following allele: " + altAllele);
            }
        }

        /// <summary>
        /// returns a string representation of this breakend
        /// </summary>
        public override string ToString()
        {
            return $"{_referenceName}:{_position}:{_orientation}:{_referenceName2}:{_position2}:{_orientation2}";
        }
    }
}
