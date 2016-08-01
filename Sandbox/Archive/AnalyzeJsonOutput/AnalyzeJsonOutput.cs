using System;
using System.Collections.Generic;
using System.IO;
using Illumina.VariantAnnotation.FileHandling;
using Ilmn.Das.Std.BioinformaticUtils.Illumina.Json;


namespace AnalyzeJsonOutput
{
	class AnalyzeJsonOutput
	{
		static void Main(string[] args)
		{
			if (args.Length != 3)
			{
				Console.WriteLine("or AnalyzeJsonOutput [inputJsonFile] [outputFile] [ncharFile]");
				return;
			}

			var jsonFileName = args[0];
			var outFileName = args[1];

			var nCharFileName = args[2];

			var ncharWriter = new StreamWriter(new FileStream(nCharFileName, FileMode.Create));
			using (ncharWriter)
			using (var reader = GZipUtilities.GetAppropriateStreamReader(jsonFileName))
			{
				string line;
				while ((line = reader.ReadLine()) != null)
				{
					ncharWriter.WriteLine(line.Length);
				}
			}


			var jsonReader = new JsonReader(new FileInfo(jsonFileName));
			var preRef = "";
			Dictionary<long, int> chrLenCounts = new Dictionary<long, int>();
			Dictionary<long, int> totalLenCounts = new Dictionary<long, int>();

			

			using (var writer = new StreamWriter(new FileStream(outFileName, FileMode.Create)))
			
			{
				foreach (var jsonPos in jsonReader)
				{
					var refName = jsonPos.Contig.Name;
					if (preRef != refName)
					{
						if (preRef != "")
						{
							Console.WriteLine($"Finish chr{preRef}");
							foreach (var kvPair in chrLenCounts)
							{
								writer.WriteLine($"{preRef}\t{kvPair.Key}\t{kvPair.Value}");
							}
							chrLenCounts = new Dictionary<long, int>();
						}
						preRef = refName;

					}

					uint pos = jsonPos.Position;
					var maxEnd = -1;
					foreach (var altAllele in jsonPos.Variants)
					{
						if (altAllele.End > maxEnd) maxEnd = altAllele.End;
					}
					var len = maxEnd - pos + 1;
					if (chrLenCounts.ContainsKey(len))
					{
						chrLenCounts[len]++;
					}
					else
					{
						chrLenCounts[len] = 1;
					}
					if (totalLenCounts.ContainsKey(len))
					{
						totalLenCounts[len]++;
					}
					else
					{
						totalLenCounts[len] = 1;
					}

				}

				Console.WriteLine($"Finish chr{preRef}");
				foreach (var kvPair in chrLenCounts)
				{
					writer.WriteLine($"{preRef}\t{kvPair.Key}\t{kvPair.Value}");
				}

				foreach (var kvPair in totalLenCounts)
				{
					writer.WriteLine($"total\t{kvPair.Key}\t{kvPair.Value}");
				}

			}



		}
	}
}
