using System;
using System.Collections.Generic;
using System.IO;

namespace SAUtils.DataStructures
{
    public sealed class TsvIndex : IDisposable
    {

        #region members

        public const string FileExtension = ".tvi";
        private readonly BinaryWriter _writer;
        public Dictionary<string, long> TagPositions { get; }

        #endregion

        #region IDisposable

        private bool _disposed;

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
                WriteToFile();
                _writer?.Flush();
                _writer?.Dispose();
            }

            // Free any unmanaged objects here.
            _disposed = true;
        }

        #endregion

        public TsvIndex(string fileName)
        {
            _writer = new BinaryWriter(File.Open(fileName, FileMode.Create, FileAccess.Write));
            TagPositions = new Dictionary<string, long>();
        }

        public TsvIndex(BinaryReader reader)
        {
            var count = reader.ReadInt32();
            if (count == 0) return;

            TagPositions = new Dictionary<string, long>(count);

            while (count > 0)
            {
                var tag = reader.ReadString();
                TagPositions[tag] = reader.ReadInt64();
                count--;
            }
        }

        public void AddTagPosition(string tag, long position)
        {
            TagPositions[tag] = position;
        }

        private void WriteToFile()
        {
            if (TagPositions == null || _writer == null) return;
            _writer.Write(TagPositions.Count);
            foreach (var tagPosition in TagPositions)
            {
                _writer.Write(tagPosition.Key);
                _writer.Write(tagPosition.Value);
            }
        }
    }
}