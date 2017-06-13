using System.Collections.Generic;
using System.IO;
using System.Linq;
using VariantAnnotation.DataStructures.SupplementaryAnnotations;

namespace VariantAnnotation.FileHandling.Omim
{
    public static class OmimDatabaseCommon
    {
        #region members

        public const uint GuardInt = 4041327495;

        public const string DataHeader = "NirvanaOmimDatabase";
        public const ushort SchemaVersion = 1;
        public const string OmimDatabaseFileName = "genePhenotypeMap.mim";
        #endregion

        public static Dictionary<string, List<OmimAnnotation>> CreateGeneMapDict(OmimDatabaseReader omimDatabaseReader)
        {

            if (omimDatabaseReader == null) return null;
            var omimGeneDict = new Dictionary<string,List<OmimAnnotation>>(); 

            foreach (var omimAnnotation in omimDatabaseReader.Read())
            {
                if (!omimGeneDict.ContainsKey(omimAnnotation.Hgnc))
                {
                    omimGeneDict[omimAnnotation.Hgnc] = new List<OmimAnnotation>();
                }
                omimGeneDict[omimAnnotation.Hgnc].Add(omimAnnotation);
            }

            return omimGeneDict;
        }

        public static OmimDatabaseReader GetOmimDatabaseReader(IEnumerable<string> omimDatabaseDirs)
        {
            if (omimDatabaseDirs == null) return null;

            var omimDirs = omimDatabaseDirs.ToList();
            if (!omimDirs.Any()) return null;

	        foreach (var omimDatabaseDir in omimDirs)
	        {
				var omimFile = Path.Combine(omimDatabaseDir, OmimDatabaseFileName);
				if(File.Exists(omimFile)) return new OmimDatabaseReader(omimFile);
			}

	        return null;
        }
    }
}