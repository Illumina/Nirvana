using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using Compression.Algorithms;
using Compression.FileHandling;
using ErrorHandling.Exceptions;
using IO;
using VariantAnnotation.Interface.Providers;
using VariantAnnotation.Providers;
using VariantAnnotation.SA;

namespace VariantAnnotation.NSA
{
    public sealed class NgaReader
    {
        public readonly IDataSourceVersion Version;
        public readonly string JsonKey;
        private readonly bool _isArray;

        private readonly Dictionary<string, List<string>> _geneSymbolToJsonStrings;

        private NgaReader(IDataSourceVersion version, string jsonKey, bool isArray, Dictionary<string, List<string>> geneSymbolToJsonStrings)
        {
            Version                  = version;
            JsonKey                  = jsonKey;
            _isArray                 = isArray;
            _geneSymbolToJsonStrings = geneSymbolToJsonStrings;
        }

        public static NgaReader Read(Stream stream)
        {
            (IDataSourceVersion version, string jsonKey, bool isArray) = ReadHeader(stream);

            Dictionary<string, List<string>> geneSymbolToJsonStrings;

            using (var blockStream = new BlockStream(new Zstandard(), stream, CompressionMode.Decompress))
            using (var reader = new ExtendedBinaryReader(blockStream))
            {
                int geneCount = reader.ReadOptInt32();
                geneSymbolToJsonStrings = new Dictionary<string, List<string>>(geneCount);

                for (var i = 0; i < geneCount; i++)
                {
                    string geneSymbol = reader.ReadAsciiString();
                    int numEntries = reader.ReadOptInt32();
                    var entries = new List<string>(numEntries);

                    for (var j = 0; j < numEntries; j++)
                    {
                        entries.Add(reader.ReadString());
                    }

                    geneSymbolToJsonStrings[geneSymbol] = entries;
                }
            }

            return new NgaReader(version, jsonKey, isArray, geneSymbolToJsonStrings);
        }

        private static (IDataSourceVersion Version, string JsonKey, bool IsArray) ReadHeader(Stream stream)
        {
            IDataSourceVersion version;
            string jsonKey;
            bool isArray;

            using (var reader = new ExtendedBinaryReader(stream, Encoding.UTF8, true))
            {
                string identifier    = reader.ReadString();

                if (identifier != SaCommon.NgaIdentifier)
                {
                    throw new InvalidDataException($"Expected the NGA identifier ({SaCommon.NgaIdentifier}), but found another value: ({identifier})");
                }

                version              = DataSourceVersion.Read(reader);
                jsonKey              = reader.ReadString();
                isArray              = reader.ReadBoolean();
                ushort schemaVersion = reader.ReadUInt16();

                if (schemaVersion != SaCommon.SchemaVersion)
                {
                    throw new UserErrorException($"Expected the schema version {SaCommon.SchemaVersion}, but found another value: ({schemaVersion}) for {jsonKey}");
                }

                uint guard = reader.ReadUInt32();

                if (guard != SaCommon.GuardInt)
                {
                    throw new InvalidDataException($"Expected a guard integer ({SaCommon.GuardInt}), but found another value: ({guard})");
                }
            }

            return (version, jsonKey, isArray);
        }

        public string GetAnnotation(string geneName) => _geneSymbolToJsonStrings.TryGetValue(geneName, out List<string> annotations) ? GetJsonString(annotations) : null;

        private string GetJsonString(List<string> annotations)
        {
            if (_isArray) return "[" + string.Join(',', annotations) + "]";
            return annotations[0];
        }
    }
}