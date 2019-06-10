using System;
using System.IO;
using System.IO.Compression;
using Compression.FileHandling;
using ErrorHandling.Exceptions;
using IO;

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

        public static StreamReader GetAppropriateStreamReader(string filePath) => FileUtilities.GetStreamReader(GetAppropriateReadStream(filePath));
        public static StreamWriter GetStreamWriter(string filePath) => new StreamWriter(GetWriteStream(filePath));
        public static Stream GetWriteStream(string filePath) => new BlockGZipStream(FileUtilities.GetCreateStream(filePath), CompressionMode.Compress);

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

        //todo: can have just one method for both file and http streams
        //used in custom annotation lambda
        public static Stream GetAppropriateStream(PersistentStream pStream)
        {
            var header = GetHeader(pStream);
            var compressionAlgorithm = IdentifyCompressionAlgorithm(header);
            pStream.Position = 0;
            var appropriateStream = GetAppropriateStream(pStream, compressionAlgorithm);
            return appropriateStream;
        }

        public static Stream GetAppropriateReadStream(string filePath)
        {
            var header = GetHeader(PersistentStreamUtils.GetReadStream(filePath));
            var compressionAlgorithm = IdentifyCompressionAlgorithm(header);
            var fileStream = PersistentStreamUtils.GetReadStream(filePath);
            return GetAppropriateStream(fileStream, compressionAlgorithm);
        }

        private static byte[] GetHeader(Stream stream)
        {
            byte[] header = null;

            try
            {
                using (var reader = new ExtendedBinaryReader(stream))
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