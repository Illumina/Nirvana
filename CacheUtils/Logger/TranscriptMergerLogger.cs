using System;
using System.IO;
using VariantAnnotation.Interface;

namespace CacheUtils.Logger
{
    public sealed class TranscriptMergerLogger : ILogger, IDisposable
    {
        private readonly StreamWriter _writer;

        public TranscriptMergerLogger(Stream stream) => _writer = new StreamWriter(stream);

        public void WriteLine()         => _writer.WriteLine();
        public void WriteLine(string s) => _writer.WriteLine(s);
        public void Write(string s)     => _writer.Write(s);

        public void SetBold()    { }
        public void ResetColor() { }

        public void Dispose() => _writer.Dispose();
    }
}
