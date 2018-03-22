using System;
using System.IO;
using System.Text;
using CacheUtils.Genes.DataStructures;

namespace CacheUtils.Genes.IO
{
    public sealed class UgaGeneWriter : IDisposable
    {
        private readonly StreamWriter _writer;

        public UgaGeneWriter(Stream stream, bool leaveOpen = false)
        {
            _writer = new StreamWriter(stream, Encoding.ASCII, 1024, leaveOpen);
        }

        public void Dispose() => _writer.Dispose();

        public void Write(UgaGene[] genes)
        {
            _writer.WriteLine(genes.Length);
            foreach (var gene in genes) _writer.WriteLine(gene.ToString());
        }
    }
}
