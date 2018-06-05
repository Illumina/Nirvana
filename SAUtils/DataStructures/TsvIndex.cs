using System;
using System.Collections.Generic;
using System.IO;
using IO;

namespace SAUtils.DataStructures
{
    public sealed class TsvIndex : IDisposable
    {

        #region members

        public const string FileExtension = ".tvi";
        private readonly BinaryWriter _writer;
        public Dictionary<string, long> TagPositions { get; }

        #endregion

        public TsvIndex(string fileName)
        {
            _writer = new BinaryWriter(File.Open(fileName, FileMode.Create, FileAccess.Write));
            TagPositions = new Dictionary<string, long>();
        }

        public TsvIndex(ExtendedBinaryReader reader)
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
            if (!TagPositions.TryAdd(tag, position))
                throw new InvalidDataException($"second block of entries for chrom:{tag}!!");
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

        public void Dispose()
        {
            WriteToFile();
            _writer?.Flush();
            _writer?.Dispose();
        }
    }
}