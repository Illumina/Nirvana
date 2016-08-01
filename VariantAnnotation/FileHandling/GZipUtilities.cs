using System;
using System.IO;
using System.IO.Compression;
using ErrorHandling.Exceptions;
using VariantAnnotation.Compression;
using VariantAnnotation.Utilities;

namespace VariantAnnotation.FileHandling
{
    public static class GZipUtilities
    {
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
            var compressionAlgorithm = IdentifyCompressionAlgorithm(filePath);
            var fileStream = FileUtilities.GetFileStream(filePath);
            Stream s;

            switch (compressionAlgorithm)
            {
                case CompressionAlgorithm.BlockGZip:
                    s = new BlockGZipStream(fileStream, CompressionMode.Decompress);
                    break;
                case CompressionAlgorithm.GZip:
                    s = new GZipStream(fileStream, CompressionMode.Decompress);
                    break;
                default:
                    s = fileStream;
                    break;
            }

            return new StreamReader(s);
        }

        /// <summary>
        /// returns a stream writer that produces compressed files
        /// </summary>
        public static StreamWriter GetStreamWriter(string filePath)
        {
            return new StreamWriter(new BlockGZipStream(new FileStream(filePath, FileMode.Create), CompressionMode.Compress));
        }

        private static CompressionAlgorithm IdentifyCompressionAlgorithm(string filePath)
        {
            const int numHeaderBytes = 18;
            byte[] header = null;

            try
            {
                using (var reader = new BinaryReader(FileUtilities.GetFileStream(filePath)))
                {
                    header = reader.ReadBytes(numHeaderBytes);
                }
            }
            catch (Exception e)
            {
                if (e.Message.Contains("because it is being used by another process."))
                {
                    throw new ProcessLockedFileException(e.Message);
                }
            }

            var result = CompressionAlgorithm.Uncompressed;
            if ((header == null) || (header.Length != numHeaderBytes)) return result;

            // check if this is a gzip file
            if ((header[0] != 31) || (header[1] != 139) || (header[2] != 8)) return result;
            result = CompressionAlgorithm.GZip;

            // check if this is a block GZip file
            if (((header[3] & 4) != 0) && (header[12] == 66) && (header[13] == 67)) result = CompressionAlgorithm.BlockGZip;

            return result;
        }
    }
}