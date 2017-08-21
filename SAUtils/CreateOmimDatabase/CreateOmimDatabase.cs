using System;
using System.IO;
using System.Linq;
using SAUtils.InputFileParsers;
using SAUtils.InputFileParsers.Omim;
using VariantAnnotation.GeneAnnotation;
using VariantAnnotation.Providers;

namespace SAUtils.CreateOmimDatabase
{
    public class CreateOmimDatabase
    {
        #region member

        private readonly OmimReader _omimReader;
        private readonly DataSourceVersion _dataVersion;
        private readonly string _outputFile;

        #endregion

        public CreateOmimDatabase(string omimFile, string outputDirectory)
	    {
            if (omimFile == null) return;

	        _outputFile = Path.Combine(outputDirectory, OmimDatabaseCommon.OmimDatabaseFileName);

            _omimReader = new OmimReader(new FileInfo(omimFile));

            _dataVersion = AddSourceVersion(omimFile);
        }

        private static DataSourceVersion AddSourceVersion(string dataFileName)
        {
            var versionFileName = dataFileName + ".version";

            if (!File.Exists(versionFileName))
            {
                throw new FileNotFoundException(versionFileName);
            }

            var versionReader = new DataSourceVersionReader(versionFileName);
            var version = versionReader.GetVersion();
            Console.WriteLine(version.ToString());
            return version;
        }

        public void Create()
        {
            var writer = new OmimDatabaseWriter(_outputFile, _dataVersion);
           
                var mimEntries = _omimReader.ToList();
                writer.WriteOmims(mimEntries);
                
            writer.Dispose();
            
        }
    }
}

   
