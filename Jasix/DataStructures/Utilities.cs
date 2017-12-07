using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using ErrorHandling.Exceptions;

namespace Jasix.DataStructures
{
    public static class Utilities
    {
        private const char DoubleQuote  = '\"';
        private const char OpenBracket  = '[';
        private const char CloseBracket = ']';

        public static (string Chromosome, int Start, int End) ParseQuery(string position)
        {
            //chr1:100-101
            //chr1:100
            //chr1 - report all entries for chr1

            var regexPos = new Regex(@"^(\w+)(?::(\d+)(?:-(\d+))?)?$", RegexOptions.Compiled);

            var trimmedPos = position.Trim(' ');
            var match = regexPos.Match(trimmedPos);
            if (!match.Success)
                throw new UserErrorException($"region {trimmedPos} is not valid, please specify a valid region, e.g., chr1, 1, 1:1234 or 1:1234-4567");
            var chromosome = match.Groups[1].ToString();
            if (!match.Groups[2].Success && !match.Groups[3].Success) return (chromosome, 1, int.MaxValue);

            var start = Convert.ToInt32(match.Groups[2].ToString());

            int end = match.Groups[3].Success ? Convert.ToInt32(match.Groups[3].ToString()) : start;

            return (chromosome, start, end);
        }

	    public static void PrintQuerySectionOpening(string sectionName, StreamWriter writer)
	    {
		    writer.Write(DoubleQuote + sectionName + DoubleQuote+ ":" + OpenBracket + Environment.NewLine);
	    }

	    public static void PrintQuerySectionClosing(StreamWriter writer)
	    {
		    writer.Write(Environment.NewLine + CloseBracket);
	    }

	    public static void PrintJsonEntry(string entry, bool needComma, StreamWriter writer)
	    {
		    if (needComma)
			    writer.Write("," + Environment.NewLine);
			writer.Write(entry);
		}

	    public static bool IsLargeVariant(int start, int end)
        {
            return end - start + 1 > JasixCommons.MinNodeWidth;
        }

        public static int GetJsonEntryEnd(JsonSchema jsonEntry)
        {
            if (jsonEntry.svEnd > 0) return jsonEntry.svEnd;
            var altAlleles = jsonEntry.altAlleles;
            int altAlleleOffset = altAlleles != null && altAlleles.All(IsNucleotideAllele) && altAlleles.Any(x => x.Length > 1) ? 1 : 0;

            return Math.Max(jsonEntry.refAllele.Length - 1, altAlleleOffset) + jsonEntry.position;
        }

        public static bool IsNucleotideAllele(string altAllele)
        {
            return string.IsNullOrEmpty(altAllele) || altAllele.ToCharArray().All(x => x == 'A' || x == 'T' || x == 'C' || x == 'G');
        }
    }
}
