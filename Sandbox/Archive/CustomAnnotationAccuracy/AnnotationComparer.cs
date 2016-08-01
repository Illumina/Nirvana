using System;
using System.Collections.Generic;
using System.IO;
using Illumina.VariantAnnotation.DataStructures;
using Illumina.VariantAnnotation.FileHandling;
using Illumina.VariantAnnotation.FileHandling.SupplementaryAnnotations;

namespace CustomAnnotationAccuracy
{
	internal sealed class AnnotationComparer
	{
		#region members

	    private readonly int _missingEntriesCount;
		private readonly string _vcfFile;

	    #endregion

        public AnnotationComparer(string vcfFile, string refFile, string cacheFile)
        {
            CacheDirectory cacheDirectory;
            _vcfFile = vcfFile;

            // generate temprary custom annotation direcory
            var tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            if (!Directory.Exists(tempDir)) Directory.CreateDirectory(tempDir);

            // create the supplementary database
            List<string> customAnnotationFiles = new List<string> { vcfFile };

            // ReSharper disable once UnusedVariable
            var supplementaryDatabaseCreator =
                new CreateSupplementaryDatabase.CreateSupplementaryDatabase(tempDir, null, null, null, null,
                    null, null, null, null, null, customAnnotationFiles);

            // initiate the count:
            _missingEntriesCount = 0;

            var dataSourceVersions = new List<DataSourceVersion>();

            NirvanaDatabaseCommon.CheckDirectoryIntegrity(cacheFile, dataSourceVersions, out cacheDirectory);

            //_annotationDestination = new JsonOutputDestination();
            //_annotationSource = new NirvanaAnnotationSource(null, _annotationDestination, cacheDirectory.RefSeqsToPaths, false, new List<string>() { tempDir});
            AnnotationLoader.Instance.LoadCompressedSequence(refFile);
        }

        public void Compare()
		{
			using (var reader = GZipUtilities.GetAppropriateStreamReader(_vcfFile))
			{
				string line;

				while ((line = reader.ReadLine()) != null)
				{
					// skip empty lines
					if (string.IsNullOrWhiteSpace(line)) continue;

					// parse head of vcf line to get the annotationType
					if (line.StartsWith("##IAE_TOP="))
					{
						GetAnnotationType(line);
						continue;
					}
					if (line.StartsWith("#")) continue;

					// check if JSON output contain the annotationType for each vcf line
					var variant = new VariantFeature();
					variant.ParseVcfLine(line);
                    AnnotationLoader.Instance.Load(variant.UcscReferenceName);

					//_annotationSource.Annotate(variant);

					//if (!(_annotationDestination.ToString().Contains(_annotationType)) || variant.AlternateAlleles[0].SupplementaryAnnotation == null)
     //               {
					//	Console.WriteLine(line);
					//	MissingEntriesCount++;
					//}

				}

				Console.WriteLine($"The total number of missing entries is {_missingEntriesCount}");
			}
		}

		private void GetAnnotationType(string line)
		{
			// ##IAE_TOP=<KEY=cosmic,MATCH=Allele>
			line = line.Substring(11); //removing ##IAE_TOP=<
			line = line.Substring(0, line.Length - 1); //removing the last '>'
			var fields = line.Split(',');

			foreach (var field in fields)
			{
				var keyValue = field.Split('=');
				var key = keyValue[0];

			    switch (key)
				{
					case "KEY":
				        break;
					case "MATCH":
						// default is allele specific
				        break;
					default:
						throw new Exception("Unknown field in top level key line :\n " + line);
				}
			}
		}
	}

}
