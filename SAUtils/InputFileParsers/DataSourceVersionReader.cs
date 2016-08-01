using System;
using System.IO;
using VariantAnnotation.FileHandling.SupplementaryAnnotations;
using VariantAnnotation.Utilities;

namespace SAUtils.InputFileParsers
{
    /// <summary>
    /// reads data version from a file that is expected to be found alongside each supplementary data file
    /// </summary>
    public sealed class DataSourceVersionReader : IDisposable
    {
        #region members

        private readonly StreamReader _reader;

        #endregion

        public void Dispose()
        {
            _reader.Dispose();
        }

        /// <summary>
        /// constructor
        /// </summary>
        public DataSourceVersionReader(string fileName) : this(FileUtilities.GetFileStream(fileName))
        { }

        /// <summary>
        /// constructor
        /// </summary>
        public DataSourceVersionReader(Stream stream)
        {
            _reader = new StreamReader(stream);
        }

        public DataSourceVersion GetVersion()
        {
            // NAME = dbSNP
            // VERSION = 147
            // DATE = 2016 - 04 - 08
            // DESCRIPTION =

            string line, name = null, version = null, date = null, description = null;

            while ((line = _reader.ReadLine()) != null)
            {
                var words = line.Split('=');
                if (words.Length < 2) continue;

                switch (words[0])
                {
                    case "NAME":
                        name = words[1];
                        break;
                    case "VERSION":
                        version = words[1];
                        break;
                    case "DATE":
                        date = words[1];
                        break;
                    case "DESCRIPTION":
                        description = words[1];
                        break;
                }
            }

            return new DataSourceVersion(name, version, DateTime.Parse(date).Ticks, description);
        }
    }
}
