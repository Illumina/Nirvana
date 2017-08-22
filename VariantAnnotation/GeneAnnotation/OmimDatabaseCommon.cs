using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace VariantAnnotation.GeneAnnotation
{
    public class OmimDatabaseCommon
    {
        #region members

        public const uint GuardInt = 4041327495;

        public const string DataHeader = "NirvanaOmimDatabase";
        public const ushort SchemaVersion = 1;
        public const string OmimDatabaseFileName = "genePhenotypeMap.mim";
        #endregion

       

        public static OmimDatabaseReader GetOmimDatabaseReader(IEnumerable<string> omimDatabaseDirs)
        {
            if (omimDatabaseDirs == null) return null;

            var omimDirs = omimDatabaseDirs.ToList();
            if (!omimDirs.Any()) return null;

            foreach (var omimDatabaseDir in omimDirs)
            {
                var omimFile = Path.Combine(omimDatabaseDir, OmimDatabaseFileName);
                if (File.Exists(omimFile)) return new OmimDatabaseReader(omimFile);
            }

            return null;
        }
    }
}