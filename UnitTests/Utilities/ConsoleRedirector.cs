using System;
using System.IO;

namespace UnitTests.Utilities
{
    public sealed class ConsoleRedirector : IDisposable
    {
        private readonly TextWriter _originalWriter;
        private StringWriter _newWriter;
        private bool _isDisposed;

        /// <summary>
        /// constructor
        /// </summary>
        public ConsoleRedirector()
        {
            _originalWriter = Console.Out;
            _newWriter = new StringWriter();
            Console.SetOut(_newWriter);
        }

        /// <summary>
        /// Closes the current ConsoleRedirector and the underlying stream.
        /// </summary>
        public void Close()
        {
            Dispose(true);
        }

        /// <summary>
        /// Releases the unmanaged resources used by the ConsoleRedirector and optionally releases the managed resources.
        /// </summary>
        private void Dispose(bool disposing)
        {
            if (!disposing || _isDisposed) return;

            _isDisposed = true;
            _newWriter.Dispose();
            Console.SetOut(_originalWriter);
            _newWriter = null;
        }

        /// <summary>
        /// Releases all resources used by the current instance of the ConsoleRedirector class.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }

        /// <summary>
        /// returns the output captured by the string writer
        /// </summary>
        public override string ToString()
        {
            return _newWriter.ToString();
        }
    }
}
