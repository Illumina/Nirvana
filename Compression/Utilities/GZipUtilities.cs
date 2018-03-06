using System;
using System.IO;
using System.IO.Compression;
using Compression.FileHandling;
using ErrorHandling.Exceptions;

namespace Compression.Utilities
{
    public static class GZipUtilities
    {
        private const int NumHeaderBytes = 18;

        private enum CompressionAlgorithm
        {
            Uncompressed,
            GZip,
            BlockGZip
        }

        /// <summary>
        /// returns a stream reader that handles compressed or uncompressed files.
        /// </summary>
        public static StreamReader GetAppropriateStreamReader(string filePath)
        {
            return new StreamReader(GetAppropriateReadStream(filePath));
        }

        public static Stream GetAppropriateStream(PeekStream peekStream)
        {
            var header = peekStream.PeekBytes(NumHeaderBytes);
            var compressionAlgorithm = IdentifyCompressionAlgorithm(header);
            return GetAppropriateStream(peekStream, compressionAlgorithm);
        }

        private static Stream GetAppropriateStream(Stream stream, CompressionAlgorithm compressionAlgorithm)
        {
            Stream newStream;

            // ReSharper disable once SwitchStatementMissingSomeCases
            switch (compressionAlgorithm)
            {
                case CompressionAlgorithm.BlockGZip:
                    newStream = new BlockGZipStream(stream, CompressionMode.Decompress);
                    break;
                case CompressionAlgorithm.GZip:
                    newStream = new GZipStream(stream, CompressionMode.Decompress);
                    break;
                default:
                    newStream = stream;
                    break;
            }

            return newStream;
        }

        /// <summary>
        /// returns a stream reader that handles compressed or uncompressed files.
        /// </summary>
        public static BinaryReader GetAppropriateBinaryReader(string filePath)
        {
            return new BinaryReader(GetAppropriateReadStream(filePath));
        }

        /// <summary>
        /// returns a stream reader that handles compressed or uncompressed files.
        /// </summary>
        public static Stream GetAppropriateReadStream(string filePath)
        {
            var header = GetHeader(filePath);
            var compressionAlgorithm = IdentifyCompressionAlgorithm(header);
            var fileStream = FileUtilities.GetReadStream(filePath);
            return GetAppropriateStream(fileStream, compressionAlgorithm);
        }

        /// <summary>
        /// returns a stream writer that produces compressed files
        /// </summary>
        public static StreamWriter GetStreamWriter(string filePath)
        {
            return new StreamWriter(GetWriteStream(filePath));
        }

        /// <summary>
        /// returns a binary writer that produces compressed files
        /// </summary>
        public static BinaryWriter GetBinaryWriter(string filePath)
        {
            return new BinaryWriter(GetWriteStream(filePath));
        }

        /// <summary>
        /// returns a stream reader that handles compressed or uncompressed files.
        /// </summary>
        private static Stream GetWriteStream(string filePath)
        {
            return new BlockGZipStream(FileUtilities.GetCreateStream(filePath), CompressionMode.Compress);
        }

        private static byte[] GetHeader(string filePath)
        {
            byte[] header = null;

            try
            {
                using (var reader = new BinaryReader(FileUtilities.GetReadStream(filePath)))
                {
                    header = reader.ReadBytes(NumHeaderBytes);
                }
            }
            catch (Exception e)
            {
                if (e.Message.Contains("because it is being used by another process."))
                {
                    throw new ProcessLockedFileException(e.Message);
                }
            }

            return header;
        }

        // ReSharper disable once SuggestBaseTypeForParameter
        private static CompressionAlgorithm IdentifyCompressionAlgorithm(byte[] header)
        {
            var result = CompressionAlgorithm.Uncompressed;
            if (header == null || header.Length != NumHeaderBytes) return result;

            // check if this is a gzip file
            if (header[0] != 31 || header[1] != 139 || header[2] != 8) return result;
            result = CompressionAlgorithm.GZip;

            // check if this is a block GZip file
            if ((header[3] & 4) != 0 && header[12] == 66 && header[13] == 67) result = CompressionAlgorithm.BlockGZip;

            return result;
        }
    }
}