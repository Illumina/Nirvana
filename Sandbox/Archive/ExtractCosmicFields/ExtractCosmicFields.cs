using System;
using System.IO;
using System.Text;
using Illumina.VariantAnnotation.FileHandling;

namespace ExtractCosmicFields
{
	public class ExtractCosmicFields
	{
		private const int MutationIdIndex       = 16;
		private const int GenomePositionIndex   = 23;
		private const int GeneNameIndex         = 0;
		private const int PrimarySiteIndex      = 7;
		private const int PrimaryHistologyIndex = 11;
		private const int StudyIdIndex          = 30;

		private const string GeneTag        = "GENE";
		private const string PrimarySiteTag = "PRI_SITE";
		private const string HistologyTag   = "HIST";
		private const string StudyIdTag     = "STUDY_ID";


		static void Main(string[] args)
		{
			if (args.Length == 0)
			{
				Console.WriteLine("Usage: ExtractCosmicFields [input tsv file name] [output tsv file stub]");
				return;
			}

			int numNoCosmicId = 0;
			var numNoGenomicPosition = 0;
			var inputTsv = args[0];
			var outputTsvStub = args[1];
			using (var reader = GZipUtilities.GetAppropriateStreamReader(inputTsv))
			using (var writerPlaced = GZipUtilities.GetStreamWriter(outputTsvStub+".placed.vcf.gz"))
				using (var writerUnplaced = GZipUtilities.GetStreamWriter(outputTsvStub + ".unplaced.vcf.gz"))
			{
				// writing header
				AddHeader(writerPlaced);
				AddHeader(writerUnplaced);
				
				string line;
				while ((line = reader.ReadLine()) != null)
				{
					var words = line.Split('\t');
					if (!HasRequiredFields(words, ref numNoCosmicId, ref numNoGenomicPosition)) continue;

					var newTsvEntry = ExtractTsvFields(words);

					if (newTsvEntry.StartsWith("unknown"))
					{
						// for multi base deletions or indels, due to lack of ref and alt allele, we may know know the exact position
						writerUnplaced.WriteLine(newTsvEntry);
						numNoGenomicPosition++;
					}
					else
						writerPlaced.WriteLine(newTsvEntry);
				}
			}

			Console.WriteLine("No of entries with no cosmic id:{0}", numNoCosmicId);
			Console.WriteLine("Number of entries with no genomic position:{0}", numNoGenomicPosition);

		}

		private static void AddHeader(StreamWriter writer)
		{
			// Gene	Primary Site	Histology	StudyId
			writer.WriteLine("##fileformat=VCFv4.1");
			writer.WriteLine("##INFO=<ID="+GeneTag+",Number=1,Type=String,Description=\"Gene name\">");
			writer.WriteLine("##INFO=<ID="+PrimarySiteTag+",Number=1,Type=String,Description=\"Primary site\">");
			writer.WriteLine("##INFO=<ID="+HistologyTag+",Number=1,Type=String,Description=\"Histology\">");
			writer.WriteLine("##INFO=<ID="+StudyIdTag+",Number=1,Type=String,Description=\"Study ID\">");
			writer.WriteLine(@"#CHROM	POS	ID	REF	ALT	QUAL	FILTER	INFO");

		}

		private static string ExtractTsvFields(string[] words)
		{
			// we will create a tab delimited entry with required fields that is as close to vcf as possible
			var sb = new StringBuilder();

			string refName;
			int startPos;

			if (string.IsNullOrEmpty(words[GenomePositionIndex]))
			{
				refName = "unknown";
				startPos = 0;
			}
			else
				ParseGenomePosition(words[GenomePositionIndex], out refName, out startPos);
			
			sb.Append(refName);
			sb.Append('\t');

			sb.Append(startPos.ToString());
			sb.Append('\t');

			sb.Append(words[MutationIdIndex]);
			sb.Append('\t');

			sb.Append(".\t.\t.\t.\t");//refAllele, altAllele, Quality, Filter

			if (!string.IsNullOrEmpty(words[GeneNameIndex]))
			{
				sb.Append(GeneTag + "=" + words[GeneNameIndex]);
				sb.Append(';');
			}

			if (!string.IsNullOrEmpty(words[PrimarySiteIndex]))
			{
				sb.Append(PrimarySiteTag + "=" + words[PrimarySiteIndex]);
				sb.Append(';');
			}

			if (!string.IsNullOrEmpty(words[PrimaryHistologyIndex]))
			{
				sb.Append(HistologyTag + "=" + words[PrimaryHistologyIndex].Replace(';',','));
				sb.Append(';');
			}

			if (!string.IsNullOrEmpty(words[StudyIdIndex]))
				sb.Append(StudyIdTag + "=" + words[StudyIdIndex]);

			return sb.ToString();
		}

		private static void ParseGenomePosition(string s, out string refName, out int startPos)
		{
			// 22:24175794-24175794
			var chrPos= s.Split(new[] { ':' }, 2);
			refName = chrPos[0];

			if (refName == "23") refName = "X";
			if (refName == "24") refName = "Y";
			if (refName == "25") refName = "MT";

			var positions = chrPos[1].Split(new[] {'-'}, 2);
			startPos = Convert.ToInt32(positions[0]);
			var endPos= Convert.ToInt32(positions[1]);


			if (endPos != startPos)
			{
				// the position is uncertain
				// for del, they increase the position by 1, but for insertion, they don't
				// as a result we are forced to take them up in the dictionary
				startPos = 0;
				refName = "unknown";
			}

		}

		private static bool HasRequiredFields(string[] words, ref int numNoCosmicId, ref int numNoGenomicPosition)
		{
			if (words.Length <= MutationIdIndex)
			{
				numNoCosmicId++;
				return false;
			}
		
			if (string.IsNullOrEmpty(words[MutationIdIndex]))
			{
				numNoCosmicId++;
				return false;
			}

			if (!words[MutationIdIndex].StartsWith("COS")) return false;//check against any line that is referring to something else

			if (words.Length <= GenomePositionIndex)
			{
				numNoGenomicPosition++;
			}
			if (string.IsNullOrEmpty(words[GenomePositionIndex]))
			{
				numNoGenomicPosition++;
			}
			return true;
		}
	}
}
