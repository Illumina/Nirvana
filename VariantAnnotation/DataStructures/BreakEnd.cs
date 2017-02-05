using System;
using System.Text.RegularExpressions;
using VariantAnnotation.Utilities;
using ErrorHandling.Exceptions;

namespace VariantAnnotation.DataStructures
{
    public sealed class BreakEnd
    {
        #region members

        private readonly string _referenceName;
        private string _referenceName2;

        private readonly ushort _referenceIndex;
        public ushort ReferenceIndex2;

        public readonly int Position;
        public int Position2;

        public char IsSuffix; // '+' means from position to end, '-' means from 1 to position
        public char IsSuffix2;

        private char _orientation; // '+' means forward, "-" means reverse
        private char _orientation2;
        private readonly ChromosomeRenamer _renamer;

        private const string ForwardBreakEnd = "[";

        #endregion

        /// <summary>
        /// constructor
        /// </summary>
        private BreakEnd(string referenceName, int position, ChromosomeRenamer renamer)
        {
            _referenceIndex = renamer.GetReferenceIndex(referenceName);
            _referenceName = _referenceIndex >= renamer.NumRefSeqs ? referenceName : renamer.EnsemblReferenceNames[_referenceIndex];
            Position = position;
            _renamer = renamer;
        }

        /// <summary>
        /// constructor
        /// </summary>
        public BreakEnd(string referenceName, string position, string refAllele, string altAllele,
            ChromosomeRenamer renamer) : this(referenceName, Convert.ToInt32(position), renamer)
        {
            ParseAltAllele(refAllele, altAllele);
        }

        /// <summary>
        /// constructor
        /// </summary>
        public BreakEnd(string referenceName, string referenceName2, int position, int position2, char isSuffix,
            char isSuffix2, ChromosomeRenamer renamer) : this(referenceName, position, renamer)
        {
            ReferenceIndex2 = renamer.GetReferenceIndex(referenceName2);
            _referenceName2 = ReferenceIndex2 >= renamer.NumRefSeqs ? referenceName2 : renamer.EnsemblReferenceNames[ReferenceIndex2];
            Position2 = position2;
            IsSuffix = isSuffix;
            IsSuffix2 = isSuffix2;

            _orientation = IsSuffix == '+' ? '-' : '+';
            _orientation2 = IsSuffix2 == '+' ? '+' : '-';
        }

        /// <summary>
        /// parses the alternate allele
        /// </summary>
        private void ParseAltAllele(string refAllele, string altAllele)
        {
            var regexSuccess = false;

            // (\w+)([\[\]])([^:]+):(\d+)([\[\]])
            // ([\[\]])([^:]+):(\d+)([\[\]])(\w+)
            if (altAllele.StartsWith(refAllele))
            {
                var forwardRegex = new Regex(@"\w+([\[\]])([^:]+):(\d+)([\[\]])", RegexOptions.Compiled);
                var match = forwardRegex.Match(altAllele);

                if (match.Success)
                {
                    IsSuffix = '-';
                    _orientation = '+';
                    _referenceName2 = match.Groups[2].Value;
                    Position2 = Convert.ToInt32(match.Groups[3].Value);
                    IsSuffix2 = match.Groups[4].Value == ForwardBreakEnd ? '+' : '-';
                    _orientation2 = match.Groups[4].Value == ForwardBreakEnd ? '+' : '-';
                    regexSuccess = true;
                    ReferenceIndex2 = _renamer.GetReferenceIndex(match.Groups[2].Value);
                    if (ReferenceIndex2 < _renamer.NumRefSeqs)
                    {
                        _referenceName2 = _renamer.EnsemblReferenceNames[ReferenceIndex2];

                    }
                    else return;
                }
            }
            else
            {
                var reverseRegex = new Regex(@"([\[\]])([^:]+):(\d+)([\[\]])\w+", RegexOptions.Compiled);
                var match = reverseRegex.Match(altAllele);

                if (match.Success)
                {
                    IsSuffix = '+';
                    _orientation = '-';
                    IsSuffix2 = match.Groups[1].Value == ForwardBreakEnd ? '+' : '-';
                    _orientation2 = match.Groups[1].Value == ForwardBreakEnd ? '+' : '-';
                    _referenceName2 = match.Groups[2].Value;
                    Position2 = Convert.ToInt32(match.Groups[3].Value);
                    regexSuccess = true;
                    ReferenceIndex2 = _renamer.GetReferenceIndex(match.Groups[2].Value);
                    if (ReferenceIndex2 < _renamer.NumRefSeqs)
                    {
                        _referenceName2 = _renamer.EnsemblReferenceNames[ReferenceIndex2];
                    }
                    else return;
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
            var ensemblReferenceName = _referenceIndex >= _renamer.NumRefSeqs ? _referenceName : _renamer.EnsemblReferenceNames[_referenceIndex];
            var ensembleRferenceName2 = ReferenceIndex2 >= _renamer.NumRefSeqs ? _referenceName2 : _renamer.EnsemblReferenceNames[ReferenceIndex2];
            return $"{ensemblReferenceName}:{Position}:{_orientation}:{ensembleRferenceName2}:{Position2}:{_orientation2}";
        }
    }
}
