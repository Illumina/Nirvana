using System.Collections.Generic;
using Genome;
using IO;
using VariantAnnotation.Interface.Providers;
using VariantAnnotation.Providers;

namespace VariantAnnotation.NSA
{
    public sealed class RefMinorIndex
    {
        private readonly ExtendedBinaryWriter _writer;
        private readonly Dictionary<ushort, (long location, int numBytes, int count)> _chromBlocks;
        private readonly IDataSourceVersion _version;
        private readonly GenomeAssembly _assembly;
        public readonly int SchemaVersion;

        public RefMinorIndex(ExtendedBinaryWriter writer, GenomeAssembly assembly, IDataSourceVersion version, int schemaVersion)
        {
            _writer      = writer;
            _chromBlocks = new Dictionary<ushort, (long location, int numBytes, int count)>();

            _assembly     = assembly;
            _version      = version;
            SchemaVersion = schemaVersion;
        }

        private ushort _chromIndex  = ushort.MaxValue;
        private long _chromLocation =-1;
        private int _blockLength    =-1;
        private int _count;
        
        public void Add(ushort chromIndex, long location)
        {
            if (_chromIndex != chromIndex)
            {
                _blockLength = (int) (location - _chromLocation);

                //if you try to add a chrom twice (i.e. the positions are not sorted by chrom), this will throw an exception
                _chromBlocks.Add(_chromIndex, (_chromLocation, _blockLength, _count));

                _chromIndex = chromIndex;
                _chromLocation = location;
                _count = 1;
            }
            else _count++;

        }

        public (long location, int numBytes, int count) GetFileRange(ushort chromIndex)
        {
            return _chromBlocks.TryGetValue(chromIndex, out var locationSize) ? locationSize : (-1, -1, 0);
        }

        public void Write(long finalLocation)
        {
            _blockLength = (int)(finalLocation - _chromLocation);

            //adding the last chrom to index
            _chromBlocks.Add(_chromIndex, (_chromLocation, _blockLength, _count));

            _writer.Write((byte)_assembly);
            _version.Write(_writer);
            _writer.WriteOpt(SchemaVersion);

            _writer.WriteOpt(_chromBlocks.Count);

            foreach ((ushort chromIndex, (long location, int numBytes, int count)) in _chromBlocks)
            {
                _writer.WriteOpt(chromIndex);
                _writer.WriteOpt(location);
                _writer.WriteOpt(numBytes); 
                _writer.WriteOpt(count);
            }
        }

        public RefMinorIndex(ExtendedBinaryReader reader)
        {
            _assembly      = (GenomeAssembly) reader.ReadByte();
            _version       = DataSourceVersion.Read(reader);
            SchemaVersion = reader.ReadOptInt32();

            var chromCount = reader.ReadOptInt32();

            _chromBlocks= new Dictionary<ushort, (long location, int numBytes, int count)>(chromCount);

            for (int i = 0; i < chromCount; i++)
            {
                var chromIndex = reader.ReadOptUInt16();
                var location   = reader.ReadOptInt64();
                var numBytes   = reader.ReadOptInt32();
                int count      = reader.ReadOptInt32();

                _chromBlocks.Add(chromIndex, (location, numBytes, count));
            }
        }
        
    }
}