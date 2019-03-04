using System;
using System.IO;
using IO;
using OptimizedCore;
using VariantAnnotation.Providers;

namespace SAUtils.InputFileParsers
{
    /// <inheritdoc />
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
        public DataSourceVersionReader(Stream fileStream)
        {
            _reader = new StreamReader(fileStream);
        }

        public static DataSourceVersion GetSourceVersion(string versionFileName)
        {
            if (!versionFileName.EndsWith(".version")) versionFileName += ".version";
            if (!File.Exists(versionFileName))
            {
                throw new FileNotFoundException(versionFileName);
            }

            var fileStream = FileUtilities.GetReadStream(versionFileName);

            return GetSourceVersion(fileStream);
        }

        private static DataSourceVersion GetSourceVersion(Stream versionFileStream)
        {
            using (var versionReader = new DataSourceVersionReader(versionFileStream))
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
                (string key, string value) = line.OptimizedKeyValue();
                if (key == null || value == null) continue;

                switch (key)
                {
                    case "NAME":
                        name = value;
                        break;
                    case "VERSION":
                        version = value;
                        break;
                    case "DATE":
                        date = value;
                        break;
                    case "DESCRIPTION":
                        description = value;
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