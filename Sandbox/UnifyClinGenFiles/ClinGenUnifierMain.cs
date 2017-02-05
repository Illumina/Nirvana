using System;
using System.IO;

namespace UnifyClinGenFiles
{
	internal static class ClinGenUnifierMain
	{
		public static void Main(string[] args)
		{
			if (args.Length != 3)
			{
				Console.WriteLine("UnifyClinGenFile.exe inputFile outputFile refNameFile\n");
				return;
			}

			var inputFileName = args[0];
			var outputFileName = args[1];
			var refNameFile = args[2];

			var unifier = new ClinGenUnifier( new FileInfo(inputFileName), new FileInfo(refNameFile));

			unifier.Unify();
			unifier.Write(outputFileName);
		}
	}
}
