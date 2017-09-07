using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using CommandLine.Utilities;
using Compression.FileHandling;
using ErrorHandling.Exceptions;
using Jasix.DataStructures;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using VariantAnnotation.Utilities;


namespace Jasix
{
    public sealed class IndexCreator : IDisposable
    {
        #region members
        private readonly BgzipTextReader _reader;
        private readonly Stream _writeStream;
        private const string SectionToIndex = "positions";
        private readonly HashSet<string> _processedChromosome;

        //private readonly PerformanceMetrics _performanceMetrics;
        
        private readonly Benchmark _chromBenchmark;
        private readonly Benchmark _benchmark;

        #endregion

        #region IDisposable
        bool _disposed;

        /// <summary>
        /// public implementation of Dispose pattern callable by consumers. 
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }


        /// <summary>
        /// protected implementation of Dispose pattern. 
        /// </summary>
        private void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                // Free any other managed objects here.
                _reader.Dispose();
                _writeStream.Flush();
                _writeStream.Dispose();
            }

            // Free any unmanaged objects here.
            //
            _disposed = true;
            // Free any other managed objects here.

        }
        #endregion

        public IndexCreator(BlockGZipStream readStream, Stream writeStream)
        {
            _reader              = new BgzipTextReader(readStream);
            _writeStream         = writeStream;
            _processedChromosome = new HashSet<string>();
            _chromBenchmark      = new Benchmark();
            _benchmark           = new Benchmark();
        }

        public IndexCreator(string fileName)
            : this(
                new BlockGZipStream(FileUtilities.GetReadStream(fileName), CompressionMode.Decompress),
                FileUtilities.GetCreateStream(fileName + JasixCommons.FileExt))
        {}

        public void CreateIndex()
        {
            var searchTag = $"\"{SectionToIndex}\":[";
            var headerTag = "{\"header\":";
            var index = new JasixIndex();
            string line;

            //skipping lines before the sectionToIndex arrives
            while ((line = _reader.ReadLine()) != null)
            {
                if (line.StartsWith(headerTag))
                    index.HeaderLine = ExtractHeader(line);
                if (line.EndsWith(searchTag)) break;
            }

            // we need the location before accessing the line
            var fileLoc = _reader.Position;

            string previousChr = "";
            int previousPos = 0;
            while ((line = _reader.ReadLine()) != null)
            {
                if (line.StartsWith("]")) break;
                line = line.TrimEnd(',');
                var chrPos = GetChromPosition(line);

                CheckFileSorted(chrPos, previousChr, previousPos);

                index.Add(chrPos.Item1, chrPos.Item2, chrPos.Item3, fileLoc);
                fileLoc = _reader.Position;
                previousChr = chrPos.Item1;
                previousPos = chrPos.Item2;

            }

            //_performanceMetrics.StopReference();

            index.Write(_writeStream);

            Console.WriteLine();

            var peakMemoryUsageBytes = MemoryUtilities.GetPeakMemoryUsage();
            var wallTimeSpan = _benchmark.GetElapsedTime();
            Console.WriteLine();
            if (peakMemoryUsageBytes > 0) Console.WriteLine("Peak memory usage: {0}", MemoryUtilities.ToHumanReadable(peakMemoryUsageBytes));
            Console.WriteLine("Time: {0}", Benchmark.ToHumanReadable(wallTimeSpan));
        }

        private string ExtractHeader(string line)
        {
            string res = null;
            var reader = new JsonTextReader(new StringReader(line));
            while (reader.Read())
            {
                if (reader.TokenType == JsonToken.PropertyName)
                {
                    // Load each object from the stream and do something with it
                    var obj = JToken.ReadFrom(reader);
                    res = obj.ToString(Formatting.None);
                    break;
                }
            }
            return res;
        }

        // ReSharper disable once UnusedParameter.Local
        private void CheckFileSorted(Tuple<string, int, int> chrPos, string previousChr, int previousPos)
        {
            var chr = chrPos.Item1;
            var pos = chrPos.Item2;
            if (chr != previousChr && _processedChromosome.Contains(chr))
            {
                throw new UserErrorException($"the Json file is not sorted at {chr}: {pos}");
            }
            if (chr == previousChr && pos < previousPos)
            {
                throw new UserErrorException($"the Json file is not sorted at {chr}: {pos}");
            }
            if (chr != previousChr)
            {
                if (previousChr != "")
                {
                    Console.WriteLine($"Ref Sequence {previousChr} indexed in {Benchmark.ToHumanReadable(_chromBenchmark.GetElapsedTime())}");
                    _chromBenchmark.Reset();
                }
            }

        }

        internal static Tuple<string, int, int> GetChromPosition(string line)
        {
            JsonSchema jsonEntry;
            try
            {
                jsonEntry = JsonConvert.DeserializeObject<JsonSchema>(line);
            }
            catch (Exception)
            {
                Console.WriteLine($"Error in line:\n{line}");
                throw;
            }

			var end = Utilities.GetJsonEntryEnd(jsonEntry);

            return new Tuple<string, int, int>(jsonEntry.chromosome, jsonEntry.position, end);
        }
    }
}
