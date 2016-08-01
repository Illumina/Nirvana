using System;
using System.Collections.Generic;
using System.IO;
using Illumina.VariantAnnotation.FileHandling;

namespace AnalyzeOneKGenomeAN
{
	class AnalyzeOneKGenomeAn
	{
		private static Dictionary<string, HashSet<int>> anDict;

		static void Main(string[] args)
		{
			if (args.Length != 2)
			{
				Console.WriteLine("AnalyzeOneKGenomeAN inputFile outPutFile");
				return;
			}

			var inputFile = args[0];
			var outputFile = args[1];

			anDict = new Dictionary<string, HashSet<int>>();

			string previousChr = "";

			using (var reader = GZipUtilities.GetAppropriateStreamReader(inputFile))
			using (var writer = new StreamWriter(new FileStream(outputFile,FileMode.Create)))
			{
				string line;

				while (( line = reader.ReadLine()) != null)
				{
					if(string.IsNullOrEmpty(line)) continue;

					if(line.StartsWith("#")) continue;

					var contents = line.Split('\t');

					string chromosome = contents[VcfCommon.ChromIndex];
					string position = contents[VcfCommon.PosIndex];

					if (chromosome != previousChr)
					{
						Console.WriteLine($"Process {chromosome}");
						previousChr = chromosome;
					}


					string infoField = contents[VcfCommon.InfoIndex];

					var infos = infoField.Split(';');
					foreach (var info in infos)
					{
						
						if(!info.Contains("AN")) continue;

						var keyValues = info.Split('=');
						var key = keyValues[0];
						if(!key.Contains("AN")) continue;

						var value = keyValues[1];

						if (ProcessAn(key, int.Parse(value)))
						{
							writer.WriteLine($"{chromosome}\t{position}\t{key}\t{value}");
						}
					}

				}
			}


		}

		public static bool ProcessAn(string key, int value)
		{
			if (!anDict.ContainsKey(key))
			{
				anDict[key] = new HashSet<int> { value };
				return true;
			}

			if (!anDict[key].Contains(value))
			{
				anDict[key].Add(value);
				return true;
			}

			return false;

		}


	}
}
