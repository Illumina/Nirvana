using System;
using System.Collections.Generic;
using System.IO;
using ErrorHandling.Exceptions;
using Genome;
using IO;
using VariantAnnotation.SA;

namespace VariantAnnotation.NSA
{
    public sealed class RefMinorDbReader : IDisposable
    {
        private readonly ExtendedBinaryReader _reader;
        private readonly RefMinorIndex _index;

        private readonly Dictionary<int, string> _annotations;
        private readonly Stream _dbStream, _indexStream;

        public RefMinorDbReader(Stream dbStream, Stream indexStream)
        {
            _dbStream = dbStream;
            _indexStream = indexStream;
            _reader      = new ExtendedBinaryReader(dbStream);
            _index       = new RefMinorIndex(new ExtendedBinaryReader(indexStream));
            _annotations = new Dictionary<int, string>();

            if (_index.SchemaVersion != SaCommon.SchemaVersion)
                throw new UserErrorException($"SA schema version mismatch. Expected {SaCommon.SchemaVersion}, observed {_index.SchemaVersion}");            
        }

        private IChromosome _chromosome;

        private void PreLoad(IChromosome chrom)
        {
            _annotations.Clear();
            _chromosome = chrom;

            (long startLocation, int numBytes, int refMinorCount) = _index.GetFileRange(chrom.Index);
            if (startLocation == -1) return;
            _reader.BaseStream.Position = startLocation;
            var buffer = _reader.ReadBytes(numBytes);

            using (var memStream = new MemoryStream(buffer))
            using(var reader = new ExtendedBinaryReader(memStream))
            {
                for (var i = 0; i < refMinorCount; i++)
                {
                    var position = reader.ReadOptInt32();
                    var globalMajor = reader.ReadAsciiString();

                    _annotations[position] = globalMajor;
                }
            }

        }

        public string GetGlobalMajorAllele(IChromosome chromosome, int position)
        {
            if (_chromosome == null || chromosome.Index != _chromosome.Index)
                PreLoad(chromosome);

            return _annotations.TryGetValue(position, out string globalMajor) ? globalMajor : null;

            
        }

        public void Dispose()
        {
            _dbStream?.Dispose();
            _indexStream?.Dispose();
            _reader?.Dispose();
        }
    }
}