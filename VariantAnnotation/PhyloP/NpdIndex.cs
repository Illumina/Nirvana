using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using Genome;
using IO;
using VariantAnnotation.Interface.Providers;
using VariantAnnotation.Providers;

namespace VariantAnnotation.PhyloP
{
    public sealed class NpdIndex
    {
        private readonly Dictionary<ushort, (long, int)> _chromRanges;
        private readonly ExtendedBinaryWriter _writer;
        public readonly IDataSourceVersion Version;
        public readonly GenomeAssembly Assembly;
        public readonly int SchemaVersion;
        private readonly string _jsonKey;
        public readonly ImmutableDictionary<double, byte> ScoreMap;

        public const int MaxChromLength = 250_000_000;

        public NpdIndex(Stream stream, GenomeAssembly assembly, IDataSourceVersion version, string jsonKey, int schemaVersion)
        {
            _writer       = new ExtendedBinaryWriter(stream);
            Assembly      = assembly;
            Version       = version;
            _jsonKey       = jsonKey;
            SchemaVersion = schemaVersion;

            _chromRanges = new Dictionary<ushort, (long, int)>(32);

        }

        public void Add(ushort chromIndex, long location, int byteCount)
        {
            _chromRanges.Add(chromIndex, (location, byteCount));
        }

        public (long location, int numBytes) GetFileRange(ushort chromIndex)
        {
            return _chromRanges.TryGetValue(chromIndex, out var fileRange) ? fileRange: (-1, -1);
        }

        public void Write(Dictionary<double, byte> scoreMap)
        {
            _writer.Write((byte)Assembly);
            Version.Write(_writer);
            _writer.WriteOptAscii(_jsonKey);
            _writer.WriteOpt(SchemaVersion);

            _writer.WriteOpt(_chromRanges.Count);

            foreach ((ushort chromIndex, (long location, int byteCount)) in _chromRanges)
            {
                _writer.WriteOpt(chromIndex);
                _writer.WriteOpt(location);
                _writer.WriteOpt(byteCount);
            }

            _writer.WriteOpt(scoreMap.Count);
            foreach ((double score, byte code) in scoreMap)
            {
                _writer.Write(score);
                _writer.Write(code);
            }
        }

        public NpdIndex(ExtendedBinaryReader reader)
        {
            Assembly = (GenomeAssembly)reader.ReadByte();
            Version = DataSourceVersion.Read(reader);
            _jsonKey = reader.ReadAsciiString();
            SchemaVersion = reader.ReadOptInt32();

            var chromCount = reader.ReadOptInt32();

            _chromRanges = new Dictionary<ushort, (long, int)>(chromCount);

            for (int i = 0; i < chromCount; i++)
            {
                var chromIndex = reader.ReadOptUInt16();
                var location   = reader.ReadOptInt64();
                var numBytes   = reader.ReadOptInt32();

                _chromRanges.Add(chromIndex, (location, numBytes));
            }

            var scoreCount = reader.ReadOptInt32();
            var scoreMap = new Dictionary<double, byte>(scoreCount);
            for (int i = 0; i < scoreCount; i++)
            {
                var score = reader.ReadDouble();
                var code = reader.ReadByte();
                scoreMap.Add(score, code);
            }

            ScoreMap = scoreMap.ToImmutableDictionary();

        }
    }
}