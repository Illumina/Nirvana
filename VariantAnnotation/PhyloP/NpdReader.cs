using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using Compression.Algorithms;
using ErrorHandling.Exceptions;
using Genome;
using IO;
using VariantAnnotation.Interface.Providers;
using VariantAnnotation.SA;

namespace VariantAnnotation.PhyloP
{
    public sealed class NpdReader:IDisposable
    {
        private readonly ExtendedBinaryReader _reader;

        private readonly byte[] _scores;
        private readonly Zstandard _zstd;

        private readonly ImmutableDictionary<byte, double> _scoreMap;

        private readonly NpdIndex _index;
        public GenomeAssembly Assembly { get; }
        public IDataSourceVersion Version { get; }

        private readonly Stream _dbStream;
        private readonly Stream _indexStream;

        public NpdReader(Stream dbStream, Stream indexStream)
        {
            _dbStream = dbStream;
            _indexStream = indexStream;
            _reader = new ExtendedBinaryReader(dbStream);

            _index   = new NpdIndex(new ExtendedBinaryReader(indexStream));
            Assembly = _index.Assembly;
            Version  = _index.Version;

            if (_index.SchemaVersion != SaCommon.SchemaVersion)
                throw new UserErrorException($"SA schema version mismatch. Expected {SaCommon.SchemaVersion}, observed {_index.SchemaVersion}");

            var scoreMap= new Dictionary<byte, double>();
            foreach ((double score, byte code)in _index.ScoreMap)
            {
                scoreMap.Add(code, score);
            }

            _scoreMap = scoreMap.ToImmutableDictionary();
            _zstd = new Zstandard();
            _scores = new byte[NpdIndex.MaxChromLength];
        }

        private IChromosome _chromosome;
        private int _lastPhylopPosition;
        private void PreLoad(IChromosome chrom)
        {
            _chromosome = chrom;
            (long startLocation, int numBytes) = _index.GetFileRange(chrom.Index);
            if (startLocation == -1)
            {
                _lastPhylopPosition = -1;
                return;
            }
            _reader.BaseStream.Position = startLocation;
            var buffer = _reader.ReadBytes(numBytes);

            _lastPhylopPosition = _zstd.Decompress(buffer, buffer.Length, _scores, _scores.Length);
            
        }

        public double? GetAnnotation(IChromosome chromosome, int position)
        {
            if (_chromosome==null || chromosome.Index != _chromosome.Index) PreLoad(chromosome);

            if (position >= _lastPhylopPosition) return null;
            var scoreCode = _scores[position - 1];
            if (scoreCode == 0) return null;
            return _scoreMap[scoreCode];
        }

        public void Dispose()
        {
            _reader?.Dispose();
            _dbStream?.Dispose();
            _indexStream?.Dispose();
        }
    }
}