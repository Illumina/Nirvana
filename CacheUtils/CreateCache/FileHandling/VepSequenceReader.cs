using System;
using System.IO;
using CacheUtils.DataDumperImport.FileHandling;
using VariantAnnotation.FileHandling;
using ErrorHandling.Exceptions;

namespace CacheUtils.CreateCache.FileHandling
{
    public sealed class VepSequenceReader : IVepReader<string>, IDisposable
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
        public VepSequenceReader(string filePath, string description, GlobalImportCommon.FileType fileType)
        {
            // sanity check
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"The specified exon file ({filePath}) does not exist.");
            }

            // open the vcf file and parse the header
            _reader = GZipUtilities.GetAppropriateStreamReader(filePath);
            VepReaderCommon.GetHeader(description, filePath, fileType, _reader);
        }

        public string Next()
        {
            // get the next line
            string line = _reader.ReadLine();
            if (line == null) return null;

            var cols = line.Split('\t');
            if (cols.Length != 2) throw new GeneralException($"Expected 2 columns but found {cols.Length} when parsing the exon entry.");

            return cols[1];
        }
    }
}
