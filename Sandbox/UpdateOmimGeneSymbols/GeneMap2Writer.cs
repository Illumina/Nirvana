using System;
using System.Collections.Generic;
using System.IO;

namespace UpdateOmimGeneSymbols
{
    public sealed class GeneMap2Writer : IDisposable
    {
        #region members

        private readonly StreamWriter _writer;

        #endregion

        #region IDisposable

        public void Dispose()
        {
            Dispose(true);
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                _writer.Dispose();
            }
        }

        #endregion

        /// <summary>
        /// constructor
        /// </summary>
        public GeneMap2Writer(Stream stream, IEnumerable<string> headerLines)
        {
            _writer = new StreamWriter(stream) { NewLine = "\n" };

            // write the comments
            foreach (var line in headerLines) _writer.WriteLine(line);
        }

        /// <summary>
        /// retrieves the next gene. Returns false if there are no more genes available
        /// </summary>
        public void Write(GeneMap2Entry entry)
        {
            _writer.WriteLine(entry);
        }
    }
}
