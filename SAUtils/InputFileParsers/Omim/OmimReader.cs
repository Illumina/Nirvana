using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Compression.Utilities;
using VariantAnnotation.GeneAnnotation;

namespace SAUtils.InputFileParsers.Omim
{
	public class OmimReader : IEnumerable<OmimEntry>
	{
		#region members

		private readonly FileInfo _omimFileInfo;
		private int _mimNumberCol;
		private int _hgncCol;
		private int _geneDescriptionCol;
		private int _phenotypeCol;

		#endregion


		public OmimReader(FileInfo omimFileInfo)
	    {
	        _omimFileInfo = omimFileInfo;
	    }
		public IEnumerator<OmimEntry> GetEnumerator()
		{
			return GetOmimItems().GetEnumerator();
		}


		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}



		private IEnumerable<OmimEntry> GetOmimItems()
		{
			using (var reader = GZipUtilities.GetAppropriateStreamReader(_omimFileInfo.FullName))
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
					var description = contents[_geneDescriptionCol].Replace(@"\\'",@"'");
					var phenotypeInfo = contents[_phenotypeCol].Replace(@",,", @",");
					var phenotypes = ParsePhenotypes(phenotypeInfo);

                    if(string.IsNullOrEmpty(geneSymbol)) continue;


					yield return new OmimEntry(geneSymbol, description, mimNumber, phenotypes);
				}
			}
		}

		private void ParseHeader(string line)
		{
			line = line.Trim('#').Trim(' ');
			var colNames = line.Split('\t').Select(x => x.Trim(' ')).ToList();
			for (var index = 0; index < colNames.Count; index++)
			{
				switch (colNames[index])
				{
					case "Mim Number":
						_mimNumberCol = index;
						break;
					case "Gene Name":
						_geneDescriptionCol = index;
						break;
					case "Approved Symbol":
						_hgncCol = index;
						break;
					case "Phenotypes":
						_phenotypeCol = index;
						break;
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
			string phenotype;
		    OmimEntry.Comments comments;
			ParsePhenotypeMapping(phenotypeGroup, out phenotype, out comments);

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
			return line.StartsWith("# Chromosome");
		}

		private static bool IsContentLine(string line)
		{
			return !line.StartsWith("#");
		}
	}
}