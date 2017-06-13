using System;

namespace Remove1000GenomesConflicts
{
	public static class RemoveConflictingOnekgEntries
	{
		static void Main(string[] args)
		{
			if (args.Length < 2)
			{
				Console.WriteLine("Usage: RemoveConflictingOnekgEntries [input sorted vcf file name] [output vcf file name]");
				return;
			}

			var inputVcf  = args[0];
			var outputVcf = args[1];

			var conflictRemover= new ConflictRemover(inputVcf,outputVcf);

			var noLinesRemoved= conflictRemover.RemoveConflictingLines();

			Console.WriteLine("No of lines removed:{0}", noLinesRemoved);
		}

		
	}
}
