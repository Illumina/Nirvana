using System;
using System.IO;
using VariantAnnotation.Providers;
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
        private DataSourceVersionReader(string fileName)
        {
            _reader = new StreamReader(FileUtilities.GetReadStream(fileName));
        }

        /// <summary>
        /// constructor
        /// </summary>
        public DataSourceVersionReader(Stream stream)
        {
            _reader = new StreamReader(stream);
        }

        public static DataSourceVersion GetSourceVersion(string versionFileName)
        {
            if (!versionFileName.EndsWith(".version")) versionFileName += ".version";
            if (!File.Exists(versionFileName))
            {
                throw new FileNotFoundException(versionFileName);
            }

            using (var versionReader = new DataSourceVersionReader(versionFileName))
            {
                var version = versionReader.GetVersion();
                return version;
            }
        }

        public DataSourceVersion GetVersion()
        {
            // NAME = dbSNP
            // VERSION = 147
            // DATE = 2016-04-08
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

            if (date == null)
            {
                date = DateTime.Now.ToString("yyyy-MM-dd");
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"WARNING: Date was missing from the data source. Using {date} instead.");
                Console.ResetColor();
            }

            return new DataSourceVersion(name, version, DateTime.Parse(date).Ticks, description);
        }
    }
}