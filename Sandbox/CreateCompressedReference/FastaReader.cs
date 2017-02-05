using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using ErrorHandling.Exceptions;
using VariantAnnotation.FileHandling;
using VariantAnnotation.Utilities;

namespace CreateCompressedReference
{
    public class FastaReader : IDisposable
    {
        private Regex _nameRegex;
        private StreamReader _reader;
        private StringBuilder _sb;

        public FastaReader(string filename)
        {
            _nameRegex = new Regex("^>(\\S+)", RegexOptions.Compiled);
            _reader    = new StreamReader(FileUtilities.GetReadStream(filename));
            _sb        = new StringBuilder();
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing) _reader.Dispose();
            _nameRegex = null;
            _reader = null;
            _sb = null;
        }

        public virtual void Close()
        {
            Dispose(true);
        }

        public ReferenceSequence GetReferenceSequence()
        {
            string input = _reader.ReadLine();
            if (input == null) return null;

            if (!input.StartsWith(">")) throw new UserErrorException($"Encountered a FASTA header that did not start with '>': {input}");

            var match = _nameRegex.Match(input);

            var referenceSequence = new ReferenceSequence
            {
                Name = match.Groups[1].Value
            };

            _sb.Clear();
            for (int index = _reader.Peek(); index != -1 && index != 62; index = _reader.Peek())
            {
                string str = _reader.ReadLine();
                if (str != null) _sb.Append(str);
                else break;
            }

            referenceSequence.Bases = _sb.ToString();
            return referenceSequence;
        }
    }
}