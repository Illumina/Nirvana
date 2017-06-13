using System.Collections.Generic;
using System.IO;
using UnitTests.Mocks;
using VariantAnnotation.AnnotationSources;
using VariantAnnotation.DataStructures;
using VariantAnnotation.FileHandling.SA;
using VariantAnnotation.Interface;
using VariantAnnotation.Utilities;

namespace UnitTests.Utilities
{
    internal static class ResourceUtilities
    {
        /// <summary>
        /// creates a new annotation source with data from the micro-cache file
        /// </summary>
        internal static IAnnotationSource GetAnnotationSource(string cachePath, List<ISupplementaryAnnotationReader> saReaders,
            IConservationScoreReader conservationScoreReader = null)
        {
            var streams    = GetAnnotationSourceStreams(cachePath);
            //var renamer    = ChromosomeRenamer.GetChromosomeRenamer(GetReadStream($"{cachePath}.bases"));
            var saProvider = new MockSupplementaryAnnotationProvider(saReaders);
            PerformanceMetrics.DisableOutput = true;

            return new NirvanaAnnotationSource(streams, saProvider, conservationScoreReader, null);
        }

        private static AnnotationSourceStreams GetAnnotationSourceStreams(string cachePath)
        {
            if (string.IsNullOrEmpty(cachePath)) return new AnnotationSourceStreams(null, null, null, null);

            var transcriptPath = cachePath + ".ndb";
            var siftPath       = cachePath + ".sift";
            var polyPhenPath   = cachePath + ".polyphen";
            var referencePath  = cachePath + ".bases";

            var transcriptStream = GetReadStream(transcriptPath);
            var siftStream       = GetReadStream(siftPath, false);
            var polyPhenStream   = GetReadStream(polyPhenPath, false);
            var referenceStream  = GetReadStream(referencePath);

            return new AnnotationSourceStreams(transcriptStream, siftStream, polyPhenStream, referenceStream);
        }

        internal static ISupplementaryAnnotationReader GetSupplementaryAnnotationReader(string saPath)
        {
            if (saPath == null) return null;

            var saStream      = GetReadStream(saPath);
            var saIndexStream = GetReadStream(saPath + ".idx");
            return new SaReader(saStream, saIndexStream);
        }

        /// <summary>
        /// given a path, this method returns a stream corresponding to the file if
        /// it exists. Otherwise a file not found exception is thrown.
        /// </summary>
        public static Stream GetReadStream(string path, bool checkMissingFile = true)
        {
            var missingFile = !File.Exists(path);
            if (!checkMissingFile && missingFile) return null;
            
            if (missingFile)
            {
                throw new FileNotFoundException($"ERROR: The unit test resource file ({path}) was not found.");
            }

            return FileUtilities.GetReadStream(path);
        }
    }
}