using System;
using System.IO;
using System.IO.Compression;
using System.Runtime.InteropServices;
using ErrorHandling.Exceptions;

namespace VariantAnnotation.Compression
{
    // BGZF/GZIP header (specialized from RFC 1952; little endian):
    // +---+---+---+---+---+---+---+---+---+---+---+---+---+---+---+---+---+---+
    // | 31|139|  8|  4|              0|  0|255|      6| 66| 67|      2|BLK_LEN|
    // +---+---+---+---+---+---+---+---+---+---+---+---+---+---+---+---+---+---+

    public class BlockGZipStream : Stream
    {
        #region members

        private byte[] _compressedBlock;
        private byte[] _uncompressedBlock;
        private int _blockOffset;
        private int _blockLength;
        private long _blockAddress;

        private readonly bool _isCompressor;
        private readonly int _compressionLevel;
        private readonly bool _leaveStreamOpen;

        private string _filePath;
        private Stream _stream;
        private BgzfNative _bgzf;

        private static class BlockGZipFormatCommon
        {
            #region members

            public const int BlockSize         = 65280;
            public const int MaxBlockSize      = 65536;
            public const int BlockHeaderLength = 18;

            #endregion
        }

        #endregion

        #region Stream

        public override bool CanRead => _stream != null && !_isCompressor && _stream.CanRead;

        public override bool CanWrite => _stream != null && _isCompressor && _stream.CanWrite;

        public override bool CanSeek => _stream != null && !_isCompressor && _stream.CanSeek;

        public override long Length
        {
            get { throw new NotSupportedException(); }
        }

        public override long Position
        {
            get { return (_blockAddress << 16) | ((long)_blockOffset & 0xffff); }
            set { throw new NotSupportedException(); }
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
            _stream?.Flush();
        }

        protected override void Dispose(bool disposing)
        {
            try
            {
                if (disposing && _stream != null)
                {
                    if (_isCompressor)
                    {
                        // flush the current BGZF block
                        Flush(_blockOffset);

                        // write an empty block (as EOF marker)
                        Flush(0);
                    }

                    if (!_leaveStreamOpen)
                    {
                        _stream.Dispose();
                        _stream = null;
                    }
                }

                _compressedBlock   = null;
                _uncompressedBlock = null;
                _bgzf              = null;
                _filePath          = null;

                _blockOffset = 0;
                _blockLength = 0;
            }
            finally
            {
                base.Dispose(disposing);
            }
        }

        #endregion

        // private constructor
        private BlockGZipStream(CompressionMode compressionMode, int compressionLevel)
        {
            _compressedBlock   = new byte[BlockGZipFormatCommon.MaxBlockSize];
            _uncompressedBlock = new byte[BlockGZipFormatCommon.MaxBlockSize];
            _bgzf              = new BgzfNative();
            _compressionLevel  = compressionLevel;

            _isCompressor = compressionMode == CompressionMode.Compress;
        }

        // stream constructor
        public BlockGZipStream(Stream stream, CompressionMode compressionMode, bool leaveStreamOpen = false, int compressionLevel = 1)
            : this(compressionMode, compressionLevel)
        {
            _filePath        = "(stream)";
            _leaveStreamOpen = leaveStreamOpen;
            _stream          = stream;

            // sanity check: make sure we can use the stream for reading or writing
            if (_isCompressor  && !_stream.CanWrite) throw new CompressionException("A stream lacking write capability was provided to the block GZip compressor.");
            if (!_isCompressor && !_stream.CanRead)  throw new CompressionException("A stream lacking read capability was provided to the block GZip decompressor.");
        }

        /// <summary>
        /// flushes the BGZF stream
        /// </summary>
        private void Flush(int uncompressedSize)
        {
            int blockLength = BlockGZipFormatCommon.MaxBlockSize;

            try
            {
                BgzfNative.Compress(_compressedBlock, ref blockLength, _uncompressedBlock, uncompressedSize, _compressionLevel);
            }
            catch (Exception e)
            {
                throw new CompressionException($"Unable to compress the data when flushing the Block GZip stream ({_filePath}): {e.Message}");
            }

            _blockOffset = 0;

            try
            {
                _stream.Write(_compressedBlock, 0, blockLength);
            }
            catch (Exception e)
            {
                throw new CompressionException($"Unable to write the compressed data to the output file ({_filePath}): {e.Message}");
            }
        }

        /// <summary>
        /// returns true if this GZip block has a valid header
        /// </summary>
        private static bool HasValidHeader(int numHeaderBytes, byte[] header)
        {
            if (numHeaderBytes != BlockGZipFormatCommon.BlockHeaderLength) return false;

            return (header[0] == 31)      &&
                   (header[1] == 139)     &&
                   (header[2] == 8)       &&
                   ((header[3] & 4) != 0) &&
                   (header[12] == 66)     &&
                   (header[13] == 67);
        }

        /// <summary>
        /// reads the next GZip block from the disk
        /// </summary>
        private int ReadBlock()
        {
            var header = new byte[BlockGZipFormatCommon.BlockHeaderLength];
            long blockAddress = _stream.Position;
            int count  = _stream.Read(header, 0, BlockGZipFormatCommon.BlockHeaderLength);

            // handle the case where no data was read
            if (count == 0)
            {
                _blockLength = 0;
                return 0;
            }

            // check the header
            if (!HasValidHeader(count, header))
            {
                throw new CompressionException($"Found an invalid header when reading the GZip block ({_filePath})");
            }

            int blockLength = BitConverter.ToUInt16(header, 16) + 1;
            int remaining   = blockLength - BlockGZipFormatCommon.BlockHeaderLength;

            Buffer.BlockCopy(header, 0, _compressedBlock, 0, BlockGZipFormatCommon.BlockHeaderLength);

            count = _stream.Read(_compressedBlock, BlockGZipFormatCommon.BlockHeaderLength, remaining);

            // handle unexpected truncation
            if (count != remaining)
            {
                throw new CompressionException($"Found unexpected truncation when reading the GZip block ({_filePath})");
            }

            count = _bgzf.Uncompress(_uncompressedBlock, _compressedBlock, blockLength);

            if (count < 0)
            {
                throw new CompressionException($"Encountered an error when uncompressing the GZip block ({_filePath})");
            }

            // Do not reset offset if this read follows a seek
            if (_blockLength != 0) _blockOffset = 0;

            _blockAddress = blockAddress;
            _blockLength  = count;

            return 0;
        }

        /// <summary>
        /// reads from the disk into the byte array
        /// </summary>
        public override int Read(byte[] data, int offset, int numBytesToRead)
        {
            if (_isCompressor) throw new CompressionException("Tried to read data from a compression BlockGZipStream.");

            if (numBytesToRead == 0) return 0;

            int numBytesRead = 0;
            int dataOffset   = offset;

            while (numBytesRead < numBytesToRead)
            {
                int numBytesAvailable = _blockLength - _blockOffset;

                if (numBytesAvailable <= 0)
                {
                    if (ReadBlock() != 0) return -1;
                    numBytesAvailable = _blockLength - _blockOffset;
                    if (numBytesAvailable <= 0) break;
                }

                int copyLength = Math.Min(numBytesToRead - numBytesRead, numBytesAvailable);
                Buffer.BlockCopy(_uncompressedBlock, _blockOffset, data, dataOffset, copyLength);

                _blockOffset += copyLength;
                dataOffset   += copyLength;
                numBytesRead += copyLength;
            }

            if (_blockOffset == _blockLength)
            {
                _blockAddress = _stream.Position;
                _blockOffset  = _blockLength = 0;
            }

            return numBytesRead;
        }

        /// <summary>
        /// writes the byte array to disk. Returns the number of bytes written
        /// </summary>
        public override void Write(byte[] data, int offset, int numBytesToWrite)
        {
            if (!_isCompressor) throw new CompressionException("Tried to write data to a decompression BlockGZipStream.");

            int numBytesWritten = 0;
            int dataOffset      = offset;

            // copy the data to the buffer
            while (numBytesWritten < numBytesToWrite)
            {
                int copyLength = Math.Min(BlockGZipFormatCommon.BlockSize - _blockOffset, numBytesToWrite - numBytesWritten);
                Buffer.BlockCopy(data, dataOffset, _uncompressedBlock, _blockOffset, copyLength);

                _blockOffset    += copyLength;
                dataOffset      += copyLength;
                numBytesWritten += copyLength;

                if (_blockOffset == BlockGZipFormatCommon.BlockSize) Flush(_blockOffset);
            }
        }
    }

    internal class BgzfNative
    {
        // constructor
        public BgzfNative()
        {
            string compressionLibraryName = Type.GetType("Mono.Runtime") == null 
                ? "BlockCompression.dll" 
                : "libBlockCompression.so";

            // check to see if we have our compression library
            try
            {
                Marshal.PtrToStringAnsi(SafeNativeMethods.GetVersion());
            }
            catch (Exception)
            {
                throw new MissingCompressionLibraryException(compressionLibraryName);
            }
        }

        /// <summary>
        /// compresses a byte array and stores the result in the output byte array
        /// </summary>
        internal static void Compress(byte[] dst, ref int dstLen, byte[] src, int srcLen, int compressionLevel)
        {
            SafeNativeMethods.bgzf_compress(dst, ref dstLen, src, srcLen, compressionLevel);
        }

        /// <summary>
        /// compresses a byte array and stores the result in the output byte array
        /// </summary>
        internal int Uncompress(
            byte[] uncompressedBlock,
            byte[] compressedBlock,
            int blockLength)
        {
            return SafeNativeMethods.uncompress_block(uncompressedBlock, compressedBlock, blockLength);
        }

        private static class SafeNativeMethods
        {
            [DllImport("BlockCompression", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
            public static extern IntPtr GetVersion();

            [DllImport("BlockCompression", CallingConvention = CallingConvention.Cdecl)]
            public static extern int uncompress_block(byte[] uncompressedBlock, byte[] compressedBlock, int blockLength);

            [DllImport("BlockCompression", CallingConvention = CallingConvention.Cdecl)]
            public static extern int bgzf_compress(byte[] compressedBlock, ref int compressedLen, byte[] uncompressedBlock, int uncompressedLen, int compressionLevel);
        }
    }
}
