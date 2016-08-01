using System.Collections.Generic;
using System.IO;
using System.Linq;
using ErrorHandling.Exceptions;
using VariantAnnotation.DataStructures;
using VariantAnnotation.FileHandling.SupplementaryAnnotations;
using VariantAnnotation.Utilities;

namespace VariantAnnotation.FileHandling.Phylop
{
	public static class PhylopCommon
	{
		public const string Header        = "NirvanaPhylopDB";
		public const ushort SchemaVersion = 4;
		public const ushort DataVersion   = 1;

		public const int MaxIntervalLength = 4048;

		public static void CheckDirectoryIntegrity(string phylopDir, List<DataSourceVersion> mainDataSourceVersions, out PhylopDirectory phylopDirectory)
		{
			DataSourceVersion version = null;
			if (string.IsNullOrEmpty(phylopDir))
			{
				phylopDirectory = null;
				return ;
			}
			var genomeAssemblies = new HashSet<GenomeAssembly>();

			foreach (var saPath in Directory.GetFiles(phylopDir, "*.npd"))
			{
				using (var reader = new PhylopReader(new BinaryReader(FileUtilities.GetFileStream(saPath))))
				{
					if (version == null) version = reader.GetDataSourceVersion();
					else
					{
						var newVersion = reader.GetDataSourceVersion();
						if (newVersion != version)
							throw new UserErrorException($"Found more than one phylop data version represented in the following directory: {phylopDir}");
					}
					genomeAssemblies.Add(reader.GetGenomeAssembly());
				}
				
			}
			if (version !=null)
				mainDataSourceVersions.Add(version);

			if(genomeAssemblies.Count>1)
				throw new UserErrorException($"Found more than one GenomeAssemblies represented in the following directory: {phylopDir}");

			phylopDirectory = genomeAssemblies.Count > 0 ? new PhylopDirectory(genomeAssemblies.First()) : null;
		}
	}

	public class PhylopDirectory
	{
		public readonly GenomeAssembly GenomeAssembly;

		public PhylopDirectory(GenomeAssembly genomeAssembly)
		{
			GenomeAssembly = genomeAssembly;
		}
	}
}
