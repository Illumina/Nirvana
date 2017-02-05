using System.IO;
using CacheUtils.DataDumperImport.FileHandling;
using VariantAnnotation.DataStructures;
using VariantAnnotation.Interface;
using ErrorHandling.Exceptions;

namespace CacheUtils.CreateCache.FileHandling
{
    public static class VepReaderCommon
    {
        /// <summary>
        /// returns the file header
        /// </summary>
        public static GlobalImportHeader GetHeader(string description, string filePath,
            GlobalImportCommon.FileType expectedFileType, StreamReader reader)
        {
            string line = reader.ReadLine();

            if (!IsValidFile(line, expectedFileType))
            {
                throw new GeneralException($"The {description} file ({filePath}) has an invalid header.");
            }

            line = reader.ReadLine();

            if (line == null)
            {
                throw new GeneralException($"The {description} file ({filePath}) has an invalid header.");
            }

            var cols = line.Split('\t');
            if (cols.Length != GlobalImportCommon.NumHeaderColumns)
            {
                throw new GeneralException($"Expected {GlobalImportCommon.NumHeaderColumns} columns in the header but found {cols.Length}");
            }

            var vepVersion       = ushort.Parse(cols[0]);
            var vepReleaseTicks  = long.Parse(cols[1]);
            var transcriptSource = (TranscriptDataSource)byte.Parse(cols[2]);
            var genomeAssembly   = (GenomeAssembly)byte.Parse(cols[3]);

            return new GlobalImportHeader(vepVersion, vepReleaseTicks, transcriptSource, genomeAssembly);
        }

        /// <summary>
        /// returns true if this is a valid VEP reader file (text)
        /// </summary>
        private static bool IsValidFile(string line, GlobalImportCommon.FileType fileType)
        {
            string expectedString = $"{GlobalImportCommon.Header}\t{(byte)fileType}";
            return line == expectedString;
        }
    }
}
