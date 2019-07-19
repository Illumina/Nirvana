using System.Collections.Generic;
using System.IO;
using Genome;
using IO;
using VariantAnnotation.Interface.Providers;
using VariantAnnotation.Providers;

namespace VariantAnnotation.NSA
{
    public sealed class ChunkedIndex
    {
        private readonly Dictionary<ushort, List<Chunk>> _chromChunks;
        private ushort _chromIndex = ushort.MaxValue;
        private readonly ExtendedBinaryWriter _writer;

        public readonly GenomeAssembly Assembly;
        public readonly IDataSourceVersion Version;
        public readonly string JsonKey;
        public readonly int SchemaVersion;
        public readonly bool IsArray;
        public readonly bool MatchByAllele;
        public readonly bool IsPositional;


        public ChunkedIndex(ExtendedBinaryWriter indexWriter, GenomeAssembly assembly, DataSourceVersion version, string jsonKey, bool matchByAllele, bool isArray, int schemaVersion, bool isPositional)
        {
            _writer       = indexWriter;
            MatchByAllele = matchByAllele;
            JsonKey       = jsonKey;
            Version       = version;
            Assembly      = assembly;
            IsArray       = isArray;
            IsPositional  = isPositional;

            indexWriter.Write((byte)assembly);
            version.Write(indexWriter);
            indexWriter.WriteOptAscii(jsonKey);
            indexWriter.Write(matchByAllele);
            indexWriter.Write(isArray);
            indexWriter.WriteOpt(schemaVersion);
            indexWriter.Write(isPositional);

            _chromChunks = new Dictionary<ushort, List<Chunk>>();
        }

        public void Add(ushort chromIndex, int start, int end, long filePosition, int dataLength)
        {
            _chromIndex = chromIndex;
            
            if (! _chromChunks.ContainsKey(_chromIndex))
            {
                _chromChunks[_chromIndex] = new List<Chunk>();
            }

            var chunk = new Chunk(start, end, filePosition, dataLength);
            _chromChunks[_chromIndex].Add(chunk);
        }

        
        public void Write()
        {
            _writer.WriteOpt(_chromChunks.Count);

            foreach ((ushort index, List<Chunk> chunks) in _chromChunks)
            {
                _writer.WriteOpt(index);
                _writer.WriteOpt(chunks.Count);
                foreach (Chunk chunk in chunks)
                {
                    chunk.Write(_writer);
                }
            }
        }

        public ChunkedIndex(Stream stream)
        {
            //reading the index in one shot
            var buffer = new byte[1048576];
            var indexLength= stream.Read(buffer, 0, 1048576);
            using (var memStream = new MemoryStream(buffer, 0, indexLength))
            using (var memReader = new ExtendedBinaryReader(memStream))
            {
                Assembly      = (GenomeAssembly)memReader.ReadByte();
                Version       = DataSourceVersion.Read(memReader);
                JsonKey       = memReader.ReadAsciiString();
                MatchByAllele = memReader.ReadBoolean();
                IsArray       = memReader.ReadBoolean();
                SchemaVersion = memReader.ReadOptInt32();
                IsPositional  = memReader.ReadBoolean();

                var chromCount = memReader.ReadOptInt32();
                _chromChunks = new Dictionary<ushort, List<Chunk>>(chromCount);
                for (var i = 0; i < chromCount; i++)
                {
                    var chromIndex = memReader.ReadOptUInt16();
                    var chunkCount = memReader.ReadOptInt32();
                    _chromChunks[chromIndex] = new List<Chunk>(chunkCount);
                    for (var j = 0; j < chunkCount; j++)
                        _chromChunks[chromIndex].Add(new Chunk(memReader));
                }
            }
        }

        public long GetFileLocation(ushort chromIndex, int start)
        {
            if (_chromChunks == null || !_chromChunks.TryGetValue(chromIndex, out var chunks)) return -1;
            var index = BinarySearch(chunks, start);

            if (index < 0) return -1;
            return chunks[index].FilePosition;
        }
        public (long startFilePosition, int chunkCount) GetFileRange(ushort chromIndex, int start, int end)
        {
            //create a static empty entry.
            if (_chromChunks == null || !_chromChunks.TryGetValue(chromIndex, out var chunks)) return (-1, 0);

            long startFilePosition = -1;
            long endFilePosition = -1;

            int startChunkIndex = BinarySearch(chunks, start);
            int endChunkIndex = BinarySearch(chunks, end);

            if (startChunkIndex < 0) startChunkIndex = ~startChunkIndex;
            if (startChunkIndex == chunks.Count) return (-1, 0); //start lands after the last chunk=> nothing to return
            if (startChunkIndex < chunks.Count)
                startFilePosition = chunks[startChunkIndex].FilePosition;

            if (endChunkIndex < 0) endChunkIndex = ~endChunkIndex - 1; //if end lands on a gap, return the the chunk to the left of end
            if (endChunkIndex < 0) return (-1, 0); //end lands before the first chunk => nothing to return
            if (endChunkIndex < chunks.Count)
                endFilePosition = chunks[endChunkIndex].FilePosition + chunks[endChunkIndex].Length;

            if (endFilePosition < startFilePosition) return (-1, 0); //both begin and end landed on the same gap.

            return (startFilePosition, endChunkIndex - startChunkIndex + 1);
        }

        private static int BinarySearch(List<Chunk> chunks, int position)
        {
            var begin = 0;
            int end = chunks.Count - 1;

            while (begin <= end)
            {
                int index = begin + (end - begin >> 1);

                int ret = chunks[index].CompareTo(position);
                if (ret == 0) return index;
                if (ret < 0) begin = index + 1;
                else end = index - 1;
            }

            return ~begin;
        }

        
    }

    
}