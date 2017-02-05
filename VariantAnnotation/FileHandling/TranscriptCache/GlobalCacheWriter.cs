using System;
using System.Collections.Generic;
using System.IO.Compression;
using VariantAnnotation.FileHandling.Compression;
using VariantAnnotation.Utilities;

namespace VariantAnnotation.FileHandling.TranscriptCache
{
    public sealed class GlobalCacheWriter : IDisposable
    {
        #region members

        private readonly BlockStream _blockStream;
        private readonly ExtendedBinaryWriter _writer;
        private readonly FileHeader _header;

        #endregion

        #region IDisposable

        /// <summary>
        /// public implementation of Dispose pattern callable by consumers. 
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }

        /// <summary>
        /// protected implementation of Dispose pattern. 
        /// </summary>
        private void Dispose(bool disposing)
        {
            if (disposing) _writer.Dispose();
        }

        #endregion

        /// <summary>
        /// constructor
        /// </summary>
        public GlobalCacheWriter(string dbPath, FileHeader header)
        {
            _blockStream = new BlockStream(new Zstandard(), FileUtilities.GetCreateStream(dbPath),
                CompressionMode.Compress);

            _writer = new ExtendedBinaryWriter(_blockStream);
            _header = header;
        }

        /// <summary>
        /// creates an index out of a array
        /// </summary>
        private static Dictionary<T, int> CreateIndex<T>(T[] array)
        {
            var index = new Dictionary<T, int>();

            if (array != null)
            {
                for (int currentIndex = 0; currentIndex < array.Length; currentIndex++) index[array[currentIndex]] = currentIndex;
            }

            return index;
        }

        /// <summary>
        /// writes the annotations to the current database file
        /// </summary>
        public void Write(DataStructures.GlobalCache cache)
        {
            // write the header
            _blockStream.WriteHeader(_header);

            // write out each of our arrays
            WriteItems(cache.RegulatoryElements);
            WriteItems(cache.Genes);
            WriteItems(cache.Introns);
            WriteItems(cache.MicroRnas);
            WriteItems(cache.PeptideSeqs);

            // write our transcripts
            WriteTranscripts(cache);
        }

        /// <summary>
        /// writes out each transcript in the cache
        /// </summary>
        private void WriteTranscripts(DataStructures.GlobalCache cache)
        {
            if (cache.Transcripts == null)
            {
                _writer.WriteOpt(0);
            }
            else
            {
                // create index dictionaries for each data type
                var geneIndices     = CreateIndex(cache.Genes);
                var intronIndices   = CreateIndex(cache.Introns);
                var microRnaIndices = CreateIndex(cache.MicroRnas);
                var peptideIndices  = CreateIndex(cache.PeptideSeqs);

                // write the transcripts
                _writer.WriteOpt(cache.Transcripts.Length);
                foreach (var transcript in cache.Transcripts)
                {
                    transcript.Write(_writer, geneIndices, intronIndices, microRnaIndices, peptideIndices);
                }
            }

            _writer.Write(CacheConstants.GuardInt);
        }

        /// <summary>
        /// writes all of the items from a array to the output
        /// </summary>
        private void WriteItems<T>(T[] list) where T : ICacheSerializable
        {
            if (list == null)
            {
                _writer.WriteOpt(0);
            }
            else
            {
                _writer.WriteOpt(list.Length);
                foreach (var item in list) item.Write(_writer);
            }

            _writer.Write(CacheConstants.GuardInt);
        }

        private void WriteItems(string[] list)
        {
            if (list == null)
            {
                _writer.WriteOpt(0);
            }
            else
            {
                _writer.WriteOpt(list.Length);
                foreach (var item in list) _writer.WriteOptAscii(item);
            }

            _writer.Write(CacheConstants.GuardInt);
        }
    }
}
