using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using Compression.Algorithms;
using Compression.DataStructures;
using ErrorHandling.Exceptions;
using VariantAnnotation.Interface.IO;

namespace Compression.FileHandling
{
	public sealed class BlockStream : Stream
	{
		private readonly bool _isCompressor;
		private readonly bool _leaveStreamOpen;

		private Stream _stream;
		private BinaryWriter _writer;
        private Action<BinaryWriter> _headerWrite;

        private readonly Block _block;
		private bool _foundEof;
		private bool _isDisposed;

		#region Stream

		public override bool CanRead => _stream.CanRead;

		public override bool CanWrite => _stream.CanWrite;

		public override bool CanSeek => _stream.CanSeek;

		public override long Length => throw new NotSupportedException();

		public override long Position
		{
			get => _stream.Position;
			set => throw new NotSupportedException();
		}

		public override long Seek(long offset, SeekOrigin origin)
		{
			throw new NotSupportedException();
		}

		public override void SetLength(long value)
		{
			throw new NotSupportedException();
		}

		public override void Flush()
		{
			if (_block.Offset > 0) _block.Write(_stream);
		}

		protected override void Dispose(bool disposing)
		{
			if (_isDisposed) return;

			try
			{
				if (_isCompressor)
				{
					Flush();
					_block.WriteEof(_stream);

                    // update the header
                    if (_headerWrite != null)
                    {
                        _stream.Position = 0;
                        _headerWrite(_writer);
                    }

                    _writer.Dispose();
					_writer = null;
				}

				if (!_leaveStreamOpen)
				{
					_stream.Dispose();
					_stream = null;
				}

				_isDisposed = true;
			}
			finally
			{
				base.Dispose(disposing);
			}
		}

		#endregion

		public BlockStream(ICompressionAlgorithm compressionAlgorithm, Stream stream, CompressionMode compressionMode,
			bool leaveStreamOpen = false, int size = 16777216)
		{
			_stream          = stream ?? throw new ArgumentNullException(nameof(stream));
			_isCompressor    = compressionMode == CompressionMode.Compress;
			_leaveStreamOpen = leaveStreamOpen;
			_block           = new Block(compressionAlgorithm, size);

			// sanity check: make sure we can use the stream for reading or writing
			if (_isCompressor  && !_stream.CanWrite) throw new ArgumentException("A stream lacking write capability was provided to the block GZip compressor.");
			if (!_isCompressor && !_stream.CanRead)  throw new ArgumentException("A stream lacking read capability was provided to the block GZip decompressor.");

			if (_isCompressor) _writer = new BinaryWriter(_stream, Encoding.UTF8, true);
        }

        public void WriteHeader(Action<BinaryWriter> headerWrite)
        {
            _headerWrite = headerWrite;
            _headerWrite(_writer);
        }

        public IFileHeader ReadHeader(Func<BinaryReader, Func<BinaryReader, ICustomCacheHeader>, IFileHeader> headerRead,
            Func<BinaryReader, ICustomCacheHeader> customRead)
        {
            IFileHeader header;
            using (var reader = new BinaryReader(_stream, Encoding.UTF8, true)) header = headerRead(reader, customRead);
            return header;
        }

        public override int Read(byte[] buffer, int offset, int count)
		{
			if (_foundEof) return 0;
			if (_isCompressor) throw new CompressionException("Tried to read data from a compression BlockGZipStream.");

			ValidateParameters(buffer, offset, count);

			var numBytesRead = 0;
			int dataOffset = offset;

			while (numBytesRead < count)
			{
				if (!_block.HasMoreData)
				{
					int numBytes = _block.Read(_stream);

					if (numBytes == -1)
					{
						_foundEof = true;
						return numBytesRead;
					}
				}

				int copyLength = _block.CopyFrom(buffer, dataOffset, count - numBytesRead);

				dataOffset += copyLength;
				numBytesRead += copyLength;
			}

			return numBytesRead;
		}

		private void ValidateParameters(byte[] array, int offset, int count)
		{
			if (array == null) throw new ArgumentNullException(nameof(array));
			if (offset < 0) throw new ArgumentOutOfRangeException(nameof(offset));
			if (count < 0) throw new ArgumentOutOfRangeException(nameof(count));
			if (array.Length - offset < count) throw new ArgumentException("Invalid Argument Offset Count");
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			if (!_isCompressor) throw new CompressionException("Tried to write data to a decompression BlockGZipStream.");

			ValidateParameters(buffer, offset, count);

			var numBytesWritten = 0;
			int dataOffset = offset;

			while (numBytesWritten < count)
			{
				int copyLength = _block.CopyTo(buffer, dataOffset, count - numBytesWritten);
				dataOffset += copyLength;
				numBytesWritten += copyLength;
				if (_block.IsFull) _block.Write(_stream);
			}
		}

		public sealed class BlockPosition
		{
			public long FileOffset;
			public int InternalOffset;
		}

		/// <summary>
		/// retrieves the current position of the block
		/// </summary>
		public void GetBlockPosition(BlockPosition bp)
		{
			bp.FileOffset = _stream.Position;
			bp.InternalOffset = _block.Offset;
		}

		public void SetBlockPosition(BlockPosition bp)
		{
			if (bp.FileOffset != _block.FileOffset)
			{
				_stream.Position = bp.FileOffset;
				_block.Read(_stream);
			}

			_block.Offset = bp.InternalOffset;
		}
	}
}
