using System;
using System.IO;
using CacheUtils.DataDumperImport.FileHandling;
using VariantAnnotation.DataStructures;
using ErrorHandling.Exceptions;
using VariantAnnotation.FileHandling.Compression;

namespace CacheUtils.CreateCache.FileHandling
{
    public sealed class VepRegulatoryReader : IVepReader<RegulatoryElement>, IDisposable
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
        public VepRegulatoryReader(string filePath)
        {
            // sanity check
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"The specified exon file ({filePath}) does not exist.");
            }

            // open the vcf file and parse the header
            _reader = GZipUtilities.GetAppropriateStreamReader(filePath);
            VepReaderCommon.GetHeader("regulatory element", filePath, GlobalImportCommon.FileType.Regulatory, _reader);
        }

        public RegulatoryElement Next()
        {
            // get the next line
            string line = _reader.ReadLine();
            if (line == null) return null;

            var cols = line.Split('\t');
            if (cols.Length != 5) throw new GeneralException($"Expected 5 columns but found {cols.Length} when parsing the regulatory element.");

            try
            {
                var referenceIndex = ushort.Parse(cols[0]);
                var start          = int.Parse(cols[1]);
                var end            = int.Parse(cols[2]);
                var id             = CompactId.Convert(cols[3]);
                var type           = RegulatoryElementUtilities.GetRegulatoryElementTypeFromString(cols[4]);

                return new RegulatoryElement(referenceIndex, start, end, id, type);
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
