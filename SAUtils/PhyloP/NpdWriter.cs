using System;
using System.Collections.Generic;
using System.IO;
using CommandLine.Utilities;
using Compression.Algorithms;
using Genome;
using IO;
using SAUtils.DataStructures;
using VariantAnnotation.PhyloP;
using VariantAnnotation.Providers;

namespace SAUtils.PhyloP
{
    public sealed class NpdWriter:IDisposable
    {
        private readonly ExtendedBinaryWriter _writer;

        private readonly byte[] _scores;
        private readonly byte[] _compressedScores;
        private readonly MemoryStream _memStream;
        private readonly ExtendedBinaryWriter _memWriter;
        private readonly Zstandard _zstd;

        

        private readonly Dictionary<double, byte> _scoreMap;
        private byte _nextScoreCode = 1; //0 is reserved to indicate no score

        private readonly NpdIndex _index;

        public NpdWriter(Stream dbStream, Stream indexStream, DataSourceVersion version, GenomeAssembly assembly, string jsonKey, int schemaVersion)
        {
            _writer = new ExtendedBinaryWriter( dbStream);
            
            _index    = new NpdIndex(indexStream, assembly, version, jsonKey, schemaVersion);
            _scoreMap = new Dictionary<double, byte>(byte.MaxValue);

            _scores = new byte[NpdIndex.MaxChromLength];
            _memStream = new MemoryStream(_scores);
            _memWriter = new ExtendedBinaryWriter(_memStream);
            _zstd = new Zstandard();

            _compressedScores = new byte[_zstd.GetCompressedBufferBounds(_scores.Length)];

        }

        private ushort _chromIndex = ushort.MaxValue;
        private string _chromName = "";
        
        public void Write(IEnumerable<PhylopItem> items)
        {
            var benchmark = new Benchmark();
            int lastPosition = 0;
            foreach (PhylopItem item in items)
            {
                if (item.Chromosome.Index != _chromIndex)
                {
                    //flush out old chrom 
                    if (_chromIndex != ushort.MaxValue)
                    {
                        WriteCompressed(lastPosition);
                        Console.WriteLine($"Chromosome {_chromName} completed in {Benchmark.ToHumanReadable(benchmark.GetElapsedTime())}");
                        benchmark.Reset();
                    }

                    _chromIndex = item.Chromosome.Index;
                    _chromName = item.Chromosome.EnsemblName;
                }

                if (! _scoreMap.TryGetValue(item.Score, out byte _))
                {
                    _scoreMap.Add(item.Score, _nextScoreCode++);
                    if (_nextScoreCode==byte.MaxValue)
                        throw new ArgumentOutOfRangeException($"No of distinct scores exceeded expected value of {_nextScoreCode}!!");
                }

                _memStream.Position = item.Position - 1;

                _memWriter.Write(_scoreMap[item.Score]);
                
                lastPosition = item.Position;
                
            }

            //closing the last chromosome
            WriteCompressed(lastPosition);
            Console.WriteLine($"Chromosome {_chromName} completed in {Benchmark.ToHumanReadable(benchmark.GetElapsedTime())}");
            benchmark.Reset();

            Console.WriteLine($"\nNumber of distinct scores oberved:{_scoreMap.Count}");


            _index.Write(_scoreMap);
        }

        private void WriteCompressed(int lastPosition)
        {
            var startLocation = _writer.BaseStream.Position;

            int compressSize = _zstd.Compress(_scores, lastPosition, _compressedScores, _compressedScores.Length);
            _writer.Write(_compressedScores, 0, compressSize);
            _index.Add(_chromIndex, startLocation, compressSize);

            Array.Clear(_scores, 0, _scores.Length);
            _memStream.Position = 0;//reset the stream

        }

        public void Dispose()
        {
            _writer?.Dispose();
            _memStream?.Dispose();
            _memWriter?.Dispose();
        }
    }
}