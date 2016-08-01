using System;
using System.Collections.Generic;
using System.IO;
using SAUtils.InputFileParsers;
using SAUtils.InputFileParsers.CustomInterval;
using VariantAnnotation.FileHandling.SupplementaryAnnotations;
using VariantAnnotation.FileHandling.CustomInterval;
using VariantAnnotation.FileHandling;
using ErrorHandling.Exceptions;

namespace SAUtils.CreateCustomIntervalDatabase
{
	public class CustomIntervalDbCreator
	{
		private readonly CustomIntervalParser _intervalParser;
		private readonly string _outputDirectory;
		private readonly HashSet<string> _observedRefSeq;
		private readonly DataSourceVersion _dataVersion;

		public CustomIntervalDbCreator(string bedFile, string outputDirectory)
		{
			if (bedFile == null) return;

			_outputDirectory = outputDirectory;
			_observedRefSeq = new HashSet<string>();

			_intervalParser = new CustomIntervalParser(new FileInfo(bedFile));

			_dataVersion = AddSourceVersion(bedFile);

		}

		private DataSourceVersion AddSourceVersion(string dataFileName)
		{
			var versionFileName = dataFileName + ".version";

			if (!File.Exists(versionFileName))
			{
				throw new FileNotFoundException(versionFileName);
			}

			var versionReader = new DataSourceVersionReader(versionFileName);
			var version = versionReader.GetVersion();
			Console.WriteLine(version.ToString());
			return version;
			
		}
		public void Create()
		{
			string refName = null;

			CustomIntervalWriter customIntervalWriter= null;
			
			foreach (var interval in _intervalParser)
			{
				if (interval.ReferenceName!= refName)
				{
					if (refName != null) _observedRefSeq.Add(refName);
					// need to close open file and open a new one
					customIntervalWriter?.Dispose();

					refName = interval.ReferenceName;
					var ucscRefName = AnnotationLoader.Instance.ChromosomeRenamer.GetUcscReferenceName(refName);

					if (_observedRefSeq.Contains(refName)) 
						throw new GeneralException("The input file does not seem to be sorted by reference names. Please sort it and retry.");

					var intervalType = interval.Type;
					customIntervalWriter = new CustomIntervalWriter(Path.Combine(_outputDirectory, ucscRefName + ".nci"), refName, intervalType, _dataVersion);
				}

				customIntervalWriter?.WriteInterval(interval);
			}

			customIntervalWriter?.Dispose();

		}
	}
}
