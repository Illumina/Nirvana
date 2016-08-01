using System.Collections.Generic;
using System.IO;
using VariantAnnotation.AnnotationSources;
using VariantAnnotation.FileHandling.CustomInterval;
using VariantAnnotation.FileHandling.SupplementaryAnnotations;
using VariantAnnotation.Interface;
using VariantAnnotation.Utilities;

namespace UnitTests.Utilities
{
    internal static class ResourceUtilities
    {
        /// <summary>
        /// creates a new annotation source with data from the micro-cache file
        /// </summary>
        internal static IAnnotationSource GetAnnotationSource(string resourcePath)
        {
            var annotatorPaths = new AnnotatorPaths(null,
                Path.Combine("Resources", "Homo_sapiens.GRCh37.75.chr1.Nirvana.dat"), null, new List<string>(),
                new List<string>());

            var annotationSource = new NirvanaAnnotationSource(annotatorPaths);
            annotationSource.DisableAnnotationLoader();
            if (resourcePath != null) annotationSource.LoadData(GetFileStream(Path.Combine("Caches", resourcePath)));
            return annotationSource;
        }

        /// <summary>
        /// creates a new supplementary annotation reader from the micro-SA files
        /// </summary>
        internal static SupplementaryAnnotationReader GetSupplementaryAnnotationReader(string resourcePath, bool isCustomAnnotation = false)
        {
            var resourceType = isCustomAnnotation ? "CustomAnnotations" : "MiniSuppAnnot";
            var path = Path.Combine("Resources", resourceType, resourcePath);
            return new SupplementaryAnnotationReader(path);
        }

        /// <summary>
        /// returns an enumerable list of supplementary intervals from the specified path
        /// </summary>
        internal static IEnumerable<ISupplementaryInterval> GetSupplementaryIntervals(string resourcePath)
        {
            IEnumerable<ISupplementaryInterval> intervals;

            using (var reader = GetSupplementaryAnnotationReader(resourcePath))
            {
                intervals = reader.GetSupplementaryIntervals();
            }

            return intervals;
        }

        /// <summary>
        /// returns an enumerable list of custom intervals from the specified path
        /// </summary>
		internal static IEnumerable<ICustomInterval> GetCustomIntervals(string ciPath)
		{
            using (var reader = new CustomIntervalReader(Path.Combine("Resources", "CustomIntervals", ciPath)))
            {
		        while (true)
		        {
		            var interval = reader.GetNextCustomInterval();
		            if (interval == null) break;
		            yield return interval;
		        }
		    }
		}

        /// <summary>
        /// returns a stream to the specified file in the resources subdirectory
        /// </summary>
        public static Stream GetFileStream(string filePath)
        {
            return FileUtilities.GetFileStream(Path.Combine("Resources", filePath));
        }
    }
}