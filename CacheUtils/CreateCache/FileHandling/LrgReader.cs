using System.Collections.Generic;
using System.IO;
using VariantAnnotation.Utilities;
using ErrorHandling.Exceptions;

namespace CacheUtils.CreateCache.FileHandling
{
    public static class LrgReader
    {
        /// <summary>
        /// loads the data in the LRG data file
        /// </summary>
        public static HashSet<string> GetTranscriptIds(string lrgPath)
        {
            var lrgTranscriptIds = new HashSet<string>();

            using (var reader = new StreamReader(FileUtilities.GetReadStream(lrgPath)))
            {
                reader.ReadLine();

                while (true)
                {
                    var line = reader.ReadLine();
                    if (string.IsNullOrEmpty(line)) break;

                    var cols = line.Split('\t');
                    if (cols.Length != 10)
                    {
                        throw new GeneralException($"Expected 10 columns, but found {cols.Length}: [{line}]");
                    }

                    var tuple = FormatUtilities.SplitVersion(cols[5]);
                    lrgTranscriptIds.Add(tuple.Item1);
                }
            }

            return lrgTranscriptIds;
        }
    }
}
