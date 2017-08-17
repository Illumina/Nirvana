using VariantAnnotation.AnnotatedPositions.Transcript;

namespace VariantAnnotation.AnnotatedPositions
{
    public static class HgvspNotation
    {
        public static string GetDelInsNotation(string proteinId, int start, int end, string refAbbreviation, string altAbbreviation)
        {
            if (altAbbreviation.StartsWith("Ter"))
                return GetSubstitutionNotation(proteinId, start, refAbbreviation.Substring(0, 3), "Ter");

            return start == end
                ? $"{proteinId}:p.({refAbbreviation}{start}delins{altAbbreviation})"
                : $"{proteinId}:p.({refAbbreviation.Substring(0, 3)}{start}_{refAbbreviation.Substring(refAbbreviation.Length - 3)}{end}delins{altAbbreviation})";
        }

        public static string GetInsertionNotation(string proteinId, int start, int end, string altAbbreviation, string peptideSeq)
        {
            // insertion past the last AA
            if (end > peptideSeq.Length) return null;

            var leftFlankingAa = AminoAcids.ConvertAminoAcidToAbbreviation(peptideSeq[start - 1]);
            if (altAbbreviation.StartsWith("Ter"))
            {
                var refAminoAcid = AminoAcids.ConvertAminoAcidToAbbreviation(peptideSeq[start]);
                return $"{proteinId}:p.({refAminoAcid}{end}Ter)";
            }

            var rightFlankingAa = end > peptideSeq.Length ? "Ter" : AminoAcids.ConvertAminoAcidToAbbreviation(peptideSeq[end - 1]);

            return $"{proteinId}:p.({leftFlankingAa}{start}_{rightFlankingAa}{end}ins{altAbbreviation})";
        }

        public static string GetFrameshiftNotation(string proteinId, int start, string refAbbreviation, string altAbbreviation, int countToStop)
        {
            if (altAbbreviation.StartsWith("Ter"))
                return $"{proteinId}:p.({refAbbreviation}{start}Ter)";

            return countToStop > 0 ?
                $"{proteinId}:p.({refAbbreviation}{start}{altAbbreviation}fsTer{countToStop})" :
                $"{proteinId}:p.({refAbbreviation}{start}{altAbbreviation}fsTer?)";
        }

        public static string GetExtensionNotation(string proteinId, int start, string refAbbreviation, string altAbbreviation, int countToStop)
        {
			return countToStop > 0 ?
                $"{proteinId}:p.({refAbbreviation}{start}{altAbbreviation.Substring(0, 3)}extTer{countToStop})" :
                $"{proteinId}:p.({refAbbreviation}{start}{altAbbreviation.Substring(0, 3)}extTer?)";
        }

        public static string GetDuplicationNotation(string proteinId, int start, int end, string altAbbreviation)
        {
            return start == end ?
                $"{proteinId}:p.({altAbbreviation}{start}dup)" :
                $"{proteinId}:p.({altAbbreviation.Substring(0, 3)}{start}_{altAbbreviation.Substring(altAbbreviation.Length - 3)}{end}dup)";
        }


        public static string GetStartLostNotation(string proteinId, int start, int end, string refAbbreviation)
        {
            return start == end
             ? $"{proteinId}:p.({refAbbreviation}{start}?)"
             : $"{proteinId}:p.({refAbbreviation.Substring(0, 3)}{start}_?{end})";
        }

        public static string GetSilentNotation(string hgvscNotation, int start, string refAbbreviation, bool isStopRetained)
        {
            return isStopRetained ? $"{hgvscNotation}(p.(Ter{start}=))" : $"{hgvscNotation}(p.({refAbbreviation}{start}=))";
        }

        internal static string GetSubstitutionNotation(string proteinId, int start, string refAbbreviation, string altAbbreviation)
        {
            // start lost
            if (start == 1 && refAbbreviation != altAbbreviation)
                return $"{proteinId}:p.({refAbbreviation}{start}?)";

            return $"{proteinId}:p.({refAbbreviation}{start}{altAbbreviation})";
        }

        internal static string GetUnknownNotation(string proteinId, int start, int end, string refAbbreviation, string altAbbreviation)
        {
            return start == end
                ? $"{proteinId}:p.({refAbbreviation}{start}{altAbbreviation})"
                : $"{proteinId}:p.({refAbbreviation}{start}_{altAbbreviation}{end})";
        }

        internal static string GetDeletionNotation(string proteinId, int start, int end, string refAbbreviation, bool isStopGained)
        {
            if (isStopGained)
                return $"{proteinId}:p.({refAbbreviation}{start}Ter)";

            return start == end ?
                $"{proteinId}:p.({refAbbreviation}{start}del)" :
                $"{proteinId}:p.({refAbbreviation.Substring(0, 3)}{start}_{refAbbreviation.Substring(refAbbreviation.Length - 3)}{end}del)";
        }
    }
}