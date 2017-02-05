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

        public static ILookup<string, OmimAnnotation> CreateGeneMapLookup(OmimDatabaseReader omimDatabaseReader)
        {
            var omimLookup = omimDatabaseReader?.Read().ToLookup((omimEntry) => omimEntry.Hgnc, (omimEntry) => omimEntry);
            return omimLookup;
        }

        public static OmimDatabaseReader GetOmimDatabaseReader(string omimDatabaseDir)
        {
            if (string.IsNullOrEmpty(omimDatabaseDir)) return null;
            var omimFile = Path.Combine(omimDatabaseDir, OmimDatabaseFileName);
            return !File.Exists(omimFile) ? null : new OmimDatabaseReader(omimFile);
        }
    }
}