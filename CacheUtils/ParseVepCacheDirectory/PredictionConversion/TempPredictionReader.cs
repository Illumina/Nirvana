using System;
using System.IO;
using CacheUtils.DataDumperImport.FileHandling;
using VariantAnnotation.FileHandling.TranscriptCache;
using VariantAnnotation.Interface;
using ErrorHandling.Exceptions;
using VariantAnnotation.DataStructures.Transcript;
using VariantAnnotation.FileHandling.Compression;

namespace CacheUtils.ParseVepCacheDirectory.PredictionConversion
{
    public sealed class TempPredictionReader : IDisposable
    {
        #region members

        private readonly BinaryReader _reader;
        public readonly GlobalImportHeader Header;

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
                _reader.Dispose();
            }
        }

        #endregion

        /// <summary>
        /// constructor
        /// </summary>
        public TempPredictionReader(string filePath, string description, GlobalImportCommon.FileType fileType)
        {
            // sanity check
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"The specified protein function prediction file ({filePath}) does not exist.");
            }

            // open the vcf file and parse the header
            _reader = GZipUtilities.GetAppropriateBinaryReader(filePath);
            Header  = GetHeader(description, filePath, fileType, _reader);
        }

        public TempPrediction Next()
        {
            try
            {
                ushort refIndex = _reader.ReadUInt16();
                int numShorts   = _reader.ReadInt32();
                var shorts      = new ushort[numShorts];

                for (int i = 0; i < numShorts; i++)
                {
                    shorts[i] = _reader.ReadUInt16();
                }

                return new TempPrediction(refIndex, shorts);
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// returns the file header
        /// </summary>
        private static GlobalImportHeader GetHeader(string description, string filePath,
            GlobalImportCommon.FileType expectedFileType, BinaryReader reader)
        {
            var header = reader.ReadString();
            var fileType = (GlobalImportCommon.FileType)reader.ReadByte();

            if (!IsValidFile(header, fileType, expectedFileType))
            {
                throw new GeneralException($"The {description} file ({filePath}) has an invalid header.");
            }

            var vepVersion       = reader.ReadUInt16();
            var vepReleaseTicks  = reader.ReadInt64();
            var transcriptSource = (TranscriptDataSource)reader.ReadByte();
            var genomeAssembly   = (GenomeAssembly)reader.ReadByte();
            var guardInt         = reader.ReadUInt32();

            if (guardInt != CacheConstants.GuardInt)
            {
                throw new GeneralException($"The {description} file ({filePath}) has an invalid header.");
            }

            return new GlobalImportHeader(vepVersion, vepReleaseTicks, transcriptSource, genomeAssembly);
        }

        /// <summary>
        /// returns true if this is a valid VEP reader file (binary)
        /// </summary>
        private static bool IsValidFile(string header, GlobalImportCommon.FileType fileType,
            GlobalImportCommon.FileType expectedFileType)
        {
            return header == GlobalImportCommon.Header && fileType == expectedFileType;
        }
    }
}
