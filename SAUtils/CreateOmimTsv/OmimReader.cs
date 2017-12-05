using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using VariantAnnotation.GeneAnnotation;

namespace SAUtils.CreateOmimTsv
{
	public sealed class OmimReader 
	{
		#region members

		private readonly Stream _omimFileStream;
        private readonly GeneSymbolUpdater _geneSymbolUpdater;
        private int _mimNumberCol = -1;
		private int _hgncCol = -1;
		private int _geneDescriptionCol = -1;
		private int _phenotypeCol =-1;

		#endregion


		public OmimReader(Stream omimFileStream, GeneSymbolUpdater geneSymbolUpdater)
	    {
            _omimFileStream = omimFileStream;
            _geneSymbolUpdater = geneSymbolUpdater;
	    }
		
		public IEnumerable<OmimEntry> GetOmimItems()
		{
			using (var reader = new StreamReader(_omimFileStream))
			{
				string line;
				while ((line = reader.ReadLine()) != null)
				{
					if (IsHeader(line))
					{
						ParseHeader(line);
						continue;
					}
					if(!IsContentLine(line)) continue;

					var contents = line.Split('\t');
					var mimNumber =Convert.ToInt32(contents[_mimNumberCol]);
					var geneSymbol = contents[_hgncCol];
					var description = _geneDescriptionCol >=0 ?contents[_geneDescriptionCol].Replace(@"\\'",@"'"):null;
					var phenotypeInfo = _phenotypeCol>=0? contents[_phenotypeCol].Replace(@",,", @","):null;
					var phenotypes = ParsePhenotypes(phenotypeInfo);

                    if(string.IsNullOrEmpty(geneSymbol)) continue;

                    var updatedGeneSymbol = _geneSymbolUpdater.UpdateGeneSymbol(geneSymbol);
					yield return new OmimEntry(updatedGeneSymbol, description, mimNumber, phenotypes);
				}
			}
		}

		private void ParseHeader(string line)
		{
			line = line.Trim('#').Trim(' ');
			var colNames = line.Split('\t').Select(x => x.Trim(' ')).ToList();
			for (var index = 0; index < colNames.Count; index++)
			{
                var colname = colNames[index].ToLower();

                if(colname == "mim number")
                {
                    _mimNumberCol = index;
                }else if(colname == "gene name")
                {
                    _geneDescriptionCol = index;
                }else if(colname== "approved symbol"|| colname.StartsWith("approved gene symbol"))
                {
                    _hgncCol = index;
                }else if(colname == "phenotypes")
                {
                    _phenotypeCol = index;
                }

			
			}
		}

		private static List<OmimEntry.Phenotype> ParsePhenotypes(string line)
		{
			var phenotypes = new List<OmimEntry.Phenotype>();

			if (string.IsNullOrEmpty(line)) return phenotypes;

			var infos = line.Split(';');
			phenotypes.AddRange(infos.Select(ExtractPhenotype));

			return phenotypes;
		}

		private static OmimEntry.Phenotype ExtractPhenotype(string info)
		{
			info = info.Trim(' ').Replace(@"\\'","'");
			
			if (string.IsNullOrWhiteSpace(info) || string.IsNullOrEmpty(info)) return null;

			var phenotypeRegex = new Regex(@"^(.+?)(?:,\s(\d{6}))?\s\((\d)\)(?:,\s)?(.*)?$");
			var match = phenotypeRegex.Match(info);
			var phenotypeGroup = match.Groups[1].ToString();
		    ParsePhenotypeMapping(phenotypeGroup, out var phenotype, out var comments);

			var mimNumber = string.IsNullOrEmpty(match.Groups[2].Value) ? 0 : Convert.ToInt32(match.Groups[2].Value);
			var mapping = (OmimEntry.Mapping) Convert.ToInt16(match.Groups[3].Value);

			var inheritance = string.IsNullOrEmpty(match.Groups[4].Value) ? null : match.Groups[4].ToString();
			var inheritances = ExtractInheritances(inheritance);
			return new OmimEntry.Phenotype(mimNumber,phenotype,mapping,comments,inheritances);
		}

		private static HashSet<string> ExtractInheritances(string inheritance)
		{			
			var inheritances = new HashSet<string>();
			if (string.IsNullOrEmpty(inheritance)) return inheritances;

			var contents = inheritance.Split(',');
			foreach (var content in contents)
			{
				var trimmedContent = content.Trim(' ');
				inheritances.Add(trimmedContent);
			}

			return inheritances;
		}

		private static void ParsePhenotypeMapping(string phenotypeGroup, out string phenotype, out OmimEntry.Comments comments)
		{
			phenotypeGroup = phenotypeGroup.Trim(' ');
			phenotype = phenotypeGroup.TrimStart('?', '{', '[').TrimEnd('}', ']');
			comments = OmimEntry.Comments.unknown;

			if (phenotypeGroup.Substring(0, 2).Contains("?"))
			{
				comments = OmimEntry.Comments.unconfirmed_or_possibly_spurious_mapping;
			}
			else
			{
				if (phenotypeGroup.StartsWith("{"))
				{
					comments = OmimEntry.Comments.contribute_to_susceptibility_to_multifactorial_disorders_or_to_susceptibility_to_infection;
				}else if (phenotypeGroup.StartsWith("["))
				{
					comments = OmimEntry.Comments.nondiseases;
				}

			}

		}

		private static bool IsHeader(string line)
		{
			return line.StartsWith("# Chromosome\t") || line.StartsWith("# MIM Number\t");
		}

		private static bool IsContentLine(string line)
		{
			return !line.StartsWith("#");
		}
	}
}