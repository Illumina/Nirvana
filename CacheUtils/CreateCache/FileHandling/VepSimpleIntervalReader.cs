using System;
using System.IO;
using CacheUtils.DataDumperImport.FileHandling;
using VariantAnnotation.DataStructures;
using VariantAnnotation.FileHandling;
using ErrorHandling.Exceptions;

namespace CacheUtils.CreateCache.FileHandling
{
    public sealed class VepSimpleIntervalReader : IVepReader<SimpleInterval>, IDisposable
    {
        #region members

        private readonly StreamReader _reader;

        #endregion

        #region IDisposable

        public void Dispose()
        {
            Dispose(true);
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                _reader.Dispose();
            }
        }

        #endregion

        /// <summary>
        /// constructor
        /// </summary>
        public VepSimpleIntervalReader(string filePath, string description, GlobalImportCommon.FileType fileType)
        {
            // sanity check
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"The specified intron file ({filePath}) does not exist.");
            }

            // open the vcf file and parse the header
            _reader = GZipUtilities.GetAppropriateStreamReader(filePath);
            VepReaderCommon.GetHeader(description, filePath, fileType, _reader);
        }

        /// <summary>
        /// retrieves the next variantFeature. Returns false if there are no more variants available
        /// </summary>
        public SimpleInterval Next()
        {
            // get the next line
            string line = _reader.ReadLine();
            if (line == null) return null;

            var cols = line.Split('\t');
            if (cols.Length != 3) throw new GeneralException($"Expected 3 columns but found {cols.Length} when parsing the simple interval.");

            try
            {
                var start = int.Parse(cols[1]);
                var end   = int.Parse(cols[2]);

                return new SimpleInterval(start, end);
            }
            catch (Exception)
            {
                Console.WriteLine("Offending line: {0}", line);
                for (int i = 0; i < cols.Length; i++) Console.WriteLine("- col {0}: [{1}]", i, cols[i]);
                throw;
            }
        }
    }
}