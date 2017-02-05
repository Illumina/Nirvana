using System.Collections.Generic;
using System.IO;
using ErrorHandling.Exceptions;
using VariantAnnotation.FileHandling.SupplementaryAnnotations;

namespace VariantAnnotation.FileHandling.CustomInterval
{
    public static class CustomIntervalCommon
    {
        #region members

        public const uint GuardInt = 4041327495;

        public const string DataHeader = "NirvanaCustomIntervals";
        public const ushort SchemaVersion = 1;

        #endregion

        public static void CheckDirectoryIntegrity(string ciDir, List<DataSourceVersion> mainDataSourceVersions)
        {
            DataSourceVersion version = null;
            if (string.IsNullOrEmpty(ciDir))
            {
                return;
            }

            foreach (var ciPath in Directory.GetFiles(ciDir, "*.nci"))
            {
                using (var reader = new CustomIntervalReader(ciPath))
                {
                    if (version == null) version = reader.DataVersion;
                    else
                    {
                        var newVersion = reader.DataVersion;
                        if (newVersion != version)
                            throw new UserErrorException($"Found more than one custom interval data version represented in the following directory: {ciDir}");
                    }
                }
            }

            if (version != null) mainDataSourceVersions.Add(version);
        }
    }
}
