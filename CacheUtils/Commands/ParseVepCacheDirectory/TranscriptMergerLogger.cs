using System;
using System.IO;

namespace CacheUtils.Commands.ParseVepCacheDirectory
{
    public sealed class TranscriptMergerLogger : IDisposable
    {
        private readonly StreamWriter _writer;

        public TranscriptMergerLogger(Stream stream) => _writer = new StreamWriter(stream);

        public void WriteLine()         => _writer.WriteLine();
        public void WriteLine(string s) => _writer.WriteLine(s);
        public void Write(string s)     => _writer.Write(s);

        public void SetBold()    {
            // not used
        }

        public void ResetColor()
        {
            // not used
        }

        public void Dispose() => _writer.Dispose();
    }
}
