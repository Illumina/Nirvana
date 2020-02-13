using System.Collections.Generic;
using System.IO;
using Genome;
using IO;
using VariantAnnotation.Interface.Providers;
using VariantAnnotation.Providers;

namespace VariantAnnotation.NSA
{
    public sealed class NsaIndex
    {
        private readonly Dictionary<ushort, List<NsaIndexBlock>> _chromBlocks;
        private ushort _chromIndex = ushort.MaxValue;
        private readonly ExtendedBinaryWriter _writer;

        public readonly GenomeAssembly Assembly;
        public readonly IDataSourceVersion Version;
        public readonly string JsonKey;
        public readonly int SchemaVersion;
        public readonly bool IsArray;
        public readonly bool MatchByAllele;
        public readonly bool IsPositional;
        public IEnumerable<ushort> ChromosomeIndices => _chromBlocks.Keys;

        public Dictionary<ushort, List<NsaIndexBlock>> GetBlocks() => _chromBlocks;
        public List<NsaIndexBlock> GetChromBlocks(ushort chromIndex) => _chromBlocks[chromIndex];

        public NsaIndex(ExtendedBinaryWriter indexWriter, GenomeAssembly assembly, IDataSourceVersion version, string jsonKey, bool matchByAllele, bool isArray, int schemaVersion, bool isPositional)
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

            _chromBlocks = new Dictionary<ushort, List<NsaIndexBlock>>();
        }

        public void Add(ushort chromIndex, int start, int end, long filePosition, int dataLength)
        {
            _chromIndex = chromIndex;
            
            if (! _chromBlocks.ContainsKey(_chromIndex))
            {
                _chromBlocks[_chromIndex] = new List<NsaIndexBlock>();
            }

            var indexBlock = new NsaIndexBlock(start, end, filePosition, dataLength);
            _chromBlocks[_chromIndex].Add(indexBlock);
        }

        
        public void Write()
        {
            _writer.WriteOpt(_chromBlocks.Count);

            foreach ((ushort index, List<NsaIndexBlock> chunks) in _chromBlocks)
            {
                _writer.WriteOpt(index);
                _writer.WriteOpt(chunks.Count);
                foreach (NsaIndexBlock chunk in chunks)
                {
                    chunk.Write(_writer);
                }
            }
        }

        public void Write(Dictionary<ushort, List<NsaIndexBlock>>  chromBlocks)
        {
            _writer.WriteOpt(chromBlocks.Count);

            foreach ((ushort index, List<NsaIndexBlock> chunks) in chromBlocks)
            {
                _writer.WriteOpt(index);
                _writer.WriteOpt(chunks.Count);
                foreach (NsaIndexBlock chunk in chunks)
                {
                    chunk.Write(_writer);
                }
            }
        }


        public NsaIndex(Stream stream)
        {
            using (var memStream = new MemoryStream())
            using (var memReader = new ExtendedBinaryReader(memStream))
            {
                stream.CopyTo(memStream);//reading all bytes in stream to memStream
                memStream.Position = 0;

                Assembly      = (GenomeAssembly)memReader.ReadByte();
                Version       = DataSourceVersion.Read(memReader);
                JsonKey       = memReader.ReadAsciiString();
                MatchByAllele = memReader.ReadBoolean();
                IsArray       = memReader.ReadBoolean();
                SchemaVersion = memReader.ReadOptInt32();
                IsPositional  = memReader.ReadBoolean();

                var chromCount = memReader.ReadOptInt32();
                _chromBlocks = new Dictionary<ushort, List<NsaIndexBlock>>(chromCount);
                for (var i = 0; i < chromCount; i++)
                {
                    var chromIndex = memReader.ReadOptUInt16();
                    var chunkCount = memReader.ReadOptInt32();
                    _chromBlocks[chromIndex] = new List<NsaIndexBlock>(chunkCount);
                    for (var j = 0; j < chunkCount; j++)
                        _chromBlocks[chromIndex].Add(new NsaIndexBlock(memReader));
                }
            }
        }

        public long GetFileLocation(ushort chromIndex, int start)
        {
            if (_chromBlocks == null || !_chromBlocks.TryGetValue(chromIndex, out var chunks)) return -1;
            var index = BinarySearch(chunks, start);

            if (index < 0) return -1;
            return chunks[index].FilePosition;
        }

        public (long startFilePosition, int chunkCount) GetFileRange(ushort chromIndex, int start, int end)
        {
            //create a static empty entry.
            if (_chromBlocks == null || !_chromBlocks.TryGetValue(chromIndex, out var chunks)) return (-1, 0);

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

        private static int BinarySearch(List<NsaIndexBlock> chunks, int position)
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