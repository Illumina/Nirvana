using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using OptimizedCore;

namespace SAUtils.MitoMap
{
    internal static class MitoMapDatabaseUtilities
    {
        private const string ReferenceQueryPrefix = "COPY reference (";
        public static MitoMapInputDb Create(string mitoMapDatabase)
        {
            var internalReferenceIdToPubmedId = new Dictionary<string, string>();
            using (var stream = new FileStream(mitoMapDatabase, FileMode.Open))
            using(var gzStream = new GZipStream(stream, CompressionMode.Decompress))
            using (var reader = new StreamReader(gzStream))
            {
                string line;
                MitoMapTable currentTable = 0;
                while ((line = reader.ReadLine()) != null)
                {
                    if (line == "\\.")
                    {
                        currentTable = 0;
                        continue;
                    }

                    switch (currentTable)
                    {
                        case 0:
                            currentTable = TryGetTable(line);
                            continue;
                        case MitoMapTable.Reference:
                            ProcessReferenceInfo(line, internalReferenceIdToPubmedId);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
            }

            return new MitoMapInputDb(internalReferenceIdToPubmedId);
        }

        private static void ProcessReferenceInfo(string line, Dictionary<string, string> internalReferenceIdToPubmedId)
        {
            var fields = line.OptimizedSplit('\t');
            if (fields.Length != 14) throw new InvalidDataException($"Invalid reference table record: {line}");
            internalReferenceIdToPubmedId[fields[0]] = fields[13];
        }

        private static MitoMapTable TryGetTable(string line)
        {
            return line.StartsWith(ReferenceQueryPrefix) ? MitoMapTable.Reference : 0;
        }
    }

    public enum MitoMapTable
    {
        Reference = 1
    }
}